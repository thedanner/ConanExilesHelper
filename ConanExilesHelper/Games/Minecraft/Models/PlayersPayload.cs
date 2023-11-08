using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConanExilesHelper.Games.Minecraft.Models;

public class PlayersPayload
{
    [JsonPropertyName("max")]
    public int Max { get; set; }

    [JsonPropertyName("online")]
    public int Online { get; set; }

    [JsonPropertyName("sample")]
    public List<Player> Sample { get; set; } = new List<Player>();
}
