
namespace Fourteen.Domain.Common
{
    public enum UserRole
    {
        admin,
        analyst
    }

    public enum VerificationStatus
    {
        Pending,
        Verified,
        Failed
    }
    public enum ScanStatus
    {
        Pending,
        Running,
        Processing,
        Completed,
        Failed
    }
    public enum ScanType
    {
        Full,
        DNS,
        SSL,
        Headers
    }

    public enum FindingType
    {
        SubdomainTakeover,
        OpenS3Bucket,
        ExposedGitRepo,
        SensitiveDataExposure,
        CORSMisconfiguration,
        DNS, 
        SSL, 
        Headers, 
        Port, 
        Configuration,
        Other
    }

    public enum Severity
    {
        Info,
        Low,
        Medium,
        High,
        Critical
    }
}