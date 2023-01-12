// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;
using IdentityModel;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Data
{
    public class SeedData
    {
        public static async void EnsureSeedData(WebApplication app)
        {
            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.EnsureDeleted();
                context.Database.Migrate();

                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var admin = userMgr.FindByNameAsync("admin").Result;
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "admin"
                    };
                    var result = userMgr.CreateAsync(admin, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(admin, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "Super Admin"),
                        new Claim(JwtClaimTypes.GivenName, "Super Admin"),
                        new Claim(JwtClaimTypes.FamilyName, "Super Admin"),
                        new Claim(JwtClaimTypes.Email, "admin@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    }).Result;

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }


                    string[] roles = new string[] { "SuperAdmin", "Role1", "Role2", "Role3" };

                    var RoleManager = scope.ServiceProvider.
                        GetRequiredService<RoleManager<IdentityRole>>();

                    foreach (string role in roles)
                    {
                        if (!context.Roles.Any(r => r.Name == role))
                        {
                            result = RoleManager.CreateAsync(new IdentityRole(role)).Result;
                        }
                    }

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    else
                    {
                        result = userMgr.AddToRoleAsync(admin, "SuperAdmin").Result;
                    }

                    Console.WriteLine("admin created");
                }
                else
                {
                    Console.WriteLine("admin already exists");
                }


            }
        }
    }
}