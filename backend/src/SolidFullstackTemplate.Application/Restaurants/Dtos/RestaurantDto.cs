using SolidFullstackTemplate.Application.Dishes.Dtos;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Application.Restaurants.Dtos;

public class RestaurantDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public bool HasDelivery { get; set; }

    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }

    public List<DishDto> Dishes { get; set; } = [];
}
