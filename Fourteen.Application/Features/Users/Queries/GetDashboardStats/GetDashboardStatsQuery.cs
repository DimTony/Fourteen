using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Users.Queries.GetDashboardStats
{
    public sealed record GetDashboardStatsQuery : IRequest<Result<MetricDto>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.GetDashboardStats;
    }
}