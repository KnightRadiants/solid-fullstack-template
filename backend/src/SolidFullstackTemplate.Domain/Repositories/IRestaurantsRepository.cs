using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Domain.Repositories;

public interface IRestaurantsRepository
{
    Task<IEnumerable<Restaurant>> GetAllAsync();
    Task<Restaurant?> GetByIdAsync(int id);
    Task<int> Create(Restaurant restaurant);
    Task Delete(Restaurant restaurant);
    Task Update(Restaurant restaurant);
}
