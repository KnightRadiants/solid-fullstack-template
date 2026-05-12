using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SolidFullstackTemplate.Api.Middlewares;
using SolidFullstackTemplate.Domain.Entities;
using SolidFullstackTemplate.Infrastructure.Extensions;

namespace SolidFullstackTemplate.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .Configure<SlowRequestLoggingOptions>(
                configuration.GetSection("SlowRequestLogging"))
            .AddScoped<ErrorHandlingMiddleware>()
            .AddScoped<SlowRequestLoggingMiddleware>();

        services.AddIdentityApiEndpoints<User>()
            .AddInfrastructureIdentityStores();
        services.AddControllers();

        return services;
    }
}
