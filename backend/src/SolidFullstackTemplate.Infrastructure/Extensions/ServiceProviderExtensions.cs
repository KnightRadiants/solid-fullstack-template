using Microsoft.Extensions.DependencyInjection;
using SolidFullstackTemplate.Infrastructure.Seeders;

namespace SolidFullstackTemplate.Infrastructure.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task InitializeInfrastructureAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IApplicationSeeder>();
        await seeder.SeedAsync();
    }
}
