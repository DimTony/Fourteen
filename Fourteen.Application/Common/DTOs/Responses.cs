using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fourteen.Application.Common.DTOs
{
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

        [JsonPropertyName("age")]
        public required int Age { get; set; }

        [JsonPropertyName("age_group")]
        public required string AgeGroup { get; set; }

        [JsonPropertyName("country_id")]
        public required string CountryId { get; set; }

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

        [JsonPropertyName("sample_size")]
        public required int SampleSize { get; set; }

        [JsonPropertyName("age")]
        public required int Age { get; set; }

        [JsonPropertyName("age_group")]
        public required string AgeGroup { get; set; }

        [JsonPropertyName("country_id")]
        public required string CountryId { get; set; }

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

    public class GetAllProfilesSuccessResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("count")]
        public required int Count { get; set; }

        [JsonPropertyName("data")]
        public required List<ProfileDto> Data { get; set; }
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

    public class ExternalDataResult
    {

    }
}
