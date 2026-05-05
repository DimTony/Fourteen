using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Domains.Queries.GetDomainById
{
    public sealed record GetDomainByIdQuery(Guid Id) : IRequest<Result<DomainEntity>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.GetDomainById;
    }
}
