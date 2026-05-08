using Microsoft.EntityFrameworkCore;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Repositories;
using SolidFullstackTemplate.Infrastructure.Persistance;

namespace SolidFullstackTemplate.Infrastructure.Repositories;

internal class DishRepository(AppDbContext dbContext)
    : IDishRepository
{
    public async Task<int> Create(Dish dish)
    {
        dbContext.Dishes.Add(dish);
        await dbContext.SaveChangesAsync();

        return dish.Id;
    }

    public async Task<Dish?> GetByIdForRestaurant(int dishId, int restaurantId)
    {
        var dish = await dbContext.Dishes.FirstOrDefaultAsync(
            dish => dish.Id == dishId && dish.RestaurantId == restaurantId);

        return dish;
    }
}
