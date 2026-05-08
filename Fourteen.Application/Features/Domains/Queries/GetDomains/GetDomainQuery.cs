using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Queries.GetDomains
{
    public sealed record GetDomainsQuery( 
        string? Status,
        Guid? UserId,
        string SortBy = "created_at",
        string Order = "asc",
        int Page = 1,
        int Limit = 10) : IRequest<Result<PagedResult<DomainDto>>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.GetDomains;
    }
}