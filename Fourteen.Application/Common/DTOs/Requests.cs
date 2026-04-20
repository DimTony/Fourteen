using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;


namespace Fourteen.Application.Common.DTOs
{
    public class ProfileFilterRequest
    {
        public string? Gender { get; set; }
        public string? AgeGroup { get; set; }
        public string? CountryId { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public float? MinGenderProbability { get; set; }
        public float? MinCountryProbability { get; set; }
        public string SortBy { get; set; } = "created_at";
        public string Order { get; set; } = "asc";
        public int Page { get; set; } = 1;
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

        public GetProfilesQuery ToQuery() =>
            new(Gender, AgeGroup, CountryId, MinAge, MaxAge,
                MinGenderProbability, MinCountryProbability,
                SortBy, Order, Page, Math.Min(Limit, 50));
    }

    public class CreateProfileRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }
}
