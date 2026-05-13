using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SolidFullstackTemplate.Api.ApiUsers;
using SolidFullstackTemplate.Api.Middlewares;
using SolidFullstackTemplate.Application.Users;
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
            .AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("bearerAuth", document),
                        []
                    }
                });
            })
            .Configure<SlowRequestLoggingOptions>(
                configuration.GetSection("SlowRequestLogging"))
            .AddScoped<ErrorHandlingMiddleware>()
            .AddScoped<SlowRequestLoggingMiddleware>();

        services.AddIdentityApiEndpoints<User>()
            .AddRoles<IdentityRole>()
            .AddInfrastructureIdentityStores();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddControllers();

        return services;
    }
}
