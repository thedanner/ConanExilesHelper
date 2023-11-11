using FluentAssertions;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Games.ConanExiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Linq;
using System.Runtime.Versioning;

namespace ConanExilesHelper.Tests.Integration.ConanExiles;

[TestFixture]
[Explicit("Run manually")]
public class ConanExilesTests
{
    private IHost _host;

    [SetUp]
    public void SetUp()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, serviceCollection) =>
            {
                var config = hostContext.Configuration!;
                serviceCollection.Configure<ConanExilesSettings>(config.GetSection("conanExilesSettings"));
            });

        _host = host.Build();
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public async Task TestRestartAsync()
    {
        // Arrange
        var options = _host.Services.GetRequiredService<IOptions<ConanExilesSettings>>();
        var settings = options.Value;
        var server = settings.Servers;

        IPAddress? serverIp;
        if (!IPAddress.TryParse(server.Hostname, out serverIp))
        {
            var addresslist = Dns.GetHostAddresses(server.Hostname);
            if (addresslist.Any())
            {
                serverIp = addresslist.First();
           }
        }

        var pingService = new PingService(new NullLogger<PingService>(), new CommandThrottler());
        var restartService = new RestartService(
            new NullLogger<RestartService>(),
            options,
            pingService);

        // Act
        var response = await restartService.TryRestartAsync();

        // Assert
        response.Should().BeTrue();
    }
}
