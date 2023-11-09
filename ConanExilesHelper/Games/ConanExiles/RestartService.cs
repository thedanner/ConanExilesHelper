using ConanExilesHelper.Helpers;
using ConanExilesHelper.Models.Configuration;
using ConanExilesHelper.Wrappers.Rcon;
using IniParser.Model;
using IniParser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Fluent;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

namespace ConanExilesHelper.Games.ConanExiles;

public class RestartService : IRestartService
{
    private static readonly SemaphoreSlim _semaphore = new(0, 1);

    private readonly ILogger<RestartService> _logger;
    private readonly ConanExilesSettings _settings;
    private readonly IPingService _pingService;
    private readonly IRCONWrapper _rcon;

    public RestartService(ILogger<RestartService> logger,
        IOptions<ConanExilesSettings>? settings,
        IPingService pingService,
        IRCONWrapper rcon)
    {
        _logger = logger;
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _pingService = pingService;
        _rcon = rcon;
    }

    [SupportedOSPlatform("windows")]
    public async Task<bool> TryRestartAsync(string hostname, ushort queryPort)
    {
        // See https://github.com/Dateranoth/ConanExilesServerUtility/blob/master/src/ConanServerUtility/ConanServerUtility.au3#L72

        // TODO this is fugly.
        var conanServer = _settings.Servers.FirstOrDefault(s =>
            hostname.Equals(s.Hostname, StringComparison.CurrentCultureIgnoreCase)
            && queryPort == s.QueryPort);

        if (conanServer is null) return false;

        if (_semaphore.CurrentCount == 0) return false;

        try
        {
            await _semaphore.WaitAsync();

            var response = await _pingService.PingAsync(hostname, queryPort);
            if (response is null || response.Players.Count > 0) return false;

            var processes = Process.GetProcessesByName("ConanSandboxServer.exe");
            if (!processes.Any()) return false;

            var process = processes.First();

            // process.MainWindowHandle // TODO send ^X ^X ^C?
            var executablePath = process.MainModule?.FileName;
            if (executablePath is null) return false;

            var serverCommandLine = process.GetCommandLine();

            // TODO will this work? What if it fails?
            await _rcon.ConnectAsync();

            var shutdownCommand = await _rcon.SendCommandAsync("shutdown");

            await process.WaitForExitAsync(CancellationToken.None);

            var serverIniPath = Path.Combine(conanServer.ServerBaseDirectory,
                @"ConanExilesDedicatedServer", @"ConanSandbox", @"Saved", @"Config", @"WindowsServer", @"ServerSettings.ini");

            var parser = new FileIniDataParser();
            var data = parser.ReadFile(serverIniPath);

            var workshopModsStr = data["ServerSettings"]["DedicatedServerLauncherModList"];

            if (!string.IsNullOrEmpty(workshopModsStr))
            {
                var workshopModIds = workshopModsStr.Split(",").Select(long.Parse).ToList();

                var steamCmdProcessStartInfo = new ProcessStartInfo(Path.Combine(conanServer.ServerBaseDirectory, @"steamcmd.exe"))
                {
                    ArgumentList =
                    {
                        "+force_install_dir",
                        Path.Combine(conanServer.ServerBaseDirectory, @"ConanExilesDedicatedServer"),
                        "+login", "anonymous",
                        "+app_update", "443030default",
                    }
                };

                foreach (var workshopModId in workshopModIds)
                {
                    steamCmdProcessStartInfo.ArgumentList.Add("+workshop_download_item");
                    steamCmdProcessStartInfo.ArgumentList.Add("440900");
                    steamCmdProcessStartInfo.ArgumentList.Add(workshopModId.ToString(CultureInfo.InvariantCulture));
                    steamCmdProcessStartInfo.ArgumentList.Add("validate");
                }

                steamCmdProcessStartInfo.ArgumentList.Add("+quit");

                var steamCmdProcess = Process.Start(steamCmdProcessStartInfo);
                if (steamCmdProcess is not null)
                {
                    await steamCmdProcess.WaitForExitAsync(CancellationToken.None);
                }
            }

            var serverProcessStartInfo = new ProcessStartInfo(Path.Combine(conanServer.ServerBaseDirectory,
                @"ConanExilesDedicatedServer", @"ConanSandboxServer.exe"))
            {
                ArgumentList =
                {
                    "/Game/Maps/ConanSandbox/ConanSandbox",
                    "-MaxPlayers=6"
                }
            };
            var serverProcess = Process.Start(serverProcessStartInfo);
            // We don't need to wait for this process.

            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
