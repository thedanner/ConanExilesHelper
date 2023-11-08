namespace ConanExilesHelper.Models.Configuration;

public class MinecraftServerEntry
{
    public string? Name { get; set; }
    public string Hostname { get; set; } = "";
    public ushort Port { get; set; }
}
