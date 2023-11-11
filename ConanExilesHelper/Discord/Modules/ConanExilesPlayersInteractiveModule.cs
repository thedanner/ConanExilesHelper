using Discord;
using Discord.Interactions;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Games.ConanExiles;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConanExilesHelper.Discord.Modules;

public class ConanExilesPlayersInteractiveModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<ConanExilesPlayersInteractiveModule> _logger;
    private readonly ConanExilesSettings? _settings;
    private readonly IPingService _pingService;
    private readonly IRestartService _restartService;

    public ConanExilesPlayersInteractiveModule(ILogger<ConanExilesPlayersInteractiveModule> logger,
        IOptions<ConanExilesSettings>? conanExilesSettings,
        IPingService pingService,
        IRestartService restartService)
    {
        _settings = conanExilesSettings?.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pingService = pingService ?? throw new ArgumentNullException(nameof(pingService));
        _restartService = restartService ?? throw new ArgumentNullException(nameof(restartService));
    }

    [SlashCommand("who", "Gets who's playing on our Conan Exiles server.")]
    public async Task HandlePingAsync()
    {
        await DeferAsync();
        await Task.Delay(Constants.DelayAfterCommand);

        if (_settings is null)
        {
            await DeleteOriginalResponseAsync();
            await Task.Delay(Constants.DelayAfterCommand);
            return;
        }

        try
        {
            if (_settings.ChannelIdFilter?.Any() == true)
            {
                var channelFilter = _settings.ChannelIdFilter!;
                if (!channelFilter.Contains(Context.Channel.Id)) return;
            }

            var server = _settings.Server;

            if (server is null) return;

            var response = await _pingService.PingAsync(server.QueryHostname, server.QueryPort);

            if (response is null)
            {
                _logger.LogInformation("No payload returned from the ping method; it may be throttled.");
                return;
            }

            var plural = response.MaximumPlayerCount == 1 ? "" : "s";
            var embedBuilder = new EmbedBuilder
            {
                Title = response.GameDescription ?? response.Name ?? server.Name,
                Description =
                    $"Server name: *{response.Name}*" +
                    $"\n{response.Players.Count} of {response.MaximumPlayerCount} player{plural}" +
                    $"\nplaying on *{response.Map}*"
            };

            var playersMessage = new StringBuilder();

            if (response.Players.Any())
            {
                foreach (var player in response.Players)
                {
                    playersMessage.Append('\n').Append(player.Name);
                }
            }
            else
            {
                playersMessage.Append("*Nobody is playing right now.*");
            }

            embedBuilder
                .AddField("Players", playersMessage.ToString())
                .WithColor(Color.DarkRed)
                .WithFooter($"{server.Hostname}:{server.ServerPort}");

            await FollowupAsync(null, embed: embedBuilder.Build());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in {className}.{methodName}().", nameof(ConanExilesPlayersInteractiveModule), nameof(HandlePingAsync));

            await FollowupAsync($"Sorry, there was an error. My logs have more information.");
        }
    }

    [SlashCommand("restart", "Restarts our Conan Exiles server if no one's playing.")]
    public async Task HandleRestartAsync()
    {
        await DeferAsync();
        await Task.Delay(Constants.DelayAfterCommand);

        if (_settings is null)
        {
            await DeleteOriginalResponseAsync();
            await Task.Delay(Constants.DelayAfterCommand);
            return;
        }

        try
        {
            if (_settings.ChannelIdFilter?.Any() == true)
            {
                var channelFilter = _settings.ChannelIdFilter!;
                if (!channelFilter.Contains(Context.Channel.Id)) return;
            }

            var server = _settings.Server;

            if (server is null) return;

            var response = await _restartService.RestartAsync();

            string message;
            switch (response)
            {
                case RestartResponse.Success: message = "Server restarted! Please wait a couple of minutes before you try connecting."; break;
                case RestartResponse.Exception: message = "An error has occurred, sorry :("; break;
                case RestartResponse.RestartInProgress: message = "A restart request is already in progress, please wait a couple of minutes."; break;
                case RestartResponse.ServerNotEmpty: message = "The server isn't empty."; break;
                case RestartResponse.CouldntFindServerProcess: message = "I'm having trouble checking if the server is running or not."; break;
                case RestartResponse.Throttled: message = "A restart happened not too long ago, please wait a couple of minutes."; break;
                case RestartResponse.InvalidRconPassword: message = "I have an incorrect RCON password configured."; break;
                default: message = $"I don't have a message for whatever happened, sorry ({response})."; break;
            }

            await FollowupAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in {className}.{methodName}().", nameof(ConanExilesPlayersInteractiveModule), nameof(HandleRestartAsync));

            await FollowupAsync($"Sorry, there was an error. My logs have more information.");
        }
    }
}
