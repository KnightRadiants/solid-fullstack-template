namespace SolidFullstackTemplate.Domain.Exceptions;

public class NotFoundExceptions(string resourceType, string resourceIdentifier)
    : Exception($"The {resourceType} with id {resourceIdentifier} was not found");
