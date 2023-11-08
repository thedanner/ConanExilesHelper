using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConanExilesHelper.TeamSuggestions.RestModels.ISteamUserStats;
public class PlayerStats
{
    [JsonPropertyName("steamID")]
    public string SteamIdString { get; set; } = "";

    public SteamId SteamId => SteamId.Create(ulong.Parse(SteamIdString));

    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = "";

    [JsonPropertyName("achievements")]
    public List<Achievenemnt> Achievements { get; set; } = new();

    [JsonPropertyName("stats")]
    public List<Stat> Stats { get; set; } = new();
}
