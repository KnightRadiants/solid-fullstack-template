using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Dishes.Commands.DeleteAllDishesForRestaurant;

public class DeleteAllDishesForRestaurantCommandHandler(
    ILogger<DeleteAllDishesForRestaurantCommandHandler> logger,
    IRestaurantsRepository restaurantsRepository,
    IDishRepository dishRepository)
    : IRequestHandler<DeleteAllDishesForRestaurantCommand>
{
    public async Task Handle(DeleteAllDishesForRestaurantCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting all dishes for restaurant with id: {RestaurantId}", request.RestaurantId);

        var restaurantExists = await restaurantsRepository.ExistsAsync(request.RestaurantId, cancellationToken);
        if (!restaurantExists)
        {
            logger.LogWarning("Restaurant with id {RestaurantId} not found", request.RestaurantId);
            throw new NotFoundExceptions(nameof(Restaurant), request.RestaurantId.ToString());
        }

        var removedRecords = await dishRepository.DeleteAllDishesForRestaurant(
            request.RestaurantId, cancellationToken);

        logger.LogInformation(
            "Deleted {RemovedRecords} dishes for restaurant with id: {RestaurantId}",
            removedRecords,
            request.RestaurantId);
    }
}
