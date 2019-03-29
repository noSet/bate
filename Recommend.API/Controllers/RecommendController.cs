using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommend.API.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Controllers
{
    [Route("api/recommends")]
    public class RecommendController : BaseController
    {
        private RecommendDbContext _dbContext;

        public RecommendController(RecommendDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET api/values
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _dbContext.ProjectRecommends.AsNoTracking()
                  .Where(r => r.UserId == UserIdentity.UserId)
                  .ToListAsync());
        }
    }
}
