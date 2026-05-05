using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Fourteen.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string Generate(User user, TimeSpan expiry)
        {
            var secret = _config["Jwt:SecretKey"];

            if (string.IsNullOrWhiteSpace(secret))
                throw new Exception("JWT SecretKey is missing in configuration");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim(ClaimTypes.Email,          user.Email ?? ""),
                new Claim(ClaimTypes.Role,           user.Role.ToString()),
                new Claim("avatar_url",              user.AvatarUrl ?? ""),
                new Claim("provider_id",               user.ProviderId)
            };

            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.Add(expiry),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenPair IssueTokenPair(User user, CancellationToken ct)
        {
            var expiryInMinutes = _config["Jwt:ExpirationMinutes"];

            if ( !int.TryParse(expiryInMinutes, out var expiry))
                throw new Exception("JWT ExpirationMinutes is missing or invalid in configuration");

            var accessToken = Generate(user, TimeSpan.FromMinutes(expiry));

            var rawRefresh = GenerateSecureToken();

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