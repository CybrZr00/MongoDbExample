using BlazorUI;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Web;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddFluentUIComponents();
builder.Services.AddHttpClient("resource_server_mongo")
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://localhost:7274"))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
builder.Services.AddScoped(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("resource_server_mongo");
});

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.ClientId = "mongoclient";
    options.ProviderOptions.Authority = "https://localhost:7071/";
    options.ProviderOptions.ResponseType = "code";

    // Note: response_mode=fragment is the best option for a SPA. Unfortunately, the Blazor WASM
    // authentication stack is impacted by a bug that prevents it from correctly extracting
    // authorization error responses (e.g error=access_denied responses) from the URL fragment.
    // For more information about this bug, visit https://github.com/dotnet/aspnetcore/issues/28344.
    //
    options.ProviderOptions.ResponseMode = "query";
    options.AuthenticationPaths.RemoteRegisterPath = "https://localhost:7071/Identity/Account/Register";
    options.AuthenticationPaths.RemoteProfilePath = "https://localhost:7071/Identity/Account/Manage";
    options.ProviderOptions.PostLogoutRedirectUri = $"{builder.HostEnvironment.BaseAddress}authentication/logout-callback";
    options.ProviderOptions.RedirectUri = $"{builder.HostEnvironment.BaseAddress}authentication/login-callback";
    options.AuthenticationPaths.LogOutPath = "https://localhost:7071/Identity/Account/logout";
    options.AuthenticationPaths.LogInPath = "https://localhost:7071/Identity/Account/login";

    options.ProviderOptions.DefaultScopes.Add(Scopes.Email);
    options.ProviderOptions.DefaultScopes.Add(Scopes.Roles);
    options.ProviderOptions.DefaultScopes.Add(Scopes.OfflineAccess);
    options.ProviderOptions.DefaultScopes.Add("mongoApi");
    options.ProviderOptions.DefaultScopes.Add("roles");
    options.UserOptions.RoleClaim = "role";
});
await builder.Build().RunAsync();
