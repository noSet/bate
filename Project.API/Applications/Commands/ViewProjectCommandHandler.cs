﻿using MediatR;
using Project.Domain.AggregatesModel;
using Project.Domain.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.Applications.Commands
{
    public class ViewProjectCommandHandler : IRequestHandler<ViewProjectCommand>
    {
        private IProjectRepository _projectRepository;

        public ViewProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task Handle(ViewProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetAsync(request.ProjectId);
            if (project == null)
            {
                throw new ProjectDomainException($"project not found: {request.ProjectId}");
            }

            if (project.UserId == request.UserId)
            {
                throw new ProjectDomainException($"you connot view your own project!");
            }

            project.AddViewer(request.UserId, request.UserName, request.Avatar);
            await _projectRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}
