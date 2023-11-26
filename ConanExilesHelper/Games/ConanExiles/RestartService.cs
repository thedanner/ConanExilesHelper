using ConanExilesHelper.Configuration;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Services;
using ConanExilesHelper.SourceQuery;
using ConanExilesHelper.SourceQuery.Rules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RconSharp;
using System;
using System.Net;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Games.ConanExiles;

public class RestartService : IRestartService
{
    private static readonly SemaphoreSlim _semaphore = new(1);
    private static readonly ICommandThrottler _pingThrottler = new CommandThrottler(TimeSpan.FromSeconds(10));
    private static readonly ICommandThrottler _restartThrottler = new CommandThrottler(TimeSpan.FromMinutes(5));

    private readonly ILogger<RestartService> _logger;
    private readonly IConanServerUtils _serverUtils;
    private readonly ConanExilesSettings _settings;


    public RestartService(ILogger<RestartService> logger, IConanServerUtils serverUtils, IOptions<ConanExilesSettings>? settings)
    {
        _logger = logger;
        _serverUtils = serverUtils;
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
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

            // The throttler checks should all be inside the area protected by _semaphore to prevent other race conditions.

            if (!await _pingThrottler.CanRunCommandAsync()) return RestartResponse.PingThrottled;
            if (!await _restartThrottler.CanRunCommandAsync()) return RestartResponse.RestartThrottled;

            var gs = new GameServer<ConanExilesRules>(ConanExilesRules.Parser);

            var endpoint = new IPEndPoint(IPAddress.Parse(server.QueryHostname), server.QueryPort);

            await gs.QueryAsync(endpoint, CancellationToken.None);

            await _pingThrottler.StartTimeoutAsync();

            if (gs.Players.Count > 0) return RestartResponse.ServerNotEmpty;

            var process = _serverUtils.GetConanServerProcess();
            if (process is null) return RestartResponse.CouldntFindServerProcess;

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

            await _restartThrottler.StartTimeoutAsync();

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
