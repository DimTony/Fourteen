using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.GetAllProfiles
{
    public sealed record GetAllProfilesQuery(string? Gender, string? CountryId, string? AgeGroup) : IRequest<Result<GetAllProfilesResult>>, IRequiresFeature
    {
        public string FeatureFlag => FeatureFlags.GetAllProfiles;
    }
}
