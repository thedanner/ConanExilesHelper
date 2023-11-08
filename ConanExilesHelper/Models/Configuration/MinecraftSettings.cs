using System.Collections.Generic;

namespace ConanExilesHelper.Models.Configuration;

public class MinecraftSettings
{
    public List<ulong>? ChannelIdFilter { get; set; } = new List<ulong>();
    public string? DefaultServerName { get; set; }
    public List<MinecraftServerEntry> Servers { get; set; } = new List<MinecraftServerEntry>();
}
