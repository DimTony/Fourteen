using Fourteen.Domain.Common;

namespace Fourteen.Domain.Aggregates.Users
{
    public class User : AggregateRoot<UserId>
    {
        public string GithubId { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string AvatarUrl { get; private set; } = default!;
        public UserRole Role { get; private set; } = UserRole.analyst;
        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private User() { }

        public static User Create(string githubId, string username, string email, string avatarUrl)
            => new()
            {
                Id = UserId.New(),
                GithubId = githubId,
                Username = username,
                Email = email,
                AvatarUrl = avatarUrl,
                Role = UserRole.analyst,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

        public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
        public void Deactivate() => IsActive = false;
    }
}