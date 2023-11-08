using System.Collections.Generic;

namespace ConanExilesHelper.Models.Configuration;

public class ConanExilesSettings
{
    public List<ulong>? ChannelIdFilter { get; set; } = new List<ulong>();
    public string? DefaultServerName { get; set; }
    public List<ConanExilesServerEntry> Servers { get; set; } = new List<ConanExilesServerEntry>();
}
