using Serilog;
using SolidFullstackTemplate.Api.Extensions;
using SolidFullstackTemplate.Api.Middlewares;
using SolidFullstackTemplate.Application.Extensions;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Infrastructure.Extensions;
using SolidFullstackTemplate.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IRestaurantSeeder>();
await seeder.SeedAsync();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<SlowRequestLoggingMiddleware>();
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGroup("api/identity").MapIdentityApi<User>();
app.MapControllers();

app.Run();
