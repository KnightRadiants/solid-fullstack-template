using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Domain.Constants;
using SolidFullstackTemplate.Domain.Interfaces;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Dishes.Commands.DeleteAllDishesForRestaurant;

public class DeleteAllDishesForRestaurantCommandHandler(
    ILogger<DeleteAllDishesForRestaurantCommandHandler> logger,
    IDishRepository dishRepository,
    IRestaurantAuthorizationService restaurantAuthorizationService)
    : IRequestHandler<DeleteAllDishesForRestaurantCommand>
{
    public async Task Handle(DeleteAllDishesForRestaurantCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting all dishes for restaurant with id: {RestaurantId}", request.RestaurantId);

        await restaurantAuthorizationService.EnsureAuthorizedAsync(
            request.RestaurantId,
            ResourceOperation.Update,
            cancellationToken);

        var removedRecords = await dishRepository.DeleteAllDishesForRestaurant(
            request.RestaurantId, cancellationToken);

        logger.LogInformation(
            "Deleted {RemovedRecords} dishes for restaurant with id: {RestaurantId}",
            removedRecords,
            request.RestaurantId);
    }
}
