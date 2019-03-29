using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public class UserService : IUserServices
    {
        private string _userServiceUrl;
        private IHttpClient _httpClient;
        private ILogger<UserService> _logger;
        public UserService(IHttpClient httpClient, IOptions<ServiceDiscoveryOptions> options, IDnsQuery dnsQuery, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var address = dnsQuery.ResolveService("service.consul", options.Value.UserServiceName);
            var host = address.First().AddressList.FirstOrDefault()?.ToString() ?? address.First().HostName;
            var post = address.First().Port;

            _userServiceUrl = $"http://{host}:{post}";
        }

        public async Task<UserIdentity> CheckOrCreateAsync(string phone)
        {
            _logger.LogTrace($"Enter into CheckOrCreate {phone}");

            var form = new Dictionary<string, string>
            {
                {"phone",phone }
            };

            try
            {
                var response = await _httpClient.PostAsync(_userServiceUrl + "/api/users/check-or-create", form);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<UserIdentity>(result);
                    _logger.LogTrace($"Completed CheckOrCreate with userId: {userInfo.Id}");
                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CheckOrCreate在重试之后失败,{ex.Message} {ex.StackTrace}");
                throw ex;
            }

            return null;
        }
    }
}
