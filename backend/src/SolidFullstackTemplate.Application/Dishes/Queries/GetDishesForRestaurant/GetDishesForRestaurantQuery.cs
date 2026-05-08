using MediatR;
using SolidFullstackTemplate.Application.Dishes.Dtos;

namespace SolidFullstackTemplate.Application.Dishes.Queries.GetDishesForRestaurant;

public sealed record GetDishesForRestaurantQuery(int RestaurantId)
    : IRequest<IEnumerable<DishDto>>;
