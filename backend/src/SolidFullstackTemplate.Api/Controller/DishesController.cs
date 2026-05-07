using MediatR;
using Microsoft.AspNetCore.Mvc;
using SolidFullstackTemplate.Application.Dishes.Commands.CreateDish;

namespace SolidFullstackTemplate.Api.Controller;

[ApiController]
[Route("api/restaurants/{restaurantId:int}/dishes")]
public class DishesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromRoute] int restaurantId, [FromBody] CreateDishCommand command)
    {
        await mediator.Send(command with { RestaurantId = restaurantId });

        return Created();
    }
}
