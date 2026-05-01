using Fourteen.Application.Common.DTOs;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Interfaces
{
    public interface IServices
    {
        Task<ExternalAPIDto> GetByName(string name, CancellationToken ct);
        Task<TransformedExternalResponse> FetchAll(string name, CancellationToken ct);
      
    }
    public interface IAuthServices
    {
        string BuildGithubRedirectUrl(string? codeChallenge, string state, string? callbackOverride);
        Task<Result<CallbackResult>> HandleCallback(string code, string state, string? codeVerifier, CancellationToken ct);
        Task<Result<TokenPair>> Refresh(string rawRefreshToken, CancellationToken ct);
        Task RevokeRefreshToken(string rawRefreshToken, CancellationToken ct);


    }
    public interface IGithubClient
    {
        string BuildAuthUrl(string state, string redirectUri);
        Task<GithubUserDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken ct = default);
    }
    public interface IJwtService
    {
        string Generate(User user, TimeSpan lifetime);
    }
}
