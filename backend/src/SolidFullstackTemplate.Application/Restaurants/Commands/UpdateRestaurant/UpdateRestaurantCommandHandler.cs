using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Restaurants.Commands.DeleteRestaurant;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommandHandler(ILogger<DeleteRestaurantCommandHandler> logger,
    IRestaurantsRepository restaurantsRepository,
    IMapper mapper) : IRequestHandler<UpdateRestaurantCommand, bool>
{
    public async Task<bool> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Updating restaurant with id: {RestaurantId} with {@UpdateRestaurantCommand}", request.Id, request);

        var restaurant = await restaurantsRepository.GetByIdAsync(request.Id);

        if (restaurant == null)
        {
            logger.LogWarning("Restaurant with id {RestaurantId} not found", request.Id);

            return false;
        }
        mapper.Map(request, restaurant);

        await restaurantsRepository.Update(restaurant);

        return true;
    }
}
