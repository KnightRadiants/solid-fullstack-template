using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolidFullstackTemplate.Domain.Constants;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Infrastructure.Persistance;

namespace SolidFullstackTemplate.Infrastructure.Seeders;

internal class RestaurantSeeder(
    AppDbContext dbContext,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    ILookupNormalizer lookupNormalizer) : IRestaurantSeeder
{
    private const string SeedUserPassword = "Password123!";

    public async Task SeedAsync()
    {
        if (await dbContext.Database.CanConnectAsync())
        {
            await SeedRolesAsync();
            await SeedUsersAsync();

            if (!await dbContext.Restaurants.AnyAsync())
            {
                var adminUser = await userManager.FindByEmailAsync(GetSeedUserEmail(UserRoles.Admin));
                if (adminUser is null)
                {
                    throw new InvalidOperationException("Admin seed user was not found.");
                }

                var restaurants = GetRestaurants(adminUser.Id);
                dbContext.Restaurants.AddRange(restaurants);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in UserRoles.All)
        {
            var normalizedRoleName = lookupNormalizer.NormalizeName(roleName);

            var existingRole = await dbContext.Roles
                .FirstOrDefaultAsync(role =>
                    role.Name == roleName || role.NormalizedName == normalizedRoleName);

            if (existingRole is not null)
            {
                if (existingRole.Name == roleName &&
                    existingRole.NormalizedName == normalizedRoleName)
                {
                    continue;
                }

                existingRole.Name = roleName;
                existingRole.NormalizedName = normalizedRoleName;

                var updateResult = await roleManager.UpdateAsync(existingRole);
                EnsureRoleOperationSucceeded(updateResult, roleName, "update");
                continue;
            }

            var createResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            EnsureRoleOperationSucceeded(createResult, roleName, "create");
        }
    }

    private async Task SeedUsersAsync()
    {
        foreach (var roleName in UserRoles.All)
        {
            var email = GetSeedUserEmail(roleName);
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, SeedUserPassword);
                EnsureIdentityOperationSucceeded(createResult, email, "create user");
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(user, roleName);
                EnsureIdentityOperationSucceeded(addToRoleResult, email, $"assign role '{roleName}' to user");
            }
        }
    }

    private static string GetSeedUserEmail(string roleName) =>
        $"{roleName.ToLowerInvariant()}@solidfullstack.local";

    private static void EnsureRoleOperationSucceeded(
        IdentityResult result,
        string roleName,
        string operation)
    {
        EnsureIdentityOperationSucceeded(result, roleName, $"{operation} role");
    }

    private static void EnsureIdentityOperationSucceeded(
        IdentityResult result,
        string subject,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error =>
            $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException(
            $"Failed to {operation} '{subject}': {errors}");
    }

    private IEnumerable<Restaurant> GetRestaurants(string ownerId)
    {
        List<Restaurant> restaurants = [
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
                    new ()
                    {
                        Name = "Nashville Hot Chicken",
                        Description = "Nashville Hot Chicken (10 pcs.)",
                        Price = 10.30M,
                    },

                    new ()
                    {
                        Name = "Chicken Nuggets",
                        Description = "Chicken Nuggets (5 pcs.)",
                        Price = 5.30M,
                    },
                ],
                Address = new ()
                {
                    City = "London",
                    Street = "Cork St 5",
                    PostalCode = "WC2N 5DU"
                },

            },
            new ()
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
