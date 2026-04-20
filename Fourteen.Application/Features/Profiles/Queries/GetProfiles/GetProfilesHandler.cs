using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.GetProfiles
{
    public class GetProfilesHandler : IRequestHandler<GetProfilesQuery, Result<PagedResult<ProfileDto>>>
    {
        private readonly IProfileRepository _profileRepo;

        public GetProfilesHandler(IProfileRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public async Task<Result<PagedResult<ProfileDto>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _profileRepo.GetPagedAsync(request, cancellationToken);
                return PagedResult<ProfileDto>.From(items.Select(ProfileDto.From), total, request.Page, request.Limit);
        }
    }
}