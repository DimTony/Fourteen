using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Commands.AddDomain
{
    public record AddDomainCommand(string Name) : IRequest<Result<DomainDto>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.AddDomain;
    }
}