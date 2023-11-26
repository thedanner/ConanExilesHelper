using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ConanExilesHelper.Scheduling.Infrastructure;

public interface ITask
{
    Task RunTaskAsync(
        DiscordSocketClient client, IReadOnlyDictionary<string, object> taskSettings,
        CancellationToken cancellationToken);
}
