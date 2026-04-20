using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Profiles.Commands.CreateProfile;
using Fourteen.Application.Features.Profiles.Commands.DeleteProfile;
using Fourteen.Application.Features.Profiles.Queries.GetProfiles;
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
        private readonly NaturalLanguageQueryParser parser;
        private readonly ILogger<ProfilesController> _logger;

        public ProfilesController(IMediator mediator, NaturalLanguageQueryParser parser, ILogger<ProfilesController> logger)
        {
            this.mediator = mediator;
            this.parser = parser;
            _logger = logger;
        }

     
        [HttpGet]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetAllProfile([FromQuery] ProfileFilterRequest request, CancellationToken ct)
        {
            if (!request.IsValid(out var error))
                return BadRequest(new { status = "error", message = error });

             var query = new GetProfilesQuery(request.Gender, request.AgeGroup, request.CountryId, request.MinAge, request.MaxAge,
                                              request.MinGenderProbability, request.MinCountryProbability,
                                              request.SortBy, request.Order, request.Page, request.Limit);

            var result = await this.mediator.Send(query, ct);
            // return Ok(new {
            //     status = "success",
            //     page = result.Page,
            //     limit = result.Limit,
            //     total = result.Total,
            //     data = result.Data
            // });

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
                Data = profilesResult.Data
            };
            return Ok(response);
        }

        [HttpGet("Search")]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> SearchProfiles([FromQuery] string? q, CancellationToken ct, int page = 1, int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { status = "error", message = "Missing or empty parameter" });

            var parsed = this.parser.Parse(q, page, Math.Min(limit, 50));
            
            if (parsed.IsFailure)
                return UnprocessableEntity(new { status = "error", message = parsed.Error });

            var result = await this.mediator.Send(parsed.Value, ct);

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
                Page = parsed.Value.Page,
                Limit = parsed.Value.Limit,
                Total = profilesResult.TotalCount,
                Data = profilesResult.Data
            };
            return Ok(response);
        }

    }
}
