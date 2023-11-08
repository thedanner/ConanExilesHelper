using System.Text.Json.Serialization;

namespace ConanExilesHelper.Games.Minecraft.Models;

public class DescriptionPayload
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
}
