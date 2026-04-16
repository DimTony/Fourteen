using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Profiles;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.GetProfileById
{
    public sealed record GetProfileByIdQuery(Guid Id) : IRequest<Result<Profile>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.GetProfileById;
    }
}
