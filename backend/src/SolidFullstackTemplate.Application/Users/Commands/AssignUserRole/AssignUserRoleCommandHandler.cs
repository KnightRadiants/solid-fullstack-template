using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;

namespace SolidFullstackTemplate.Application.Users.Commands.AssignUserRole;

public class AssignUserRoleCommandHandler(ILogger<AssignUserRoleCommandHandler> logger,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager
    ) : IRequestHandler<AssignUserRoleCommand>
{
    public async Task Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning role {RoleName} to user with email: {UserEmail}",
            request.RoleName, request.UserEmail);

        var user = await userManager.FindByEmailAsync(request.UserEmail);
        if (user == null)
        {
            logger.LogWarning("User with email {UserId} not found", request.UserEmail);
            throw new NotFoundExceptions(nameof(User), request.UserEmail);
        }

        var role = await roleManager.FindByNameAsync(request.RoleName);
        if (role == null)
        {
            logger.LogWarning("Role with name {RoleName} not found", request.RoleName);
            throw new NotFoundExceptions(nameof(IdentityRole), request.RoleName);
        }

        await userManager.AddToRoleAsync(user, request.RoleName);
    }
}
