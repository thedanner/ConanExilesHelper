using System;

namespace ConanExilesHelper.Helpers;

public static class Constants
{
    public const int DelayAfterCommandMs = 250;
    public static readonly TimeSpan DelayAfterCommand = TimeSpan.FromMilliseconds(DelayAfterCommandMs);

    public const string GroupL4d = "l4d";
    public const string GroupL4d2 = "l4d2";
    public const string GroupLfd = "lfd";
    public const string GroupLfd2 = "lfd2";
}
