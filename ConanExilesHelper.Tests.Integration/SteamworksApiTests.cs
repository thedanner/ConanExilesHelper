using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using NUnit.Framework;
using ConanExilesHelper.Services.Steamworks;

namespace ConanExilesHelper.Tests.Integration;

[TestFixture]
[Explicit("Run manually")]
public class SteamworksApiTests
{
    [Test]
    public async Task TestRestartAsync()
    {
        // Arrange
        var api = new SteamworksApi(new HttpClient(), new NullLogger<SteamworksApi>());
        var ids = "1823412793,880454836,877108545,1369743238,2723987721,1797359985,2050780234,1966733568,1815573406,1928978003,1696888680,1159180273,2411388528,2300463941"
            .Split(",").Select(long.Parse).ToList();

        // Act
        var response = await api.GetPublishedFileDetailsAsync(ids, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
    }
}
