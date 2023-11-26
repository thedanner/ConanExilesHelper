using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using NUnit.Framework;
using ConanExilesHelper.Services.Steamworks;
using ConanExilesHelper.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ConanExilesHelper.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace ConanExilesHelper.Tests.Integration;

[TestFixture]
[Explicit("Run manually")]
public class ModUpdatesTests
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
                serviceCollection.AddTransient<IConanServerUtils, ConanServerUtils>();

                var config = hostContext.Configuration!;
                serviceCollection.Configure<ConanExilesSettings>(config.GetSection("conanExilesSettings"));
            });

        _host = host.Build();
    }

    [Test]
    public async Task SteamworksApiAsync()
    {
        // Arrange
        var api = new SteamworksApi(new HttpClient(), new NullLogger<SteamworksApi>());
        var ids = "1823412793,880454836,877108545,1369743238,2723987721,1797359985,2050780234,1966733568,1815573406,1928978003,1696888680,1159180273,2411388528,2300463941"
            .Split(",").Select(long.Parse).ToList();

        // Act
        var response = await api.GetPublishedFileDetailsAsync(ids, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Response.Should().NotBeNull();
        response.Response.Result.Should().Be(1);
        response.Response.ResultCount.Should().Be(ids.Count);
    }

    [Test]
    public void CheckWorkshopAddonVersionsTask()
    {
        // Arrange
        var options = _host.Services.GetRequiredService<IOptions<ConanExilesSettings>>();
        var serverUtils = new ConanServerUtils(new NullLogger<ConanServerUtils>(), options);
        var modIds = serverUtils.GetWorkshopAddonIds().ToList();

        // Act
        var mods = serverUtils.GetWorkshopModsLastUpdated();

        // Assert
        mods.Count.Should().Be(modIds.Count);
    }

    [Test]
    public async Task CompareVersionsAsync()
    {
        // Arrange
        var options = _host.Services.GetRequiredService<IOptions<ConanExilesSettings>>();
        var serverUtils = new ConanServerUtils(new NullLogger<ConanServerUtils>(), options);
        var api = new SteamworksApi(new HttpClient(), new NullLogger<SteamworksApi>());

        // Act
        var modIds = serverUtils.GetWorkshopAddonIds().ToList();
        var localModsUpdated = serverUtils.GetWorkshopModsLastUpdated();
        var workshopModsResponse = await api.GetPublishedFileDetailsAsync(modIds, CancellationToken.None);
        var workshopModsUpdated = workshopModsResponse.Response.PublishedFileDetails.ToDictionary(d => d.PublishedFileId, d => d.TimeUpdated);

        foreach (var mod in localModsUpdated.Keys)
        {
            var localTime = localModsUpdated[mod];
            var workshopTime = workshopModsUpdated[mod];
            var areTheSame = localTime == workshopTime;
            Debug.WriteLine($"{mod}: {localTime} / {workshopTime}, same? {areTheSame}");

            areTheSame.Should().BeTrue("mod {0} doesn't seem to match", mod);
        }
    }
}
