using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SolidFullstackTemplate.Application.User;

namespace SolidFullstackTemplate.Api.User;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public CurrentUser? GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user is null)
            throw new InvalidOperationException("User not found in HttpContext");

        if (user.Identity is null || !user.Identity.IsAuthenticated)
            return null;

        var userId = user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
        var email = user.FindFirst(c => c.Type == ClaimTypes.Email)!.Value;
        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray();

        return new CurrentUser(userId, email, roles);
    }
}
