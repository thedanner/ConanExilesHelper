namespace ConanExilesHelper.Models.Configuration;

public class DiscordUser : IDiscordUser
{
    public DiscordUser()
    {
        Name = "";
    }

    public string Name { get; set; }

    public ulong DiscordId { get; set; }
}
