using SolidFullstackTemplate.Infrastructure.Persistance;

namespace SolidFullstackTemplate.Infrastructure.Seeders;

public interface IApplicationSeeder
{
    Task SeedAsync();
}

internal class ApplicationSeeder(
    AppDbContext dbContext,
    IRoleSeeder roleSeeder,
    IUserSeeder userSeeder,
    IRestaurantSeeder restaurantSeeder) : IApplicationSeeder
{
    public async Task SeedAsync()
    {
        if (!await dbContext.Database.CanConnectAsync())
        {
            return;
        }

        await roleSeeder.SeedAsync();
        await userSeeder.SeedAsync();
        await restaurantSeeder.SeedAsync();
    }
}
