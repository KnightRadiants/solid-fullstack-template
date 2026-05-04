using AutoMapper;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Restaurants.Dtos;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Restaurants;

internal class RestaurantsService(IRestaurantsRepository restaurantsRepository,
    ILogger<RestaurantsService> logger,
    IMapper mapper) : IRestaurantsService
{
    public async Task<IEnumerable<RestaurantDto>> GetAllRestaurants()
    {
        logger.LogInformation("Getting all restaurants");
        var restaurants = await restaurantsRepository.GetAllAsync();
        var restaurantsDto = mapper.Map<IEnumerable<RestaurantDto>>(restaurants);

        return restaurantsDto!;
    }

    public async Task<RestaurantDto?> GetRestaurantById(int id)
    {
        logger.LogInformation("Getting restaurant with id: {Id}", id);
        var restaurant = await restaurantsRepository.GetByIdAsync(id);
        var restaurantDto = mapper.Map<RestaurantDto>(restaurant);

        return restaurantDto;
    }
}
