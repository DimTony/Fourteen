using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.SearchProfiles
{
    public class SearchProfilesHandler : IRequestHandler<SearchProfilesQuery, Result<PagedResult<ProfileDto>>>
    {
        private readonly IProfileRepository _profileRepo;

        public SearchProfilesHandler(IProfileRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public async Task<Result<PagedResult<ProfileDto>>> Handle(SearchProfilesQuery request, CancellationToken cancellationToken)
        {
            var filter = NaturalLanguageQueryParser.Parse(request.RawQuery);

            if (filter.IsEmpty)
                throw new UnparsableQueryException(request.RawQuery);

            var (items, total) = await _profileRepo.NaturalLanguageSearch(filter, request.Page, request.Limit, cancellationToken);

            return PagedResult<ProfileDto>.From(items.Select(ProfileDto.From), total, request.Page, request.Limit);
        }
    }
}