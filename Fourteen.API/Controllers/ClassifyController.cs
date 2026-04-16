using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Classify.Queries.ClassifyName;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fourteen.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassifyController : ControllerBase
    {
        private readonly IMediator mediator;

        public ClassifyController(IMediator mediator)
            => this.mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> ClassifyName(
            [FromQuery] string? name,
            CancellationToken ct)
        {
            if (name is null || string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = "Missing or empty name parameter"
                });
            }

            var result = await this.mediator.Send(new ClassifyByNameQuery(name), ct);

            if (result.IsFailure)
            {
                // Feature disabled or other business failure
                if (result.Error.Contains("disabled"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse
                    {
                        Status = "error",
                        Message = result.Error
                    });
                }

                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            return Ok(new ApiSuccessResponse
            {
                Status = "success",
                Data = new GenderDataDto
                {
                    Name = result.Value.Name,
                    Gender = result.Value.Gender,
                    Probability = result.Value.Probability,
                    SampleSize = result.Value.SampleSize,
                    IsConfident = result.Value.IsConfident,
                    ProcessedAt = result.Value.ProcessedAt
                }
            });
        }
    }

}
