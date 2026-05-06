using MediatR;
using Microsoft.AspNetCore.Mvc;
using SolidFullstackTemplate.Application.Restaurants.Commands.CreateRestaurant;
using SolidFullstackTemplate.Application.Restaurants.Commands.DeleteRestaurant;
using SolidFullstackTemplate.Application.Restaurants.Commands.UpdateRestaurant;
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRestaurant([FromRoute] int id)
    {
        bool isDeleted = await mediator.Send(new DeleteRestaurantCommand(id));

        if (isDeleted)
        {
            return NoContent();
        }

        return NotFound();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update([FromBody] UpdateRestaurantCommand command, [FromRoute] int id)
    {
        var isUpdated = await mediator.Send(command with { Id = id });
        if (isUpdated)
        {
            return NoContent();
        }

        return NotFound();
    }
}
