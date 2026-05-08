namespace SolidFullstackTemplate.Domain.Exceptions;

public class NotFoundExceptions : Exception
{
    public NotFoundExceptions(string resourceType, string resourceIdentifier) : base($"The {resourceType} with id {resourceIdentifier} was not found")
    {
    }

    public NotFoundExceptions(string message) : base(message)
    {
    }
}
