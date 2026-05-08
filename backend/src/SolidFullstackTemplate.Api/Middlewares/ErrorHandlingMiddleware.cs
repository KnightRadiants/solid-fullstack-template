using SolidFullstackTemplate.Domain.Exceptions;

namespace SolidFullstackTemplate.Api.Middlewares;

internal class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundExceptions notFound)
        {
            logger.LogWarning(notFound, "Resource not found");

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync(notFound.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred while processing request");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Something went wrong");
        }
    }
}
