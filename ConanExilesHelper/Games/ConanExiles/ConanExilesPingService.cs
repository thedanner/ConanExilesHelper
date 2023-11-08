using ConanExilesHelper.Helpers;
using ConanExilesHelper.SourceQuery;
using ConanExilesHelper.SourceQuery.Rules;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Games.ConanExiles;

// Adapted from https://gist.github.com/csh/2480d14fbbb33b4bbae3
public class ConanExilesPingService : IConanExilesPingService
{
    private static readonly object _lock = new();

    private readonly ILogger<ConanExilesPingService> _logger;
    private readonly IPingThrottler _pingThrottler;

    public ConanExilesPingService(ILogger<ConanExilesPingService> logger, IPingThrottler pingThrottler)
    {
        _logger = logger;
        _pingThrottler = pingThrottler;
    }

    public async Task<GameServer<ConanExilesRules>?> PingAsync(string hostname, ushort queryPort)
    {
        lock (_lock)
        {
            if (!_pingThrottler.TryCanPing()) return null;
        }

        var gs = new GameServer<ConanExilesRules>(ConanExilesRules.Parser);

        var endpoint = new IPEndPoint(IPAddress.Parse(hostname), queryPort);

        await gs.QueryAsync(endpoint, CancellationToken.None);

        return gs;
    }
}
