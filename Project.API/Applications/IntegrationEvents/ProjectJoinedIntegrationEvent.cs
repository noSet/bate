using Project.Domain.AggregatesModel;

namespace Project.API.Applications.IntegrationEvents
{
    public class ProjectJoinedIntegrationEvent
    {
        public string Company { get; set; }

        public string Introduction { get; set; }

        public ProjectContributor Contributor { get; set; }
    }
}
