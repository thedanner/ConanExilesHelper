namespace ConanExilesHelper.Models.Configuration;

public interface IDiscordUser : IGenericUser
{
    ulong DiscordId { get; set; }
}
