using ConanExilesHelper.Scheduling.Infrastructure;
using ConanExilesHelper.Services;
using ConanExilesHelper.Services.Steamworks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Scheduling.Tasks;

public class CheckWorkshopAddonVersionsTask : ITask
{
    private readonly ILogger<CheckWorkshopAddonVersionsTask> _logger;
    private readonly ISteamworksApi _api;
    private readonly IConanServerUtils _serverUtils;

    public CheckWorkshopAddonVersionsTask(ILogger<CheckWorkshopAddonVersionsTask> logger, ISteamworksApi api, IConanServerUtils serverUtils)
    {
        _logger = logger;
        _api = api;
        _serverUtils = serverUtils;
    }

    public async Task RunTaskAsync(DiscordSocketClient client, IReadOnlyDictionary<string, object> taskSettings, CancellationToken cancellationToken)
    {
        var addonIds = _serverUtils.GetWorkshopAddonIds().ToList();

        if (!addonIds.Any()) return;

        // TODO check if steamcmd is running. If so, we're potentially updating mods now, so wait a bit and try again.

        var steamsworksInfo = await _api.GetPublishedFileDetailsAsync(addonIds, cancellationToken);
        var currentlyInstalledInfo = _serverUtils.GetWorkshopModsLastUpdated();
        

    }
}
