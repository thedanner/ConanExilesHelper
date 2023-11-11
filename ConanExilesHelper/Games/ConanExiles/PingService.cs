using ConanExilesHelper.Helpers;
using ConanExilesHelper.SourceQuery;
using ConanExilesHelper.SourceQuery.Rules;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Games.ConanExiles;

// Adapted from https://gist.github.com/csh/2480d14fbbb33b4bbae3
public class PingService : IPingService
{
    private static readonly object _lock = new();

    private readonly ILogger<PingService> _logger;
    private readonly ICommandThrottler _commandThrottler;

    public PingService(ILogger<PingService> logger, ICommandThrottler commandThrottler)
    {
        _logger = logger;
        _commandThrottler = commandThrottler;
    }

    public async Task<GameServer<ConanExilesRules>?> PingAsync(string hostname, ushort queryPort)
    {
        lock (_lock)
        {
            if (!_commandThrottler.TryCanRunCommand()) return null;
        }

        var gs = new GameServer<ConanExilesRules>(ConanExilesRules.Parser);

        var endpoint = new IPEndPoint(IPAddress.Parse(hostname), queryPort);

        await gs.QueryAsync(endpoint, CancellationToken.None);

        return gs;
    }
}
