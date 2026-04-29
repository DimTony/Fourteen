using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Features.Classify.Queries.ClassifyName;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fourteen.API.Controllers
{
    [ApiController]
    [EnableRateLimiting("api")]
    [Route("api/[controller]")]
    public class ClassifyController : ControllerBase
    {
        private readonly IMediator mediator;

        public ClassifyController(IMediator mediator)
            => this.mediator = mediator;


    }

}
