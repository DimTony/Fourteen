using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Profiles.Queries.SearchProfiles
{
    public sealed record SearchProfilesQuery( 
        string RawQuery,
        string SortBy = "created_at",
        string Order = "asc",
        int Page = 1,
        int Limit = 10) : IRequest<Result<PagedResult<ProfileDto>>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.SearchProfiles;
    }

    
}