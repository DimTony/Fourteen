using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Commands.DeleteDomain
{

    public record DeleteDomainCommand(Guid Id) : IRequest<Result>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.DeleteDomain;
    }
}
