namespace SolidFullstackTemplate.Domain.Constants;

public static class UserRoles
{
    public const string Admin = nameof(Admin);
    public const string Owner = nameof(Owner);
    public const string User = nameof(User);

    public static readonly string[] All = [Admin, Owner, User];
}
