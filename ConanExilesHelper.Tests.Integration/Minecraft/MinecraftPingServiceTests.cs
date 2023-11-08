using FluentAssertions;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using ConanExilesHelper.Games.Minecraft;
using ConanExilesHelper.Helpers;

namespace ConanExilesHelper.Tests.Integration.Minecraft;

[TestFixture]
[Explicit("Run manually")]
public class ConanExilesPingServiceTests
{
    private MinecraftSettings _minecraftSettings;

    [SetUp]
    public void SetUp()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();

        _minecraftSettings = config.GetSection("minecraft").Get<MinecraftSettings>();
    }

    [Test]
    public async Task TestPingAsync()
    {
        // Arrange
        var pinger = new MinecraftPingService(
            new NullLogger<MinecraftPingService>(),
            new PingThrottler());
        var serverEntry = _minecraftSettings.Servers[0];

        // Act
        var response = await pinger.PingAsync(serverEntry.Hostname, serverEntry.Port);

        // Assert
        response.Should().NotBeNull();
    }
}
