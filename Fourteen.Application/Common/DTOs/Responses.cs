using System.Text.Json.Serialization;
using Fourteen.Domain.Aggregates.Profiles;
using DomainEntity = Fourteen.Domain.Aggregates.Domains.Domain;
using Microsoft.AspNetCore.WebUtilities;
using Fourteen.Domain.Aggregates.Domains;

namespace Fourteen.Application.Common.DTOs
{
    public class ScanDto
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("domain_id")]
        public required string DomainId { get; set; }

        [JsonPropertyName("domain_name")]
        public required string DomainName { get; set; }

        [JsonPropertyName("scan_type")]
        public required string ScanType { get; set; }

        public static ScanDto From(Scan scan)
        {
            return new ScanDto
            {
                Id = scan.Id.Value.ToString(),
                DomainId = "",
                DomainName = "",
                ScanType = "",

            };
        }
    }
    public class ApiResponse<T>
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public required T Data { get; set; }
    }
    public class PagedResponse<T>
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("page")]
        public required int Page { get; set; }

        [JsonPropertyName("limit")]
        public required int Limit { get; set; }

        [JsonPropertyName("total")]
        public required int Total { get; set; }

        [JsonPropertyName("total_pages")]
        public required int TotalPages { get; set; }

        [JsonPropertyName("links")]
        public required PageLinks Links { get; set; }

        [JsonPropertyName("data")]
        public required IEnumerable<T> Data { get; set; }
    }

    public class DomainDto
    {
        [JsonPropertyName("id")]
        public required Guid Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("verification_status")]
        public required string VerificationStatus { get; set; }

        public static DomainDto From(DomainEntity domain)
        {
            return new DomainDto
            {
                Id = domain.Id.Value,
                Name = domain.Name,
                VerificationStatus = domain.VerificationStatus.ToString()
            };
        }
    }

    public class AddDomainResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("data")]
        public required DomainDto Data { get; set; }
    }
    public class AuthTokenDto
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public required string RefreshToken { get; set; }

        [JsonPropertyName("username")]
        public required string Username { get; set; }

        [JsonPropertyName("avatar_url")]
        public required string AvatarUrl { get; set; }
    }
    public class AuthResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("data")]
        public required AuthTokenDto Data { get; set; }
    }
    public class GoogleUserInfo
    {
        public string GoogleId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool EmailVerified { get; set; }
        public string FullName { get; set; } = default!;
        public string GivenName { get; set; } = default!;
        public string FamilyName { get; set; } = default!;
        public string AvatarUrl { get; set; } = default!;
    }

    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = default!;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = default!;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = default!;
    }
    public class GetDashboardStatsResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("data")]
        public required List<MetricDto> Data { get; set; }

    }

    public class MetricDto
    {
        [JsonPropertyName("label")]
        public required string Label { get; set; }

        [JsonPropertyName("value")]
        public required string Value { get; set; }

        [JsonPropertyName("change")]
        public required string Change { get; set; }

        [JsonPropertyName("icon")]
        public required string Icon { get; set; }

        [JsonPropertyName("color")]
        public required string Color { get; set; }
    }

    public class UserGrowthDto
    {
        [JsonPropertyName("label")]
        public required string Label { get; set; }

        [JsonPropertyName("total_users")]
        public int TotalUsers { get; set; }

        [JsonPropertyName("user_count_yesterday")]
        public int PreviousPeriodUsers { get; set; }

        [JsonPropertyName("user_count_today")]
        public int CurrentPeriodUsers { get; set; }

        [JsonPropertyName("percentage_increase")]
        public double PercentageIncrease { get; set; }
    }
    public sealed record PageLinks(string Self, string? Next, string? Prev);

    public class PagedResult<T>
    {
        public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages { get; init; }
        public PageLinks Links { get; init; } = default!;

        public static PagedResult<T> From(
            IReadOnlyList<T> data,
            int totalCount,
            int page,
            int pageSize,
            string basePath,
            string? queryString)
        {
            if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var filtered = QueryHelpers.ParseQuery(queryString?.TrimStart('?'))
                .Where(kvp => kvp.Key != "page" && kvp.Key != "limit")
                .SelectMany(kvp => kvp.Value, (kvp, v) => $"{kvp.Key}={Uri.EscapeDataString(v ?? "")}")
                .ToList();

            var qs = filtered.Count > 0 ? "&" + string.Join("&", filtered) : "";

            string Build(int p) => $"{basePath}?page={p}&limit={pageSize}{qs}";

            return new PagedResult<T>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Links = new PageLinks(
                    Self: Build(page),
                    Next: page < totalPages ? Build(page + 1) : null,
                    Prev: page > 1 ? Build(page - 1) : null
                )
            };
        }
    }

    public class SeedFile
    {
        [JsonPropertyName("profiles")]
        public List<SeedProfileDto> Profiles { get; set; } = [];
    }
    public class SeedProfileDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("gender_probability")]
        public float GenderProbability { get; set; }

        [JsonPropertyName("sample_size")]
        public int SampleSize { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("age_group")]
        public string AgeGroup { get; set; } = string.Empty;

        [JsonPropertyName("country_id")]
        public string CountryId { get; set; } = string.Empty;

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; } = string.Empty;

        [JsonPropertyName("country_probability")]
        public float CountryProbability { get; set; }
    }
    
    public class GenderizeResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        //public required string Name { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = default!;
        //public string? Gender { get; set; }

        [JsonPropertyName("probability")]
        public double Probability { get; set; }
        //public double? Probability { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
    public class AgifyResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        //public required string Name { get; set; }

        [JsonPropertyName("age")]
        public int? Age { get; set; }
        //public int Age { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
    public class NationalizeResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
        //public required string Name { get; set; }

        [JsonPropertyName("country")]
        public List<CountryProbability> Country { get; set; } = new();
        //public List<CountryProbability> Country { get; set; }

        [JsonPropertyName("probability")]
        public double? Probability { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class CountryProbability
    {
        [JsonPropertyName("country_id")]
        //public string CountryId { get; set; }
        public string Country_Id { get; set; } = default!;
        [JsonPropertyName("probability")]
        public double Probability { get; set; }
    }
    public class TransformedExternalResponse
    {
        public string? Gender { get; set; }
        public double? GenderProbability { get; set; }
        public int Count { get; set; }
        public int? Age { get; set; }
        public string? Name { get; set; }
        public double? CountryProbability { get; set; }
        public string? Country_Id { get; set; }

    }

    public class ExternalAPIDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("probability")]
        public double? Probability { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class ProfileDto
    {
        [JsonPropertyName("id")]
        public required Guid Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("gender")]
        public required string Gender { get; set; }

        [JsonPropertyName("gender_probability")]
        public required double GenderProbability { get; set; }

        [JsonPropertyName("age")]
        public required int Age { get; set; }

        [JsonPropertyName("age_group")]
        public required string AgeGroup { get; set; }

        [JsonPropertyName("country_id")]
        public required string CountryId { get; set; }

        [JsonPropertyName("country_name")]
        public required string CountryName { get; set; }

        [JsonPropertyName("country_probability")]
        public required double CountryProbability { get; set; }

        [JsonPropertyName("created_at")]
        public required string CreatedAt { get; set; }

        public static ProfileDto From(Profile profile)
        {
            return new ProfileDto
            {
                Id = profile.Id.Value,
                Name = profile.Name,
                Gender = profile.Gender,
                GenderProbability = profile.GenderProbability,
                Age = profile.Age,
                AgeGroup = profile.AgeGroup,
                CountryId = profile.CountryId,
                CountryName = profile.CountryName,
                CountryProbability = profile.CountryProbability,
                CreatedAt = profile.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }


    }

    public class CreateProfileDto
    {
        [JsonPropertyName("id")]
        public required Guid Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("gender")]
        public required string Gender { get; set; }

        [JsonPropertyName("gender_probability")]
        public required double GenderProbability { get; set; }

        [JsonPropertyName("age")]
        public required int Age { get; set; }

        [JsonPropertyName("age_group")]
        public required string AgeGroup { get; set; }

        [JsonPropertyName("country_id")]
        public required string CountryId { get; set; }

        [JsonPropertyName("country_name")]
        public required string CountryName { get; set; }

        [JsonPropertyName("country_probability")]
        public required double CountryProbability { get; set; }

        [JsonPropertyName("created_at")]
        public required string CreatedAt { get; set; }

    }

    public class CreateProfileSuccessResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public required CreateProfileDto Data { get; set; }

    }

    public class SingleProfileDto
    {
        [JsonPropertyName("id")]
        public required Guid Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("gender")]
        public required string Gender { get; set; }

        [JsonPropertyName("gender_probability")]
        public required double GenderProbability { get; set; }

        [JsonPropertyName("sample_size")]
        public required int SampleSize { get; set; }

        [JsonPropertyName("age")]
        public required int Age { get; set; }

        [JsonPropertyName("age_group")]
        public required string AgeGroup { get; set; }

        [JsonPropertyName("country_id")]
        public required string CountryId { get; set; }

        [JsonPropertyName("country_name")]
        public required string CountryName { get; set; }

        [JsonPropertyName("country_probability")]
        public required double CountryProbability { get; set; }

        [JsonPropertyName("created_at")]
        public required string CreatedAt { get; set; }

    }

    public class SingleProfileSuccessResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public required SingleProfileDto Data { get; set; }

    }

    public class GetAllProfilesSuccessResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("count")]
        public required int Count { get; set; }

        [JsonPropertyName("data")]
        public required List<ProfileDto> Data { get; set; }
    }

    public class ResponseLinks
    {
        [JsonPropertyName("self")]
        public required string Self { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("prev")]
        public string? Prev { get; set; }
    }

    public class GetProfilesSuccessResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("page")]
        public required int Page { get; set; }

        [JsonPropertyName("limit")]
        public required int Limit { get; set; }

        [JsonPropertyName("total")]
        public required int Total { get; set; }

        [JsonPropertyName("total_pages")]
        public required int TotalPages { get; set; }

        [JsonPropertyName("links")]
        public required PageLinks Links { get; set; }

        [JsonPropertyName("data")]
        public required IEnumerable<ProfileDto> Data { get; set; }
    }


    public class GetAllProfilesResult
    {
        public List<ProfileDto> Profiles { get; set; } = [];
        public int Count { get; set; }
    }

    public class GenderDataDto
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("gender")]
        public required string Gender { get; set; }

        [JsonPropertyName("probability")]
        public required double Probability { get; set; }

        [JsonPropertyName("sample_size")]
        public required int SampleSize { get; set; }

        [JsonPropertyName("is_confident")]
        public required bool IsConfident { get; set; }

        [JsonPropertyName("processed_at")]
        public required string ProcessedAt { get; set; }
    }

    public class ApiSuccessResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("data")]
        public required GenderDataDto Data { get; set; }
    }

    public class ApiErrorResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }


    public record TokenPair(
    string AccessToken,
    string RefreshToken,
    string Username,
    string AvatarUrl,
    string Role);

    public record CallbackResult(TokenPair TokenPair, string? CliCallback);

    public sealed record GithubUserDto(
        string Id,
        string Login,
        string? Email,
        string AvatarUrl
    );

    public record GithubTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")]   string TokenType,
        [property: JsonPropertyName("scope")]        string Scope
    );

    public record GithubUserProfile(
        [property: JsonPropertyName("id")]         long   Id,
        [property: JsonPropertyName("login")]      string Login,
        [property: JsonPropertyName("email")]      string? Email,
        [property: JsonPropertyName("avatar_url")] string? AvatarUrl
    );

    public record GithubEmail(
        [property: JsonPropertyName("email")]    string Email,
        [property: JsonPropertyName("primary")]  bool   Primary,
        [property: JsonPropertyName("verified")] bool   Verified
    );

}
