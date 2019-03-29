using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Data;
using Contact.API.Service;
using Contact.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Contact.API.Controllers
{
    [Route("api/contacts")]
    public class ContactController : BaseController
    {
        private IContacaApplyRequestRepository _contacaApplyRequestRepository;
        private IContactRepository _contactRepository;
        private IUserService _userService;

        public ContactController(IContacaApplyRequestRepository contacaApplyRequestRepository, IContactRepository contactRepository, IUserService userService)
        {
            _contacaApplyRequestRepository = contacaApplyRequestRepository;
            _contactRepository = contactRepository;
            _userService = userService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactAsync(UserIdentity.UserId, cancellationToken));
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> Get(int userId, CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactAsync(userId, cancellationToken));
        }

        [HttpPut]
        [Route("tag")]
        public async Task<IActionResult> TagContact([FromBody]TagContactInputViewModel viewModel, CancellationToken cancellationToken)
        {
            var result = await _contactRepository.TagContactAsync(UserIdentity.UserId, viewModel.ContacId, viewModel.Tags, cancellationToken);
            if (result)
            {
                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequest(CancellationToken cancellationToken)
        {
            var request = await _contacaApplyRequestRepository.GetRequestListAsync(UserIdentity.UserId, cancellationToken);
            return Ok(request);
        }

        /// <summary>
        /// 添加好友请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("apply-requests/{userId}")]
        public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken)
        {
            var result = await _contacaApplyRequestRepository.AddRequestAsync(new Models.ContactApplyRequest
            {
                UserId = userId,
                ApplierId = UserIdentity.UserId,
                Name = UserIdentity.Name,
                Company = UserIdentity.Company,
                Title = UserIdentity.Title,
                Avatar = UserIdentity.Avatar,
                ApplyTime = DateTime.Now
            }, cancellationToken);

            if (!result)
            {
                // log
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("apply-requests/{applierId}")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId, CancellationToken cancellationToken)
        {
            var result = await _contacaApplyRequestRepository.ApprovalAsync(UserIdentity.UserId, applierId, cancellationToken);

            if (!result)
            {
                // log
                return BadRequest();
            }

            var applier = await _userService.GetBaseUserInfoAsync(applierId);
            var userInfo = await _userService.GetBaseUserInfoAsync(UserIdentity.UserId);

            await _contactRepository.AddContactAsync(UserIdentity.UserId, applier, cancellationToken);
            await _contactRepository.AddContactAsync(applierId, userInfo, cancellationToken);

            return Ok();
        }
    }
}
