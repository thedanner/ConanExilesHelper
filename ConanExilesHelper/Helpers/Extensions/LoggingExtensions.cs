using Discord;
using Microsoft.Extensions.Logging;

namespace ConanExilesHelper.Helpers.Extensions;

public static class LoggingExtensions
{
    public static LogLevel ToLogLevel(this LogSeverity severity)
    {
        var level = severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information,
        };
        return level;
    }
}
