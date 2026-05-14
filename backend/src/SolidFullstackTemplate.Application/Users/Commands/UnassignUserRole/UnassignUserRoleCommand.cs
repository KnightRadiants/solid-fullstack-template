using MediatR;

namespace SolidFullstackTemplate.Application.Users.Commands.UnassignUserRole;

public record UnassignUserRoleCommand(
    string UserEmail,
    string RoleName
) : IRequest;
