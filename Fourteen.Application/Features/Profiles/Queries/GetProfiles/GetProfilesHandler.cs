using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Profiles.Queries.GetProfiles
{
    public class GetProfilesHandler : IRequestHandler<GetProfilesQuery, Result<PagedResult<ProfileDto>>>
    {
        private readonly IProfileRepository _profileRepo;
        private readonly IHttpContextAccessor _http;


        public GetProfilesHandler(IProfileRepository profileRepo, IHttpContextAccessor http)
        {
            _profileRepo = profileRepo;
            _http = http;
        }

        public async Task<Result<PagedResult<ProfileDto>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _profileRepo.GetPagedAsync(request, cancellationToken);

            var http = _http.HttpContext!;
            var basePath = http.Request.Path;
            var queryString = http.Request.QueryString.ToString();

            return PagedResult<ProfileDto>.From(items.Select(ProfileDto.From).ToList(), total, request.Page, request.Limit, basePath!, queryString);
        }
    }
}