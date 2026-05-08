using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Domains;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Commands.StartScan
{
    public class StartScanHandler : IRequestHandler<StartScanCommand, Result<ScanDto>>
    {
        private readonly IScanRepository _scanRepo;
        private readonly IDomainRepository _domainRepo;
        private readonly IUserService _currentUser;
        private readonly IRedisService _redisService;

        private readonly IUnitOfWork _unitOfWork;

        public StartScanHandler(IScanRepository scanRepo, IDomainRepository domainRepo, IUserService currentUser, IRedisService redisService, IUnitOfWork unitOfWork)
        {
            _scanRepo = scanRepo;
            _domainRepo = domainRepo;
            _currentUser = currentUser;
            _redisService = redisService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ScanDto>> Handle(StartScanCommand request, CancellationToken cancellationToken)
        {
            var domain = await _domainRepo.GetByIdAsync(request.DomainId, cancellationToken);

            var userId = _currentUser.UserId;

            if (userId is null)
                return Result.Failure<ScanDto>("User is not authenticated");

            if (domain == null || domain.OwnerId != new UserId(userId.Value))
                return Result.Failure<ScanDto>("Domain not found");

            if (domain.VerificationStatus != VerificationStatus.Verified)
                return Result.Failure<ScanDto>("Domain must be verified before scanning");

            var activeScans = await _scanRepo.GetActiveByDomain(request.DomainId, cancellationToken);

            if (activeScans.Any())
                return Result.Failure<ScanDto>("A scan is already running for this domain");

            var scan = Scan.Create(request.DomainId, new UserId(userId.Value), request.Type);

            await _scanRepo.AddAsync(scan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Publish to Redis — Java worker picks this up
            await _redisService.PublishJob("vulnscan:queue:scan_jobs", new ScanJob(
                ScanId: scan.Id.Value.ToString(),
                DomainId: domain.Id.Value.ToString(),
                DomainName: domain.Name,
                ScanType: request.Type.ToString(),
                EnqueuedAt: DateTime.UtcNow
            ), cancellationToken);

            return Result.Success(ScanDto.From(scan));
        }
    }
}
