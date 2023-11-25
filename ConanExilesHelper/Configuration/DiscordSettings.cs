namespace ConanExilesHelper.Configuration;

public class DiscordSettings
{
    public DiscordSettings()
    {
        BotToken = "";
        ConanExilesSettings = new ConanExilesSettings();
    }

    public string BotToken { get; set; }
    public ConanExilesSettings ConanExilesSettings { get; set; }
}
