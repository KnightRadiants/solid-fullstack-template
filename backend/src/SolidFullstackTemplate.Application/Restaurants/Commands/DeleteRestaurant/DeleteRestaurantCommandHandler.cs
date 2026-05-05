using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.DeleteRestaurant;

public class DeleteRestaurantCommandHandler(ILogger<DeleteRestaurantCommandHandler> logger,
    IRestaurantsRepository restaurantsRepository) : IRequestHandler<DeleteRestaurantCommand, bool>
{
    public async Task<bool> Handle(DeleteRestaurantCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting restaurant with id: {RestaurantId}", request.Id);
        var restaurant = await restaurantsRepository.GetByIdAsync(request.Id);
        if (restaurant == null)
        {
            logger.LogWarning("Restaurant with id {RestaurantId} not found", request.Id);

            return false;
        }
        await restaurantsRepository.Delete(restaurant);

        return true;
    }
}
