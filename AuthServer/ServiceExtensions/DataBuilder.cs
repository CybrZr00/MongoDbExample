using AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Shared.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.ServiceExtensions
{
    public static class DataBuilder
    {
        public static WebApplicationBuilder AddData(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<AuthServerDbContext>(options =>
            {
                options.UseOpenIddict();
                options.UseSqlite($"Filename={Path.Combine(Environment.CurrentDirectory, "openiddict-server.sqlite3")}");
            });
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Configure Identity to use the same JWT claims as OpenIddict instead
                // of the legacy WS-Federation claims it uses by default (ClaimTypes),
                // which saves you from doing the mapping in your authorization controller.
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.ClaimsIdentity.EmailClaimType = Claims.Email;
                // Note: to require account confirmation before login,
                // register an email sender service (IEmailSender) and
                // set options.SignIn.RequireConfirmedAccount to true.
                //
                // For more information, visit https://aka.ms/aspaccountconf.
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddEntityFrameworkStores<AuthServerDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();
            builder.Services.AddQuartz(options =>
            {
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });
            // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
            builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
            return builder;
        }
    }
}
