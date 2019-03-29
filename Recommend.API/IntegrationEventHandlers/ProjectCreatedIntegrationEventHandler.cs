using DotNetCore.CAP;
using Recommend.API.Data;
using Recommend.API.IntegrationEvents;
using Recommend.API.Models;
using Recommend.API.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.IntegrationEventHandlers
{
    public class ProjectCreatedIntegrationEventHandler : ICapSubscribe
    {
        private IUserService _userService;
        private IContactService _contactService;
        private RecommendDbContext _context;

        public ProjectCreatedIntegrationEventHandler(RecommendDbContext context, IUserService userService, IContactService contactService)
        {
            _context = context;
            _userService = userService;
            _contactService = contactService;
        }

        [CapSubscribe("finbook.projectapi.projectcreated")]
        public async Task CreateRecommendFromProject(ProjectCreatedIntegraitionEvent @event)
        {
            var fromUser = await _userService.GetBaseUserInfoAsync(@event.UserId);
            var contacts = await _contactService.GetContactsByUserId(@event.UserId);

            var recommends = contacts.Select(c => new ProjectRecommend
            {
                FromUserId = @event.UserId,
                Company = @event.Company,
                Tags = @event.Tags,
                ProjectId = @event.ProjectId,
                ProjectAvatar = @event.ProjectAvatar,
                FinStage = @event.FinStage,
                RecommendTime = DateTime.Now,
                CreateTime = @event.CreatedTime,
                Introduction = @event.Introduction,
                RecommendType = EnumRecommendType.Friend,
                FromUserAvatar = fromUser.Avatar,
                FromUserName = fromUser.Name,
                UserId = c.UserId
            });

            await _context.ProjectRecommends.AddRangeAsync(recommends);
            await _context.SaveChangesAsync();
        }
    }
}
