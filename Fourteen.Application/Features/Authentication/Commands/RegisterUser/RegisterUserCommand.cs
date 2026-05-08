using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Authentication.Commands.RegisterUser
{
    public record RegisterUserCommand(string Email, string Password, string Username, string? AvatarUrl, UserRole Role = UserRole.analyst) : IRequest<Result<TokenPair>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.RegisterUser;
    }
}