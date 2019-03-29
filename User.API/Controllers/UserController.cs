using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using User.API.Data;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Models;
using DotNetCore.CAP;

namespace User.API.Controllers
{
    [Route("api/users")]
    public class UserController : BaseController
    {
        private UserContext _userContext;
        private ICapPublisher _capPublisher;
        private ILogger<UserController> _logger;

        public UserController(UserContext userContext, ICapPublisher capPublisher, ILogger<UserController> logger)
        {
            _userContext = userContext;
            _capPublisher = capPublisher;
            _logger = logger;
        }


        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users
                .AsNoTracking()
                .Include(u => u.Properties)
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            if (user == null)
            {
                throw new UserOperationException($"错误的用户上下文Id{UserIdentity.UserId}");
            }

            return Json(user);
        }

        private void RaiseUserprfileChangedEvent(AppUser user)
        {
            if (_userContext.Entry(user).Property(nameof(user.Name)).IsModified
                || _userContext.Entry(user).Property(nameof(user.Title)).IsModified
                || _userContext.Entry(user).Property(nameof(user.Company)).IsModified
                || _userContext.Entry(user).Property(nameof(user.Avatar)).IsModified)
            {
                _capPublisher.Publish("finbook.userapi.userprofilechanged", new Dtos.UserIdentity
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Company = user.Company,
                    Title = user.Title,
                    Avatar = user.Avatar
                });
            }
        }

        [Route("")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<AppUser> patch)
        {
            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            patch.ApplyTo(user);

            foreach (var property in user.Properties)
            {
                _userContext.Entry(property).State = EntityState.Deleted;
            }

            var originProperties = await _userContext.UserProperties.AsNoTracking().Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync();
            var allProperties = originProperties.Union(user.Properties).Distinct();

            var removeProperties = originProperties.Except(user.Properties);
            var newProperties = allProperties.Except(originProperties);

            foreach (var property in removeProperties)
            {
                _userContext.Remove(property);
            }

            foreach (var property in newProperties)
            {
                _userContext.Add(property);
            }

            using (var transaction = _userContext.Database.BeginTransaction())
            {
                // 发布用户属性变更消息
                RaiseUserprfileChangedEvent(user);

                _userContext.Users.Update(user);
                await _userContext.SaveChangesAsync();

                transaction.Commit();
            }

            return Json(user);
        }

        /// <summary>
        /// 获取用户标签资料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync());
        }

        /// <summary>
        /// 更新用户标签数据
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateUserTags([FromBody]List<string> tags)
        {
            var originTags = await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync(); ;
            var newTags = tags.Except(originTags.Select(t => t.Tag));

            await _userContext.UserTags.AddRangeAsync(newTags.Select(t => new UserTag
            {
                CreateTime = DateTime.Now,
                UserId = UserIdentity.UserId,
                Tag = t
            }));

            await _userContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// 根据手机查找用户资料
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _userContext.Users.Include(u => u.Properties).SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId));
        }

        /// <summary>
        /// 检查或者创建用户
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("check-or-create")]
        public async Task<IActionResult> CheckOrCreate(string phone)
        {
            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Phone == phone);

            if (user == null)
            {
                user = new AppUser { Phone = phone };
                _userContext.Users.Add(user);
                await _userContext.SaveChangesAsync();
            }

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }


        [HttpGet]
        [Route("baseinfo/{userId}")]
        public async Task<IActionResult> GetBaseInfo(int userId)
        {
            // TBD 检查用户是否好友关系
            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                UserId = user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }
    }
}
