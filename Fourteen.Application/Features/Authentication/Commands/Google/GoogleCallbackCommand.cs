using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Authentication.Commands.GoogleCallback
{
    public record GoogleCallbackCommand(string Code, string State) : IRequest<Result<TokenPair>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.GoogleAuth;
    }
}