using ConanExilesHelper.Configuration;
using ConanExilesHelper.Services.ModComparison;
using IniParser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConanExilesHelper.Services;

public class ConanServerUtils : IConanServerUtils
{
    private readonly ILogger<ConanServerUtils> _logger;
    private readonly ConanExilesSettings _settings;

    public ConanServerUtils(ILogger<ConanServerUtils> logger, IOptions<ConanExilesSettings> settings)
    {
        _logger = logger;
        _settings = settings?.Value ?? throw new ArgumentException("Settings argument was not populated.", nameof(settings));
    }

    public Process? GetConanServerProcess()
    {
        var processes = Process.GetProcessesByName("ConanSandboxServer");
        if (!processes.Any()) return null;

        var process = processes.First();

        return process;
    }

    public string GetServerBaseDirectoryPath()
    {
        var server = _settings.Server;

        var process = GetConanServerProcess();

        if (process is null) return server.ServerBaseDirectory;

        var executablePath = process.MainModule?.FileName;

        var serverBaseDirectory = Path.GetDirectoryName(executablePath) ?? server.ServerBaseDirectory;

        return serverBaseDirectory;
    }

    public IEnumerable<long> GetWorkshopAddonIds()
    {
        var serverBaseDirectory = GetServerBaseDirectoryPath();

        var serverIniPath = Path.Combine(serverBaseDirectory,
                @"ConanExilesDedicatedServer", @"ConanSandbox", @"Saved", @"Config", @"WindowsServer", @"ServerSettings.ini");

        var parser = new FileIniDataParser();
        var data = parser.ReadFile(serverIniPath);

        var workshopModsStr = data["ServerSettings"]["DedicatedServerLauncherModList"];

        if (string.IsNullOrEmpty(workshopModsStr)) return Enumerable.Empty<long>();

        var workshopModIds = workshopModsStr.Split(",").Select(long.Parse).ToList();

        return workshopModIds;
    }

    public Dictionary<long, DateTimeOffset> GetWorkshopModsLastUpdated()
    {
        var serverBaseDirectory = GetServerBaseDirectoryPath();

        var acfPath = Path.Combine(serverBaseDirectory,
            @"ConanExilesDedicatedServer", @"steamapps", @"workshop", @"appworkshop_440900.acf");

        var reader = new AcfReader(acfPath);
        var acfStruct = reader.ACFFileToStruct();

        var modsData = acfStruct.SubACF["WorkshopItemsInstalled"];

        var dict = new Dictionary<long, DateTimeOffset>();

        foreach (var modData in modsData.SubACF)
        {
            var modId = long.Parse(modData.Key);
            var timeUpdatedSec = long.Parse(modData.Value.SubItems["timeupdated"]);
            var timeUpdated = DateTimeOffset.FromUnixTimeSeconds(timeUpdatedSec);

            dict.Add(modId, timeUpdated);
        }

        return dict;
    }
}
