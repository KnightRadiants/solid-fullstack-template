using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolidFullstackTemplate.Domain.Repositories;
using SolidFullstackTemplate.Infrastructure.Persistance;
using SolidFullstackTemplate.Infrastructure.Repositories;
using SolidFullstackTemplate.Infrastructure.Seeders;

namespace SolidFullstackTemplate.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
                .EnableSensitiveDataLogging());

        services.AddScoped<IApplicationSeeder, ApplicationSeeder>();
        services.AddScoped<IRoleSeeder, RoleSeeder>();
        services.AddScoped<IUserSeeder, UserSeeder>();
        services.AddScoped<IRestaurantSeeder, RestaurantSeeder>();
        services.AddScoped<IRestaurantsRepository, RestaurantsRepository>();
        services.AddScoped<IDishRepository, DishRepository>();

        return services;
    }
}
