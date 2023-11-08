using System;

namespace ConanExilesHelper.TeamSuggestions;

public struct SteamId
{
    private SteamId(ulong value)
    {
        Value = value;
    }

    public ulong Value { get; }


    public static SteamId Create(string steamId)
    {
        return new SteamId(FromStringId(steamId));
    }

    public static SteamId Create(ulong steamId) => new(steamId);



    // Adapted from https://gist.github.com/bcahue/4eae86ae1d10364bb66d
    private const ulong SteamId64Ident = 76561197960265728;


    public static ulong FromStringId(string stringId)
    {
        if (!stringId.StartsWith("STEAM_0:") && !stringId.StartsWith("STEAM_1:"))
        {
            throw new ArgumentException($"SteamId {stringId} is not in the correct format.", nameof(stringId));
        }

        var parts = stringId.Split(':', 3, StringSplitOptions.None);

        if (parts.Length != 3) throw new ArgumentException($"SteamId {stringId} is not in the correct format.", nameof(stringId));

        var id = ulong.Parse(parts[2]) * 2;

        if (id < 0) throw new ArgumentException($"SteamId {stringId} is not in the correct format.", nameof(stringId));

        if (parts[1] == "1") id++;

        id += SteamId64Ident;

        return id;
    }

    public static string ToStringId(ulong id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "ID must be positive.");

        var steamIdInOldFormat = "STEAM_1:";

        var ulongId = id - SteamId64Ident;

        steamIdInOldFormat += ulongId % 2 == 0
            ? "0:"
            : "1:";

        steamIdInOldFormat += ulongId / 2;

        return steamIdInOldFormat;
    }
}
