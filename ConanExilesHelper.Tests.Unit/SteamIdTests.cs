using FluentAssertions;
using ConanExilesHelper.TeamSuggestions;
using NUnit.Framework;

namespace ConanExilesHelper.Tests.Unit;

[TestFixture]
public class SteamIdTests
{
    [Test]
    [TestCase("STEAM_0:0:4662915")]
    [TestCase("STEAM_1:0:4662915")]
    public void FromStringId_Middle0_ReturnsCorrectEvenValue(string steamId)
    {
        // Arrange

        // Act
        var result = SteamId.FromStringId(steamId);

        // Assert
        result.Should().Be(76561197969591558);
    }

    [Test]
    [TestCase("STEAM_0:1:4662915")]
    [TestCase("STEAM_1:1:4662915")]
    public void FromStringId_Middle1_ReturnsCorrectOddValue(string steamId)
    {
        // Arrange

        // Act
        var result = SteamId.FromStringId(steamId);

        // Assert
        result.Should().Be(76561197969591559);
    }

    [Test]
    public void ToStringId_Even_ReturnsCorrectMiddle0Value()
    {
        // Arrange

        // Act
        var result = SteamId.ToStringId(76561197969591558);

        // Assert
        result.Should().Be("STEAM_1:0:4662915");
    }

    [Test]
    public void ToStringId_Odd_ReturnsCorrectMiddle1Value()
    {
        // Arrange

        // Act
        var result = SteamId.ToStringId(76561197969591559);

        // Assert
        result.Should().Be("STEAM_1:1:4662915");
    }
}
