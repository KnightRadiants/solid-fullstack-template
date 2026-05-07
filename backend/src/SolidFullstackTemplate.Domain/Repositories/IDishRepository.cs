using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Domain.Repositories;

public interface IDishRepository
{
    Task<int> Create(Dish dish);
}
