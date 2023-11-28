using System.Collections.Generic;

namespace ConanExilesHelper.Configuration;

public class ConanExilesSettings
{
    public ulong GuildId{ get; set; }
    public ulong ChannelId { get; set; }
    public List<ulong>? RequireRoleIdsForRestart { get; set; } = new List<ulong>();
    public ConanExilesServerEntry Server { get; set; } = new ConanExilesServerEntry();
}
