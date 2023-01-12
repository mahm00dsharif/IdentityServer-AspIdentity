using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Security.Claims;

namespace IdentityServer
{
    public static class InMemoryConfig
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() =>
          new List<IdentityResource>
          {
          new IdentityResources.OpenId(),
          new IdentityResources.Profile(),
          //new IdentityResource("Roles", "User role(s)", new List<string> { "role" })
          };

        public static List<TestUser> GetUsers() =>
        new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "a9ea0f25-b964-409f-bcce-c923266249b4",
                Username = "Mick",
                Password = "MickPassword",
                Claims = new List<Claim>
                {
                    new Claim("given_name", "Mick"),
                    new Claim("family_name", "Mining"),
                    new Claim("role", "SuperAdmin")
                }
            },
            new TestUser
            {
                SubjectId = "c95ddb8c-79ec-488a-a485-fe57a1462340",
                Username = "Jane",
                Password = "JanePassword",
                Claims = new List<Claim>
                {
                    new Claim("given_name", "Jane"),
                    new Claim("family_name", "Downing"),
                    new Claim("role", "Gym")
                }
            }
        };

        //this one define what clients wants to use this auth server
        public static IEnumerable<Client> GetClients() =>
        new List<Client>
        {
           new Client
           {
                AllowOfflineAccess = true,
                AllowAccessTokensViaBrowser = true,
                ClientId = "ApiClient",
                ClientSecrets = new [] { new Secret("1851DAC237CF7639".Sha512()) },
                //AllowedGrantTypes = GrantTypes.Hybrid,
                //RequirePkce = true,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId,
                   "SystemApi","ApiRoles", IdentityServerConstants.LocalApi.ScopeName }
            },
           //if we want to use for cookie authentication or mvc client 
           //new Client
           // {
           //     ClientName = "mvc Client",
           //     ClientId = "mvc-client",
           //     AllowedGrantTypes = GrantTypes.Hybrid,
           //     RedirectUris = new List<string>{ "https://localhost:5010/signin-oidc" },
           //     RequirePkce = false,
           //     AllowedScopes = { IdentityServerConstants.StandardScopes.OpenId,
           //        IdentityServerConstants.StandardScopes.Profile },
           //     ClientSecrets = { new Secret("MVCSecret".Sha512()) }
           // }
        };

        //this for api 
        public static IEnumerable<ApiScope> GetApiScopes() =>
        new List<ApiScope> {
            new ApiScope("SystemApi", "API"),
            new ApiScope("ApiRoles","User role(s)", new[] { "role" }),
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
        };


        public static IEnumerable<ApiResource> GetApiResources() =>
        new List<ApiResource>
        {
            new ApiResource("SystemApi", "API")
            {
                Scopes = { "SystemApi" }
            },
            new ApiResource("ApiRoles", "User role(s)", new[] { "role" }),
            new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
        };


    }
}
