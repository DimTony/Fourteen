using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Features.Domains.Commands.VerifyDomain
{
    public class VerifyDomainHandler : IRequestHandler<VerifyDomainCommand, Result<DomainDto>>
    {
        private readonly IDomainRepository _domainRepo;
        private readonly IUserService _currentUser;
        private readonly IDnsService _dnsService;

        private readonly IUnitOfWork _unitOfWork;

        public VerifyDomainHandler(IDomainRepository domainRepo, IUserService currentUser, IDnsService dnsService, IUnitOfWork unitOfWork)
        {
            _domainRepo = domainRepo;
            _currentUser = currentUser;
            _dnsService = dnsService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DomainDto>> Handle(VerifyDomainCommand request, CancellationToken cancellationToken)
        {
            var domain = await _domainRepo.GetByIdAsync(new DomainId(request.Id), cancellationToken);

            var userId = _currentUser.UserId;

            if (userId is null)
                return Result.Failure<DomainDto>("User is not authenticated");

            if (domain == null || domain.OwnerId != new UserId(userId.Value))
            {
                return Result.Failure<DomainDto>("Domain not found");
            }

            if (domain.VerificationStatus == VerificationStatus.Verified)
                return Result.Success(DomainDto.From(domain));

            var txtRecordHost = $"_vulnscan-challenge.{domain.Name}";
            var found = await _dnsService.CheckTxtRecord(txtRecordHost, domain.VerificationToken, cancellationToken);

            if (!found)
                return Result.Failure<DomainDto>(
                    $"TXT record not found. Add: {txtRecordHost} → {domain.VerificationToken}");

            domain.MarkVerified();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(DomainDto.From(domain));
        }
    }
}