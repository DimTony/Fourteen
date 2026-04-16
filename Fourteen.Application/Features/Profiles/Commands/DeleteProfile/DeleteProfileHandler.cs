using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Commands.DeleteProfile
{
    public class DeleteProfileHandler : IRequestHandler<DeleteProfileCommand, Result>
    {
        private readonly IProfileRepository _profileRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProfileHandler(IProfileRepository profileRepo, IUnitOfWork unitOfWork)
        {
            _profileRepo = profileRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
        {
            var profile = await _profileRepo.GetByIdAsync(new ProfileId(request.Id), cancellationToken);
            if (profile == null)
            {
                return Result.Failure("Profile not found");
            }

            await _profileRepo.DeleteAsync(profile, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
