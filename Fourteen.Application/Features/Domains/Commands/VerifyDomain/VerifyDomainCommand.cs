using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Commands.VerifyDomain
{

    public record VerifyDomainCommand(Guid Id) : IRequest<Result<DomainDto>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.VerifyDomain;
    }
}
