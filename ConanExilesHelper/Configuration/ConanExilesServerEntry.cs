namespace ConanExilesHelper.Configuration;

public class ConanExilesServerEntry
{
    public string? Name { get; set; }
    public string Hostname { get; set; } = "";
    public ushort ServerPort { get; set; } = 7777;
    public string QueryHostname { get; set; } = "";
    public ushort QueryPort { get; set; } = 27015;
    public ushort RconPort { get; set; } = 27015;
    public string RconPassword { get; set; } = "";
    public string ServerBaseDirectory { get; set; } = "";
}
