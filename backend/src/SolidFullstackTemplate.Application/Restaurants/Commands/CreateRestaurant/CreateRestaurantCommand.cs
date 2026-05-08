using MediatR;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.CreateRestaurant;

public sealed record CreateRestaurantCommand : IRequest<int>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public bool HasDelivery { get; init; }

    public string ? ContactEmail { get; init; }
    public string ? ContactNumber { get; init; }

    public string? City { get; init; }
    public string? Street { get; init; }
    public string? PostalCode { get; init; }
}
