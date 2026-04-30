using System.Text;
using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Profiles.Commands.CreateProfile;
using Fourteen.Application.Features.Profiles.Commands.DeleteProfile;
using Fourteen.Application.Features.Profiles.Queries.GetProfileById;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
using Fourteen.Application.Features.Profiles.Queries.SearchProfiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fourteen.API.Controllers
{
    [ApiController]
    [EnableRateLimiting("api")]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<ProfilesController> _logger;

        public ProfilesController(IMediator mediator, ILogger<ProfilesController> logger)
        {
            this.mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> CreateProfile(
            [FromBody] CreateProfileRequest request,
            CancellationToken ct)
        {
            if (request.Name is null || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = "Missing or empty name"
                });
            }

            var result = await this.mediator.Send(new CreateProfileCommand(request.Name), ct);

            if (result.IsFailure)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            var profileResult = result.Value;
            var profile = profileResult.Profile;

            var response = new CreateProfileSuccessResponse
            {
                Status = "success",
                Message = profileResult.IsNewProfile ? null : "Profile already exists",
                Data = new CreateProfileDto
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
                },
            };

            return profileResult.IsNewProfile
                ? Created($"api/profiles/{profile.Id.Value}", response)
                : Ok(response);
        }
     
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAllProfile([FromQuery] ProfileFilterApiRequest request, CancellationToken ct)
        {
            if (!request.IsValid(out var error))
                return BadRequest(new { status = "error", message = error });

             var query = new GetProfilesQuery(request.Gender, request.AgeGroup, request.CountryId, request.MinAge, request.MaxAge,
                                              request.MinGenderProbability, request.MaxGenderProbability, request.MinCountryProbability,
                                              request.SortBy, request.Order, request.Page, request.Limit);

            var result = await this.mediator.Send(query, ct);

            if (result.IsFailure)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            var profilesResult = result.Value;
            var response = new GetProfilesSuccessResponse
            {
                Status = "success",
                Page = request.Page,
                Limit = request.Limit,
                Total = profilesResult.TotalCount,
                TotalPages = profilesResult.TotalPages,
                Links = profilesResult.Links,
                Data = profilesResult.Data
            };
            return Ok(response);
        }

        [HttpGet("export")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportProfiles(
            [FromQuery] ProfileFilterApiRequest request,
            [FromQuery] string format = "csv",
            CancellationToken ct = default)
        {
            if (!request.IsValid(out var error))
                return BadRequest(new { status = "error", message = error });
 
            var query = new GetProfilesQuery(
                request.Gender, request.AgeGroup, request.CountryId,
                request.MinAge, request.MaxAge,
                request.MinGenderProbability, request.MaxGenderProbability,
                request.MinCountryProbability,
                request.SortBy, request.Order,
                request.Page, request.Limit);
 
            var result = await this.mediator.Send(query, ct);
 
            if (result.IsFailure)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }
 
            var profilesResult = result.Value;

            return format.ToLowerInvariant() switch
            {
                "csv"  => BuildCsvResult(profilesResult.Data),
                "json" => BuildJsonResult(request, profilesResult), 
                _      => BadRequest(new { status = "error", message = $"Unsupported format: '{format}'. Supported: csv" })
            };
 
        }
        
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(SingleProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetSingleProfile(
                [FromRoute] Guid id,
                CancellationToken ct)
        {
            _logger.LogInformation("GetSingleProfile endpoint called with Id: {Id}", id);

            var result = await this.mediator.Send(new GetProfileByIdQuery(id), ct);

            if (result.IsFailure)
            {
                _logger.LogWarning("GetSingleProfile failed: {Error}", result.Error);
                return NotFound(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            _logger.LogInformation("Profile retrieved successfully: Id={Id}", id);

            var profile = result.Value;

            var response = new SingleProfileSuccessResponse
            {
                Status = "success",
                Data = new SingleProfileDto
                {
                    Id = profile.Id.Value,
                    Name = profile.Name,
                    Gender = profile.Gender,
                    GenderProbability = profile.GenderProbability,
                    SampleSize = profile.SampleSize,
                    Age = profile.Age,
                    AgeGroup = profile.AgeGroup,
                    CountryId = profile.CountryId,
                    CountryName = profile.CountryName,
                    CountryProbability = profile.CountryProbability,
                    CreatedAt = profile.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")

                }
            };

            return Ok(response);

        }


        [HttpGet("Search")]
        [Authorize]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> SearchProfiles([FromQuery] SearchApiRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.RawQuery))
                return BadRequest(new { status = "error", message = "Missing or empty parameter" });
            

            var result = await this.mediator.Send(new SearchProfilesQuery(request.RawQuery, request.SortBy, request.Order, request.Page, request.Limit), ct);

            if (result.IsFailure)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            var profilesResult = result.Value;
            var response = new GetProfilesSuccessResponse
            {
                Status = "success",
                Page = request.Page,
                Limit = request.Limit,
                Total = profilesResult.TotalCount,
                TotalPages = profilesResult.TotalPages,
                Links = profilesResult.Links,
                Data = profilesResult.Data
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> DeleteProfile(
               [FromRoute] Guid id,
               CancellationToken ct)
        {
            var result = await this.mediator.Send(new DeleteProfileCommand(id), ct);
            if (result.IsFailure)
            {
                return NotFound(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }
            return NoContent();
        }

        private OkObjectResult BuildJsonResult(ProfileFilterApiRequest request, PagedResult<ProfileDto> profilesResult)
        {
            var response = new GetProfilesSuccessResponse
            {
                Status = "success",
                Page = request.Page,
                Limit = request.Limit,
                Total = profilesResult.TotalCount,
                TotalPages = profilesResult.TotalPages,
                Links = profilesResult.Links,
                Data = profilesResult.Data
            };
            return Ok(response);
        }
        private static string BuildCsv(IEnumerable<ProfileDto> profiles)
        {
            var sb = new StringBuilder();
 
            sb.AppendLine("id,name,gender,gender_probability,age,age_group,country_id,country_name,country_probability,created_at");
 
            foreach (var p in profiles)
            {
                sb.Append(EscapeCsvField(p.Id.ToString())).Append(',');
                sb.Append(EscapeCsvField(p.Name)).Append(',');
                sb.Append(EscapeCsvField(p.Gender)).Append(',');
                sb.Append(EscapeCsvField(p.GenderProbability.ToString() ?? string.Empty)).Append(',');
                sb.Append(EscapeCsvField(p.Age.ToString() ?? string.Empty)).Append(',');
                sb.Append(EscapeCsvField(p.AgeGroup ?? string.Empty)).Append(',');
                sb.Append(EscapeCsvField(p.CountryId ?? string.Empty)).Append(',');
                sb.Append(EscapeCsvField(p.CountryName ?? string.Empty)).Append(',');
                sb.Append(EscapeCsvField(p.CountryProbability.ToString() ?? string.Empty)).Append(',');
                sb.AppendLine(EscapeCsvField(p.CreatedAt ?? string.Empty));
            }
 
            return sb.ToString();
        }
        private FileContentResult BuildCsvResult(IEnumerable<ProfileDto> profiles)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var filename = $"profiles_{timestamp}.csv";
            var bytes = Encoding.UTF8.GetBytes(BuildCsv(profiles));
            return File(bytes, "text/csv", filename);
        }
        private static string EscapeCsvField(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
 
            return value;
        }

    }
}
