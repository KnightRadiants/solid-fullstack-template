using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SolidFullstackTemplate.Application.Extensions;
using SolidFullstackTemplate.Infrastructure.Extensions;
using SolidFullstackTemplate.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddFluentValidationAutoValidation()
    .AddSwaggerGen()
    .AddControllers();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


var app = builder.Build();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IRestaurantSeeder>();
await seeder.SeedAsync();

app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();
app.MapControllers();

app.Run();
