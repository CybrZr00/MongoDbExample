using AuthServer.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shared.Helpers.V1;
using System.Text;

using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using AuthServer.Data;

namespace AuthServer.ServiceExtensions
{
    public static class OidcBuilder
    {
        public static WebApplicationBuilder AddOidc(this WebApplicationBuilder builder)
        {
            var jwtSettings = new JwtSettings();
            builder.Configuration.Bind(nameof(jwtSettings), jwtSettings);
            builder.Services.AddSingleton(jwtSettings);
            //builder.Services.Configure<CertificateConfig>(builder.Configuration.GetSection(nameof(CertificateConfig)));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddOpenIdConnect(options =>
                        {
                            options.SignInScheme = "Identity.External";
                            options.Authority = "https://localhost:7071";
                            options.ClientId = "resource_server_mongo";
                            options.ClientSecret = "1468AA3F-CCA8-4D11-9681-D0EF57B3F4AF";

                            options.GetClaimsFromUserInfoEndpoint = true;
                            options.Scope.Add("openid");
                            options.Scope.Add("profile");
                            options.SaveTokens = true;
                            options.ResponseType = OpenIdConnectResponseType.Code;
                            options.RequireHttpsMetadata = true;

                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                NameClaimType = "name",
                                RoleClaimType = ClaimTypes.Role,
                                ValidateIssuer = true
                            };
                        });
            builder.Services.AddOpenIddict()

                        // Register the OpenIddict core components.
                        .AddCore(options =>
                        {
                            // Configure OpenIddict to use the Entity Framework Core stores and models.
                            // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                            options.UseEntityFrameworkCore()
                                   .UseDbContext<AuthServerDbContext>();

                            // Enable Quartz.NET integration.
                            options.UseQuartz();
                        })

                        // Register the OpenIddict server components.
                        .AddServer(options =>
                        {
                            options.SetAccessTokenLifetime(jwtSettings.TokenLifetime);
                            // Enable the authorization, logout, token and userinfo endpoints.
                            options.SetAuthorizationEndpointUris("/" + ServerRoutes.Authorisation.Authorize)
                                      .SetLogoutEndpointUris("/" + ServerRoutes.Authorisation.Logout)
                                      .SetIntrospectionEndpointUris("/" + ServerRoutes.Authorisation.Introspect)
                                      .SetTokenEndpointUris("/" + ServerRoutes.Authorisation.Token)
                                      .SetUserinfoEndpointUris("/" + ServerRoutes.UserInfo.Get)
                                      .SetVerificationEndpointUris("/" + ServerRoutes.Authorisation.Verify);

                            // Note: this sample uses the code, device code, password and refresh token flows, but you
                            // can enable the other flows if you need to support implicit or client credentials.
                            options
                            .AllowAuthorizationCodeFlow()
                            .RequireProofKeyForCodeExchange()
                            .AllowClientCredentialsFlow()
                            .AllowHybridFlow()
                            .AllowRefreshTokenFlow();

                            options.DisableAccessTokenEncryption();

                            // Mark the "email", "profile", "roles", "phone" and "core" scopes as supported scopes.
                            options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.Phone, "mongoApi");
                            string certPW = Environment.GetEnvironmentVariable("DEFAULT_CERT", EnvironmentVariableTarget.Machine) ?? string.Empty;
                            // use local certs in debug - in production these should be retrieved from a certificate store
                            if (builder.Environment.IsDevelopment() && !string.IsNullOrEmpty(certPW))
                            {
                                var signingPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "signing-certificate.pfx");
                                options.AddSigningCertificate(new FileStream(signingPath, FileMode.Open), certPW);
                                var encryptionCertPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "encryption-certificate.pfx");
                                options.AddEncryptionCertificate(new FileStream(encryptionCertPath, FileMode.Open), certPW);
                            }else if (builder.Environment.IsDevelopment())
                            {
                                options.AddDevelopmentEncryptionCertificate();
                                options.AddDevelopmentSigningCertificate();
                            }



                            // Register the signing and encryption credentials.
                            options.AddEncryptionKey(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.EncrytionKey)));
                            options.AddSigningKey(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SigningKey)));
                            // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                            options.UseAspNetCore()
                                   .EnableAuthorizationEndpointPassthrough()
                                   .EnableLogoutEndpointPassthrough()
                                   .EnableTokenEndpointPassthrough()
                                   .EnableUserinfoEndpointPassthrough()
                                   .EnableStatusCodePagesIntegration();
                        })

                        // Register the OpenIddict validation components.
                        .AddValidation(options =>
                        {
                            // Register the System.Net.Http integration.
                            options.UseSystemNetHttp();
                            // Import the configuration from the local OpenIddict server instance.
                            options.UseLocalServer();
                            // Register the ASP.NET Core host.
                            options.UseAspNetCore();
                        });
            builder.Services.AddAuthorization(options =>
            {
                // Add policies here
            });
            builder.Services.AddHostedService<Worker>();
            return builder;
        }
    }
}
