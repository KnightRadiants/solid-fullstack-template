using Microsoft.Extensions.DependencyInjection;
using SolidFullstackTemplate.Application.Restaurants;

namespace SolidFullstackTemplate.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRestaurantsService, RestaurantsService>();

        return services;
    }
}
