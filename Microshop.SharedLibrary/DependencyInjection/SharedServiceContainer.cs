using Microshop.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Microshop.SharedLibrary.DependencyInjection;

public static class SharedServiceContainer
{
    public static IServiceCollection AddSharedServices<T>(this IServiceCollection services, IConfiguration configuration,
        string filename) where T : DbContext
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.File(
                path: $"{filename}-.text",
                restrictedToMinimumLevel:Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-mm-dd HH:mm:ss.fff zzz [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection"),
                healthQuery: "select 1", name: "SQL Server", failureStatus: HealthStatus.Unhealthy, tags: new[]{"Feedback", "Database"})
            .AddCheck<RemoteHealthCheck>("Remote Endpoints Health Check", failureStatus: HealthStatus.Unhealthy)
            .AddCheck<MemoryHealthCheck>("Feedback Service Memory Check", failureStatus: HealthStatus.Unhealthy, tags: new[]{"Feedback Service"})
            .AddUrlGroup(new Uri("http://localhost:5095/api/health/self"),name: "base URL", failureStatus: HealthStatus.Unhealthy);
//HealthCheck UI
        
        services.AddHealthChecksUI(opts =>
        {
            opts.SetEvaluationTimeInSeconds(10);
            opts.MaximumHistoryEntriesPerEndpoint(60);
            opts.SetApiMaxActiveRequests(1);
            opts.AddHealthCheckEndpoint("feedback", "/api/health");
        }).AddInMemoryStorage();
        

        return services;
    }

    public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder builder )
    {
        //Use Global Exception
        builder.UseMiddleware<GlobalException>();
        //Register middleware to block all outside API CALLS
        //builder.UseMiddleware<ListenToOnlyApiGateway>();
       

        return builder;
    }
}