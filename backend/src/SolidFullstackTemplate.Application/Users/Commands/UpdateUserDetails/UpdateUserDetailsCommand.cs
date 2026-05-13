using MediatR;

namespace SolidFullstackTemplate.Application.Users.Commands.UpdateUserDetails;

public sealed record UpdateUserDetailsCommand(
    DateOnly? DateOfBirth,
    string? Nationality
) : IRequest;
