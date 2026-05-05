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

namespace Fourteen.Application.Features.Domains.Queries.GetDomains
{
    public class GetDomainsHandler : IRequestHandler<GetDomainsQuery, Result<PagedResult<DomainDto>>>
    {
        private readonly IDomainRepository _domainRepo;
        private readonly IHttpContextAccessor _http;


        public GetDomainsHandler(IDomainRepository domainRepo, IHttpContextAccessor http)
        {
            _domainRepo = domainRepo;
            _http = http;
        }

        public async Task<Result<PagedResult<DomainDto>>> Handle(GetDomainsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _domainRepo.GetPaged(request, cancellationToken);
            
            var http = _http.HttpContext!;
            var basePath = http.Request.Path;
            var queryString = http.Request.QueryString.ToString();

            return PagedResult<DomainDto>.From(items.Select(DomainDto.From).ToList(), total, request.Page, request.Limit, basePath!, queryString);
        }
    }
}