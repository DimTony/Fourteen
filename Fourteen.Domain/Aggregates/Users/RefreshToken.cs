using Fourteen.Domain.Common;

namespace Fourteen.Domain.Aggregates.Users
{
    public class RefreshToken : AggregateRoot<RefreshTokenId>
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = default!;
        public bool IsRevoked { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsRevoked && !IsExpired;

        public void Revoke() => IsRevoked = true;

        public static RefreshToken Create(Guid userId, string token)
            => new()
        {
            Id = RefreshTokenId.New(),
            UserId = userId,
            Token = BCrypt.Net.BCrypt.HashPassword(token),
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };
    }
}