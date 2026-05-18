namespace SolidFullstackTemplate.Infrastructure.Seeders;

internal static class SeedUserDefaults
{
    public const string Password = "Password123!";

    public static string GetEmail(string roleName) =>
        $"{roleName.ToLowerInvariant()}@solidfullstack.local";
}
