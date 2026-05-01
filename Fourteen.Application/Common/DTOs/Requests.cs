using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;


namespace Fourteen.Application.Common.DTOs
{
    public class ProfileFilterApiRequest
    {
        [FromQuery(Name = "gender")]
        public string? Gender { get; set; }

        [FromQuery(Name = "age_group")]
        public string? AgeGroup { get; set; }

        [FromQuery(Name = "country_id")]
        public string? CountryId { get; set; }

        [FromQuery(Name = "min_age")]
        public int? MinAge { get; set; }

        [FromQuery(Name = "max_age")]
        public int? MaxAge { get; set; }

        [FromQuery(Name = "min_gender_probability")]
        public float? MinGenderProbability { get; set; }

        [FromQuery(Name = "max_gender_probability")]
        public float? MaxGenderProbability { get; set; }

        [FromQuery(Name = "min_country_probability")]
        public float? MinCountryProbability { get; set; }

        [FromQuery(Name = "sort_by")]
        public string SortBy { get; set; } = "created_at";

        [FromQuery(Name = "order")]
        public string Order { get; set; } = "asc";

        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;
        
        [FromQuery(Name = "limit")]
        public int Limit { get; set; } = 10;

        private static readonly HashSet<string> ValidSortFields = ["age", "created_at", "gender_probability"];
        private static readonly HashSet<string> ValidGenders = ["male", "female"];
        private static readonly HashSet<string> ValidAgeGroups = ["child", "teenager", "adult", "senior"];

        public bool IsValid(out string? error)
        {
            if (Limit > 50) { error = "Invalid query parameters"; return false; }
            if (Gender != null && !ValidGenders.Contains(Gender)) { error = "Invalid query parameters"; return false; }
            if (AgeGroup != null && !ValidAgeGroups.Contains(AgeGroup)) { error = "Invalid query parameters"; return false; }
            if (!ValidSortFields.Contains(SortBy)) { error = "Invalid query parameters"; return false; }
            if (Order is not "asc" and not "desc") { error = "Invalid query parameters"; return false; }
            error = null;
            return true;
        }
    }

    public class CreateProfileRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }

    public class SearchApiRequest
    {
        [FromQuery(Name = "q")]
        public string? RawQuery { get; set; }
        [FromQuery(Name = "sort_by")]
        public string SortBy { get; set; } = "created_at";       // age | created_at | gender_probability
        [FromQuery(Name = "order")]
        public string Order { get; set; } = "asc";
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;
        [FromQuery(Name = "limit")]
        public int Limit { get; set; } = 10;
    }
    public class ParsedProfileFilter
    {
        public string? Gender { get; init; }
        public int? AgeMin { get; init; }
        public int? AgeMax { get; init; }
        public string? CountryId { get; init; }

        public bool IsEmpty =>
            Gender is null && AgeMin is null && AgeMax is null && CountryId is null;
    }

    public record OAuthState(string? CodeChallenge, string? CliCallback);

    public class LogoutRequest
    {
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }

    public class RefreshRequest
    {
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}
