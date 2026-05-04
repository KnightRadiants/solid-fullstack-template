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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRestaurantSeeder, RestaurantSeeder>();
        services.AddScoped<IRestaurantsRepository, RestaurantsRepository>();

        return services;
    }
}
