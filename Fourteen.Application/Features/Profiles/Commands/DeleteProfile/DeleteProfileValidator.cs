using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Commands.DeleteProfile
{
    public class DeleteProfileValidator : AbstractValidator<DeleteProfileCommand>
    {
        public DeleteProfileValidator()
        {
            RuleFor(x => x.Id)
               .NotEmpty().WithMessage("Profile ID is required")
               .NotEqual(Guid.Empty).WithMessage("Profile ID cannot be empty");
        }
    }
}
