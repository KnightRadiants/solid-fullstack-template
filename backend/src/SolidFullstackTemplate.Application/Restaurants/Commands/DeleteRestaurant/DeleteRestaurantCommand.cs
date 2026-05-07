using MediatR;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.DeleteRestaurant;

public sealed record DeleteRestaurantCommand(int Id) : IRequest;
