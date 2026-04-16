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
            // Only intercept requests that declare a feature requirement
            if (request is not IRequiresFeature flagged)
            {
                _logger.LogDebug("Request {RequestType} does not require feature flag check", 
                    typeof(TRequest).Name);
                return await next();
            }

            var isEnabled = _config.GetValue<bool>(flagged.FeatureFlag);

            _logger.LogInformation(
                "Feature Flag Check: {FeatureFlag} = {IsEnabled} for Request {RequestType}",
                flagged.FeatureFlag,
                isEnabled,
                typeof(TRequest).Name);

            if (!isEnabled)
            {
                _logger.LogWarning(
                    "Feature {FeatureFlag} is DISABLED. Blocking request {RequestType}",
                    flagged.FeatureFlag,
                    typeof(TRequest).Name);

                // Return a failure Result without ever touching the handler
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

            _logger.LogInformation(
                "Feature {FeatureFlag} is ENABLED. Proceeding with request {RequestType}",
                flagged.FeatureFlag,
                typeof(TRequest).Name);

            return await next();
        }
    }

}
