using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Restaurants;

public interface IRestaurantsService
{
    Task<IEnumerable<Restaurant>> GetAllRestaurants();

    Task<Restaurant?> GetRestaurantById(int id);
}
