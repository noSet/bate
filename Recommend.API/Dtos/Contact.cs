using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Dtos
{
    public class Contact
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 好友ID
        /// </summary>
        public int ContactId { get; set; }
    }
}
