using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using Mongotest.Data;

using Serilog;

using System.IO.Compression;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes =
        ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "image/svg+xml", "image/webp", "font/otf", "font/sfnt", "font/ttf", "font/woff", "font/woff2", "application/vnd.ms-fontobject" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = false;
});
builder.Services.AddCors();
builder.Services.AddHttpContextAccessor();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
var log = new LoggerConfiguration()
    .WriteTo.MongoDBBson(
    databaseUrl: "mongodb://localhost:27017/mongotest_v1", 
    collectionName: "logs", 
    rollingInterval: Serilog.Sinks.MongoDB.RollingInterval.Day, 
    cappedMaxSizeMb: 2048)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .CreateLogger();
    
//builder.Services.AddDbContext<ApplicationEFContext>(options =>
//    options.UseMongoDB(builder.Configuration.GetConnectionString("DefaultConnection"), "mongotest_v1_ef"));
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-GB" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Note: the validation handler uses OpenID Connect discovery
        // to retrieve the address of the introspection endpoint.
        options.SetIssuer("https://localhost:7071");
        options.AddAudiences("resource_server_mongo");

        // Configure the validation handler to use introspection and register the client
        // credentials used when communicating with the remote introspection endpoint.
        // Use environment variable to store the secret
        var secret = "1468AA3F-CCA8-4D11-9681-D0EF57B3F4AF";
        if (secret != null)
            options.UseIntrospection()
                   .SetClientId("resource_server_mongo")
                   .SetClientSecret(secret);

        // Register the System.Net.Http integration.
        options.UseSystemNetHttp();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();

    });
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // Add other serialization options here as needed
});
// Register the logger as a singleton
builder.Services.AddSingleton<Serilog.ILogger>(log);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Demo API",
        Description = "An ASP.NET Core Web API That uses MongoDb",
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddScoped<IApplicationDA, ApplicationDA>();
var app = builder.Build();
app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:7197"));
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles(); // Enables default file mapping on the web root.
app.UseStaticFiles(); // Marks files in web root as servable.

app.MapControllers();

app.Run();
