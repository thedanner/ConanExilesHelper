using ConanExilesHelper.Helpers;
using ConanExilesHelper.Models.Configuration;
using IniParser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using RconSharp;

namespace ConanExilesHelper.Games.ConanExiles;

public class RestartService : IRestartService
{
    private static readonly SemaphoreSlim _semaphore = new(1);

    private readonly ILogger<RestartService> _logger;
    private readonly ConanExilesSettings _settings;
    private readonly IPingService _pingService;

    public RestartService(ILogger<RestartService> logger,
        IOptions<ConanExilesSettings>? settings,
        IPingService pingService)
    {
        _logger = logger;
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _pingService = pingService;
    }

    [SupportedOSPlatform("windows")]
    public async Task<bool> TryRestartAsync()
    {
        // See https://github.com/Dateranoth/ConanExilesServerUtility/blob/master/src/ConanServerUtility/ConanServerUtility.au3#L72

        // TODO this is fugly.
        //var conanServer = _settings.Servers.FirstOrDefault(s =>
        //    hostname.Equals(s.Hostname, StringComparison.CurrentCultureIgnoreCase)
        //    && queryPort == s.QueryPort);
        var conanServer = _settings.Servers;

        if (conanServer is null) return false;

        if (_semaphore.CurrentCount == 0) return false;
        RconClient? rcon = null;

        try
        {
            await _semaphore.WaitAsync();

            rcon = RconClient.Create(conanServer.QueryHostname, conanServer.RconPort);

            var response = await _pingService.PingAsync(conanServer.QueryHostname, conanServer.QueryPort);
            if (response is null || response.Players.Count > 0) return false;

            var processes = Process.GetProcessesByName("ConanSandboxServer");
            if (!processes.Any()) return false;

            var process = processes.First();

            // process.MainWindowHandle // TODO send ^X ^X ^C?
            var executablePath = process.MainModule?.FileName;
            if (executablePath is null) return false;

            var serverBaseDirectory = Path.GetDirectoryName(executablePath) ?? conanServer.ServerBaseDirectory;

            var serverFullCommandLine = process.GetCommandLine();

            if (string.IsNullOrEmpty(serverFullCommandLine)) return false;

            var commandLine = (serverFullCommandLine.StartsWith("\"")
                ? serverFullCommandLine.Substring(serverFullCommandLine.IndexOf("\"", 2) + 1)
                : serverFullCommandLine.Substring(serverFullCommandLine.IndexOf(" ", 1)))
                .Trim();

            await rcon.ConnectAsync();

            var authenticated = await rcon.AuthenticateAsync(conanServer.RconPassword);
            if (!authenticated) return false;

            var shutdownCommand = await rcon.ExecuteCommandAsync("shutdown");

            await process.WaitForExitAsync(CancellationToken.None);

            var serverIniPath = Path.Combine(serverBaseDirectory,
                @"ConanExilesDedicatedServer", @"ConanSandbox", @"Saved", @"Config", @"WindowsServer", @"ServerSettings.ini");

            var parser = new FileIniDataParser();
            var data = parser.ReadFile(serverIniPath);

            var workshopModsStr = data["ServerSettings"]["DedicatedServerLauncherModList"];

            if (!string.IsNullOrEmpty(workshopModsStr))
            {
                var workshopModIds = workshopModsStr.Split(",").Select(long.Parse).ToList();

                var steamCmdProcessStartInfo = new ProcessStartInfo(Path.Combine(serverBaseDirectory, @"steamcmd.exe"))
                {
                    ArgumentList =
                    {
                        "+force_install_dir",
                        Path.Combine(serverBaseDirectory, @"ConanExilesDedicatedServer"),
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

            var serverProcessStartInfo = new ProcessStartInfo(Path.Combine(serverBaseDirectory,
                @"ConanExilesDedicatedServer", @"ConanSandboxServer.exe"));
            if (!string.IsNullOrEmpty(commandLine))
            {
                serverProcessStartInfo.Arguments = commandLine;
            }
            else
            {
                serverProcessStartInfo.ArgumentList.Add("/Game/Maps/ConanSandbox/ConanSandbox");
                serverProcessStartInfo.ArgumentList.Add("-MaxPlayers=6");
            }

            var serverProcess = Process.Start(serverProcessStartInfo);
            // We don't need to wait for this process.

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attempting to restart Conan Exiles server at IP {ip}:{port}.", conanServer.Hostname, conanServer.ServerPort);
            return false;
        }
        finally
        {
            try { rcon?.Disconnect(); } catch { }
            _semaphore.Release();
        }
    }
}
