using Serilog;
using SolidFullstackTemplate.Api.Extensions;
using SolidFullstackTemplate.Application.Extensions;
using SolidFullstackTemplate.Domain.Extensions;
using SolidFullstackTemplate.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDomain()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();
await app.Services.InitializeInfrastructureAsync();
app.UseApi();

app.Run();
