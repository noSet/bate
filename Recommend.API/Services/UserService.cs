using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recommend.API.Dtos;
using Resilience;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Services
{
    public class UserService : IUserService
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

        public async Task<UserIdentity> GetBaseUserInfoAsync(int userId)
        {
            _logger.LogTrace($"Enter into Baseinfo {userId}");

            try
            {
                var result = await _httpClient.GetStringAsync(_userServiceUrl + "/api/users/baseinfo/" + userId);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    var userInfo = JsonConvert.DeserializeObject<UserIdentity>(result);
                    _logger.LogTrace($"Completed Baseinfo with userId: {userId}");
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
