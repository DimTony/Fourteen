using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Fourteen.Infrastructure.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly IGithubClient _github;
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IJwtService _jwtService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AuthServices> _logger;
        private readonly IConfiguration _config;

        private readonly IUnitOfWork _unitOfWork;


        public AuthServices(IGithubClient github, IUserRepository userRepo, 
                         IRefreshTokenRepository refreshTokenRepo, IJwtService jwtService,
                         IMemoryCache memoryCache, ILogger<AuthServices> logger, IConfiguration config, IUnitOfWork unitOfWork)
        {
            _github = github;
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _jwtService = jwtService;
            _memoryCache = memoryCache;
            _logger = logger;
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public string BuildGithubRedirectUrl(string? codeChallenge, string state, string? callbackOverride)
        {
            var redirectUri = _config["GitHub:RedirectUri"];

            _memoryCache.Set($"oauth:{state}", new OAuthState(codeChallenge, callbackOverride), 
                    TimeSpan.FromMinutes(10));

            var redirect = callbackOverride ?? redirectUri;

            return _github.BuildAuthUrl(state, redirect);
        }

        public async Task<Result<CallbackResult>> HandleCallback(string code, string state, string? codeVerifier, CancellationToken ct)
        {
            var oauthState = _memoryCache.Get<OAuthState>($"oauth:{state}");
            if (oauthState is null)
                return Result.Failure<CallbackResult>("Invalid or expired state");

            if (oauthState.CodeChallenge is not null)
            {
                if (string.IsNullOrWhiteSpace(codeVerifier))
                    return Result.Failure<CallbackResult>("Missing code verifier");

                var expectedChallenge = Base64UrlEncode(
                    SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

                if (expectedChallenge != oauthState.CodeChallenge)
                    return Result.Failure<CallbackResult>("PKCE validation failed");
            }

            var redirectUri = oauthState.CliCallback ?? string.Empty;

            var githubUser = await _github.ExchangeCodeAsync(code, redirectUri, ct);

            var user = await _userRepo.FindByGithubId(githubUser.Id);
            
            if (user == null)
            {
                user = User.Create(
                    githubUser.Id,
                    githubUser.Login,
                    githubUser.Email ?? string.Empty,
                    githubUser.AvatarUrl);

                await _userRepo.AddAsync(user);
            }

            if (!user.IsActive)
                return Result.Failure<CallbackResult>("Account is deactivated");

            user.RecordLogin();

            await _unitOfWork.SaveChangesAsync(ct);

            var tokenPair = await IssueTokenPairAsync(user, ct);

            _memoryCache.Remove($"oauth:{state}");

            return Result.Success(new CallbackResult(tokenPair, oauthState.CliCallback));
        }
  
        public async Task<Result<TokenPair>> Refresh(string rawRefreshToken, CancellationToken ct)
        {
            var stored = await _refreshTokenRepo.FindValidByUser(rawRefreshToken, ct)
                ?? throw new UnauthorizedException("Invalid or expired refresh token");

            stored.Revoke();

            await _unitOfWork.SaveChangesAsync(ct);

            var user = await _userRepo.GetByIdAsync(new UserId(stored.UserId), ct)!;

            if (user == null)
            {
                return Result.Failure<TokenPair>("Invalid or expired refresh token");
            }
            
            if (!user.IsActive) return Result.Failure<TokenPair>("Account is deactivated");

            return await IssueTokenPairAsync(user, ct);
        }

        public async Task RevokeRefreshToken(string rawRefreshToken, CancellationToken ct)
        {
            var stored = await _refreshTokenRepo.FindValidByUser(rawRefreshToken, ct);

            if (stored != null)
            {
                stored.Revoke();
                await _unitOfWork.SaveChangesAsync(ct);
            }
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