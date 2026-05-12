using Serilog;
using SolidFullstackTemplate.Api.Middlewares;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<SlowRequestLoggingMiddleware>();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGroup("api/identity").MapIdentityApi<User>();
        app.MapControllers();

        return app;
    }
}
