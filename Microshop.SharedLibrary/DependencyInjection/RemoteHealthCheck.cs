using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Core;

namespace Microshop.SharedLibrary.DependencyInjection;

public class RemoteHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RemoteHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory =  httpClientFactory;
        _configuration = configuration;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var url = _configuration["HealthCheck:Url"];
        using (var httpClient = _httpClientFactory.CreateClient())
        {
            var respone = await httpClient.GetAsync($"https://www.google.com/");
            if (respone.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Remote endpoints is healthy");
            }
            return HealthCheckResult.Unhealthy("Remote endpoints is unhealthy");
        }
    }
}