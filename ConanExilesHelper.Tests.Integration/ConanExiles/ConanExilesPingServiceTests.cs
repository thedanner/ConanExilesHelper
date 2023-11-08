using FluentAssertions;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Games.ConanExiles;

namespace ConanExilesHelper.Tests.Integration.ConanExiles;

[TestFixture]
[Explicit("Run manually")]
public class ConanExilesPingServiceTests
{
    private ConanExilesSettings _conanExilesSettings;

    [SetUp]
    public void SetUp()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();

        _conanExilesSettings = config.GetSection("conanExiles").Get<ConanExilesSettings>();
    }

    [Test]
    public async Task TestPingAsync()
    {
        // Arrange
        var pinger = new ConanExilesPingService(
            new NullLogger<ConanExilesPingService>(),
            new PingThrottler());
        var serverEntry = _conanExilesSettings.Servers[0];

        // Act
        var response = await pinger.PingAsync(serverEntry.QueryHostname, serverEntry.QueryPort);

        // Assert
        response.Should().NotBeNull();
    }
}
