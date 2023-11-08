using System;
using System.Collections.Generic;
using System.Linq;

namespace ConanExilesHelper.Models.Configuration;

public class UserMapping
{
    private readonly List<string> _steamIds;

    public UserMapping()
    {
        Name = "";

        _steamIds = new List<string>();
    }

    public string Name { get; set; }

    public ulong DiscordId { get; set; }

    public string SteamId
    {
        // This needs to be a public accessor because the settings framework skips it if it's not.
        [Obsolete("Use the SteamIds property.", true)]
        get => _steamIds.FirstOrDefault() ?? "<none>";
        set => _steamIds.Add(value);
    }

    public List<string> SteamIds
    {
        get
        {
            var fullIds = new List<string>(_steamIds.Count + 1);
            
            foreach (var id in _steamIds)
            {
                if (string.IsNullOrEmpty(id)) continue;

                if (!fullIds.Contains(id))
                {
                    fullIds.Add(id);
                }

                if (id.StartsWith("STEAM_0:"))
                {
                    var altId = id.Replace("STEAM_0:", "STEAM_1:");

                    if (!fullIds.Contains(altId))
                    {
                        fullIds.Add(altId);
                    }
                }
            }
            return fullIds;
        }
        set => _steamIds.AddRange(value);
    }

    public override string ToString() => $"{Name} [SteamIds:{string.Join(",", SteamIds)}, DiscordId:{DiscordId}]";
}
