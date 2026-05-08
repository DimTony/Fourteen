using Fourteen.Application.Common.DTOs;
using Fourteen.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fourteen.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("api")]
    public class ImportController : ControllerBase
    {
        private readonly IBulkProfileImporter _importer;
        private readonly ILogger<ImportController> _logger;
 
        public ImportController(IBulkProfileImporter importer, ILogger<ImportController> logger)
        {
            _importer = importer;
            _logger = logger;
        }
 
        [HttpPost("profiles")]
        [Authorize(Policy = "AdminOnly")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BulkImportResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadCsv(
            IFormFile? file,
            CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new ApiErrorResponse
                {
                    Status  = "error",
                    Message = "No file provided or file is empty"
                });
 
            var ext = Path.GetExtension(file.FileName);
            if (!string.IsNullOrEmpty(ext) &&
                !ext.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status  = "error",
                    Message = "Only CSV files are accepted"
                });
            }
 
            _logger.LogInformation(
                "BulkImport started: file={Name} size={Size}",
                file.FileName, file.Length);

            await using var stream = file.OpenReadStream();

            var result = await _importer.Import(stream, ct);
 
            _logger.LogInformation(
                "BulkImport finished: inserted={Inserted} skipped={Skipped}",
                result.Inserted, result.Skipped);
 
            return Ok(result);
        }
    }
}