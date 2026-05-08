using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Common.Helpers;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace Fourteen.Application.Features.Profiles.Queries.SearchProfiles
{
    public class SearchProfilesHandler : IRequestHandler<SearchProfilesQuery, Result<PagedResult<ProfileDto>>>
    {
        private readonly IProfileRepository _profileRepo;
        private readonly IQueryCache _cache;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<SearchProfilesHandler> _logger;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(90);

        public SearchProfilesHandler(IProfileRepository profileRepo, IQueryCache cache, IHttpContextAccessor http, ILogger<SearchProfilesHandler> logger)
        {
            _profileRepo = profileRepo;
            _cache = cache;
            _http = http;
            _logger = logger;
        }

        public async Task<Result<PagedResult<ProfileDto>>> Handle(SearchProfilesQuery request, CancellationToken cancellationToken)
        {
            var filter = NaturalLanguageQueryParser.Parse(request.RawQuery);

            if (filter.IsEmpty)
                throw new UnparsableQueryException(request.RawQuery);

            _logger.LogInformation("Filtering search with filters={@filter}", filter);
            

            var cacheKey = CacheKeyBuilder.ForSearch(filter, request.Page, request.Limit);
 
            var cached = await _cache.Get<PagedResult<ProfileDto>>(cacheKey, cancellationToken);

            if (cached is not null)
            {
                _logger.LogInformation("Caching returned from key={Key}", cacheKey);
                return Result.Success(cached);
            }

            var (items, total) = await _profileRepo.NaturalLanguageSearch(filter, request.Page, request.Limit, cancellationToken);

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
                _logger.LogInformation("Caching set with key={Key}", cacheKey);

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Caching failed for key={Key}", cacheKey);

            }
 
            return Result.Success(result);
        }
    }
}