
using AuthServer.Data;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Shared.Models;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthServerDbContext>();
            try
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
                await CreateApplicationsAsync();
                await CreateScopesAsync();
                await CreateUsersAndRolesAsync();

                async Task CreateUsersAndRolesAsync()
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    // roles
                    var roles = new List<string>();
                    if (await roleManager.FindByNameAsync("Administrator") is null)
                    {
                        var adminRole = new ApplicationRole
                        {
                            Name = "Administrator"
                        };
                        await roleManager.CreateAsync(adminRole);
                        roles.Add(adminRole.Name);
                    }
                    if (await roleManager.FindByNameAsync("User") is null)
                    {
                        var userRole = new ApplicationRole
                        {
                            Name = "User"
                        };
                        await roleManager.CreateAsync(userRole);
                        roles.Add(userRole.Name);
                    }
                    // users
                    if (await userManager.FindByNameAsync("admin@localhost") is null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = "admin@localhost",
                            Email = "admin@localhost",
                            EmailConfirmed = true
                        };
                        await userManager.CreateAsync(user, "Pass123$");
                        await userManager.AddToRoleAsync(user, "Administrator");
                    }
                    if (await userManager.FindByNameAsync("user") is null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = "user",
                            Email = "user@localhost",
                            EmailConfirmed = true
                        };
                        await userManager.CreateAsync(user, "Pass123$");
                        await userManager.AddToRoleAsync(user, "User");
                    }
                }
                async Task CreateApplicationsAsync()
                {
                    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
                    if (await manager.FindByClientIdAsync("mongoclient") is null)
                    {
                        await manager.CreateAsync(new OpenIddictApplicationDescriptor
                        {
                            ClientId = "mongoclient",
                            ClientType = ClientTypes.Public,
                            RedirectUris =
                            {
                                new Uri("https://localhost:7197/authentication/login-callback")
                            },
                            PostLogoutRedirectUris =
                            {
                                new Uri("https://localhost:7197/authentication/logout-callback")
                            },
                            Permissions =
                            {
                                Permissions.Endpoints.Authorization,
                                Permissions.Endpoints.Logout,
                                Permissions.Endpoints.Token,
                                Permissions.GrantTypes.AuthorizationCode,
                                Permissions.GrantTypes.RefreshToken,
                                Permissions.ResponseTypes.Code,
                                Permissions.Scopes.Email,
                                Permissions.Scopes.Profile,
                                Permissions.Scopes.Roles,
                                Permissions.Prefixes.Scope + "mongoApi",
                            },
                            Requirements =
                            {
                                Requirements.Features.ProofKeyForCodeExchange,
                            },
                        });
                    }
                    if (await manager.FindByClientIdAsync("resource_server_mongo") is null)
                    {
                        await manager.CreateAsync(new OpenIddictApplicationDescriptor
                        {
                            ClientId = "resource_server_mongo",
                            ClientSecret = "1468AA3F-CCA8-4D11-9681-D0EF57B3F4AF",
                            Permissions =
                            {
                                Permissions.Endpoints.Introspection
                            }
                        });
                    }

                    // Note: no client registration is created for resource_server_2
                    // as it uses local token validation instead of introspection.
                }

                async Task CreateScopesAsync()
                {
                    var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                    if (await manager.FindByNameAsync("mongoApi") is null)
                    {
                        await manager.CreateAsync(new OpenIddictScopeDescriptor
                        {
                            Name = "mongoApi",
                            Resources =
                            {
                                "resource_server_mongo"
                            }
                        });
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
