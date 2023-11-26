using ConanExilesHelper.Games.ConanExiles;
using ConanExilesHelper.Scheduling.Infrastructure;
using ConanExilesHelper.Services;
using ConanExilesHelper.Services.Steamworks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Scheduling.Tasks;

public class CheckWorkshopAddonVersionsTask : ITask
{
    private readonly ILogger<CheckWorkshopAddonVersionsTask> _logger;
    private readonly ISteamworksApi _api;
    private readonly IConanServerUtils _serverUtils;
    private readonly IRestartService _restartService;

    public CheckWorkshopAddonVersionsTask(ILogger<CheckWorkshopAddonVersionsTask> logger,
        ISteamworksApi api, IConanServerUtils serverUtils, IRestartService restartService)
    {
        _logger = logger;
        _api = api;
        _serverUtils = serverUtils;
        _restartService = restartService;
    }

    public async Task RunTaskAsync(DiscordSocketClient client, IReadOnlyDictionary<string, object> taskSettings, CancellationToken cancellationToken)
    {
        var addonIds = _serverUtils.GetWorkshopAddonIds().ToList();

        if (!addonIds.Any()) return;

        for (var i = 4; i >= 0; i--)
        {
            if (_serverUtils.IsSteamCmdRunning())
            {
                if (i == 0) return;

                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        var modIds = _serverUtils.GetWorkshopAddonIds().ToList();
        var localModsLastUpdated = _serverUtils.GetWorkshopModsLastUpdated();
        var workshopModsResponse = await _api.GetPublishedFileDetailsAsync(modIds, CancellationToken.None);

        if (workshopModsResponse?.Response is null) return;

        var workshopModsLastUpdated = workshopModsResponse.Response.PublishedFileDetails.ToDictionary(d => d.PublishedFileId, d => d.TimeUpdated);

        var areAllTheSame = true;
        foreach (var mod in localModsLastUpdated.Keys)
        {
            var localTime = localModsLastUpdated[mod];
            var workshopTime = workshopModsLastUpdated[mod];
            var areTheSame = localTime == workshopTime;
            if (!areAllTheSame)
            {
                areAllTheSame = false;
                break;
            }
        }

        if (areAllTheSame) return;

        _logger.LogInformation("Some mod updates found; restarting server.");

        await _restartService.RestartAsync();
    }
}
