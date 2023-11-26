using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConanExilesHelper.Services;

public interface IConanServerUtils
{
    Process? GetConanServerProcess();
    string GetServerBaseDirectoryPath();
    IEnumerable<long> GetWorkshopAddonIds();
    Dictionary<long, DateTimeOffset> GetWorkshopModsLastUpdated();
}
