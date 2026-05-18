using Microsoft.AspNetCore.Identity;

namespace SolidFullstackTemplate.Infrastructure.Seeders;

internal static class SeedIdentityResult
{
    public static void EnsureSucceeded(
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
}
