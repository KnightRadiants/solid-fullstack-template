using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SolidFullstackTemplate.Infrastructure.Persistance;

namespace SolidFullstackTemplate.Infrastructure.Extensions;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddInfrastructureIdentityStores(this IdentityBuilder builder)
    {
        return builder.AddEntityFrameworkStores<AppDbContext>();
    }
}
