using MediatR;
using SolidFullstackTemplate.Application.Restaurants.Dtos;

namespace SolidFullstackTemplate.Application.Restaurants.Queries.GetRestaurantById;

public sealed record GetRestaurantByIdQuery(int Id) : IRequest<RestaurantDto?>;
