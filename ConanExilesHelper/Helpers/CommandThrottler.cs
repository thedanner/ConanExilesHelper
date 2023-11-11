using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Helpers;

public class CommandThrottler : ICommandThrottler
{
    private readonly SemaphoreSlim _semaphore = new(1);

    private readonly TimeSpan _minimumWait;

    private DateTimeOffset _lastCommand;

    public CommandThrottler() : this(TimeSpan.FromSeconds(10))
    {

    }

    public CommandThrottler(TimeSpan minimumWait)
    {
        if (minimumWait < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(minimumWait), "Value must be positive.");

        _minimumWait = minimumWait;
    }

    public async Task<bool> TryCanRunCommandAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            var now = DateTimeOffset.Now;
            if (_lastCommand + _minimumWait < now)
            {
                _lastCommand = now;
                return true;
            }
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
