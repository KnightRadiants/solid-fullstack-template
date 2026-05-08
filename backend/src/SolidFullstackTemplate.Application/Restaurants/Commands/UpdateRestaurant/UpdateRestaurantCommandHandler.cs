using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Application.Restaurants.Commands.DeleteRestaurant;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;
using SolidFullstackTemplate.Domain.Repositories;

namespace SolidFullstackTemplate.Application.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommandHandler(ILogger<DeleteRestaurantCommandHandler> logger,
    IRestaurantsRepository restaurantsRepository,
    IMapper mapper) : IRequestHandler<UpdateRestaurantCommand>
{
    public async Task Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Updating restaurant with id: {RestaurantId} with {@UpdateRestaurantCommand}", request.Id, request);

        var restaurant = await restaurantsRepository.GetByIdAsync(request.Id);

        if (restaurant == null)
        {
            logger.LogWarning("Restaurant with id {RestaurantId} not found", request.Id);

            throw new NotFoundExceptions(nameof(Restaurant), request.Id.ToString());
        }
        mapper.Map(request, restaurant);

        await restaurantsRepository.Update(restaurant);
    }
}
