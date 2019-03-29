using Microsoft.EntityFrameworkCore;
using Project.Domain.AggregatesModel;
using Project.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ProjectEntity = Project.Domain.AggregatesModel.Project;

namespace Project.Infrastructure.Repositories
{
    public class ProjectRespository : IProjectRepository
    {
        private ProjectContext _context;

        public IUnitOfWork UnitOfWork => _context;


        public ProjectRespository(ProjectContext context)
        {
            _context = context;
        }

        public async Task<ProjectEntity> AddAsync(ProjectEntity project)
        {
            if (project.IsTransient())
            {
                return (await _context.AddAsync(project)).Entity;
            }

            return project;
        }

        public async Task<ProjectEntity> GetAsync(int id)
        {
            var project = await _context.Projects
                                .Include(p => p.Properties)
                                .Include(p => p.Viewers)
                                .Include(p => p.Contributors)
                                .Include(p => p.VisibleRule)
                                .SingleOrDefaultAsync();

            return project;
        }

        public void Update(ProjectEntity project)
        {
            _context.Update(project);
        }
    }
}
