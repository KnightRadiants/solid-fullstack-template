using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Dishes.Dtos;
using SolidFullstackTemplate.Application.Restaurants.Queries.GetRestaurantById;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Dishes.Queries.GetDishByIdForRestaurant;

public class GetDishByIdForRestaurantQueryHandler(ILogger<GetRestaurantByIdQueryHandler> logger,
    IDishRepository dishRepository,
    IMapper mapper)
    : IRequestHandler<GetDishByIdForRestaurantQuery, DishDto>
{
    public async Task<DishDto> Handle(
        GetDishByIdForRestaurantQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving dish: {DishId}, for restaurant with id: {RestaurantId}",
            request.DishId, request.RestaurantId);

        var dish = await dishRepository
            .GetByIdForRestaurant(request.DishId, request.RestaurantId)
                ?? throw new NotFoundExceptions(
                    $"Dish with id: {request.DishId} not found for restaurant with id: {request.RestaurantId}");

        var result = mapper.Map<DishDto>(dish);

        return result;
    }
}
