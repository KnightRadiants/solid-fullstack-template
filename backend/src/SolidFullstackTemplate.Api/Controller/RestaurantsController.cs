using MediatR;
using Microsoft.AspNetCore.Mvc;
using SolidFullstackTemplate.Application.Restaurants.Commands.CreateRestaurant;
using SolidFullstackTemplate.Application.Restaurants.Queries.GetAllRestaurants;
using SolidFullstackTemplate.Application.Restaurants.Queries.GetRestaurantById;

namespace SolidFullstackTemplate.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var restaurants = await mediator.Send(new GetAllRestaurantsQuery());

        return Ok(restaurants);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));

        if (restaurant == null)
        {
            return NotFound();
        }

        return Ok(restaurant);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantCommand command)
    {
        int id = await mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}
