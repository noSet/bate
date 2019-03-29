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
    public class ProjectViewedIntegrationEventHandler : INotificationHandler<ProjectViewedEvent>
    {
        ICapPublisher _capPublisher;
        public ProjectViewedIntegrationEventHandler(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }

        public async Task Handle(ProjectViewedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectViewedIntegrationEvent
            {
                Company = notification.Company,
                Introduction = notification.Introduction,
                Viewer = notification.Viewer
            };

            await _capPublisher.PublishAsync("finbook.projectapi.projectviewed", @event);
        }
    }
}
