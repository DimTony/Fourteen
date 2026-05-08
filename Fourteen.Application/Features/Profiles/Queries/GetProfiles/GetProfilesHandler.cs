using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Common.Helpers;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace Fourteen.Application.Features.Profiles.Queries.GetProfiles
{
    public class GetProfilesHandler : IRequestHandler<GetProfilesQuery, Result<PagedResult<ProfileDto>>>
    {
        private readonly IProfileRepository _profileRepo;
        private readonly IQueryCache _cache;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<GetProfilesHandler> _logger;

        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);


        public GetProfilesHandler(IProfileRepository profileRepo, IQueryCache cache, IHttpContextAccessor http, ILogger<GetProfilesHandler> logger)
        {
            _profileRepo = profileRepo;
            _cache = cache;
            _http = http;
            _logger = logger;
        }

        public async Task<Result<PagedResult<ProfileDto>>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeyBuilder.ForGetProfiles(request);
 
            var cached = await _cache.Get<PagedResult<ProfileDto>>(cacheKey, cancellationToken);

            if (cached is not null)
                return Result.Success(cached);

            var (items, total) = await _profileRepo.GetPaged(request, cancellationToken);

            var http = _http.HttpContext!;
            var basePath = http.Request.Path;
            var queryString = http.Request.QueryString.ToString();

               var result = PagedResult<ProfileDto>.From(
                items.Select(ProfileDto.From).ToList(),
                total,
                request.Page,
                request.Limit,
                basePath,
                queryString);
 
            try
            {
                await _cache.Set(cacheKey, result, CacheTtl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Caching failed for key={Key}", cacheKey);
            }
 
            return Result.Success(result);
        }
    }
}