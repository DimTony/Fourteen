using Fourteen.Domain.Common;

namespace Fourteen.Domain.Aggregates.Users
{
    public class User : AggregateRoot<UserId>
    {
        public string ProviderId { get; private set; } = default!;
        public string Username { get; private set; } = default!;
        public string? Password { get; private set; }
        public string Email { get; private set; } = default!;
        public string AvatarUrl { get; private set; } = default!;
        public UserRole Role { get; private set; } = UserRole.analyst;
        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private User() { }

        public static User Create(string providerId, string username, string email, string avatarUrl, UserRole role = UserRole.analyst, string? password = null)
            => new()
            {
                Id = UserId.New(),
                ProviderId = providerId,
                Username = username,
                Email = email,
                AvatarUrl = avatarUrl,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Password = password
            };

        public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
        public void Deactivate() => IsActive = false;

        public bool VerifyPassword(string password)
        {
            if (Password == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, Password);
        }

        public void UpdateProfile(string? username, string? avatarUrl)
        {
            if (!string.IsNullOrEmpty(username) && Username != username)
            {
                Username = username;
            }

            if (!string.IsNullOrEmpty(avatarUrl) && AvatarUrl != avatarUrl)
            {
                AvatarUrl = avatarUrl;
            }
        }
    }
}