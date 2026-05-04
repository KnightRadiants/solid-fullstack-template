using SolidFullstackTemplate.Application.Extensions;
using SolidFullstackTemplate.Infrastructure.Extensions;
using SolidFullstackTemplate.Infrastructure.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenApi()
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddControllers();

var app = builder.Build();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IRestaurantSeeder>();
await seeder.SeedAsync();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
