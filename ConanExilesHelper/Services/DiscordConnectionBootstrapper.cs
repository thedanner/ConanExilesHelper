using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ConanExilesHelper.Helpers.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConanExilesHelper.Configuration;

namespace ConanExilesHelper.Services;

public class DiscordConnectionBootstrapper : IDiscordConnectionBootstrapper
{
    private readonly ILogger<DiscordConnectionBootstrapper> _logger;
    private readonly DiscordSettings _discordSettings;
    private readonly InteractionService _interactionService;

    public DiscordConnectionBootstrapper(ILogger<DiscordConnectionBootstrapper> logger, IOptions<DiscordSettings>? discordSettings,
        InteractionService interactionService)
    {
        _logger = logger;
        _discordSettings = discordSettings?.Value ?? throw new ArgumentNullException(nameof(discordSettings));
        _interactionService = interactionService;
    }

    public async Task StartAsync(DiscordSocketClient client, CancellationToken cancellationToken)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));

        var readyComplete = new TaskCompletionSource<bool>();
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        client.Connected += async () => _logger.LogInformation("Discord client event: Connected");

        async Task initalReadyAsync() => await ReadyHandlerWithSignalAsync(readyComplete);
        client.Ready += initalReadyAsync;

        // NOTE: the client configuration will likely have ExclusiveBulkDelete set to true, which means that
        // for messages bulk-deleted with that specific API call, only the MessagesBulkDeleted event will be fired,
        // and not individual MessageDeleted events for the messages that are bulk-deleted.
        // This can be changed in DiscoredSocketConfig when the DiscordSocketClient is created.
        // See https://github.com/discord-net/Discord.Net/releases/tag/2.1.0

        client.Disconnected += async (ex) => _logger.LogError(ex, "Discord client event: Disconnected");
        client.GuildAvailable += async (guild) =>
        {
            _logger.LogInformation("Discord client event: GuildAvailable ({id}: {name})", guild.Id, guild.Name);

            // NOTE! global commands take about 1 hour to register.
            // Since we're a private bot on only a couple of servers, register to guilds only.
            var registeredCommands = await _interactionService.RegisterCommandsToGuildAsync(guild.Id, true);

            _logger.LogInformation("Registered {count} commands in guild ({id}: {name})", registeredCommands.Count, guild.Id, guild.Name);
        };
        client.GuildMembersDownloaded += async (guild) => _logger.LogInformation("Discord client event: GuildMembersDownloaded");
        client.Log += async (logMessage) =>
        {
            var level = logMessage.Severity.ToLogLevel();

            _logger.Log(
                level,
                logMessage.Exception,
                "Discord client event: Log: (Source: {source}): {message}",
                logMessage.Source, logMessage.Message);
        };
        client.LoggedIn += async () => _logger.LogInformation("Discord client event: LoggedIn");
        client.LoggedOut += async () => _logger.LogInformation("Discord client event: LoggedOut");

        await client.LoginAsync(TokenType.Bot, _discordSettings.BotToken);
        await client.StartAsync();

        await readyComplete.Task;

        client.Ready -= initalReadyAsync;
        client.Ready += async () =>
        {
            _logger.LogInformation("Discord client event: Ready");
        };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        await client.SetStatusAsync(UserStatus.Online);
        await client.SetGameAsync("/who, /restart", type: ActivityType.Playing);
    }

    private Task ReadyHandlerWithSignalAsync(TaskCompletionSource<bool> readyComplete)
    {
        _logger.LogInformation("Discord client event: Ready");
        readyComplete.SetResult(true);
        return Task.FromResult(0);
    }
}
