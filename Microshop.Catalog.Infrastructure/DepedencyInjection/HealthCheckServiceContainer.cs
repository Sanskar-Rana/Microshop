using Microshop.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microshop.Catalog.Infrastructure.DependencyInjection;

public static class HealthCheckServiceContainer
{
    public static IServiceCollection AddHealthCheckService(this IServiceCollection services,
        IConfiguration configuration)
    {
        var url = configuration["HealthCheck:Url"];
        HealthCheckContainer.AddHealthCheck(services, configuration, url);
        
        return services;
    }

    public static IApplicationBuilder UseHealthCheckService(this IApplicationBuilder app)
    {
        HealthCheckContainer.UseHealthCheckService(app);
        return app;
    }
}