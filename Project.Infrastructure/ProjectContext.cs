using MediatR;
using Microsoft.EntityFrameworkCore;
using Project.Domain.SeedWork;
using Project.Infrastructure.EntityConfiguration;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Infrastructure
{
    public class ProjectContext : DbContext, IUnitOfWork
    {
        private IMediator _mediator;

        public DbSet<Domain.AggregatesModel.Project> Projects { get; set; }

        public ProjectContext(DbContextOptions<ProjectContext> options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProjectContributorEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectPropertyEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectViewerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectVisibleRuleEntityConfiguration());
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _mediator.DispatchDomainEventsAsync(this);
            await SaveChangesAsync();
            return true;
        }
    }
}
