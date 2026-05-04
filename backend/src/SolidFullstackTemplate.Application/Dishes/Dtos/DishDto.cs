using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Dishes.Dtos;

public class DishDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int? KiloCalories { get; set; }

    public static DishDto? FromEntity(Dish? dish)
    {
        if (dish is null)
            return null;

        return new DishDto
        {
            Id = dish.Id,
            Name = dish.Name,
            Description = dish.Description,
            Price = dish.Price,
            KiloCalories = dish.KiloCalories
        };
    }
}
