using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.API.Applications.Commands;
using Project.API.Applications.Queries;
using Project.API.Applications.Service;
using System;
using System.Threading.Tasks;

namespace Project.API.Controllers
{
    [Route("api/projects")]
    public class ProjectController : BaseController
    {
        private IMediator _mediator;
        private IRecommendService _recommendService;
        private IProjectQueries _projectQueries;

        public ProjectController(IMediator mediator, IRecommendService recommendService, IProjectQueries projectQueries)
        {
            _mediator = mediator;
            _recommendService = recommendService;
            _projectQueries = projectQueries;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetProjects()
        {
            var project = await _projectQueries.GetProjectsByUserIdAsync(UserIdentity.UserId);
            return Ok(project);
        }
        [HttpGet]
        [Route("my/{projectId}")]
        public async Task<IActionResult> GetMyProjectDetail(int projectId)
        {
            var project = await _projectQueries.GetProjectDetailAsync(projectId);

            if (project.UserId == UserIdentity.UserId)
            {
                return Ok(project);
            }
            else
            {
                return BadRequest("无权查看该项目");
            }
        }

        [HttpGet]
        [Route("recommends/{projectid}")]
        public async Task<IActionResult> GetRecommendProjectDetail(int projectId)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                var project = await _projectQueries.GetProjectDetailAsync(projectId);
                return Ok(project);
            }
            else
            {
                return BadRequest("无权查看该项目");
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateProject([FromBody]Domain.AggregatesModel.Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            project.UserId = UserIdentity.UserId;
            var command = new CreateProjectCommand
            {
                Project = project
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut]
        [Route("view/{projectId}")]
        public async Task<IActionResult> ViewProject(int projectId)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                return BadRequest("没有查看该项目的权限");
            }

            var command = new ViewProjectCommand
            {
                UserId = UserIdentity.UserId,
                UserName = UserIdentity.Name,
                Avatar = UserIdentity.Avatar,
                ProjectId = projectId
            };

            await _mediator.Send(command);
            return Ok();
        }

        [HttpPut]
        [Route("join/{projectId}")]
        public async Task<IActionResult> JoinProject(int projectid, [FromBody] Domain.AggregatesModel.ProjectContributor contributor)
        {
            if (await _recommendService.IsProjectInRecommend(projectid, UserIdentity.UserId))
            {
                return BadRequest("没有查看该项目的权限");
            }

            var command = new JoinProjectCommand
            {
                Contributor = contributor,
            };

            await _mediator.Send(command);
            return Ok();
        }
    }
}
