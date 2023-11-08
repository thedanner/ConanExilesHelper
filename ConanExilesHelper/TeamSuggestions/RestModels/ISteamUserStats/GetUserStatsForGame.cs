using System.Text.Json.Serialization;

namespace ConanExilesHelper.TeamSuggestions.RestModels.ISteamUserStats;

public class GetUserStatsForGame
{
    [JsonPropertyName("playerstats")]
    public PlayerStats PlayerStats { get; set; } = new PlayerStats();
}
