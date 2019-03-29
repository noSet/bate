using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Resilience
{
    public class ResilienceHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 根据url origin去创建policy
        /// </summary>
        private readonly Func<string, IEnumerable<Policy>> _policyCreator;

        /// <summary>
        /// 把policy打包成组合policy wraper，进行本地缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, PolicyWrap> _policyWrappers;
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(Func<string, IEnumerable<Policy>> policyCreator, ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = new HttpClient();
            _policyWrappers = new ConcurrentDictionary<string, PolicyWrap>();
            this._policyCreator = policyCreator;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authorizationToken, string requestId = null, string authorizationMethod = "Bearer")
        {
            return await DoPostPutAsync(HttpMethod.Post, url, () => CreateHttpRequestMessage(HttpMethod.Post, url, item), authorizationMethod, requestId, authorizationMethod);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> form, string authorizationToken, string requestId = null, string authorizationMethod = "Bearer")
        {
            return await DoPostPutAsync(HttpMethod.Post, url, () => CreateHttpRequestMessage(HttpMethod.Post, url, form), authorizationMethod, requestId, authorizationMethod);
        }

        private HttpRequestMessage CreateHttpRequestMessage<T>(HttpMethod httpMethod, string url, T item)
        {
            return new HttpRequestMessage(httpMethod, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json")
            };
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, string url, Dictionary<string, string> form)
        {
            return new HttpRequestMessage(httpMethod, url)
            {
                Content = new FormUrlEncodedContent(form)
            };
        }

        private Task<HttpResponseMessage> DoPostPutAsync(HttpMethod httpMethod, string url, Func<HttpRequestMessage> httpRequestMessageFunc, string authorizationToken, string requestId = null, string authorizationMethod = "Bearer")
        {
            if (httpMethod != HttpMethod.Post && httpMethod != HttpMethod.Put)
            {
                throw new ArgumentException("Value must be either post or put", nameof(httpMethod));
            }

            var origin = GetOriginFromUri(url);

            return HttpInvoker(origin, async () =>
            {
                var httpRequestMessage = httpRequestMessageFunc();

                SetAuthorizationHeader(httpRequestMessage);

                if (requestId != null)
                {
                    httpRequestMessage.Headers.Add("x-requestid", requestId);
                }

                var response = await _httpClient.SendAsync(httpRequestMessage);

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }

        private async Task<T> HttpInvoker<T>(string origin, Func<Task<T>> action)
        {
            var normalizedOrigin = NormalizeOrigin(origin);

            if (!_policyWrappers.TryGetValue(normalizedOrigin, out var policyWrap))
            {
                policyWrap = Policy.WrapAsync(_policyCreator(normalizedOrigin).ToArray());
                _policyWrappers.TryAdd(normalizedOrigin, policyWrap);
            }

            return await policyWrap.ExecuteAsync(action, new Context(normalizedOrigin));
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private static string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);

            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";

            return origin;
        }

        private void SetAuthorizationHeader(HttpRequestMessage httpRequestMessage)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(authorizationHeader))
            {
                httpRequestMessage.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }
        }

        public Task<string> GetStringAsync(string uri, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            var origin = GetOriginFromUri(uri);

            return HttpInvoker(origin, async () =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

                SetAuthorizationHeader(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            });
        }

        public Task<HttpResponseMessage> PutAsync<T>(string uri, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync(HttpMethod.Put, uri, () => CreateHttpRequestMessage(HttpMethod.Put, uri, item), authorizationToken, requestId, authorizationMethod);
        }
    }
}
