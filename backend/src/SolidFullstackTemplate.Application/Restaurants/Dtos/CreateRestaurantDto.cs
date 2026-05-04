namespace SolidFullstackTemplate.Application.Restaurants.Dtos;

public class CreateRestaurantDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public bool HasDelivery { get; set; }

    public string ? ContactEmail { get; set; }
    public string ? ContactNumber { get; set; }

    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
}
