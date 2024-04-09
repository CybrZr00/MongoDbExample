using Mongotest.Data;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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
    

// Register the logger as a singleton
builder.Services.AddSingleton<Serilog.ILogger>(log);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IApplicationDA, ApplicationDA>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
