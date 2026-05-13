using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Domain.Exceptions;

namespace SolidFullstackTemplate.Application.Users.Commands.UpdateUserDetails;

public class UpdateUserDetailsCommandHandler(ILogger<UpdateUserDetailsCommandHandler> logger,
    IUserContext userContext,
    IUserStore<User> userStore)
    : IRequestHandler<UpdateUserDetailsCommand>
{
    public async Task Handle(UpdateUserDetailsCommand request, CancellationToken cancellationToken)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Updating user: {UserId}, with {@Request}", currentUser.Id, request);

        var dbUser = await userStore.FindByIdAsync(currentUser.Id, cancellationToken);
        if (dbUser == null)
        {
            logger.LogWarning("User with id {UserId} not found", currentUser.Id);
            throw new NotFoundExceptions(nameof(User), currentUser.Id.ToString());
        }

        dbUser.DateOfBirth = request.DateOfBirth;
        dbUser.Nationality = request.Nationality;

        await userStore.UpdateAsync(dbUser, cancellationToken);
        logger.LogInformation("User with id {UserId} updated successfully", currentUser.Id);
    }
}
