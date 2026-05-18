using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolidFullstackTemplate.Domain.Constants;
using SolidFullstackTemplate.Infrastructure.Persistance;

namespace SolidFullstackTemplate.Infrastructure.Seeders;

public interface IRoleSeeder
{
    Task SeedAsync();
}

internal class RoleSeeder(
    AppDbContext dbContext,
    RoleManager<IdentityRole> roleManager,
    ILookupNormalizer lookupNormalizer) : IRoleSeeder
{
    public async Task SeedAsync()
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
                SeedIdentityResult.EnsureSucceeded(updateResult, roleName, "update role");
                continue;
            }

            var createResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            SeedIdentityResult.EnsureSucceeded(createResult, roleName, "create role");
        }
    }
}
