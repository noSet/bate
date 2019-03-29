using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MediatR;
using Project.API.Applications.IntegrationEvents;
using Project.Domain.Events;

namespace Project.API.Applications.DomainEventHandlers
{
    public class ProjectCreatedDomainEventHandler : INotificationHandler<ProjectCreatedEvent>
    {
        ICapPublisher _capPublisher;
        public ProjectCreatedDomainEventHandler(ICapPublisher capPublisher)
        {
            this._capPublisher = capPublisher;
        }

        public async Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectCreatedIntegraitionEvent
            {
                CreatedTime = DateTime.Now,
                ProjectId = notification.Project.Id,
                UserId = notification.Project.UserId,
                Company = notification.Project.Company,
                FinStage = notification.Project.FinStage,
                Introduction = notification.Project.Introduction,
                ProjectAvatar = notification.Project.Avatar,
                Tags = notification.Project.Tags
            };

            await _capPublisher.PublishAsync("finbook.projectapi.projectcreated", @event);
        }
    }
}
