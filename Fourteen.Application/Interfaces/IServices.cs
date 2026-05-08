using Fourteen.Application.Common.DTOs;
using Fourteen.Domain.Aggregates.Users;
using Fourteen.Domain.Common;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Interfaces
{
    public interface IBulkProfileImporter
    {

        Task<BulkImportResult> Import(
            Stream csvStream,
            CancellationToken ct = default);
    }
    public interface IQueryCache
    {
        Task<T?> Get<T>(string key, CancellationToken ct = default) where T : class;
        Task Set<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class;
        Task Remove(string key, CancellationToken ct = default);
        Task RemoveByPrefix(string prefix, CancellationToken ct = default);
    }
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
        TokenPair IssueTokenPair(User user, CancellationToken ct);
    }

    public interface IUserService
    {
        Guid? UserId { get; }
        string? IpAddress { get; }
        string? Name { get; }
        string? Email { get; }
        Guid? ProviderId { get; }
        string? Role { get; }
    }

    public interface IDnsService
    {
        Task<bool> CheckTxtRecord(string host, string expectedValue, CancellationToken ct);
    }

    public interface IRedisService
    {
        Task PublishJob<T>(string queue, T message, CancellationToken ct = default);
    }
}
