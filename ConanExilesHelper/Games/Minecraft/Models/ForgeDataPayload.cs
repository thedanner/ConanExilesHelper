using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConanExilesHelper.Games.Minecraft.Models;

public class ForgeDataPayload
{
    [JsonPropertyName("channels")]
    public List<ForgeDataChannel> Channels { get; set; } = new List<ForgeDataChannel>();

    [JsonPropertyName("mods")]
    public List<ForgeDataMod> Mods { get; set; } = new List<ForgeDataMod>();

    [JsonPropertyName("fmlNetworkVersion")]
    public int FmlNetworkVersion { get; set; }

    [JsonPropertyName("truncated")]
    public bool Truncated { get; set; }
}
