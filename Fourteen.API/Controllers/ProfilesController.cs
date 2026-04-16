using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Profiles.Commands.CreateProfile;
using Fourteen.Application.Features.Profiles.Commands.DeleteProfile;
using Fourteen.Application.Features.Profiles.Queries.GetAllProfiles;
using Fourteen.Application.Features.Profiles.Queries.GetProfileById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fourteen.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly IMediator mediator;

        public ProfilesController(IMediator mediator)
            => this.mediator = mediator;

        [HttpPost]
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
                    SampleSize = profile.SampleSize,
                    Age = profile.Age,
                    AgeGroup = profile.AgeGroup,
                    CountryId = profile.CountryId,
                    CountryProbability = profile.CountryProbability,
                    CreatedAt = profile.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
                },
            };

            return profileResult.IsNewProfile
                ? Created($"api/profiles/{profile.Id.Value}", response)
                : Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
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

            var result = await this.mediator.Send(new GetProfileByIdQuery(id), ct);

            if (result.IsFailure)
            {
                return NotFound(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            var profile = result.Value;

            var response = new CreateProfileSuccessResponse
            {
                Status = "success",
                Data = new CreateProfileDto
                {
                    Id = profile.Id.Value,
                    Name = profile.Name,
                    Gender = profile.Gender,
                    GenderProbability = profile.GenderProbability,
                    SampleSize = profile.SampleSize,
                    Age = profile.Age,
                    AgeGroup = profile.AgeGroup,
                    CountryId = profile.CountryId,
                    CountryProbability = profile.CountryProbability,
                    CreatedAt = profile.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")

                }
            };

            return Ok(response);

        }



        [HttpGet]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAllProfile(
                [FromQuery(Name = "gender")] string? gender,
                [FromQuery(Name = "country_id")] string? countryId,
                [FromQuery(Name = "age_group")] string? ageGroup,
                CancellationToken ct)
        {
            var query = new GetAllProfilesQuery(gender, countryId, ageGroup);
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
            var response = new GetAllProfilesSuccessResponse
            {
                Status = "success",
                Count = profilesResult.Count,
                Data = profilesResult.Profiles
            };
            return Ok(response);
        }


        [HttpDelete("{id}")]
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


    }
}
