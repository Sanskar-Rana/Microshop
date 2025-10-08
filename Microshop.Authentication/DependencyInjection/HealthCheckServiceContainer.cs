using Microshop.SharedLibrary.DependencyInjection;

namespace Microshop.Authentication.DependencyInjection;

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