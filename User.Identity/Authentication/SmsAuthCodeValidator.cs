﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using User.Identity.Services;

namespace User.Identity.Authentication
{
    public class SmsAuthCodeValidator : IExtensionGrantValidator
    {
        private readonly IAuthCodeService _authCodeService;
        private readonly IUserServices _userServices;

        public SmsAuthCodeValidator(IAuthCodeService authCodeService, IUserServices userServices)
        {
            _authCodeService = authCodeService;
            _userServices = userServices;
        }

        public string GrantType => "sms_auth_code";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw["phone"];
            var code = context.Request.Raw["auth_code"];
            var errorValidationResult = new GrantValidationResult(TokenRequestErrors.InvalidGrant);

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(code))
            {
                context.Result = errorValidationResult;
                return;
            }

            // 检查验证码是否正确
            if (!_authCodeService.Validate(phone, code))
            {
                context.Result = errorValidationResult;
                return;
            }

            // 完成用户注册
            var userInfo = await _userServices.CheckOrCreateAsync(phone);
            if (userInfo == null)
            {
                context.Result = errorValidationResult;
                return;
            }

            var claims = new Claim[] {
                new Claim("name",userInfo.Name??string.Empty),
                new Claim("company",userInfo.Company??string.Empty),
                new Claim("title",userInfo.Title??string.Empty),
                new Claim("avatar",userInfo.Avatar??string.Empty),
            };

            context.Result = new GrantValidationResult(userInfo.Id.ToString(), GrantType, claims);
        }
    }
}
