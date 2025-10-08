using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Core;

namespace Microshop.SharedLibrary.DependencyInjection;

public class RemoteHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RemoteHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory =  httpClientFactory;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        using (var httpClient = _httpClientFactory.CreateClient())
        {
            var respone = await httpClient.GetAsync("api/health");
            if (respone.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Remote endpoints is healthy");
            }
            return HealthCheckResult.Unhealthy("Remote endpoints is unhealthy");
        }
    }
}