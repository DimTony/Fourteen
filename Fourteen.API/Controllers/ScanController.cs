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
    public class ScanController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly ILogger<ScanController> _logger;

        public ScanController(IMediator mediator, ILogger<ScanController> logger)
        {
            this.mediator = mediator;
            _logger = logger;
        }

        [HttpPost("domains/{domainId:guid}/scans")]
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
    }
}