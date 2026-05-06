using MediatR;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.UpdateRestaurant;

public sealed record UpdateRestaurantCommand(
    int Id,
    string Name,
    string Description,
    bool HasDelivery) : IRequest<bool>;
