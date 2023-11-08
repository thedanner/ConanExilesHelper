using FluentAssertions;
using ConanExilesHelper.Rcon;
using NUnit.Framework;
using System;

namespace ConanExilesHelper.Tests.Unit;

[TestFixture]
public class PrintInfoTests
{
    [Test]
    public void PrintInfoPlayerParse_ValidTestString_ReturnsCorrectResult()
    {
        // Arrange


        // Act
        var result = PrintInfoPlayer.Parse("This is a name<1><STEAMID:1:2><><3><Survivor>");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("This is a name");
        result.ClientIndex.Should().Be(1);
        result.SteamId.Should().Be("STEAMID:1:2");
        result.Unused1.Should().Be("");
        result.TeamIndex.Should().Be(3);
        result.TeamName.Should().Be("Survivor");
    }

    [Test]
    public void PrintInfoParse_ActualCampaignString_ReturnsCorrectResult()
    {
        // Arrange
        
        // Act
        var result = new PrintInfoParser().Parse(
            $"[PI] BEGIN{Environment.NewLine}" +
            $"[PI] Da Danner<2><STEAM_1:0:1234567><><2><Survivor>{Environment.NewLine}" +
            $"[PI] Louis<3><BOT><><2><Survivor>{Environment.NewLine}" +
            $"[PI] Zoey<4><BOT><><2><Survivor>{Environment.NewLine}" +
            $"[PI] Bill<5><BOT><><2><Survivor>{Environment.NewLine}" +
            "[PI] END");

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(4);

        result.Players[0].Name.Should().Be("Da Danner");
        result.Players[0].ClientIndex.Should().Be(2);
        result.Players[0].SteamId.Should().Be("STEAM_1:0:1234567");
        result.Players[0].Unused1.Should().Be("");
        result.Players[0].TeamIndex.Should().Be(2);
        result.Players[0].TeamName.Should().Be("Survivor");

        result.Players[1].Name.Should().Be("Louis");
        result.Players[1].ClientIndex.Should().Be(3);
        result.Players[1].SteamId.Should().Be("BOT");
        result.Players[1].Unused1.Should().Be("");
        result.Players[1].TeamIndex.Should().Be(2);
        result.Players[1].TeamName.Should().Be("Survivor");

        result.Players[2].Name.Should().Be("Zoey");
        result.Players[2].ClientIndex.Should().Be(4);
        result.Players[2].SteamId.Should().Be("BOT");
        result.Players[2].Unused1.Should().Be("");
        result.Players[2].TeamIndex.Should().Be(2);
        result.Players[2].TeamName.Should().Be("Survivor");

        result.Players[3].Name.Should().Be("Bill");
        result.Players[3].ClientIndex.Should().Be(5);
        result.Players[3].SteamId.Should().Be("BOT");
        result.Players[3].Unused1.Should().Be("");
        result.Players[3].TeamIndex.Should().Be(2);
        result.Players[3].TeamName.Should().Be("Survivor");
    }

    [Test]
    public void PrintInfoParse_ActualVersusString_ReturnsCorrectResult()
    {
        // Act
        var result = new PrintInfoParser().Parse(
            $"[PI] BEGIN{Environment.NewLine}" +
            $"[PI] Da Danner<2><STEAM_1:0:1234567><><3><Infected>{Environment.NewLine}" +
            $"[PI] Louis<3><BOT><><2><Survivor>{Environment.NewLine}" +
            $"[PI] Zoey<4><BOT><><2><Survivor>{Environment.NewLine}" +
            $"[PI] Bill<5><BOT><><2><Survivor>{Environment.NewLine}" +
            $"[PI] Francis<6><BOT><><2><Survivor>{Environment.NewLine}" +
            "[PI] END");

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(5);

        result.Players[0].Name.Should().Be("Da Danner");
        result.Players[0].ClientIndex.Should().Be(2);
        result.Players[0].SteamId.Should().Be("STEAM_1:0:1234567");
        result.Players[0].Unused1.Should().Be("");
        result.Players[0].TeamIndex.Should().Be(3);
        result.Players[0].TeamName.Should().Be("Infected");

        result.Players[1].Name.Should().Be("Louis");
        result.Players[1].ClientIndex.Should().Be(3);
        result.Players[1].SteamId.Should().Be("BOT");
        result.Players[1].Unused1.Should().Be("");
        result.Players[1].TeamIndex.Should().Be(2);
        result.Players[1].TeamName.Should().Be("Survivor");

        result.Players[2].Name.Should().Be("Zoey");
        result.Players[2].ClientIndex.Should().Be(4);
        result.Players[2].SteamId.Should().Be("BOT");
        result.Players[2].Unused1.Should().Be("");
        result.Players[2].TeamIndex.Should().Be(2);
        result.Players[2].TeamName.Should().Be("Survivor");

        result.Players[3].Name.Should().Be("Bill");
        result.Players[3].ClientIndex.Should().Be(5);
        result.Players[3].SteamId.Should().Be("BOT");
        result.Players[3].Unused1.Should().Be("");
        result.Players[3].TeamIndex.Should().Be(2);
        result.Players[3].TeamName.Should().Be("Survivor");

        result.Players[4].Name.Should().Be("Francis");
        result.Players[4].ClientIndex.Should().Be(6);
        result.Players[4].SteamId.Should().Be("BOT");
        result.Players[4].Unused1.Should().Be("");
        result.Players[4].TeamIndex.Should().Be(2);
        result.Players[4].TeamName.Should().Be("Survivor");
    }

