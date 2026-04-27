namespace SolidFullstackTemplate.Domain.Entities;

public class Restaurant
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public bool HasDelivery { get; set; }

    public string ? ContactEmail { get; set; }
    public string ? ContactNumber { get; set; }

    public Address? Address { get; set; }
    public List<Dish> Dishes { get; set; } = new();
}
