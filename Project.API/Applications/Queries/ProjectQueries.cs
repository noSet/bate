using Dapper;
using Microsoft.EntityFrameworkCore;
using Project.Infrastructure;
using System.Threading.Tasks;

namespace Project.API.Applications.Queries
{
    public class ProjectQueries : IProjectQueries
    {
        private ProjectContext _context;

        public ProjectQueries(ProjectContext context)
        {
            _context = context;
        }

        public async Task<dynamic> GetProjectDetailAsync(int projectId)
        {
            var sql = @"
SELECT
	a.Company,
	a.City,
	a.Area,
	a.Province,
	a.FinStage,
	a.FinMoney,
	a.Valuation,
	a.FinPercentage,
	a.Introduction,
	a.UserId,
	a.Income,
	a.Revenue,
	a.UserName,
	a.Avatar,
	a.BrokerageOptions,
	b.Tags,
	b.Visible 
FROM
	Projects a
	INNER JOIN ProjectVisibleRules b ON a.Id = b.ProjectId 
WHERE
	a.Id = @projectId 
	AND a.UserId = @userId";

            using (var conn = _context.Database.GetDbConnection())
            {
                conn.Open();

                var result = await conn.QueryAsync<dynamic>(sql, new { projectId });
                return result;
            }
        }

        public async Task<dynamic> GetProjectsByUserIdAsync(int userId)
        {
            var sql = @"
SELECT
	t.Id,
	t.Avatar,
	t.Company,
	t.FinStage,
	t.Introduction,
	t.Tags,
	t.ShowSecurityInfo,
	t.CreatedTime
FROM
	Projects t
WHERE
	t.UserId = @userId";

            using (var conn = _context.Database.GetDbConnection())
            {
                conn.Open();

                var result = await conn.QueryAsync<dynamic>(sql, new { userId });
                return result;
            }
        }
    }
}
