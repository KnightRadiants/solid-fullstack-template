using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Users;

namespace SolidFullstackTemplate.Infrastructure.Authorization.Requirements;

public class MinimumAgeRequirementHandler(
    ILogger<MinimumAgeRequirementHandler> logger,
    IUserContext userContext)
        : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Checking if user with id: {UserId} is of minimum age: {MinimumAge}",
            currentUser.Id, requirement.MinimumAge);

        if (currentUser.DateOfBirth is null)
        {
            logger.LogWarning("User with id: {UserId} has no date of birth", currentUser.Id);
            context.Fail();

            return Task.CompletedTask;
        }

        if (currentUser.DateOfBirth.Value.AddYears(requirement.MinimumAge) <= DateOnly.FromDateTime(DateTime.UtcNow))
        {
            logger.LogInformation("Authorization granted for user with id: {UserId}", currentUser.Id);
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
