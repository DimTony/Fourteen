using Fourteen.Application.Interfaces;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Queries.GetDomainById
{
    public class GetDomainByIdHandler : IRequestHandler<GetDomainByIdQuery, Result<DomainEntity>>
    {
        private readonly IDomainRepository _domainRepo;


        public GetDomainByIdHandler(IDomainRepository domainRepo)
        {
            _domainRepo = domainRepo;
        }

        public async Task<Result<DomainEntity>> Handle(GetDomainByIdQuery request, CancellationToken cancellationToken)
        {
            var domain = await _domainRepo.GetByIdAsync(new DomainId(request.Id), cancellationToken);
            if (domain == null)
            {
                return Result.Failure<DomainEntity>("Domain not found");
            }
            return Result.Success(domain);
        }
    }

}
