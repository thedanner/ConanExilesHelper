using ConanExilesHelper.Configuration;
using ConanExilesHelper.Games.ConanExiles;
using ConanExilesHelper.Scheduling.Infrastructure;
using ConanExilesHelper.Services;
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
    private readonly ISteamworksApi _api;
    private readonly IConanServerUtils _serverUtils;
    private readonly IRestartService _restartService;

    public CheckWorkshopAddonVersionsTask(ILogger<CheckWorkshopAddonVersionsTask> logger,
        IOptions<ConanExilesSettings> settings,
        ISteamworksApi api, IConanServerUtils serverUtils, IRestartService restartService)
    {
        _logger = logger;
        _settings = settings.Value ?? throw new ArgumentException("Settings argument was not populated.", nameof(settings));
        _api = api;
        _serverUtils = serverUtils;
        _restartService = restartService;
    }

    public async Task RunTaskAsync(DiscordSocketClient client, IReadOnlyDictionary<string, object> taskSettings, CancellationToken cancellationToken)
    {
        var addonIds = _serverUtils.GetWorkshopAddonIds().ToList();

        if (!addonIds.Any()) return;

        for (var i = 4; i >= 0 && _serverUtils.IsSteamCmdRunning(); i--)
        {
            if (i == 0) return;

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }

        var modIds = _serverUtils.GetWorkshopAddonIds().ToList();
        var localModsLastUpdated = _serverUtils.GetWorkshopModsLastUpdated();
        var workshopModsResponse = await _api.GetPublishedFileDetailsAsync(modIds, CancellationToken.None);

        if (workshopModsResponse?.Response is null) return;

        var workshopModsDict = workshopModsResponse.Response.PublishedFileDetails.ToDictionary(d => d.PublishedFileId, d => d);

        var differentMods = new List<PublishedFileDetails>();
        foreach (var mod in localModsLastUpdated.Keys)
        {
            var localTime = localModsLastUpdated[mod];
            var workshopTime = workshopModsDict[mod].TimeUpdated;
            if (localTime != workshopTime)
            {
                differentMods.Add(workshopModsDict[mod]);
            }
        }

        if (!differentMods.Any()) return;

        _logger.LogInformation("Some mod updates found; restarting server.");

        var restartResult = await _restartService.TryRestartAsync();

        var guild = client.GetGuild(_settings.GuildId);
        if (guild is null) return;

        var channel = guild.GetTextChannel(_settings.ChannelId);
        if (channel is null) return;

        var mods = string.Join(", ", differentMods.Select(m => m.Title));

        if (restartResult == RestartResponse.Success)
        {
            await channel.SendMessageAsync($"Mod updates were found and the server has been restarted.\nUpdates found for {mods}.");
        }
        else if (restartResult == RestartResponse.ServerNotEmpty)
        {
            await channel.SendMessageAsync($"Mod updates were found, but the server can't be restarted because it's not empty.\nUpdates found for {mods}.");
        }
    }
}
