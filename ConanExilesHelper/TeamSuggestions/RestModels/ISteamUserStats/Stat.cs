using System.Text.Json.Serialization;

namespace ConanExilesHelper.TeamSuggestions.RestModels.ISteamUserStats;

public class Stat
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}
