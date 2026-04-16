using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Profiles.Commands.CreateProfile
{
    public class CreateProfileHandler : IRequestHandler<CreateProfileCommand, Result<CreateProfileResult>>
    {
        private readonly IServices _external;
        private readonly IProfileRepository _profileRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProfileHandler(IServices external, IProfileRepository profileRepo, IUnitOfWork unitOfWork)
        {
            _external = external;
            _profileRepo = profileRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreateProfileResult>> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
        {
            var name = request.Name.ToLower();

            var existingProfile = await _profileRepo.GetByName(name, cancellationToken);

            if (existingProfile != null)
            {
                return Result.Success(new CreateProfileResult
                {
                    Profile = existingProfile,
                    IsNewProfile = false
                });
            }

            var data = await _external.FetchAll(name, cancellationToken);

            var profile = Profile.Create(
                data.Name ?? name,
                data.Gender!,
                data.GenderProbability ?? 0,
                data.Count,
                data.Age!.Value,
                data.Country_Id!,
                data.CountryProbability ?? 0);

            await _profileRepo.AddAsync(profile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new CreateProfileResult
            {
                Profile = profile,
                IsNewProfile = true
            });
        }
    }
}
