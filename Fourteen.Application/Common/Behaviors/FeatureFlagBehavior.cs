using Fourteen.Application.Interfaces;
using Fourteen.Domain.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fourteen.Application.Common.Behaviors
{
    public class FeatureFlagBehaviour<TRequest, TResponse>
     : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FeatureFlagBehaviour<TRequest, TResponse>> _logger;

        public FeatureFlagBehaviour(
            IConfiguration config,
            ILogger<FeatureFlagBehaviour<TRequest, TResponse>> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            if (request is not IRequiresFeature flagged)
            {
                _logger.LogDebug("Request {RequestType} does not require feature flag check", 
                    typeof(TRequest).Name);
                return await next();
            }

            var isEnabled = _config.GetValue<bool>(flagged.FeatureFlag);

            if (!isEnabled)
            {
                _logger.LogWarning(
                    "Feature {FeatureFlag} is DISABLED. Blocking request {RequestType}",
                    flagged.FeatureFlag,
                    typeof(TRequest).Name);

                var resultType = typeof(TResponse);

                if (resultType == typeof(Result))
                    return (TResponse)(object)Result.Failure(
                        $"This feature is currently disabled: {flagged.FeatureFlag}");

                if (resultType.IsGenericType &&
                    resultType.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var innerType = resultType.GetGenericArguments()[0];
                    var failureMethod = typeof(Result)
                        .GetMethod(nameof(Result.Failure), 1, new[] { typeof(string) })!
                        .MakeGenericMethod(innerType);

                    return (TResponse)failureMethod.Invoke(null, new object[] {
                    $"This feature is currently disabled: {flagged.FeatureFlag}"
                })!;
                }

                throw new InvalidOperationException(
                    $"Feature '{flagged.FeatureFlag}' is disabled and response type " +
                    $"'{resultType.Name}' does not support failure results.");
            }

            return await next();
        }
    }

}
