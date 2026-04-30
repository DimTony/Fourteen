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
        string BuildGithubRedirectUrl(string state,string flow = "web");
        Task<Result<CallbackResult>> HandleCallback(string code, string state, CancellationToken ct);
        Task<Result<TokenPair>> Refresh(string rawRefreshToken, CancellationToken ct);
        Task RevokeRefreshToken(string rawRefreshToken, CancellationToken ct);


    }
    public interface IJwtService
    {
        string Generate(User user, TimeSpan lifetime);
    }
}
