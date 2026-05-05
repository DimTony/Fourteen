using System.Security.Cryptography;
using Fourteen.Domain.Common;
using Fourteen.Domain.EventHandlers;

namespace Fourteen.Domain.Aggregates.Domains
{
    public class Domain : AggregateRoot<DomainId>
    {
        public UserId OwnerId { get; private set; } = default!;
        public string Name { get; private set; }  = default!; // e.g. "example.com"
        public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Pending;
        public string VerificationToken { get; private set; }  = default!; // TXT record value
        public DateTime? VerifiedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Domain() { }

        public static Domain Create(UserId ownerId, string name)
            => new()
            {
                Id = DomainId.New(),
                OwnerId = ownerId,
                Name = name.ToLower().Trim(),
                VerificationStatus = VerificationStatus.Pending,
                VerificationToken = GenerateVerificationToken(),
                CreatedAt = DateTime.UtcNow
            };

        public Result MarkVerified()
        {
            if (VerificationStatus == VerificationStatus.Verified)
                return Result.Failure("Domain already verified");

            VerificationStatus = VerificationStatus.Verified;
            VerifiedAt = DateTime.UtcNow;
            RaiseDomainEvent(new DomainVerifiedEvent(Id, OwnerId, Name));
            return Result.Success();
        }

        private static string GenerateVerificationToken()
        {
            var bytes = new byte[24];
            RandomNumberGenerator.Fill(bytes);
            return $"vulnscan-verify={Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
        }
    }
}