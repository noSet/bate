using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public interface IUserServices
    {

        /// <summary>
        /// 检查手机号是否注册，如果没注册就注册一个用户
        /// </summary>
        /// <param name="phone"></param>
        Task<UserIdentity> CheckOrCreateAsync(string phone);
    }
}
