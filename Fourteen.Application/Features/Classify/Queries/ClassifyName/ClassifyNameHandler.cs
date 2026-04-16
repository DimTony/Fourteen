using Fourteen.Application.Interfaces;
using Fourteen.Domain.Aggregates.Names;
using Fourteen.Domain.Common;
using Fourteen.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fourteen.Application.Features.Classify.Queries.ClassifyName
{
    public sealed class ClassifyNameHandler
          : IRequestHandler<ClassifyByNameQuery, Result<ClassifyNameDto>>
    {
        private readonly IServices _client;
        private readonly ILogger<ClassifyNameHandler> _logger;

        public ClassifyNameHandler(IServices client, ILogger<ClassifyNameHandler> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result<ClassifyNameDto>> Handle(
            ClassifyByNameQuery request, CancellationToken ct)
        {
            try
            {
                var name = Name.Create(request.Name);
                var raw = await _client.GetByName(name.Value, ct);

                if (raw.Gender is null || raw.Count == 0)
                {
                    return Result.Failure<ClassifyNameDto>(
                        "No prediction available for the provided name");
                }

                var probability = raw.Probability ?? 0.0;
                var sampleSize = raw.Count;
                var isConfident = probability >= 0.7 && sampleSize >= 100;
                var processedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

                var dto = new ClassifyNameDto(
                    Name: name.Value,
                    Gender: raw.Gender,
                    Probability: probability,
                    SampleSize: sampleSize,
                    IsConfident: isConfident,
                    ProcessedAt: processedAt);

                return Result.Success(dto);
            }
            catch (InvalidNameException ex)
            {
                return Result.Failure<ClassifyNameDto>(ex.Message);
            }
            catch (NoPredictionException ex)
            {
                return Result.Failure<ClassifyNameDto>(ex.Message);
            }
            catch (UpstreamApiException ex)
            {
                _logger.LogError(ex, "Upstream API error while classifying name: {Name}", request.Name);
                return Result.Failure<ClassifyNameDto>("Upstream service error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while classifying name: {Name}", request.Name);  
                return Result.Failure<ClassifyNameDto>("An unexpected error occurred");
            }
        }
    }
}
