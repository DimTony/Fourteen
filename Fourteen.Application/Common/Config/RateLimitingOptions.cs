
namespace Fourteen.Application.Common.Config
{
    public class RateLimitingOptions
    {
        public RateLimitRule Auth { get; set; } = new();
        public RateLimitRule Api { get; set; } = new();
    }

    public class RateLimitRule
    {
        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
    }
}