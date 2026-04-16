using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Commands.CreateProfile
{
    public class CreateProfileValidator : AbstractValidator<CreateProfileCommand>
    {
        public CreateProfileValidator()
        {


            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .NotNull().WithMessage("Name cannot be null")
                //.MinimumLength(2).WithMessage("Name must be at least 2 characters")
                //.MaximumLength(100).WithMessage("Name must not exceed 100 characters")
                .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes");

        }
    }
}
