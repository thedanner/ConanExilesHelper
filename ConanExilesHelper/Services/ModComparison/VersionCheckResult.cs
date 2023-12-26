using ConanExilesHelper.Services.Steamworks;
using System.Collections.Generic;

namespace ConanExilesHelper.Services.ModComparison;

public record VersionCheckResult(
    VersionCheckResultStatus Result,
    IList<PublishedFileDetails> UpdatedMods
);

public enum VersionCheckResultStatus
{
    Success,
    RetryLater_Throttled,
    RetryLater_SteamCmdRunning,
}
