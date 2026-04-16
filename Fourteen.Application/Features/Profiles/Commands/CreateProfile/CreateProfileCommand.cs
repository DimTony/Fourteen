using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Profiles.Commands.CreateProfile
{
    public record CreateProfileCommand(string Name) : IRequest<Result<CreateProfileResult>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.CreateProfile;
    }

    public class CreateProfileResult
    {
        public required Profile Profile { get; init; }
        public required bool IsNewProfile { get; init; }
    }
}
