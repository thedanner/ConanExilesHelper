namespace ConanExilesHelper.Models.Configuration;

public class GuildSettings
{
    public GuildSettings()
    {
        Channels = new DiscordVoiceChannels();
        ConfigMaintainers = new DiscordUser[0];
    }

    public ulong Id { get; set; }
    public DiscordVoiceChannels Channels { get; set; }
    public DiscordUser[] ConfigMaintainers { get; set; }
}
