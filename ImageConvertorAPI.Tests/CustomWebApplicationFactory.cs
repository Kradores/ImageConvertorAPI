using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;

namespace ImageConvertorAPI.Tests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // remove existing rate limiter
            var descriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(RateLimiterOptions));

            if (descriptor != null)
                services.Remove(descriptor);

            // or just skip middleware via environment
        });

        builder.UseEnvironment("Testing");
    }
}