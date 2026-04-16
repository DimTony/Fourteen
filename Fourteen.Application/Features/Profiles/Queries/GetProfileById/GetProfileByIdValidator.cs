using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.GetProfileById
{
    public class GetProfileByIdQueryValidator : AbstractValidator<GetProfileByIdQuery>
    {
        public GetProfileByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Profile ID is required")
                .NotEqual(Guid.Empty).WithMessage("Profile ID cannot be empty");
        }
    }
}
