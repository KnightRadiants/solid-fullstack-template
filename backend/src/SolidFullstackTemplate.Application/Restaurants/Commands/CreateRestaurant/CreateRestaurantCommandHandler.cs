using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Users;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandHandler(
    IRestaurantsRepository restaurantsRepository,
    ILogger<CreateRestaurantCommandHandler> logger,
    IMapper mapper,
    IUserContext userContext) : IRequestHandler<CreateRestaurantCommand, int>
{
    public async Task<int> Handle(
        CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Creating new restaurant {@Restaurant} for user: {UserEmail}",
            request, currentUser.Email);

        var restaurant = mapper.Map<Restaurant>(request);
        restaurant.OwnerId = currentUser.Id;

        var id = await restaurantsRepository.Create(restaurant);

        return id;
    }
}
