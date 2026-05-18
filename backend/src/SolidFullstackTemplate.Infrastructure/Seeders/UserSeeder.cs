using Microsoft.AspNetCore.Identity;
using SolidFullstackTemplate.Domain.Constants;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Infrastructure.Seeders;

public interface IUserSeeder
{
    Task SeedAsync();
}

internal class UserSeeder(UserManager<User> userManager) : IUserSeeder
{
    public async Task SeedAsync()
    {
        foreach (var roleName in UserRoles.All)
        {
            var email = SeedUserDefaults.GetEmail(roleName);
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, SeedUserDefaults.Password);
                SeedIdentityResult.EnsureSucceeded(createResult, email, "create user");
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(user, roleName);
                SeedIdentityResult.EnsureSucceeded(
                    addToRoleResult,
                    email,
                    $"assign role '{roleName}' to user");
            }
        }
    }
}
