using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Domains.Commands.DeleteDomain
{
    public class DeleteDomainHandler : IRequestHandler<DeleteDomainCommand, Result>
    {
        private readonly IDomainRepository _domainRepo;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDomainHandler(IDomainRepository domainRepo, IUnitOfWork unitOfWork)
        {
            _domainRepo = domainRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteDomainCommand request, CancellationToken cancellationToken)
        {
            var domain = await _domainRepo.GetByIdAsync(new DomainId(request.Id), cancellationToken);
            if (domain == null)
            {
                return Result.Failure("Domain not found");
            }

            await _domainRepo.DeleteAsync(domain, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
