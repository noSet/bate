using MediatR;

namespace Project.API.Applications.Commands
{
    public class JoinProjectCommand : IRequest
    {
        public Domain.AggregatesModel.ProjectContributor Contributor { get; set; }
    }
}
