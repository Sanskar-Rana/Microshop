using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microshop.Catalog.Infrastructure.DepedencyInjection;

public class RemoteHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RemoteHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        using (var httpClicent = _httpClientFactory.CreateClient())
        {
            var response = await httpClicent.GetAsync("http://localhost:5095/api/health/self");
            if(response.IsSuccessStatusCode)
                return HealthCheckResult.Healthy("Remote Endpoints is healthy");
            return HealthCheckResult.Unhealthy("Remote Endpoints is unhealthy");
        }
    }
}