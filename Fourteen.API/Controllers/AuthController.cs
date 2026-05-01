using System.Security.Claims;
using System.Security.Cryptography;
using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Profiles.Commands.CreateProfile;
using Fourteen.Application.Features.Profiles.Commands.DeleteProfile;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using Fourteen.Application.Features.Profiles.Queries.SearchProfiles;
using Fourteen.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace Fourteen.API.Controllers
{
    [ApiController]
    [EnableRateLimiting("auth")]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthServices authService, IConfiguration config, ILogger<AuthController> logger)
        {
            _authService = authService;
            _config = config;
            _logger = logger;
        }

        [HttpGet("github")]
        public IActionResult RedirectToGithub(
            [FromQuery] string? code_challenge,
            [FromQuery] string? state,
            [FromQuery] string? cli_callback)
        {
            var resolvedState = state ?? Guid.NewGuid().ToString("N");

            var redirectUrl = _authService.BuildGithubRedirectUrl(
                code_challenge, resolvedState, cli_callback);

            return Redirect(redirectUrl);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            [FromBody] LogoutRequest? body,
            CancellationToken ct)
        {
            var rawToken = body?.RefreshToken
                        ?? Request.Cookies["refresh_token"];

            if (!string.IsNullOrWhiteSpace(rawToken))
                await _authService.RevokeRefreshToken(rawToken, ct);

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            Response.Cookies.Delete("csrf_token");

            return Ok(new { status = "success", message = "Logged out successfully" });
        }
    
        [HttpGet("me")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);

            return Ok(new
            {
                status = "success",
                data   = new
                {
                    id         = claims.GetValueOrDefault(ClaimTypes.NameIdentifier),
                    username   = claims.GetValueOrDefault(ClaimTypes.Name),
                    email      = claims.GetValueOrDefault(ClaimTypes.Email),
                    role       = claims.GetValueOrDefault(ClaimTypes.Role),
                    avatar_url = claims.GetValueOrDefault("avatar_url")
                }
            });
        }

        [HttpGet("github/callback")]
        public async Task<IActionResult> GithubCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string? code_verifier,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(new { status = "error", message = "Missing authorization code" });

            if (string.IsNullOrWhiteSpace(state))
                return BadRequest(new { status = "error", message = "Missing state parameter" });

            var result = await _authService.HandleCallback(code, state, code_verifier, ct);

            if (result.IsFailure)
                return Unauthorized(new { status = "error", message = result.Error });

            var (tokenPair, cliCallback) = result.Value;

            // CLI flow — return tokens in the JSON body so the CLI can persist them.
            if (!string.IsNullOrWhiteSpace(cliCallback))
            {
                return Ok(new
                {
                    status        = "success",
                    access_token  = tokenPair.AccessToken,
                    refresh_token = tokenPair.RefreshToken,
                    username      = tokenPair.Username,
                    avatar_url    = tokenPair.AvatarUrl
                });
            }

            // Web flow — deliver tokens via HttpOnly cookies and expose a CSRF token.
            AppendAuthCookies(tokenPair);

            return Ok(new
            {
                status = "success",
                message = "Logged in successfully",
                data = new
                {
                    username   = tokenPair.Username,
                    avatar_url = tokenPair.AvatarUrl,
                    role       = tokenPair.Role
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshRequest? body,
            CancellationToken ct)
        {
            var rawToken = body?.RefreshToken
                        ?? Request.Cookies["refresh_token"];

            if (string.IsNullOrWhiteSpace(rawToken))
                return BadRequest(new { status = "error", message = "Missing refresh token" });

            var result = await _authService.Refresh(rawToken, ct);

            if (result.IsFailure)
                return Unauthorized(new { status = "error", message = result.Error });

            var tokenPair = result.Value;

            // If the original request used cookies, refresh via cookies too.
            if (Request.Cookies.ContainsKey("refresh_token"))
            {
                AppendAuthCookies(tokenPair);
                return Ok(new { status = "success" });
            }

            // Otherwise (CLI / API clients) return tokens in the body.
            return Ok(new
            {
                status        = "success",
                access_token  = tokenPair.AccessToken,
                refresh_token = tokenPair.RefreshToken
            });
        }

        private void AppendAuthCookies(TokenPair tokenPair)
        {
            // Access token: 3 minutes (matches JWT expiry)
            Response.Cookies.Append("access_token", tokenPair.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure   = true,
                Path     = "/",
                Expires  = DateTimeOffset.UtcNow.AddMinutes(3)
            });

            // Refresh token: 5 minutes (matches RefreshToken.Create)
            Response.Cookies.Append("refresh_token", tokenPair.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure   = true,
                Path     = "/",
                Expires  = DateTimeOffset.UtcNow.AddMinutes(5)
            });

            var csrfToken = GenerateCsrfToken();
            Response.Cookies.Append("csrf_token", csrfToken, new CookieOptions
            {
                HttpOnly = false,          // Must be readable by JS
                SameSite = SameSiteMode.None,
                Secure   = true,
                Path     = "/",
                Expires  = DateTimeOffset.UtcNow.AddMinutes(3)
            });
        }
        private static string GenerateCsrfToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}