using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Profiles.Commands.DeleteProfile
{

    public record DeleteProfileCommand(Guid Id) : IRequest<Result>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.DeleteProfile;
    }
}
