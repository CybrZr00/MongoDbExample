using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;

using Quartz;

using System.Globalization;
using System.Security.Claims;

using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseMicrosoftDependencyInjectionJobFactory();
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartz();

builder.Services.AddDbContext<DbContext>(options =>
{
    // Configure the context to use sqlite.
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-server.sqlite3")}");

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need
    // to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<DbContext>();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        // Enable the authorization, introspection and token endpoints.
        options.SetAuthorizationEndpointUris("authorize")
               .SetIntrospectionEndpointUris("introspect")
               .SetTokenEndpointUris("token");

        // Note: this sample only uses the authorization code and refresh token
        // flows but you can enable the other flows if you need to support implicit,
        // password or client credentials.
        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        // Register the encryption credentials. This sample uses a symmetric
        // encryption key that is shared between the server and the Api2 sample
        // (that performs local token validation instead of using introspection).
        //
        // Note: in a real world application, this encryption key should be
        // stored in a safe place (e.g in Azure KeyVault, stored as a secret).
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        // Register the signing credentials.
        options.AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        //
        // Note: unlike other samples, this sample doesn't use token endpoint pass-through
        // to handle token requests in a custom MVC action. As such, the token requests
        // will be automatically handled by OpenIddict, that will reuse the identity
        // resolved from the authorization code to produce access and identity tokens.
        //
        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough();
    })

    // Register the OpenIddict validation components.
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

builder.Services.AddCors();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5112"));
app.UseHttpsRedirection();

// Create new application registrations matching the values configured in Zirku.Client1 and Zirku.Api1.
// Note: in a real world application, this step should be part of a setup script.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DbContext>();
    await context.Database.EnsureCreatedAsync();

    await CreateApplicationsAsync();
    await CreateScopesAsync();

    async Task CreateApplicationsAsync()
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        if (await manager.FindByClientIdAsync("spa") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "spa",
                ClientType = ClientTypes.Public,
                RedirectUris =
                {
                    new Uri("http://localhost:5112/index.html"),
                    new Uri("http://localhost:5112/signin-callback.html"),
                    new Uri("http://localhost:5112/signin-silent-callback.html"),
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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("api", [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
(ClaimsPrincipal user) => user.Identity!.Name);

app.MapMethods("authorize", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context, IOpenIddictScopeManager manager) =>
{
    // Retrieve the OpenIddict server request from the HTTP context.
    var request = context.GetOpenIddictServerRequest();

    var identifier = (int?)request["hardcoded_identity_id"];
    if (identifier is not (1 or 2))
    {
        return Results.Challenge(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified hardcoded identity is invalid."
            }));
    }

    // Create the claims-based identity that will be used by OpenIddict to generate tokens.
    var identity = new ClaimsIdentity(
        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
        nameType: Claims.Name,
        roleType: Claims.Role);

    // Add the claims that will be persisted in the tokens.
    identity.AddClaim(new Claim(Claims.Subject, identifier.Value.ToString(CultureInfo.InvariantCulture)));
    identity.AddClaim(new Claim(Claims.Name, identifier switch
    {
        1 => "Alice",
        2 => "Bob",
        _ => throw new InvalidOperationException()
    }));
    identity.AddClaim(new Claim(Claims.PreferredUsername, identifier switch
    {
        1 => "Alice",
        2 => "Bob",
        _ => throw new InvalidOperationException()
    }));

    var principal = (await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
    principal.SetScopes(identifier switch
    {
        1 => request.GetScopes(),
        2 => new[] { "mongoApi" }.Intersect(request.GetScopes()),
        _ => throw new InvalidOperationException()
    });

    principal.SetResources(await manager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

    // Allow all claims to be added in the access tokens.
    foreach (var claim in principal!.Claims)
    {
        claim.SetDestinations(Destinations.AccessToken);
    }
    return Results.SignIn(new ClaimsPrincipal(identity), properties: null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});

app.Run();
