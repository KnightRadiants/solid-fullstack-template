using MediatR;

namespace SolidFullstackTemplate.Application.Dishes.Commands.CreateDish;

public sealed record CreateDishCommand : IRequest<int>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int? KiloCalories { get; set; }

    public int RestaurantId { get; set; }
}
