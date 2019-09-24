using System;
using System.Net.Http;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Serialization;

using Polly;

namespace ResilientHttp
{
    public class ResilientHttpClientFactory : IResilientHttpClientFactory
    {
        private ResilientHttpClientFactory()
        { }

        public ResilientHttpClientFactory(ILogger<ResilientHttpClient> logger, IHttpContextAccessor httpContextAccessor, int exceptionsAllowedBeforeBreaking = 5, int retryCount = 6)
            : this(logger, httpContextAccessor, new CamelCasePropertyNamesContractResolver(), exceptionsAllowedBeforeBreaking, retryCount)
        { }

        public ResilientHttpClientFactory(ILogger<ResilientHttpClient> logger, IHttpContextAccessor httpContextAccessor, IContractResolver contractResolver, int exceptionsAllowedBeforeBreaking = 5, int retryCount = 6)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _contractResolver = contractResolver ?? throw new ArgumentNullException(nameof(contractResolver));
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            _retryCount = retryCount;
        }

        private readonly int _retryCount;

        private readonly int _exceptionsAllowedBeforeBreaking;

        private readonly IContractResolver _contractResolver;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<ResilientHttpClient> _logger;

        private Policy[] CreatePolicies() => new Policy[]
            {
                Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"Retry {retryCount} implemented with Polly's RetryPolicy " +
                            $"of {context.PolicyKey} " +
                            $"at {context.OperationKey}, " +
                            $"due to: {exception}.";
                        _logger.LogWarning(msg);
                        _logger.LogDebug(msg);
                    }),Policy.Handle<HttpRequestException>()
                    .CircuitBreakerAsync(_exceptionsAllowedBeforeBreaking, TimeSpan.FromMinutes(1), (exception, duration) =>
                    {
                        _logger.LogTrace("Circuit breaker opened");
                    },
                    () =>
                    {
                        _logger.LogTrace("Circuit breaker reset");
                    })
            };

        public IHttpClient CreateResilientHttpClient() => new ResilientHttpClient(origin => CreatePolicies(), _httpContextAccessor, _contractResolver);
    }
}