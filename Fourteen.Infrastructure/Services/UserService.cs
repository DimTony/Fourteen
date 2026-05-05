using Fourteen.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fourteen.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        
        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {

                var value = this.httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(value, out var id) ? id : null;
            }
        }
        
        public string? IpAddress =>
            this.httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        /// <inheritdoc/>
        public Guid? ProviderId
        {
            get
            {
                var value = this.httpContextAccessor.HttpContext?
                    .User.FindFirst("provider_id")?.Value;
                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        public string? Role => this.httpContextAccessor.HttpContext?.User
                   .FindFirst(ClaimTypes.Role)?.Value;

        public string? Name => this.httpContextAccessor.HttpContext?.User
                   .FindFirst(ClaimTypes.Name)?.Value;
        public string? Email =>
            this.httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
    }
}