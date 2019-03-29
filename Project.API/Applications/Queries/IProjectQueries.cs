using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Applications.Queries
{
    public interface IProjectQueries
    {
        Task<dynamic> GetProjectsByUserIdAsync(int userId);

        Task<dynamic> GetProjectDetailAsync(int projectId);
    }
}
