using Discord;
using Discord.WebSocket;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConanExilesHelper.Tests.Integration;

[TestFixture]
[Explicit("Run manually")]
public class DiscordIntegrationTests
{
    private DiscordSettings _discordSettings;

    private string _botToken;
    private ulong _guildId;
    private ulong _primaryChannelId;
    private ulong _secondaryChannelId;

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    [SetUp]
    public void SetUp()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();

        _discordSettings = config.GetSection("discordSettings").Get<DiscordSettings>();

        _botToken = _discordSettings.BotToken;
        var guildSettings = _discordSettings.GuildSettings.First();
        _guildId = guildSettings.Id;

        _primaryChannelId = guildSettings.Channels.Primary.Id;
        _secondaryChannelId = guildSettings.Channels.Secondary.Id;
    }

    [Test]
    public async Task Test()
    {
        using var client = await GetClientAsync();

        var guild = client.GetGuild(_guildId);

        var primaryChannel = guild.GetVoiceChannel(_primaryChannelId);
        var secondaryChannel = guild.GetVoiceChannel(_secondaryChannelId);
    }

    [Test]
    public async Task GetRoles()
    {
        using var client = await GetClientAsync();

        var guild = client.GetGuild(_guildId);
        var allRolesSorted = new List<SocketRole>(guild.Roles.OrderByDescending(r => r.Position));

        var gradientRoles = new List<SocketRole>();

        var adding = false;

        foreach (var role in allRolesSorted)
        {
            if (role.Name == "Parents") adding = true;

            if (adding)
            {
                gradientRoles.Add(role);
            }

            if (role.Name == "Titanfall Pilots")
            {
                adding = false;
                break;
            }
        }
    }

    private async Task<DiscordSocketClient> GetClientAsync()
    {
        using var client = new DiscordSocketClient();

        client.Log += Log;

        await client.LoginAsync(TokenType.Bot, _botToken);
        await client.StartAsync();

        var tsc = new TaskCompletionSource<bool>();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        client.Ready += async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            tsc.SetResult(true);
        };

        await tsc.Task;

        return client;
    }
}
