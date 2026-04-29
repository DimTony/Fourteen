using System.Security.Claims;
using System.Security.Cryptography;
using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Users.Queries.GetDashboardStats;
using Fourteen.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace Fourteen.API.Controllers
{
    [ApiController]
    [EnableRateLimiting("api")]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            this.mediator = mediator;
            _logger = logger;
        }

        [HttpGet("Stats")]
        [ProducesResponseType(typeof(CreateProfileSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetDashboardStats(CancellationToken ct)
        {
            var query = new GetDashboardStatsQuery();

            var result = await this.mediator.Send(query, ct);

            if (result.IsFailure)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            var dashboardResult = result.Value;

            var response = new GetDashboardStatsResponse
            {
                Status = "success",
                Data = new List<MetricDto>
                {
                    new MetricDto
                    {
                        Label = dashboardResult.Label,
                        Value = dashboardResult.Value,
                        Change = dashboardResult.Change,
                        Icon = dashboardResult.Icon,
                        Color = dashboardResult.Color
                    }
                }
            };

            return Ok(response);
        }
    }
}