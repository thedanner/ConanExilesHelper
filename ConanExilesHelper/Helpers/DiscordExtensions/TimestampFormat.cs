namespace ConanExilesHelper.Helpers.DiscordExtensions;

public enum TimestampFormat : byte
{
    ShortDateTime = (byte)'f', // f (default): Short Date/Time (e.g. 30 June 2021 9:41 PM)
    ShortTime = (byte)'t', // t: Short time (e.g 9:41 PM)
    LongTime = (byte)'T', //T: Long Time (e.g. 9:41:30 PM)
    ShortDate = (byte)'d', //d: Short Date (e.g. 30/06/2021)
    LongDate = (byte)'D', //D: Long Date (e.g. 30 June 2021)
    LongDateTime = (byte)'F', //F: Long Date/Time (e.g. Wednesday, June, 30, 2021 9:41 PM)
    RelativeTime = (byte)'R', //R: Relative Time (e.g. 2 months ago, in an hour)
}
