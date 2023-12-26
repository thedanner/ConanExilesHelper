using ConanExilesHelper.Configuration;
using ConanExilesHelper.Games.ConanExiles;
using ConanExilesHelper.Scheduling.Infrastructure;
using ConanExilesHelper.Services.ModComparison;
using ConanExilesHelper.Services.Steamworks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Scheduling.Tasks;

public class CheckWorkshopAddonVersionsTask : ITask
{
    private readonly ILogger<CheckWorkshopAddonVersionsTask> _logger;
    private readonly ConanExilesSettings _settings;
    private readonly IModVersionChecker _modVersionChecker;
    private readonly IRestartService _restartService;

    public CheckWorkshopAddonVersionsTask(ILogger<CheckWorkshopAddonVersionsTask> logger,
        IOptions<ConanExilesSettings> settings, IModVersionChecker modVersionChecker, IRestartService restartService)
    {
        _logger = logger;
        _settings = settings.Value ?? throw new ArgumentException("Settings argument was not populated.", nameof(settings));
        _modVersionChecker = modVersionChecker;
        _restartService = restartService;
    }

    public async Task RunTaskAsync(DiscordSocketClient client, IReadOnlyDictionary<string, object> taskSettings, CancellationToken cancellationToken)
    {
        var guild = client.GetGuild(_settings.GuildId) ?? throw new Exception(
                "Couldn't get the guild from the settings in the Discord client. " +
                "Double-check the value and make sure the bot is connected to the right server.");
        
        var channel = guild.GetTextChannel(_settings.ChannelId) ?? throw new Exception(
                "Couldn't get the channel from the settings in the Discord client. " +
                "Double-check the value and make sure the channel still exists, and check its ID.");

        var result = await _modVersionChecker.CheckForUpdatesAsync(cancellationToken);

        if (result.Result == VersionCheckResultStatus.RetryLater_Throttled) return;

        for (var i = 4; i >= 0 && result.Result == VersionCheckResultStatus.RetryLater_SteamCmdRunning; i--)
        {
            _logger.LogDebug("  It looks like steamcmd is running, going to wait a bit.");

            if (i == 0) return;

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

            result = await _modVersionChecker.CheckForUpdatesAsync(cancellationToken);

            if (result.Result == VersionCheckResultStatus.RetryLater_Throttled) return;
        }

        if (!result.UpdatedMods.Any()) return;

        _logger.LogInformation("  Updates found for the following {count} mod{plural}: {list}.",
            result.UpdatedMods.Count, result.UpdatedMods.Count == 1 ? "" : "s",
            string.Join(", ", result.UpdatedMods.Select(m => $"{m.Title} ({m.PublishedFileId})")));
        _logger.LogInformation("  Attempting a server restart.");

        var restartResult = await _restartService.TryRestartAsync();

        var mods = string.Join(", ", result.UpdatedMods.Select(m => m.Title));

        if (restartResult == RestartResponse.Success)
        {
            _logger.LogInformation("  Server restarted.");
            await channel.SendMessageAsync(
                $"Mod updates were found and the server has been restarted.\n" +
                $"Updates found for {mods}.\n\n" +
                $"If you didn't get those updates or can't connect, try restarting Steam.");
        }
        else if (restartResult == RestartResponse.ServerNotEmpty)
        {
            _logger.LogInformation("  Server could not be restarted because it's not empty.");
            await channel.SendMessageAsync($"Mod updates were found, but the server can't be restarted because it's not empty.\nUpdates found for {mods}.");
        }
        else
        {
            _logger.LogWarning("Server could not be restarted; the reason given is {reason}.", restartResult);
        }
    }
}
