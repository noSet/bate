using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace User.Identity.Authentication
{
    public class ProfileService : IProfileService
    {
        private ILogger<ProfileService> _logger;

        public ProfileService(ILogger<ProfileService> logger)
        {
            _logger = logger;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.LogProfileRequest(_logger);

            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));
            var subjectId = subject.Claims.Where(c => c.Type == "sub").FirstOrDefault()?.Value;

            if (!int.TryParse(subjectId, out var inuserId))
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            context.IssuedClaims.AddRange(context.Subject.Claims);

            context.LogIssuedClaims(_logger);
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            _logger.LogDebug("IsActive called from: {caller}", context.Caller);

            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));
            var subjectId = subject.Claims.Where(c => c.Type == "sub").FirstOrDefault()?.Value;
            context.IsActive = int.TryParse(subjectId, out _);

            return Task.CompletedTask;
        }
    }
}
