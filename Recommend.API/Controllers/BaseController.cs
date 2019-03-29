﻿using Microsoft.AspNetCore.Mvc;
using Recommend.API.Dtos;
using System;
using System.Linq;

namespace Recommend.API.Controllers
{
    public class BaseController : Controller
    {
        protected UserIdentity UserIdentity
        {
            get
            {
                var identity = new UserIdentity();
                identity.UserId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);
                identity.Name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                identity.Company = User.Claims.FirstOrDefault(c => c.Type == "company")?.Value;
                identity.Title = User.Claims.FirstOrDefault(c => c.Type == "title")?.Value;
                identity.Avatar = User.Claims.FirstOrDefault(c => c.Type == "avatar")?.Value;
                return identity;
            }
        }
    }
}
