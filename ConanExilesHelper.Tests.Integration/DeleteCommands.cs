using Discord;
using Discord.WebSocket;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConanExilesHelper.Tests.Integration;

[TestFixture]
[Explicit("Run manually")]
public class DeleteCommands
{
    private DiscordSettings _discordSettings;

    private string _botToken;
    private ulong _guildId;

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
    }

    [Test]
    public async Task DeleteAllCommands()
    {
        using var client = await GetClientAsync(true);

        var globalCommands = await client.GetGlobalApplicationCommandsAsync();
        foreach (var command in globalCommands)
        {
            await command.DeleteAsync();
        }

        var guild = client.GetGuild(_guildId);
        var guildCommands = await guild.GetApplicationCommandsAsync();
        foreach (var command in guildCommands)
        {
            await command.DeleteAsync();
        }
    }

    [Test]
    public async Task DeleteGlobalCommands()
    {
        using var client = await GetClientAsync(false);

        var allCommands = await client.GetGlobalApplicationCommandsAsync();
        foreach (var command in allCommands)
        {
            await command.DeleteAsync();
        }
    }

    [Test]
    public async Task DeleteGuildCommands()
    {
        using var client = await GetClientAsync(true);

        var guild = client.GetGuild(_guildId);
        var guildCommands = await guild.GetApplicationCommandsAsync();
        foreach (var command in guildCommands)
        {
            await command.DeleteAsync();
        }
    }

    private async Task<DiscordSocketClient> GetClientAsync(bool waitForGuildAvailable)
    {
        var client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                    | GatewayIntents.GuildIntegrations
                    | GatewayIntents.GuildVoiceStates
                    | GatewayIntents.GuildPresences
                    | GatewayIntents.GuildMessages
        });

        var readyComplete = new TaskCompletionSource<bool>();
        var guildAvailable = new TaskCompletionSource<bool>();
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        client.Connected += async () => Console.WriteLine("Discord client event: Connected");

        async Task initalReadyAsync() => await ReadyHandlerWithSignalAsync(readyComplete);
        client.Ready += initalReadyAsync;

        // NOTE: the client configuration will likely have ExclusiveBulkDelete set to true, which means that
        // for messages bulk-deleted with that specific API call, only the MessagesBulkDeleted event will be fired,
        // and not individual MessageDeleted events for the messages that are bulk-deleted.
        // This can be changed in DiscoredSocketConfig when the DiscordSocketClient is created.
        // See https://github.com/discord-net/Discord.Net/releases/tag/2.1.0

        client.Disconnected += async (ex) => Console.WriteLine("Discord client event: Disconnected");
        client.GuildAvailable += async (guild) =>
        {
            Console.WriteLine("Discord client event: GuildAvailable");

            if (guild.Id == _guildId)
            {
                guildAvailable.SetResult(true);
            }
        };
        client.GuildMembersDownloaded += async (guild) => Console.WriteLine("Discord client event: GuildMembersDownloaded");
        client.Log += async (logMessage) =>
        {
            await LogAsync(logMessage);

            Console.WriteLine(
                "Discord client event: Log: (Source: {0}): {1}",
                logMessage.Source, logMessage.Message);
        };
        client.LoggedIn += async () => Console.WriteLine("Discord client event: LoggedIn");
        client.LoggedOut += async () => Console.WriteLine("Discord client event: LoggedOut");

        await client.LoginAsync(TokenType.Bot, _discordSettings.BotToken);
        await client.StartAsync();

        await readyComplete.Task;

        if (waitForGuildAvailable)
        {
            await guildAvailable.Task;
        }

        client.Ready -= initalReadyAsync;
        client.Ready += async () =>
        {
            Console.WriteLine("Discord client event: Ready");
        };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        await client.SetStatusAsync(UserStatus.Online);

        return client;
    }

    private Task ReadyHandlerWithSignalAsync(TaskCompletionSource<bool> readyComplete)
    {
        Console.WriteLine("Discord client event: Ready");
        readyComplete.SetResult(true);
        return Task.FromResult(0);
    }

    private Task LogAsync(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
