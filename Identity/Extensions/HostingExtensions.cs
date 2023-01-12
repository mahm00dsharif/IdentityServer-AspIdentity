using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer4;
using MailService.Configuration;
using MailService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

namespace IdentityServer.Extensions
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var migrationAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

            builder.Services.AddRazorPages();

            builder.Services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromHours(2));

            builder.Services.AddLocalApiAuthentication();

            var IdentityServerBuilder = builder.Services
                  .AddIdentityServer(options =>
                  {
                      options.Events.RaiseErrorEvents = true;
                      options.Events.RaiseInformationEvents = true;
                      options.Events.RaiseFailureEvents = true;
                      options.Events.RaiseSuccessEvents = true;
                      options.Discovery.CustomEntries.Add("oauth", "~/OAuth");
                      // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                      options.EmitStaticAudienceClaim = true;
                  })
                  //.AddInMemoryApiScopes(InMemoryConfig.GetApiScopes())
                  //.AddInMemoryApiResources(InMemoryConfig.GetApiResources())
                  //.AddInMemoryIdentityResources(InMemoryConfig.GetIdentityResources())
                  //.AddInMemoryClients(InMemoryConfig.GetClients())
                  .AddAspNetIdentity<ApplicationUser>()

                  .AddConfigurationStore(opt =>
                  {
                      opt.ConfigureDbContext = c => c.UseSqlServer(builder.Configuration
                          .GetConnectionString("DefaultConnection"),
                          sql => sql.MigrationsAssembly(migrationAssembly));
                  })
                  .AddOperationalStore(opt =>
                  {
                      opt.ConfigureDbContext = o => o.UseSqlServer(builder.Configuration
                          .GetConnectionString("DefaultConnection"),
                          sql => sql.MigrationsAssembly(migrationAssembly));
                  });

            //.AddTestUsers(InMemoryConfig.GetUsers())
            if (builder.Environment.IsDevelopment())
            {
                IdentityServerBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(IdentityServerConstants.LocalApi.PolicyName, policy =>
                {
                    policy.AddAuthenticationSchemes(IdentityServerConstants.LocalApi.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    // custom requirements
                });
            });

            //builder.Services.AddAuthentication()
            //    .AddGoogle(options =>
            //    {
            //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            //        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            //    });


            var emailConfig = builder.Configuration.GetSection("EmailConfiguration")
            .Get<EmailConfiguration>();

            builder.Services.AddSingleton(emailConfig);
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
