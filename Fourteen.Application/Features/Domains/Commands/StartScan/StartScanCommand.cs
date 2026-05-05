using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Commands.StartScan
{

    public record StartScanCommand(DomainId DomainId, ScanType Type) : IRequest<Result<ScanDto>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.StartScan;
    }
}
