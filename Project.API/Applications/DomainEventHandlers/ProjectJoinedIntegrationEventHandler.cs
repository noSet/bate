using DotNetCore.CAP;
using MediatR;
using Project.API.Applications.IntegrationEvents;
using Project.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.Applications.DomainEventHandlers
{
    public class ProjectJoinedIntegrationEventHandler : INotificationHandler<ProjectJoninedEvnet>
    {
        ICapPublisher _capPublisher;
        public ProjectJoinedIntegrationEventHandler(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }

        public async Task Handle(ProjectJoninedEvnet notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectJoinedIntegrationEvent
            {
                Company = notification.Company,
                Introduction = notification.Introduction,
                Contributor = notification.Contributor
            };

            await _capPublisher.PublishAsync("finbook.projectapi.projectjoined", @event);
        }
    }
}
