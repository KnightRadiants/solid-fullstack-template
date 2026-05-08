using MediatR;
using SolidFullstackTemplate.Application.Dishes.Dtos;

namespace SolidFullstackTemplate.Application.Dishes.Queries.GetDishByIdForRestaurant;

public sealed record GetDishByIdForRestaurantQuery(int RestaurantId, int DishId) : IRequest<DishDto>;
