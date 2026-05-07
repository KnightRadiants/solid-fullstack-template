using MediatR;
using Microsoft.AspNetCore.Mvc;
using SolidFullstackTemplate.Application.Restaurants.Commands.CreateRestaurant;
using SolidFullstackTemplate.Application.Restaurants.Commands.DeleteRestaurant;
using SolidFullstackTemplate.Application.Restaurants.Commands.UpdateRestaurant;
using SolidFullstackTemplate.Application.Restaurants.Dtos;
using SolidFullstackTemplate.Application.Restaurants.Queries.GetAllRestaurants;
using SolidFullstackTemplate.Application.Restaurants.Queries.GetRestaurantById;

namespace SolidFullstackTemplate.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Description = "All restaurants found")]
    public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetAll()
    {
        var restaurants = await mediator.Send(new GetAllRestaurantsQuery());

        return Ok(restaurants);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Description = "Restaurant found")]
    public async Task<ActionResult<RestaurantDto>> GetById([FromRoute] int id)
    {
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));

        return Ok(restaurant);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Description = "Restaurant created successfully")]
    public async Task<IActionResult> CreateRestaurant([FromBody] CreateRestaurantCommand command)
    {
        int id = await mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Restaurant deleted successfully")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Description = "Restaurant not found")]

    public async Task<IActionResult> DeleteRestaurant([FromRoute] int id)
    {
        await mediator.Send(new DeleteRestaurantCommand(id));

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Restaurant updated successfully")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Description = "Restaurant not found")]
    public async Task<IActionResult> Update([FromBody] UpdateRestaurantCommand command, [FromRoute] int id)
    {
        await mediator.Send(command with { Id = id });

        return NoContent();
    }
}
