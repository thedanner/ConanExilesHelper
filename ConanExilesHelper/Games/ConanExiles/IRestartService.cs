using System.Threading.Tasks;

namespace ConanExilesHelper.Games.ConanExiles;

public interface IRestartService
{
    Task<RestartResponse> RestartAsync();
}

public enum RestartResponse
{
    Success,
    Exception,
    RestartInProgress,
    ServerNotEmpty,
    CouldntFindServerProcess,
    Throttled,
    InvalidRconPassword
}
