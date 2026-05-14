using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolidFullstackTemplate.Application.Users.Commands.AssignUserRole;
using SolidFullstackTemplate.Application.Users.Commands.UnassignUserRole;
using SolidFullstackTemplate.Application.Users.Commands.UpdateUserDetails;
using SolidFullstackTemplate.Domain.Constants;

namespace SolidFullstackTemplate.Api.Controller;

[ApiController]
[Route("api/identity")]
public class IdentityController(IMediator mediator) : ControllerBase
{
    [Authorize]
    [HttpPatch("user")]
    public async Task<IActionResult> UpdateUserDetails(UpdateUserDetailsCommand command)
    {
        await mediator.Send(command);

        return NoContent();
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPatch("userRole")]
    public async Task<IActionResult> UpdateUserDetails(AssignUserRoleCommand command)
    {
        await mediator.Send(command);

        return NoContent();
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpDelete("userRole")]
    public async Task<IActionResult> UpdateUserDetails(UnassignUserRoleCommand command)
    {
        await mediator.Send(command);

        return NoContent();
    }
}
