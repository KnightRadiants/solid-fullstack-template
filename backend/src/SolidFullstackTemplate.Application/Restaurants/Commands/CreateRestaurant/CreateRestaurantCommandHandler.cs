using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandHandler(
    IRestaurantsRepository restaurantsRepository,
    ILogger<CreateRestaurantCommandHandler> logger,
    IMapper mapper) : IRequestHandler<CreateRestaurantCommand, int>
{
    public async Task<int> Handle(
        CreateRestaurantCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating new restaurant");

        var restaurant = mapper.Map<Restaurant>(command);

        var id = await restaurantsRepository.Create(restaurant);

        return id;
    }
}
