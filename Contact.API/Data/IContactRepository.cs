using Contact.API.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Contact.API.Data
{
    public interface IContactRepository
    {
        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="userIdentity"></param>
        /// <returns></returns>
        Task<bool> UpdateContactInofAsync(UserIdentity userIdentity, CancellationToken cancellationToken);

        /// <summary>
        /// 添加联系人信息
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddContactAsync(int userId, UserIdentity contact, CancellationToken cancellationToken);

        /// <summary>
        /// 联系人列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Models.Contact>> GetContactAsync(int userId, CancellationToken cancellationToken);

        /// <summary>
        /// 更新好友标签
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken);
    }
}
