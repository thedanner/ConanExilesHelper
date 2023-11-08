﻿using ConanExilesHelper.SourceQuery;
using ConanExilesHelper.SourceQuery.Rules;
using System.Threading.Tasks;

namespace ConanExilesHelper.Games.ConanExiles;

public interface IConanExilesPingService
{
    Task<GameServer<ConanExilesRules>?> PingAsync(string hostname, ushort queryPort);
}
