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
    private const string CommandName = "who";

    private readonly ILogger<ConanExilesPlayersInteractiveModule> _logger;
    private readonly IConanExilesPingService _conanExilesPingService;
    private readonly ConanExilesSettings? _conanExilesSettings;

    public ConanExilesPlayersInteractiveModule(ILogger<ConanExilesPlayersInteractiveModule> logger,
        IOptions<ConanExilesSettings>? conanExilesSettings,
        IConanExilesPingService conanExilesPingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _conanExilesPingService = conanExilesPingService ?? throw new ArgumentNullException(nameof(conanExilesPingService));
        _conanExilesSettings = conanExilesSettings?.Value;
    }

    [SlashCommand(CommandName, "Gets who's playing on our Conan Exiles server.")]
    public async Task HandleCommandAsync()
    {
        await DeferAsync();
        await Task.Delay(Constants.DelayAfterCommand);

        if (_conanExilesSettings is null)
        {
            await DeleteOriginalResponseAsync();
            await Task.Delay(Constants.DelayAfterCommand);
            return;
        }

        try
        {
            if (_conanExilesSettings.ChannelIdFilter?.Any() == true)
            {
                var channelFilter = _conanExilesSettings.ChannelIdFilter!;
                if (!channelFilter.Contains(Context.Channel.Id)) return;
            }

            var defaultServerName = _conanExilesSettings.DefaultServerName ?? "";
            var server = _conanExilesSettings.Servers.FirstOrDefault(s => s.Name == defaultServerName)
                ?? _conanExilesSettings.Servers.FirstOrDefault();

            if (server is null) return;

            var response = await _conanExilesPingService.PingAsync(server.QueryHostname, server.QueryPort);

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
            _logger.LogError(e, "Error in {className}.{methodName}().", nameof(ConanExilesPlayersInteractiveModule), nameof(HandleCommandAsync));

            await FollowupAsync($"Sorry, there was an error. My logs have more information.");
        }
    }
}
