using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Users.Queries.GetDashboardStats
{
    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Result<MetricDto>>
    {
        private readonly IUserRepository _userRepo;

        public GetDashboardStatsHandler(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<Result<MetricDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var stats = await _userRepo.GetUserGrowthAsync(cancellationToken);

            return Result.Success(stats);
        }
    }
}