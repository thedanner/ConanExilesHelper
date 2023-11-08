using Discord.WebSocket;
using ConanExilesHelper.Wrappers.Rcon;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Services;

public interface IDiscordChatMover
{
    Task<MoveResult> MovePlayersToCorrectChannelsAsync(
        IRCONWrapper rcon,
        DiscordSocketClient client,
        SocketGuild guild,
        SocketVoiceChannel usedInChannel,
        CancellationToken cancellationToken);

    Task<ReuniteResult> RenuitePlayersAsync(
        DiscordSocketClient client,
        SocketGuild guild,
        SocketVoiceChannel usedInChannel,
        CancellationToken cancellationToken);
}
