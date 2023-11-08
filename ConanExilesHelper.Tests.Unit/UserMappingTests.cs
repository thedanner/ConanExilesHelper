using FluentAssertions;
using ConanExilesHelper.Models.Configuration;
using NUnit.Framework;
using System.Collections.Generic;

namespace ConanExilesHelper.Tests.Unit;

[TestFixture]
public class UserMappingTests
{
    [Test]
    public void SetSteamIds_Steam1Only_ReturnsOneSteamId()
    {
        // Arrange
        const string SteamId1 = "STEAM_1:1:2345";
        var userMapping = new UserMapping
        {
            SteamIds = new List<string> { SteamId1 }
        };

        // Act
        var resultingIds = userMapping.SteamIds;

        // Assert
        resultingIds.Should().BeEquivalentTo(new[] { SteamId1 });
    }

    [Test]
    public void SetSteamIds_Steam0Only_ReturnsOneSteamId()
    {
        // Arrange
        const string SteamId0 = "STEAM_0:1:2345";
        const string SteamId1 = "STEAM_1:1:2345";
        var userMapping = new UserMapping
        {
            SteamIds = new List<string> { SteamId0 }
        };

        // Act
        var resultingIds = userMapping.SteamIds;

        // Assert
        resultingIds.Should().BeEquivalentTo(new[] { SteamId1, SteamId0 });
    }
}
