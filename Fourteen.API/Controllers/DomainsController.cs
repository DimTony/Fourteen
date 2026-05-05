using System.Security.Claims;
using System.Security.Cryptography;
using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Authentication.Commands.UpdateUser;
using Fourteen.Application.Features.Domains.Commands.AddDomain;
using Fourteen.Application.Features.Domains.Commands.DeleteDomain;
using Fourteen.Application.Features.Domains.Commands.VerifyDomain;
using Fourteen.Application.Features.Domains.Queries.GetDomainById;
using Fourteen.Application.Features.Domains.Queries.GetDomains;
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
    [Route("api/[controller]")]
    public class DomainsController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<DomainsController> _logger;

        public DomainsController(IMediator mediator, ILogger<DomainsController> logger)
        {
            this.mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "AnalystOnly")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddDomain(
            [FromBody] AddDomainCommand command,
            CancellationToken ct)
        {
            var result = await this.mediator.Send(command, ct);

            if (result.IsFailure)
            {
                return BadRequest(new { status = "error", message = result.Error });
            }

            var domain = result.Value;
            var response = new AddDomainResponse
            {
                Status = "success", 
                Message = "Domain added successfully",
                Data = domain
            };

            return Created($"api/domains/{domain.Id}", response);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PagedResponse<DomainDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetDomains([FromQuery] DomainsFilterApiRequest request, CancellationToken ct)
        {
            if (!request.IsValid(out var error))
                return BadRequest(new { status = "error", message = error });

             var query = new GetDomainsQuery(request.VerificationStatus, request.UserId,
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

            var domainResult = result.Value;
            var response = new PagedResponse<DomainDto>
            {
                Status = "success",
                Page = request.Page,
                Limit = request.Limit,
                Total = domainResult.TotalCount,
                TotalPages = domainResult.TotalPages,
                Links = domainResult.Links,
                Data = domainResult.Data
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<DomainDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetSingleDomain(
                [FromRoute] Guid id,
                CancellationToken ct)
        {
            var result = await this.mediator.Send(new GetDomainByIdQuery(id), ct);

            if (result.IsFailure)
            {
                return NotFound(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            var domain = result.Value;

            var response = new ApiResponse<DomainDto>
            {
                Status = "success",
                Data = new DomainDto
                {
                    Id = domain.Id.Value,
                    Name = domain.Name,
                    VerificationStatus = domain.VerificationStatus.ToString()
                }
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
        public async Task<IActionResult> DeleteDomain(
               [FromRoute] Guid id,
               CancellationToken ct)
        {
            var result = await this.mediator.Send(new DeleteDomainCommand(id), ct);
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

        [HttpPost("{id:guid}/verify")]
        public async Task<IActionResult> Verify([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await this.mediator.Send(new VerifyDomainCommand(id), ct);

            if (result.IsFailure)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = "error",
                    Message = result.Error
                });
            }

            var domain = result.Value;

            var response = new ApiResponse<DomainDto>
            {
                Status = "success",
                Data = new DomainDto
                {
                    Id = domain.Id,
                    Name = domain.Name,
                    VerificationStatus = domain.VerificationStatus.ToString()
                }
            };

            return Ok(response);
        }

    }
}