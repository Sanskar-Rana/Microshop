using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microshop.SharedLibrary.DependencyInjection;

public static class HealthCheckContainer
{
    public static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration configuration, string url)
    {
        services.Configure<MemoryCheckOptions>("Feedback Service Memory Check", opts =>
        {
            opts.Threshold = 1_000_000_000;
        });
        Console.WriteLine($"Base Url: {url}");
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection"),
                healthQuery: "SELECT 1", name: "SQL Server", failureStatus: HealthStatus.Unhealthy, tags: new[]{"Feedback", "Database"})
            .AddCheck<RemoteHealthCheck>("Remote Endpoints Health Check", failureStatus: HealthStatus.Unhealthy)
            .AddCheck<MemoryHealthCheck>("Feedback Service Memory Check", failureStatus: HealthStatus.Unhealthy, tags: new[]{"Feedback Service"})
            .AddUrlGroup(new Uri($"{url}/api/health/self"),name: "base URL", failureStatus: HealthStatus.Unhealthy);
//HealthCheck UI
        
        services.AddHealthChecksUI(opts =>
        {
            opts.SetEvaluationTimeInSeconds(30);
            opts.MaximumHistoryEntriesPerEndpoint(60);
            opts.SetApiMaxActiveRequests(1);
            opts.AddHealthCheckEndpoint("feedback", "/api/health");
        }).AddInMemoryStorage();
        

        return services;
    }

    public static IApplicationBuilder UseHealthCheckService(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/api/health", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecks("/api/health/self", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.UseHealthChecksUI(delegate(Options options)
        {
            options.UIPath = "/healthcheck-ui";
   
        });

        return app;
    }


}