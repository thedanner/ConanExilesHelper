using System.Text.Json.Serialization;

namespace ConanExilesHelper.Games.Minecraft.Models;

/// <summary>
/// C# represenation of the following JSON file
/// https://gist.github.com/thinkofdeath/6927216
/// </summary>
public class MinecraftPingPayload
{
    /// <summary>
    /// Protocol that the server is using and the given name
    /// </summary>
    [JsonPropertyName("version")]
    public VersionPayload Version { get; set; } = new VersionPayload();

    [JsonPropertyName("players")]
    public PlayersPayload Players { get; set; } = new PlayersPayload();

    [JsonPropertyName("description")]
    public DescriptionPayload Description { get; set; } = new DescriptionPayload();

    [JsonPropertyName("forgeData")]
    public ForgeDataPayload? ForgeData { get; set; }
}
