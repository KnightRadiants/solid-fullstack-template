using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Dishes.Dtos;

public class DishDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int? KiloCalories { get; set; }
}
