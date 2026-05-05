using System.Security.Cryptography;
using Fourteen.Domain.Common;
using Fourteen.Domain.EventHandlers;

namespace Fourteen.Domain.Aggregates.Domains
{
    public class Scan : AggregateRoot<ScanId>
    {
        public DomainId DomainId { get; private set; } = default!;
        public UserId RequestedBy { get; private set; } = default!;
        public ScanStatus Status { get; private set; }
        public ScanType Type { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? FailureReason { get; private set; }
        private Scan() { }
        public static Scan Create(DomainId domainId, UserId requestedBy, ScanType type) => new()
        {
            Id = ScanId.New(),
            DomainId = domainId,
            RequestedBy = requestedBy,
            Status = ScanStatus.Pending,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        public Result Start()
        {
            if (Status != ScanStatus.Pending)
                return Result.Failure("Only pending scans can be started");
            Status = ScanStatus.Running;
            StartedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkProcessing()
        {
            if (Status != ScanStatus.Running)
                return Result.Failure("Scan must be running to enter processing");
            Status = ScanStatus.Processing;
            return Result.Success();
        }

        public Result Complete()
        {
            if (Status != ScanStatus.Processing)
                return Result.Failure("Scan must be processing to complete");
            Status = ScanStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            RaiseDomainEvent(new ScanCompletedEvent(Id, DomainId, RequestedBy));
            return Result.Success();
        }

        public Result Fail(string reason)
        {
            Status = ScanStatus.Failed;
            FailureReason = reason;
            CompletedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result Cancel()
        {
            if (Status is ScanStatus.Completed or ScanStatus.Failed)
                return Result.Failure("Cannot cancel a finished scan");
            Status = ScanStatus.Failed;
            FailureReason = "Cancelled by user";
            CompletedAt = DateTime.UtcNow;
            return Result.Success();
        }

    }
}
