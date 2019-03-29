using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recommend.API.Dtos;
using Resilience;

namespace Recommend.API.Services
{
    public class ContactService : IContactService
    {
        private string _contactServiceUrl;
        private IHttpClient _httpClient;
        private ILogger<ContactService> _logger;

        public ContactService(IHttpClient httpClient, IOptions<ServiceDiscoveryOptions> options, IDnsQuery dnsQuery, ILogger<ContactService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var address = dnsQuery.ResolveService("service.consul", options.Value.ContactServiceName);
            var host = address.First().AddressList.FirstOrDefault()?.ToString() ?? address.First().HostName;
            var post = address.First().Port;

            _contactServiceUrl = $"http://{host}:{post}";
        }


        public async Task<List<Contact>> GetContactsByUserId(int userId)
        {
            _logger.LogTrace($"Enter into GetContactsByUserId {userId}");

            try
            {
                var result = await _httpClient.GetStringAsync(_contactServiceUrl + "/api/contacts/" + userId);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    var contacts = JsonConvert.DeserializeObject<List<Contact>>(result);
                    _logger.LogTrace($"Completed GetContactsByUserId with userId: {userId}");
                    return contacts;
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
