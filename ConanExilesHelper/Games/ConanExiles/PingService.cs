﻿using ConanExilesHelper.Helpers;
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
    private static readonly SemaphoreSlim _semaphore = new(1);
    private static readonly CommandThrottler _commandThrottler = new();

    private readonly ILogger<PingService> _logger;

    public PingService(ILogger<PingService> logger)
    {
        _logger = logger;
    }

    public async Task<GameServer<ConanExilesRules>?> PingAsync(string hostname, ushort queryPort)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (!await _commandThrottler.TryStartTimeoutAsync()) return null;

            var gs = new GameServer<ConanExilesRules>(ConanExilesRules.Parser);

            var endpoint = new IPEndPoint(IPAddress.Parse(hostname), queryPort);

            await gs.QueryAsync(endpoint, CancellationToken.None);

            return gs;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
