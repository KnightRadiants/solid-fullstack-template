using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolidFullstackTemplate.Domain.Constants;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Infrastructure.Persistance;

namespace SolidFullstackTemplate.Infrastructure.Seeders;

public interface IRestaurantSeeder
{
    Task SeedAsync();
}

internal class RestaurantSeeder(
    AppDbContext dbContext,
    UserManager<User> userManager) : IRestaurantSeeder
{
    public async Task SeedAsync()
    {
        if (await dbContext.Restaurants.AnyAsync())
        {
            return;
        }

        var adminUser = await userManager.FindByEmailAsync(SeedUserDefaults.GetEmail(UserRoles.Admin));
        if (adminUser is null)
        {
            throw new InvalidOperationException("Admin seed user was not found.");
        }

        var restaurants = GetRestaurants(adminUser.Id);
        dbContext.Restaurants.AddRange(restaurants);
        await dbContext.SaveChangesAsync();
    }

    private IEnumerable<Restaurant> GetRestaurants(string ownerId)
    {
        List<Restaurant> restaurants =
        [
            new()
            {
                Name = "KFC",
                Category = "Fast Food",
                Description =
                    "KFC (short for Kentucky Fried Chicken) is an American fast food restaurant chain headquartered in Louisville, Kentucky, that specializes in fried chicken.",
                ContactEmail = "contact@kfc.com",
                OwnerId = ownerId,
                HasDelivery = true,
                Dishes =
                [
                    new()
                    {
                        Name = "Nashville Hot Chicken",
                        Description = "Nashville Hot Chicken (10 pcs.)",
                        Price = 10.30M,
                    },

                    new()
                    {
                        Name = "Chicken Nuggets",
                        Description = "Chicken Nuggets (5 pcs.)",
                        Price = 5.30M,
                    },
                ],
                Address = new()
                {
                    City = "London",
                    Street = "Cork St 5",
                    PostalCode = "WC2N 5DU"
                },
            },
            new()
            {
                Name = "McDonald",
                Category = "Fast Food",
                Description =
                    "McDonald's Corporation (McDonald's), incorporated on December 21, 1964, operates and franchises McDonald's restaurants.",
                ContactEmail = "contact@mcdonald.com",
                OwnerId = ownerId,
                HasDelivery = true,
                Address = new Address()
                {
                    City = "London",
                    Street = "Boots 193",
                    PostalCode = "W1F 8SR"
                }
            }
        ];

        return restaurants;
    }
}
