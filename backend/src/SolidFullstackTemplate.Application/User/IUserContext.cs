namespace SolidFullstackTemplate.Application.User;

public interface IUserContext
{
    CurrentUser? GetCurrentUser();
}
