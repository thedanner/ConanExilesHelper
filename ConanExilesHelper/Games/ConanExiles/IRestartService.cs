using System.Threading.Tasks;

namespace ConanExilesHelper.Games.ConanExiles;

public interface IRestartService
{
    Task<RestartResponse> TryRestartAsync();
}

public enum RestartResponse
{
    Success,
    Exception,
    RestartInProgress,
    ServerNotEmpty,
    CouldntFindServerProcess,
    RestartThrottled,
    PingThrottled,
    InvalidRconPassword
}
