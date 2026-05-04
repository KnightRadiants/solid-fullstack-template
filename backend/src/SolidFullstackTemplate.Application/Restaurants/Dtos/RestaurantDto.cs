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

    public static RestaurantDto? FromEntity(Restaurant? restaurant)
    {
        if (restaurant is null)
            return null;

        return new RestaurantDto
        {
            Category = restaurant.Category,
            Description = restaurant.Description,
            Id = restaurant.Id,
            HasDelivery = restaurant.HasDelivery,
            Name = restaurant.Name,
            City = restaurant.Address?.City,
            Street = restaurant.Address?.Street,
            PostalCode = restaurant.Address?.PostalCode,
            Dishes = restaurant.Dishes.Select(DishDto.FromEntity).ToList()!
        };
    }
}
