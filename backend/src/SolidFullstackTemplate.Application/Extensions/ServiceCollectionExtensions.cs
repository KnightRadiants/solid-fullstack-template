using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SolidFullstackTemplate.Application.Restaurants;

namespace SolidFullstackTemplate.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;

        services.AddScoped<IRestaurantsService, RestaurantsService>();
        services.AddAutoMapper(cfg => { }, applicationAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
