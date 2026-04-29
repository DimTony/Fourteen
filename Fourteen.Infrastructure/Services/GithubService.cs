using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Fourteen.Infrastructure.Services
{
    public class GithubService : IGithubClient
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<GithubService> _logger;

        public GithubService(IConfiguration config, ILogger<GithubService> logger)
        {
            _config = config;
            _logger = logger;
        }


        public string BuildAuthUrl(string state, string redirectUri)                                                        
        {
            var query = QueryHelpers.AddQueryString(_config["GitHub:GithubAuthUrl"]!,
                new Dictionary<string, string?>
                {
                    ["client_id"]    = _config["GitHub:ClientId"]!,
                    ["redirect_uri"] = redirectUri,
                    ["scope"]        = "read:user user:email",
                    ["state"]        = state
                });

            return query;
        }

        public async Task<GithubUserDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken ct)
        {
            var tokenResponse = await RequestGithubTokenAsync(code, redirectUri, ct);

            var user = await FetchGithubUserAsync(tokenResponse.AccessToken, ct);

            var email = user.Email ?? await FetchPrimaryEmailAsync(tokenResponse.AccessToken, ct);

            return new GithubUserDto(
                Id:        user.Id.ToString(),
                Login:     user.Login,
                Email:     email,
                AvatarUrl: user.AvatarUrl ?? "");
        }

        // -------------------------------------------------------------------------

        private async Task<GithubTokenResponse> RequestGithubTokenAsync(string code, string redirectUri, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                _config["GitHub:GithubTokenUrl"]!);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = JsonContent.Create(new
            {
                client_id     = _config["GitHub:ClientId"]!,
                client_secret = _config["GitHub:ClientSecret"]!,
                code          = code,
                redirect_uri  = redirectUri
            });

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GithubTokenResponse>(ct);

            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
                throw new InvalidOperationException("GitHub did not return an access token");

            return result;
        }

        private async Task<GithubUserProfile> FetchGithubUserAsync(string accessToken, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _config["GitHub:GithubApiUrl"]!);
            AttachGithubHeaders(request, accessToken);

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var user = await response.Content.ReadFromJsonAsync<GithubUserProfile>(ct);

            return user ?? throw new InvalidOperationException("Failed to deserialize GitHub user");
        }

        private async Task<string?> FetchPrimaryEmailAsync(string accessToken, CancellationToken ct)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _config["GitHub:GithubEmailsApiUrl"]!);
            AttachGithubHeaders(request, accessToken);

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return null;

            var emails = await response.Content.ReadFromJsonAsync<List<GithubEmail>>(ct);

            return emails?
                .FirstOrDefault(e => e.Primary && e.Verified)
                ?.Email;
        }

        private static void AttachGithubHeaders(HttpRequestMessage request, string accessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("InsightaLabs", "1.0"));
        }
    }
}