namespace SolidFullstackTemplate.Domain.Entities;

public class Dish
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int? KiloCalories { get; set; }

    public int RestaurantId { get; set; }
}
