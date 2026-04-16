using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.GetProfileById
{
    public class GetProfileByIdHandler : IRequestHandler<GetProfileByIdQuery, Result<Profile>>
    {
        private readonly IProfileRepository _profileRepo;


        public GetProfileByIdHandler(IProfileRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public async Task<Result<Profile>> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepo.GetByIdAsync(new ProfileId(request.Id), cancellationToken);
            if (profile == null)
            {
                return Result.Failure<Profile>("Profile not found");
            }
            return Result.Success(profile);
        }
    }

}
