using MediatR;
using Project.Domain.AggregatesModel;
using Project.Domain.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.Applications.Commands
{
    public class JoinProjectCommandHandler : IRequestHandler<JoinProjectCommand>
    {
        private IProjectRepository _projectRepository;

        public JoinProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }
        public async Task Handle(JoinProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetAsync(request.Contributor.ProjectId);
            if (project == null)
            {
                throw new ProjectDomainException($"project not found: {request.Contributor.ProjectId}");
            }

            if (project.UserId == request.Contributor.UserId)
            {
                throw new ProjectDomainException($"you connot join your own project!");
            }

            project.AddContributor(request.Contributor);
            await _projectRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}
