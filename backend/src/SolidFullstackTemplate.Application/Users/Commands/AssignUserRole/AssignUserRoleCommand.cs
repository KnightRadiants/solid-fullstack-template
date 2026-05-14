using MediatR;

namespace SolidFullstackTemplate.Application.Users.Commands.AssignUserRole;

public sealed record AssignUserRoleCommand(
    string UserEmail,
    string RoleName
) : IRequest;
