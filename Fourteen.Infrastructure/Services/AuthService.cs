using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Fourteen.Infrastructure.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IJwtService _jwtService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AuthServices> _logger;
        private readonly IConfiguration _config;

        private readonly IUnitOfWork _unitOfWork;


        public AuthServices(IHttpClientFactory httpClientFactory, IUserRepository userRepo, 
                         IRefreshTokenRepository refreshTokenRepo, IJwtService jwtService,
                         IMemoryCache memoryCache, ILogger<AuthServices> logger, IConfiguration config, IUnitOfWork unitOfWork)
        {
            _httpClientFactory = httpClientFactory;
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtService = jwtService;
            _memoryCache = memoryCache;
            _logger = logger;
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public string BuildGithubRedirectUrl(string state, string flow)
        {
            // var state = Guid.NewGuid().ToString("N");
            var (codeVerifier, codeChallenge) = PkceHelper.GeneratePkce();

            _memoryCache.Set($"oauth:{state}", new OAuthState(state, flow, codeVerifier),
                TimeSpan.FromMinutes(10));

            var redirectUri = _config["GitHub:RedirectUri"] ?? "http://localhost:5261/auth/github/callback";

            var query = QueryHelpers.AddQueryString(_config["GitHub:GithubAuthUrl"]!,
                new Dictionary<string, string?>
                {
                    ["client_id"]             = _config["GitHub:ClientId"]!,
                    ["redirect_uri"]          = redirectUri,
                    ["scope"]                 = "read:user user:email",
                    ["state"]                 = state,
                    ["code_challenge"]        = codeChallenge,
                    ["code_challenge_method"] = "S256"
                });

            return query;
        }


        public async Task<Result<CallbackResult>> HandleCallback(
            string code, string state, CancellationToken ct)
        {
            var oauthState = _memoryCache.Get<OAuthState>($"oauth:{state}");

            if (oauthState is null)
            {
                return Result.Failure<CallbackResult>("Invalid or expired state");
            }

            if (string.IsNullOrWhiteSpace(oauthState.CodeVerifier))
            {
                return Result.Failure<CallbackResult>("Missing code verifier");
            }

            var client = _httpClientFactory.CreateClient();


            var tokenRequest = new HttpRequestMessage(HttpMethod.Post,
                _config["GitHub:GithubTokenUrl"]!);

            tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            tokenRequest.Content = JsonContent.Create(new
            {
                client_id     = _config["GitHub:ClientId"]!,
                client_secret = _config["GitHub:ClientSecret"]!,
                code          = code,
                redirect_uri  = _config["GitHub:RedirectUri"]!,
                code_verifier = oauthState.CodeVerifier
            });

            
            var tokenResponse = await client.SendAsync(tokenRequest, ct);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorBody = await tokenResponse.Content.ReadAsStringAsync(ct);
                return Result.Failure<CallbackResult>($"GitHub token exchange failed: {errorBody}");
            }

            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<GithubTokenResponse>(ct);

            if (tokenResult is null || string.IsNullOrWhiteSpace(tokenResult.AccessToken))
            {
                return Result.Failure<CallbackResult>("GitHub did not return an access token");
            }

            var userRequest = new HttpRequestMessage(HttpMethod.Get,
                _config["GitHub:GithubApiUrl"]!);

            AttachGithubHeaders(userRequest, tokenResult.AccessToken);

            var userResponse = await client.SendAsync(userRequest, ct);

            userResponse.EnsureSuccessStatusCode();

            var userResult = await userResponse.Content.ReadFromJsonAsync<GithubUserProfile>(ct);

            if (userResult is null)
            {
                return Result.Failure<CallbackResult>("Failed to fetch GitHub user profile");
            }

            var emailRequest = new HttpRequestMessage(HttpMethod.Get,
                _config["GitHub:GithubEmailsApiUrl"]!);

            AttachGithubHeaders(emailRequest, tokenResult.AccessToken);

            var emailsResponse = await client.SendAsync(emailRequest, ct);

            if (!emailsResponse.IsSuccessStatusCode)
            {
                return Result.Failure<CallbackResult>("Failed to fetch GitHub user emails");
            }

            var githubEmails = await emailsResponse.Content
                .ReadFromJsonAsync<List<GithubEmail>>(ct);

            var userEmail = userResult.Email
                ?? githubEmails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email;

            var user = await _userRepo.FindByGithubId(userResult.Id.ToString(), ct);

            if (user == null)
            {
                user = User.Create(
                    userResult.Id.ToString(),
                    userResult.Login,
                    userEmail ?? string.Empty,
                    userResult.AvatarUrl ?? "");

                await _userRepo.AddAsync(user, ct);
            }

            if (!user.IsActive)
            {
                return Result.Failure<CallbackResult>("Account is deactivated");
            }

            user.RecordLogin();

            await _unitOfWork.SaveChangesAsync(ct);

            var tokenPair = await IssueTokenPairAsync(user, ct);

            _memoryCache.Remove($"oauth:{state}");

            return Result.Success(new CallbackResult(tokenPair, oauthState));
        }
  
        public async Task<Result<TokenPair>> Refresh(string rawRefreshToken, CancellationToken ct)
        {
            var decodedToken = Uri.UnescapeDataString(rawRefreshToken);
            
            var stored = await _refreshTokenRepo.FindValidByUser(decodedToken, ct);

            if (stored is null)
                return Result.Failure<TokenPair>("Invalid or expired refresh token");

            stored.Revoke();
            await _unitOfWork.SaveChangesAsync(ct);

            var user = await _userRepo.GetByIdAsync(new UserId(stored.UserId), ct);

            if (user is null)
                return Result.Failure<TokenPair>("Invalid or expired refresh token");

            if (!user.IsActive)
                return Result.Failure<TokenPair>("Account is deactivated");

            return await IssueTokenPairAsync(user, ct);
        }

        public async Task RevokeRefreshToken(string rawRefreshToken, CancellationToken ct)
        {
            // Decode the refresh token in case it's URL-encoded
            var decodedToken = Uri.UnescapeDataString(rawRefreshToken);
            
            var stored = await _refreshTokenRepo.FindValidByUser(decodedToken, ct);

            if (stored != null)
            {
                stored.Revoke();
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        private static void AttachGithubHeaders(HttpRequestMessage request, string accessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("InsightaLabs", "1.0"));
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = System.Convert.ToBase64String(input);
            return base64.Replace("+", "-")
                         .Replace("/", "_")
                         .TrimEnd('=');
        }

        private async Task<TokenPair> IssueTokenPairAsync(User user, CancellationToken ct)
        {
            var expiryInMinutes = _config["Jwt:ExpirationMinutes"];

            if ( !int.TryParse(expiryInMinutes, out var expiry))
                throw new Exception("JWT ExpirationMinutes is missing or invalid in configuration");

            var accessToken = _jwtService.Generate(user, TimeSpan.FromMinutes(expiry));

            var rawRefresh = GenerateSecureToken();

            await _refreshTokenRepo.AddAsync(RefreshToken.Create(user.Id.Value, rawRefresh), ct);

            await _unitOfWork.SaveChangesAsync(ct);

            return new TokenPair(
                AccessToken:  accessToken,
                RefreshToken: rawRefresh,
                Username:     user.Username,
                AvatarUrl:    user.AvatarUrl,
                Role:         user.Role.ToString());
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

       
    }
}