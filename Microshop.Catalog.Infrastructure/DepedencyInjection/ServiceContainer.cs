using Microshop.Catalog.Infrastructure.Data;
using Microshop.Catalog.Infrastructure.Repositories;
using Microshop.SharedLibrary.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Micrsoshop.Catalog.Application.Interfaces;

namespace Microshop.Catalog.Infrastructure.DepedencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Add Database Connectivity
        //Add Authentication Scheme
        SharedServiceContainer.AddSharedServices<AppDbContext>(services, configuration, configuration["MySerilog: FineName"]!);
        
        //Create Dependency Injection (DI)
        services.AddScoped<ICategory, CategoryRepository>();
        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        SharedServiceContainer.UseSharedPolicies(app);
        
        return app;
    }
}