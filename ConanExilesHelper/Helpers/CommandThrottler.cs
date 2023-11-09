using System;

namespace ConanExilesHelper.Helpers;

public class CommandThrottler : ICommandThrottler
{
    private readonly object _lock = new();

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

    public bool TryCanRunCommand()
    {
        lock (_lock)
        {
            var now = DateTimeOffset.Now;
            if (_lastCommand + _minimumWait < now)
            {
                _lastCommand = now;
                return true;
            }
            return false;
        }
    }
}
