using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Common.Helpers;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _http;

        public SearchProfilesHandler(IProfileRepository profileRepo, IHttpContextAccessor http)
        {
            _profileRepo = profileRepo;
            _http = http;
        }

        public async Task<Result<PagedResult<ProfileDto>>> Handle(SearchProfilesQuery request, CancellationToken cancellationToken)
        {
            var filter = NaturalLanguageQueryParser.Parse(request.RawQuery);

            if (filter.IsEmpty)
                throw new UnparsableQueryException(request.RawQuery);

            var (items, total) = await _profileRepo.NaturalLanguageSearch(filter, request.Page, request.Limit, cancellationToken);

            var http = _http.HttpContext!;
            var basePath = http.Request.Path;
            var queryString = http.Request.QueryString.ToString();

            return PagedResult<ProfileDto>.From(items.Select(ProfileDto.From).ToList(), total, request.Page, request.Limit, basePath!, queryString);
        }
    }
}