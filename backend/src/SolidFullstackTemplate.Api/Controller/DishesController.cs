using MediatR;
using Microsoft.AspNetCore.Mvc;
using SolidFullstackTemplate.Application.Dishes.Commands.CreateDish;
using SolidFullstackTemplate.Application.Dishes.Commands.DeleteAllDishesForRestaurant;
using SolidFullstackTemplate.Application.Dishes.Dtos;
using SolidFullstackTemplate.Application.Dishes.Queries.GetDishByIdForRestaurant;
using SolidFullstackTemplate.Application.Dishes.Queries.GetDishesForRestaurant;

namespace SolidFullstackTemplate.Api.Controller;

[ApiController]
[Route("api/restaurants/{restaurantId:int}/dishes")]
public class DishesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Description = "Dish created successfully")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Description = "Restaurant not found")]
    public async Task<IActionResult> Create([FromRoute] int restaurantId, [FromBody] CreateDishCommand command)
    {
        await mediator.Send(command with { RestaurantId = restaurantId });

        return Created();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Description = "All dishes found for restaurant")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Description = "Restaurant not found")]
    public async Task<ActionResult<IEnumerable<DishDto>>> GetForAllRestaurants([FromRoute] int restaurantId)
    {
        var dishes = await mediator.Send(new GetDishesForRestaurantQuery(restaurantId));

        return Ok(dishes);
    }

    [HttpGet("{dishId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Description = "Dish found")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Description = "Dish or restaurant not found")]
    public async Task<ActionResult<DishDto>> GetByIdForRestaurant([FromRoute] int restaurantId, [FromRoute] int dishId)
    {
        var dish = await mediator.Send(new GetDishByIdForRestaurantQuery(restaurantId, dishId));

        return Ok(dish);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent, Description = "Dishes deleted successfully")]
    [ProducesResponseType(StatusCodes.Status404NotFound, Description = "Restaurant not found")]
    public async Task<IActionResult> DeleteAllForRestaurant([FromRoute] int restaurantId)
    {
        await mediator.Send(new DeleteAllDishesForRestaurantCommand(restaurantId));

        return NoContent();
    }
}
