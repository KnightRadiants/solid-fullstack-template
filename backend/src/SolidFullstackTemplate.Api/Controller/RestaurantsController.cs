using Microsoft.AspNetCore.Mvc;
using SolidFullstackTemplate.Application.Restaurants;

namespace SolidFullstackTemplate.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController(IRestaurantsService restaurantsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var restaurants = await restaurantsService.GetAllRestaurants();

        return Ok(restaurants);
    }
}
