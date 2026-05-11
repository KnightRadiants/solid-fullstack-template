using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Dishes.Dtos;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Dishes.Queries.GetDishByIdForRestaurant;

public class GetDishByIdForRestaurantQueryHandler(ILogger<GetDishByIdForRestaurantQueryHandler> logger,
    IDishRepository dishRepository,
    IRestaurantsRepository restaurantsRepository,
    IMapper mapper)
    : IRequestHandler<GetDishByIdForRestaurantQuery, DishDto>
{
    public async Task<DishDto> Handle(
        GetDishByIdForRestaurantQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving dish: {DishId}, for restaurant with id: {RestaurantId}",
            request.DishId, request.RestaurantId);

        var restaurantExists = await restaurantsRepository.ExistsAsync(request.RestaurantId, cancellationToken);
        if (!restaurantExists)
        {
            logger.LogWarning("Restaurant with id {RestaurantId} not found", request.RestaurantId);
            throw new NotFoundExceptions(
                nameof(Restaurant), request.RestaurantId.ToString());
        }

        var dish = await dishRepository
            .GetByIdForRestaurant(request.DishId, request.RestaurantId);

        if (dish is null)
        {
            logger.LogWarning("Dish with id {DishId} not found for restaurant with id {RestaurantId}",
                request.DishId, request.RestaurantId);

            throw new NotFoundExceptions(
                nameof(Dish), request.DishId.ToString());
        }
        var result = mapper.Map<DishDto>(dish);

        return result;
    }
}
