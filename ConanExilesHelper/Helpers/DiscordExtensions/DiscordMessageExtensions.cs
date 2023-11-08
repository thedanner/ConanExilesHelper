using Discord;
using System;

namespace ConanExilesHelper.Helpers.DiscordExtensions;

public static class DiscordMessageExtensions
{
    // https://github.com/discord-net/Discord.Net/issues/1926#issuecomment-927549728
    public static string ToDiscordEmoteRef(this ulong id) => $"<:em:{id}>";

    // https://www.reddit.com/r/discordapp/comments/ob2h2l/discord_added_new_timestamp_formatting/
    /*
    Use <t:TIMESTAMP:FLAG> to send it
    Available flags:
    t: Short time (e.g 9:41 PM)
    T: Long Time (e.g. 9:41:30 PM)
    d: Short Date (e.g. 30/06/2021)
    D: Long Date (e.g. 30 June 2021)
    f (default): Short Date/Time (e.g. 30 June 2021 9:41 PM)
    F: Long Date/Time (e.g. Wednesday, June, 30, 2021 9:41 PM)
    R: Relative Time (e.g. 2 months ago, in an hour)
    */

    public static string ToDiscordMessageTs(this DateTimeOffset when, TimestampFormat format = TimestampFormat.ShortDateTime)
        => $"<t:{when.ToUnixTimeSeconds()}:{format.ToDiscordFormatStringValue()}>";

    public static string ToDiscordFormatStringValue(this TimestampFormat format) => new string(new[] { (char)format });

    public static string ToMessageRef(this IChannel channel) => ToDiscordChannelIdMessageRef(channel.Id);
    public static string ToDiscordChannelIdMessageRef(this ulong channelId) => $"<#{channelId}>";

    public static string ToMessageRef(this IUser user) => ToDiscordUserIdMessageRef(user.Id);
    public static string ToDiscordUserIdMessageRef(this ulong userId) => $"<@{userId}>";
}
