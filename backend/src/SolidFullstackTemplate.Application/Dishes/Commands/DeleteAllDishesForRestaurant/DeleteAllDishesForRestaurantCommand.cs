using MediatR;

namespace SolidFullstackTemplate.Application.Dishes.Commands.DeleteAllDishesForRestaurant;

public sealed record DeleteAllDishesForRestaurantCommand(int RestaurantId) : IRequest;
