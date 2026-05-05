using MediatR;
using SolidFullstackTemplate.Application.Restaurants.Dtos;

namespace SolidFullstackTemplate.Application.Restaurants.Queries.GetAllRestaurants;

public sealed record GetAllRestaurantsQuery : IRequest<IEnumerable<RestaurantDto>>;
