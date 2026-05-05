using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Commands.ProcessScanResult
{

    public record ProcessScanResultCommand(ScanResult Result) : IRequest<Result>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.ProcessScanResult;
    }
}
