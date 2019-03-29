﻿using Project.Domain.SeedWork;
using System;

namespace Project.Domain.AggregatesModel
{
    public class ProjectContributor : Entity
    {
        public int ProjectId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Avatar { get; set; }

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 关闭者
        /// </summary>
        public bool IsCloser { get; set; }

        /// <summary>
        /// 1 财务顾问 2 投资金沟
        /// </summary>
        public int ContributorType { get; set; }
    }
}
