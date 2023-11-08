using System.Collections.Generic;

namespace ConanExilesHelper.Services;

public partial class MoveResult
{
    public MoveResult()
    {
        UnmappedSteamUsers = new List<UnmappedSteamUser>();
    }

    public MoveFailureReason FailureReason { get; set; }
    public int MoveCount { get; set; }
    public List<UnmappedSteamUser> UnmappedSteamUsers { get; set; }

    public enum MoveFailureReason
    {
        None,
        NotEnoughEmptyVoiceChannels,
    }
}
