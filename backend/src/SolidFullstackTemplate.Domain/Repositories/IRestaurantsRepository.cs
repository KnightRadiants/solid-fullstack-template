using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Domain.Repositories;

public interface IRestaurantsRepository
{
    Task<IEnumerable<Restaurant>> GetAllAsync();
}
