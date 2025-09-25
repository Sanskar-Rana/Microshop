using Microshop.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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