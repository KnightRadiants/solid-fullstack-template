using Microsoft.AspNetCore.Authorization;

namespace SolidFullstackTemplate.Infrastructure.Authorization.Requirements;

public record MinimumAgeRequirement(int MinimumAge) : IAuthorizationRequirement;
