using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Helpers;

public class CommandThrottler
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

    public async Task<bool> CanRunCommandAsync()
    {
        return await HandleRequest(false);
    }

    public async Task ForceStartTimeoutAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            _lastCommand = DateTimeOffset.Now;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryStartTimeoutAsync()
    {
        return await HandleRequest(true);
    }

    private async Task<bool> HandleRequest(bool update)
    {
        await _semaphore.WaitAsync();

        try
        {
            var now = DateTimeOffset.Now;
            if (_lastCommand + _minimumWait < now)
            {
                if (update) _lastCommand = now;
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
