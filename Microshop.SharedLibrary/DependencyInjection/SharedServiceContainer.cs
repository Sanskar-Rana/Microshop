using Microshop.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Microshop.SharedLibrary.DependencyInjection;

public static class SharedServiceContainer
{
    public static IServiceCollection AddSharedServices<T>(this IServiceCollection services, IConfiguration configuration,
        string filename) where T : DbContext
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var elasticUri = "http://localhost:9200";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.File(
                path: $"{filename}-.log",
                restrictedToMinimumLevel:Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"myapp-log-{environment.ToLower()}-{DateTime.UtcNow:yyyy.MM.dd}",
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                NumberOfShards = 1,
                NumberOfReplicas = 1
                
            })
            
            .CreateLogger();

        
       

        return services;
    }

    public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder builder )
    {
        //Use Global Exception
        builder.UseMiddleware<GlobalException>();
        Log.Information("Hello from Serilog with ElasticSearch!");
        //Register middleware to block all outside API CALLS
        //builder.UseMiddleware<ListenToOnlyApiGateway>();
       

        return builder;
    }
}