using CoreRCON;
using ConanExilesHelper.Models.Configuration;
using ConanExilesHelper.Rcon;
using ConanExilesHelper.SourceQuery;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Tests.Integration;

[TestFixture]
[Explicit("Run manually")]
public class Left4DeadIntegrationTests
{
    private Left4DeadSettings _left4DeadSettings;

    [SetUp]
    public void SetUp()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) //From NuGet Package Microsoft.Extensions.Configuration.Json
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();

        _left4DeadSettings = config.GetSection("left4DeadSettings").Get<Left4DeadSettings>();
    }

    [Test]
    public async Task TestQuery()
    {
        var gs = new GameServer();

        var endpoint = new IPEndPoint(IPAddress.Parse(_left4DeadSettings.ServerInfo.Ip), _left4DeadSettings.ServerInfo.Port);

        await gs.QueryAsync(endpoint, CancellationToken.None);
    }

    [Test]
    public async Task TestRconPrintInfo()
    {
        var serverInfo = _left4DeadSettings.ServerInfo;

        var endpoint = new IPEndPoint(IPAddress.Parse(_left4DeadSettings.ServerInfo.Ip), _left4DeadSettings.ServerInfo.Port);

        using var rcon = new RCON(endpoint, serverInfo.RconPassword);
        await rcon.ConnectAsync();

        var printInfo = await rcon.SendCommandAsync<PrintInfo>("sm_printinfo");
    }

    [Test]
    public async Task TestRconGameMode()
    {
        var serverInfo = _left4DeadSettings.ServerInfo;

        var addresses = Dns.GetHostAddresses(_left4DeadSettings.ServerInfo.Ip);

        var endpoint = new IPEndPoint(addresses[0], _left4DeadSettings.ServerInfo.Port);

        using var rcon = new RCON(endpoint, serverInfo.RconPassword);
        await rcon.ConnectAsync();

        var gamemode = await rcon.SendCommandAsync<SmCvar>("sm_cvar mp_gamemode");
    }
}
