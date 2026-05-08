using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Restaurants.Commands.CreateRestaurant;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Dishes.Commands.CreateDish;

public class CreateDishCommandHandler(
    ILogger<CreateRestaurantCommandHandler> logger,
    IMapper mapper,
    IRestaurantsRepository restaurantsRepository,
    IDishRepository dishRepository)
    : IRequestHandler<CreateDishCommand, int>
{
    public async Task<int> Handle(
        CreateDishCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating new dish {@Dish}", request);

        var restaurant = await restaurantsRepository.GetByIdAsync(request.RestaurantId);

        if (restaurant == null)
        {
            logger.LogWarning("Restaurant with id {RestaurantId} not found", request.RestaurantId);

            throw new NotFoundExceptions(nameof(Restaurant), request.RestaurantId.ToString());
        }
        var dish = mapper.Map<Dish>(request);

        var id = await dishRepository.Create(dish);
        // restaurant.Dishes.Add(dish);
        // await restaurantsRepository.Update(restaurant);

        return id;
    }
}
