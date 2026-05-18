using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Users;
using SolidFullstackTemplate.Domain.Constants;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;
using SolidFullstackTemplate.Domain.Interfaces;
using SolidFullstackTemplate.Infrastructure.Persistance;

namespace SolidFullstackTemplate.Infrastructure.Authorization.Services;

internal class RestaurantAuthorizationService(
    ILogger<RestaurantAuthorizationService> logger,
    IUserContext userContext,
    AppDbContext dbContext) : IRestaurantAuthorizationService
{
    public async Task EnsureAuthorizedAsync(
        int restaurantId,
        ResourceOperation resourceOperation,
        CancellationToken cancellationToken = default)
    {
        var restaurant = await dbContext.Restaurants
            .AsNoTracking()
            .Where(r => r.Id == restaurantId)
            .Select(r => new RestaurantAuthorizationData(r.Name, r.OwnerId))
            .FirstOrDefaultAsync(cancellationToken);

        if (restaurant is null)
        {
            logger.LogWarning("Restaurant with id {RestaurantId} not found", restaurantId);

            throw new NotFoundExceptions(nameof(Restaurant), restaurantId.ToString());
        }

        if (!IsAuthorized(restaurant.Name, restaurant.OwnerId, resourceOperation))
        {
            throw new ForbidException(
                $"User is not authorized to {resourceOperation.ToString().ToLowerInvariant()} restaurant with id {restaurantId}");
        }
    }

    public void EnsureAuthorized(Restaurant restaurant, ResourceOperation resourceOperation)
    {
        if (IsAuthorized(restaurant.Name, restaurant.OwnerId, resourceOperation))
        {
            return;
        }

        throw new ForbidException(
            $"User is not authorized to {resourceOperation.ToString().ToLowerInvariant()} restaurant with id {restaurant.Id}");
    }

    private bool IsAuthorized(string restaurantName, string ownerId, ResourceOperation resourceOperation)
    {
        var user = userContext.GetCurrentUser();

        logger.LogInformation("Authorizing user {UserEmail}, to {Operation} for restaurant {RestaurantName}",
            user.Email, resourceOperation, restaurantName);

        if (resourceOperation is ResourceOperation.Read or ResourceOperation.Create)
        {
            logger.LogInformation("Create/read operation - successful authorization");

            return true;
        }

        if (resourceOperation is ResourceOperation.Update or ResourceOperation.Delete &&
            user.IsInRole(UserRoles.Admin))
        {
            logger.LogInformation("Admin user, {Operation} operation - successful authorization", resourceOperation);

            return true;
        }

        if (resourceOperation is ResourceOperation.Update or ResourceOperation.Delete &&
            user.Id == ownerId)
        {
            logger.LogInformation("Owner user, {Operation} operation - successful authorization", resourceOperation);

            return true;
        }

        return false;
    }

    private sealed record RestaurantAuthorizationData(string Name, string OwnerId);
}
