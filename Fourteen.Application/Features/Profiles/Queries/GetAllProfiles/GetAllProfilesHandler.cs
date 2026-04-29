using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

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
                GenderProbability = p.GenderProbability,
                Age = p.Age,
                AgeGroup = p.AgeGroup,
                CountryId = p.CountryId,
                CountryName = p.CountryName,
                CountryProbability = p.CountryProbability,
                CreatedAt = p.CreatedAt.ToString("o")
            }).ToList() ?? [];

            return Result.Success(new GetAllProfilesResult
            {
                Profiles = profileDtos,
                Count = profileDtos.Count
            });
        }
    }

}
