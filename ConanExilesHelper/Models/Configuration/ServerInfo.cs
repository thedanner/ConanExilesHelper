namespace ConanExilesHelper.Models.Configuration;

public class ServerInfo
{
    public ServerInfo()
    {
        Ip = "0.0.0.0";
        Port = 27015;
        RconPassword = "";
    }

    public string Ip { get; set; }
    public ushort Port { get; set; }
    public string RconPassword { get; set; }
}
