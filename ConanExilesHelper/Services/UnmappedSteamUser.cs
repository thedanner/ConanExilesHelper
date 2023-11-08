using System;

namespace ConanExilesHelper.Services;

public class UnmappedSteamUser
{
    public UnmappedSteamUser(string name, string steamId)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
        }

        if (string.IsNullOrEmpty(steamId))
        {
            throw new ArgumentException($"'{nameof(steamId)}' cannot be null or empty", nameof(steamId));
        }

        Name = name;
        SteamId = steamId;
    }

    public string Name { get; private set; }
    public string SteamId { get; private set; }
}