    [Test]
    public void PrintInfoParse_ActualVersusStringForFullGame_ReturnsCorrectResult()
    {
        // Act
        var result = new PrintInfoParser().Parse(
            $"[PI] BEGIN{Environment.NewLine}" +
            $"[PI] (F?T) Nushaa<2><STEAM_1:1:27058429><><2><Survivor>{Environment.NewLine}" +
            $"[PI] (F?T) Da Danner<3><STEAM_1:0:4662915><><2><Survivor>{Environment.NewLine}" +
            $"[PI] (F?T) Yoyo<4><STEAM_1:1:17115154><><2><Survivor>{Environment.NewLine}" +
            $"[PI] (F?T) The President<5><STEAM_1:0:18975559><><2><Survivor>{Environment.NewLine}" +
            $"[PI] QuasiImp<6><STEAM_1:0:113054019><><3><Infected>{Environment.NewLine}" +
            $"[PI] Filthy Causal<7><STEAM_1:1:6063400><><3><Infected>{Environment.NewLine}" +
            $"[PI] Jackball<8><STEAM_1:1:15762437><><3><Infected>{Environment.NewLine}" +
            $"[PI] (F?T) Steel Talon<9><STEAM_1:1:3730409><><3><Infected>{Environment.NewLine}" +
            "[PI] END");

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(8);

        // Survivors
        result.Players[0].Name.Should().Be("(F?T) Nushaa");
        result.Players[0].ClientIndex.Should().Be(2);
        result.Players[0].SteamId.Should().Be("STEAM_1:1:27058429");
        result.Players[0].Unused1.Should().Be("");
        result.Players[0].TeamIndex.Should().Be(2);
        result.Players[0].TeamName.Should().Be("Survivor");

        result.Players[1].Name.Should().Be("(F?T) Da Danner");
        result.Players[1].ClientIndex.Should().Be(3);
        result.Players[1].SteamId.Should().Be("STEAM_1:0:4662915");
        result.Players[1].Unused1.Should().Be("");
        result.Players[1].TeamIndex.Should().Be(2);
        result.Players[1].TeamName.Should().Be("Survivor");

        result.Players[2].Name.Should().Be("(F?T) Yoyo");
        result.Players[2].ClientIndex.Should().Be(4);
        result.Players[2].SteamId.Should().Be("STEAM_1:1:17115154");
        result.Players[2].Unused1.Should().Be("");
        result.Players[2].TeamIndex.Should().Be(2);
        result.Players[2].TeamName.Should().Be("Survivor");

        result.Players[3].Name.Should().Be("(F?T) The President");
        result.Players[3].ClientIndex.Should().Be(5);
        result.Players[3].SteamId.Should().Be("STEAM_1:0:18975559");
        result.Players[3].Unused1.Should().Be("");
        result.Players[3].TeamIndex.Should().Be(2);
        result.Players[3].TeamName.Should().Be("Survivor");

        // Infected
        result.Players[4].Name.Should().Be("QuasiImp");
        result.Players[4].ClientIndex.Should().Be(6);
        result.Players[4].SteamId.Should().Be("STEAM_1:0:113054019");
        result.Players[4].Unused1.Should().Be("");
        result.Players[4].TeamIndex.Should().Be(3);
        result.Players[4].TeamName.Should().Be("Infected");

        result.Players[5].Name.Should().Be("Filthy Causal");
        result.Players[5].ClientIndex.Should().Be(7);
        result.Players[5].SteamId.Should().Be("STEAM_1:1:6063400");
        result.Players[5].Unused1.Should().Be("");
        result.Players[5].TeamIndex.Should().Be(3);
        result.Players[5].TeamName.Should().Be("Infected");

        result.Players[6].Name.Should().Be("Jackball");
        result.Players[6].ClientIndex.Should().Be(8);
        result.Players[6].SteamId.Should().Be("STEAM_1:1:15762437");
        result.Players[6].Unused1.Should().Be("");
        result.Players[6].TeamIndex.Should().Be(3);
        result.Players[6].TeamName.Should().Be("Infected");

        result.Players[7].Name.Should().Be("(F?T) Steel Talon");
        result.Players[7].ClientIndex.Should().Be(9);
        result.Players[7].SteamId.Should().Be("STEAM_1:1:3730409");
        result.Players[7].Unused1.Should().Be("");
        result.Players[7].TeamIndex.Should().Be(3);
        result.Players[7].TeamName.Should().Be("Infected");
    }
}
