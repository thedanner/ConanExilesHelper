using System.Threading.Tasks;

namespace ConanExilesHelper.Helpers;

public interface ICommandThrottler
{
    Task<bool> CanRunCommandAsync();
    Task StartTimeoutAsync();
    Task<bool> TryStartTimeoutAsync();
}
