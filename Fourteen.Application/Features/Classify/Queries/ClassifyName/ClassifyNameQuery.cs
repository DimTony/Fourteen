using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Classify.Queries.ClassifyName
{
    public sealed record ClassifyByNameQuery(string Name) : IRequest<Result<ClassifyNameDto>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.ClassifyName;
    }
}
