using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 性别 1男0女
        /// </summary>
        public byte Gender { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }

        /// <summary>
        /// 省Id
        /// </summary>
        public int ProvinceId { get; set; }

        /// <summary>
        /// 省名称
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市Id
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 名片地址
        /// </summary>
        public string NameCard { get; set; }


        public List<UserProperty> Properties { get; set; } = new List<UserProperty>();
    }
}
