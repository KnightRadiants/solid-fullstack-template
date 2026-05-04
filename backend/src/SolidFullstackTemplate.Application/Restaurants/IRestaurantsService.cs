using SolidFullstackTemplate.Application.Restaurants.Dtos;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Restaurants;

public interface IRestaurantsService
{
    Task<IEnumerable<RestaurantDto>> GetAllRestaurants();

    Task<RestaurantDto?> GetRestaurantById(int id);
    Task<int> Create(CreateRestaurantDto dto);
}
