using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace User.Identity.Infrastructure
{
    public class ResilienceClientFactory
    {
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 重试次数
        /// </summary>
        private int _retryCount;

        /// <summary>
        /// 熔断之前允许异常次数
        /// </summary>
        private int _exceptionCountAllowedBeforBreaking;

        public ResilienceClientFactory(IHttpContextAccessor httpContextAccessor,
            ILogger<ResilienceHttpClient> logger,
            int retryCount,
            int exceptionCountAllowedBeforBreaking)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforBreaking = exceptionCountAllowedBeforBreaking;
        }

        public ResilienceHttpClient GetResilienceHttpClient() =>
            new ResilienceHttpClient(origin => CreatePolicy(origin), _logger, _httpContextAccessor);

        private Policy[] CreatePolicy(string origin)
        {
            return new Policy[] {
                Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    _retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"第{retryCount}次重试 " +
                        $"of {context.PolicyKey}" +
                        $"at {context.ExecutionKey}," +
                        $"due to: {exception}";
                        _logger.LogWarning(msg);
                        _logger.LogDebug(msg);
                    }),

                Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    _exceptionCountAllowedBeforBreaking,
                    TimeSpan.FromMinutes(1),
                    (exception,duration)=>{
                        _logger.LogTrace("熔断器打开");
                    },
                    ()=>{
                        _logger.LogTrace("熔断器关闭");
                    })
            };
        }
    }
}
