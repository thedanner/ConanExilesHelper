using ConanExilesHelper.Services.Steamworks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using ConanExilesHelper.Helpers;
using System;

namespace ConanExilesHelper.Services.ModComparison;

public class ModVersionChecker : IModVersionChecker
{
    private static readonly CommandThrottler _commandThrottler = new(TimeSpan.FromMinutes(5));

    private readonly ILogger<ModVersionChecker> _logger;
    private readonly IConanServerUtils _serverUtils;
    private readonly ISteamworksApi _api;

    public ModVersionChecker(ILogger<ModVersionChecker> logger, IConanServerUtils serverUtils, ISteamworksApi api)
    {
        _logger = logger;
        _serverUtils = serverUtils;
        _api = api;
    }

    public async Task<VersionCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        if (!await _commandThrottler.TryStartTimeoutAsync())
        {
            return new VersionCheckResult(VersionCheckResultStatus.RetryLater_Throttled, new List<PublishedFileDetails>(0));
        }

        var addonIds = _serverUtils.GetWorkshopAddonIds().ToList();

        _logger.LogInformation("Starting mod update check, found {count} mod{plural}.", addonIds.Count, addonIds.Count == 1 ? "" : "s");

        if (!addonIds.Any()) return new VersionCheckResult(VersionCheckResultStatus.Success, new List<PublishedFileDetails>(0));

        if (_serverUtils.IsSteamCmdRunning())
        {
            return new VersionCheckResult(VersionCheckResultStatus.RetryLater_SteamCmdRunning, new List<PublishedFileDetails>(0));
        }

        var modIds = _serverUtils.GetWorkshopAddonIds().ToList();
        var localModsLastUpdated = _serverUtils.GetWorkshopModsLastUpdated();
        var workshopModsResponse = await _api.GetPublishedFileDetailsAsync(modIds, CancellationToken.None);

        if (workshopModsResponse?.Response is null) return new VersionCheckResult(VersionCheckResultStatus.Success, new List<PublishedFileDetails>(0));

        var workshopModsDict = workshopModsResponse.Response.PublishedFileDetails.ToDictionary(d => d.PublishedFileId);

        _logger.LogDebug("  Got a response from Steamworks with {count} mod{plural}.", workshopModsDict.Count, workshopModsDict.Count == 1 ? "" : "s");

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

        _logger.LogDebug("  Found {count} mod difference{plural}.", differentMods.Count, differentMods.Count == 1 ? "" : "s");

        return new VersionCheckResult(VersionCheckResultStatus.Success, differentMods);
    }
}
