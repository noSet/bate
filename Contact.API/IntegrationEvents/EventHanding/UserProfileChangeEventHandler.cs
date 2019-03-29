using Contact.API.Data;
using Contact.API.IntegrationEvents.Events;
using DotNetCore.CAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Contact.API.IntegrationEvents.EventHanding
{
    public class UserProfileChangeEventHandler : ICapSubscribe
    {
        private IContactRepository _contactRepository;

        public UserProfileChangeEventHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [CapSubscribe("finbook.userapi.userprofilechanged")]
        public async Task UpdateContactInfo(UserProfileChangedEvent @event)
        {
            var token = new CancellationToken();

            await _contactRepository.UpdateContactInofAsync(new Dtos.UserIdentity
            {
                Avatar = @event.Avatar,
                Company = @event.Company,
                Name = @event.Name,
                Title = @event.Title,
                UserId = @event.UserId,
            }, token);
        }
    }
}
