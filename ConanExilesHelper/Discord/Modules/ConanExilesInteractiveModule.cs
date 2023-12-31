using Discord;
using Discord.Interactions;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Games.ConanExiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using ConanExilesHelper.Configuration;
using ConanExilesHelper.Services.ModComparison;
using System.Threading;
using System.Threading.Channels;

namespace ConanExilesHelper.Discord.Modules;

public class ConanExilesInteractiveModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ConanExilesInteractiveModule> _logger;
    private readonly ConanExilesSettings? _settings;
    private readonly IPingService _pingService;
    private readonly IRestartService _restartService;
    private readonly IModVersionChecker _modVersionChecker;

    public ConanExilesInteractiveModule(ILogger<ConanExilesInteractiveModule> logger,
        IOptions<ConanExilesSettings>? conanExilesSettings,
        IPingService pingService,
        IRestartService restartService,
        IModVersionChecker modVersionChecker)
    {
        _settings = conanExilesSettings?.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pingService = pingService ?? throw new ArgumentNullException(nameof(pingService));
        _restartService = restartService ?? throw new ArgumentNullException(nameof(restartService));
        _modVersionChecker = modVersionChecker ?? throw new ArgumentNullException(nameof(modVersionChecker));
    }

    [SlashCommand("who", "Gets who's playing on our Conan Exiles server.")]
    public async Task HandlePingAsync()
    {
        await DeferAsync();
        await Task.Delay(Constants.DelayAfterCommand);

        if (_settings is null)
        {
            await DeleteOriginalResponseAsync();
            await Task.Delay(Constants.DelayAfterCommand);
            return;
        }

        try
        {
            if (_settings.ChannelId != Context.Channel.Id)
            {
                // The Discord API doesn't support modifying the ephemeral state after the DeferAsync(), so we can't make these private.
                await FollowupAsync($"This command can only be run from the right channel.");
                return;
            }

            var server = _settings.Server;

            if (server is null) return;

            var response = await _pingService.PingAsync(server.QueryHostname, server.QueryPort);

            if (response is null)
            {
                _logger.LogInformation("No payload returned from the ping method; it may be throttled.");
                return;
            }

            var plural = response.MaximumPlayerCount == 1 ? "" : "s";
            var embedBuilder = new EmbedBuilder
            {
                Title = response.GameDescription ?? response.Name ?? server.Name,
                Description =
                    $"Server name: *{response.Name}*" +
                    $"\n{response.Players.Count} of {response.MaximumPlayerCount} player{plural}" +
                    $"\nplaying on *{response.Map}*"
            };

            var playersMessage = new StringBuilder();

            if (response.Players.Count != 0)
            {
                foreach (var player in response.Players)
                {
                    playersMessage.Append('\n').Append(player.Name);
                }
            }
            else
            {
                playersMessage.Append("*Nobody is playing right now.*");
            }

            embedBuilder
                .AddField("Players", playersMessage.ToString())
                .WithColor(Color.DarkRed)
                .WithFooter($"{server.Hostname}:{server.ServerPort}");

            await FollowupAsync(null, embed: embedBuilder.Build());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred :(");

            await FollowupAsync($"Sorry, there was an error. My logs have more information.");
        }
    }

    [SlashCommand("check", "Check for mod updates, and restart if no one's playing.")]
    public async Task HandleCheckAsync()
    {
        await DeferAsync();
        await Task.Delay(Constants.DelayAfterCommand);

        if (_settings is null)
        {
            await DeleteOriginalResponseAsync();
            await Task.Delay(Constants.DelayAfterCommand);
            return;
        }

        try
        {
            if (_settings.ChannelId != Context.Channel.Id)
            {
                // The Discord API doesn't support modifying the ephemeral state after the DeferAsync(), so we can't make these private.
                await FollowupAsync($"This can only be run from the right channel.");
                return;
            }

            if (_settings.RequireRoleIdsForRestart?.Count != 0)
            {
                var userRoles = ((SocketGuildUser)Context.User).Roles.Select(r => r.Id);
                var anyInCommon = userRoles.Intersect(_settings.RequireRoleIdsForRestart!).Any();
                if (!anyInCommon)
                {
                    await FollowupAsync($"This can only be run by users with the appropriate role.");
                    return;
                }
            }

            var result = await _modVersionChecker.CheckForUpdatesAsync(CancellationToken.None);

            switch (result.Result)
            {
                case VersionCheckResultStatus.RetryLater_SteamCmdRunning:
                    await FollowupAsync("It looks like the server is starting up. Try again later.");
                    return;
                case VersionCheckResultStatus.RetryLater_Throttled:
                    await FollowupAsync("Updates were checked recently (maybe automatically). Try again later.");
                    return;
            };

            if (result.UpdatedMods.Count == 0)
            {
                await FollowupAsync("No mod upddates were found.");
                return;
            }

            _logger.LogInformation("  Updates found for the following {count} mod{plural}: {list}.",
                result.UpdatedMods.Count, result.UpdatedMods.Count == 1 ? "" : "s",
                string.Join(", ", result.UpdatedMods.Select(m => $"{m.Title} ({m.PublishedFileId})")));
            _logger.LogInformation("  Attempting a server restart.");

            var server = _settings.Server;

            if (server is null) return;

            var response = await _restartService.TryRestartAsync();
            string message = response switch
            {
                RestartResponse.Success => "Server restarted! Please wait a couple of minutes before you try connecting.",
                RestartResponse.Exception => "An error has occurred, sorry :(",
                RestartResponse.RestartInProgress => "A restart request is already in progress, please wait a couple of minutes.",
                RestartResponse.ServerNotEmpty => "The server isn't empty.",
                RestartResponse.CouldntFindServerProcess => "I'm having trouble checking if the server is running or not.",
                RestartResponse.RestartThrottled => "A restart happened not too long ago, please wait a couple of minutes.",
                RestartResponse.PingThrottled => "An attempt to restart just happened recently, please wait a couple of seconds.",
                RestartResponse.InvalidRconPassword => "I have an incorrect RCON password configured.",
                _ => $"I don't have a message for whatever happened, sorry ({response}).",
            };

            if (response == RestartResponse.Success)
            {
                var mods = string.Join(", ", result.UpdatedMods.Select(m => m.Title));
                message += $"\nUpdates found for {mods}.";
            }

            await FollowupAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred :(");

            await FollowupAsync($"Sorry, there was an error. My logs have more information.");
        }
    }

    [SlashCommand("restart", "Restarts the server if no one's playing.")]
    public async Task HandleRestartAsync()
    {
        await DeferAsync();
        await Task.Delay(Constants.DelayAfterCommand);

        if (_settings is null)
        {
            await DeleteOriginalResponseAsync();
            await Task.Delay(Constants.DelayAfterCommand);
            return;
        }

        try
        {
            if (_settings.ChannelId != Context.Channel.Id)
            {
                // The Discord API doesn't support modifying the ephemeral state after the DeferAsync(), so we can't make these private.
                await FollowupAsync($"This can only be run from the right channel.");
                return;
            }

            if (_settings.RequireRoleIdsForRestart?.Count != 0)
            {
                var userRoles = ((SocketGuildUser) Context.User).Roles.Select(r => r.Id);
                var anyInCommon = userRoles.Intersect(_settings.RequireRoleIdsForRestart!).Any();
                if (!anyInCommon)
                {
                    await FollowupAsync($"This can only be run by users with the appropriate role.");
                    return;
                }
            }

            var server = _settings.Server;

            if (server is null) return;

            var result = await _modVersionChecker.CheckForUpdatesAsync(CancellationToken.None);

            var response = await _restartService.TryRestartAsync();
            string message = response switch
            {
                RestartResponse.Success => "Server restarted! Please wait a couple of minutes before you try connecting.",
                RestartResponse.Exception => "An error has occurred, sorry :(",
                RestartResponse.RestartInProgress => "A restart request is already in progress, please wait a couple of minutes.",
                RestartResponse.ServerNotEmpty => "The server isn't empty.",
                RestartResponse.CouldntFindServerProcess => "I'm having trouble checking if the server is running or not.",
                RestartResponse.RestartThrottled => "A restart happened not too long ago, please wait a couple of minutes.",
                RestartResponse.PingThrottled => "An attempt to restart just happened recently, please wait a couple of seconds.",
                RestartResponse.InvalidRconPassword => "I have an incorrect RCON password configured.",
                _ => $"I don't have a message for whatever happened, sorry ({response}).",
            };

            if (response == RestartResponse.Success && result.UpdatedMods.Count != 0)
            {
                var mods = string.Join(", ", result.UpdatedMods.Select(m => m.Title));
                message += $"\nUpdates found for {mods}.";
            }

            await FollowupAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred :(");

            await FollowupAsync($"Sorry, there was an error. My logs have more information.");
        }
    }
}
