using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Configurations;
using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Domains;
using Fourteen.Domain.Common;
using MediatR;

namespace Fourteen.Application.Features.Domains.Commands.ProcessScanResult
{
    public class ProcessScanResultHandler : IRequestHandler<ProcessScanResultCommand, Result>
    {
        private readonly IScanRepository _scanRepo;
        private readonly IDomainRepository _domainRepo;
        private readonly IFindingRepository _findingRepo;
        private readonly IUserService _currentUser;
        private readonly IRedisService _redisService;

        private readonly IUnitOfWork _unitOfWork;

        public ProcessScanResultHandler(IScanRepository scanRepo, IDomainRepository domainRepo, IFindingRepository findingRepo, IUserService currentUser, IRedisService redisService, IUnitOfWork unitOfWork)
        {
            _scanRepo = scanRepo;
            _domainRepo = domainRepo;
            _findingRepo = findingRepo;
            _currentUser = currentUser;
            _redisService = redisService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(ProcessScanResultCommand request, CancellationToken ct)
        {
            var result = request.Result;
            var scan = await _scanRepo.GetByIdAsync(new ScanId(Guid.Parse(result.ScanId)), ct);

            if (scan is null)
                return Result.Failure("Scan not found");

            if (!result.Success)
            {
                scan.Fail(result.FailureReason ?? "Worker reported failure");
                await _unitOfWork.SaveChangesAsync(ct);
                return Result.Success();
            }

            scan.MarkProcessing();

            foreach (var f in result.Findings)
            {
                var finding = Finding.Create(
                    scan.Id,
                    Enum.Parse<FindingType>(f.Type),
                    Enum.Parse<Severity>(f.Severity),
                    f.Title,
                    f.RawData);

                await _findingRepo.AddAsync(finding, ct);
            }

            scan.Complete();
            await _unitOfWork.SaveChangesAsync(ct);

            // Trigger AI enrichment
            await _redisService.PublishJob("vulnscan:queue:ai_jobs", new AiJob(
                ScanId: result.ScanId,
                Findings: result.Findings
            ), ct);

            return Result.Success();
        }
    }
}