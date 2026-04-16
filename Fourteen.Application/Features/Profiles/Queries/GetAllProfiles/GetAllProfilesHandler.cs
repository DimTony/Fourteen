using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.GetAllProfiles
{
    public class GetAllProfilesHandler : IRequestHandler<GetAllProfilesQuery, Result<GetAllProfilesResult>>
    {
        private readonly IProfileRepository _profileRepo;


        public GetAllProfilesHandler(IProfileRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public async Task<Result<GetAllProfilesResult>> Handle(GetAllProfilesQuery request, CancellationToken cancellationToken)
        {
            var profiles = await _profileRepo.GetAll(request.Gender, request.CountryId, request.AgeGroup, cancellationToken);

            var profileDtos = profiles?.Select(p => new ProfileDto
            {
                Id = p.Id.Value,
                Name = p.Name,
                Gender = p.Gender,
                Age = p.Age,
                AgeGroup = p.AgeGroup,
                CountryId = p.CountryId
            }).ToList() ?? [];

            return Result.Success(new GetAllProfilesResult
            {
                Profiles = profileDtos,
                Count = profileDtos.Count
            });
        }
    }

}
