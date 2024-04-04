using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using ConanExilesHelper.Games.ConanExiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;
using ConanExilesHelper.Configuration;
using ConanExilesHelper.Services;
using System.Reactive;
using ConanExilesHelper.SourceQuery.Rules;
using ConanExilesHelper.SourceQuery;
using System.Net;
using System.Threading;

namespace ConanExilesHelper.Tests.Integration;

[TestFixture]
[Explicit("Run manually")]
public class SdtdTests
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
                //serviceCollection.Configure<ConanExilesSettings>(config.GetSection("conanExilesSettings"));
            });

        _host = host.Build();
    }

    [Test]
    public async Task TestRestartAsync()
    {
        // Arrange
        var gs = new GameServer();
        var endpoint = new IPEndPoint(IPAddress.Parse("69.174.97.252"), 27010);

        // Act
        await gs.QueryAsync(endpoint, CancellationToken.None);

        // Assert
        gs.Should().Be(RestartResponse.Success);
    }
}
