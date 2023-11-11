using ConanExilesHelper.Helpers;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using RconSharp;

namespace ConanExilesHelper.Games.ConanExiles;

public class RestartService : IRestartService
{
    private static readonly SemaphoreSlim _semaphore = new(1);
    private static readonly ICommandThrottler _commandThrottler = new CommandThrottler(TimeSpan.FromMinutes(5));

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
    public async Task<RestartResponse> RestartAsync()
    {
        var server = _settings.Server;

        if (_semaphore.CurrentCount == 0) return RestartResponse.RestartInProgress;

        RconClient? rcon = null;

        try
        {
            await _semaphore.WaitAsync();

            if (!await _commandThrottler.TryCanRunCommandAsync())
            {
                return RestartResponse.Throttled;
            }

            var response = await _pingService.PingAsync(server.QueryHostname, server.QueryPort);
            if (response is null || response.Players.Count > 0) return RestartResponse.ServerNotEmpty;

            var processes = Process.GetProcessesByName("ConanSandboxServer");
            if (!processes.Any()) return RestartResponse.CouldntFindServerProcess;

            var process = processes.First();

            //var executablePath = process.MainModule?.FileName;
            //if (executablePath is null) return false;
            //
            //var serverBaseDirectory = Path.GetDirectoryName(executablePath) ?? server.ServerBaseDirectory;
            //
            //var serverFullCommandLine = process.GetCommandLine();
            //
            //if (string.IsNullOrEmpty(serverFullCommandLine)) return false;
            //
            //var commandLine = (serverFullCommandLine.StartsWith("\"")
            //    ? serverFullCommandLine[(serverFullCommandLine.IndexOf("\"", 2) + 1)..]
            //    : serverFullCommandLine[serverFullCommandLine.IndexOf(" ", 1)..])
            //    .Trim();

            rcon = RconClient.Create(server.QueryHostname, server.RconPort);
            await rcon.ConnectAsync();

            if (!await rcon.AuthenticateAsync(server.RconPassword)) return RestartResponse.InvalidRconPassword;

            var shutdownCommand = await rcon.ExecuteCommandAsync("shutdown");

            await process.WaitForExitAsync(CancellationToken.None);

            // If Dedicated Server Manager is running, it'll auto-restart the server when it crashes (assuming the box is checked).
            // Just rely on that for now.

            /*
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
            */

            return RestartResponse.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attempting to restart Conan Exiles server at IP {ip}:{port}.", server.Hostname, server.ServerPort);
            return RestartResponse.Exception;
        }
        finally
        {
            try { rcon?.Disconnect(); } catch { }
            _semaphore.Release();
        }
    }
}
