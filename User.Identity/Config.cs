using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace User.Identity
{
    public static class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("geteway_api", "user service"),
                new ApiResource("contact_api", "contact service"),
                new ApiResource("user_api", "user service"),
                new ApiResource("project_api", "project service"),
                new ApiResource("recommend_api","project recommend service")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client()
                {
                    ClientId = "android",
                    ClientSecrets = {new Secret("secret".Sha256())},

                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AllowOfflineAccess = true,
                    RequireClientSecret = false,
                    AllowedGrantTypes =  { "sms_auth_code"},
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = {
                        "geteway_api",
                        "contact_api",
                        "user_api",
                        "project_api",
                        "recommend_api",
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    }
                }
             };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>{
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>{
                new TestUser{
                    SubjectId = "1",
                    Username = "cbb",
                    Password = "123456",
                     Claims = new List<Claim> {
                         new Claim("name","cbb@cbb.com"),
                         new Claim("website","cbb.com"),
                     }
                }
            };
        }
    }
}
