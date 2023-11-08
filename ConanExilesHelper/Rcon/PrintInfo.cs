using CoreRCON.Parsers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ConanExilesHelper.Rcon;

// Based on https://github.com/Challengermode/CoreRcon/blob/master/src/CoreRCON/Parsers/Standard/Status.cs .

public class PrintInfo : IParseable
{
    public static readonly int Unassigned = 0;
    public static readonly int TeamIndexSpectator = 1;
    public static readonly int TeamIndexSurvivor = 2;
    public static readonly int TeamIndexInfected = 3;

    public PrintInfo()
    {
        Players = new List<PrintInfoPlayer>();
    }

    public IList<PrintInfoPlayer> Players { get; set; }
}

public class PrintInfoPlayer
{
    public PrintInfoPlayer()
    {
        Name = "";
        ClientIndex = -1;
        SteamId = "";
        Unused1 = "";
        TeamIndex = -1;
        TeamName = "";
    }

    public string Name { get; set; }
    public int ClientIndex { get; set; }
    public string SteamId { get; set; }
    public string Unused1 { get; set; }
    /// <summary>
    /// For Left 4 Dead & 2: UNASSIGNED = 0, SPECTATOR = 1, SURVIVOR = 2, INFECTED = 3
    /// </summary>
    public int TeamIndex { get; set; }
    public string TeamName { get; set; }


    public static PrintInfoPlayer? Parse(string line)
    {
        // Output line is "[PI] %L<%i><%s>" in SourceMod string formatting.
        // https://wiki.alliedmods.net/Format_Class_Functions_(SourceMod_Scripting)
        // %L expands to 1<2><3><> where 1 is the player's name, 2 is the player's userid,
        // and 3 is the player's Steam ID. If the client index is 0, the string will be: Console<0><Console><Console>
        // The next <> is team #. SPECTATOR = 1, SURVIVORS = 2, INFECTED = 3
        // The final <> is team name (Survivors, Infected, Spectator maybe?)
        // So, that means, if elements are indexed by <> with the name being 0th:
        // [0]: player's name
        // [1]: player's userid
        // [2]: player's steamid
        // [3]: ???
        // [4]: player's team index
        // [5]: player's team name

        if (!line.EndsWith(">")) return null;

        var result = new PrintInfoPlayer();
        var sectionEndIndexInclusive = line.Length - 2; // omit the closing >.

        var sectionStartIndex = line.LastIndexOf("<");
        if (sectionStartIndex < 0) return null;

        result.TeamName = line.Substring(sectionStartIndex + 1, sectionEndIndexInclusive - sectionStartIndex);

        sectionEndIndexInclusive = sectionStartIndex - 2;
        sectionStartIndex = line.LastIndexOf("<", sectionEndIndexInclusive);
        if (sectionStartIndex < 0) return null;

        var teamIndexString = line.Substring(sectionStartIndex + 1, sectionEndIndexInclusive - sectionStartIndex);
        if (int.TryParse(teamIndexString, out int teamIndexInt))
        {
            result.TeamIndex = teamIndexInt;
        }
        else
        {
            result.TeamIndex = -1;
        }

        sectionEndIndexInclusive = sectionStartIndex - 2;
        sectionStartIndex = line.LastIndexOf("<", sectionEndIndexInclusive);
        if (sectionStartIndex < 0) return null;

        result.Unused1 = line.Substring(sectionStartIndex + 1, sectionEndIndexInclusive - sectionStartIndex);

        sectionEndIndexInclusive = sectionStartIndex - 2;
        sectionStartIndex = line.LastIndexOf("<", sectionEndIndexInclusive);
        if (sectionStartIndex < 0) return null;

        result.SteamId = line.Substring(sectionStartIndex + 1, sectionEndIndexInclusive - sectionStartIndex);

        sectionEndIndexInclusive = sectionStartIndex - 2;
        sectionStartIndex = line.LastIndexOf("<", sectionEndIndexInclusive);
        if (sectionStartIndex < 0) return null;

        var clientIndexString = line.Substring(sectionStartIndex + 1, sectionEndIndexInclusive - sectionStartIndex);
        if (int.TryParse(clientIndexString, out int clientIndexInd))
        {
            result.ClientIndex = clientIndexInd;
        }
        else
        {
            result.ClientIndex = -1;
        }

        result.Name = line.Substring(0, sectionStartIndex);

        return result;
    }
}

public class PrintInfoParser : IParser<PrintInfo>
{
    public static readonly string Prefix = "[PI] ";

    public string Pattern => throw new NotSupportedException();

    public bool IsMatch(string input)
    {
        return input.StartsWith(Prefix + "BEGIN") && input.EndsWith(Prefix + "END");
    }

    public PrintInfo Load(GroupCollection groups) => throw new NotSupportedException();

    public PrintInfo Parse(string input)
    {
        var output = new PrintInfo();

        var lines = input.Split(Environment.NewLine);
        if (lines.Length <= 2) return output;

        output.Players = new List<PrintInfoPlayer>(lines.Length - 2);

        for (var i = 1; i < lines.Length - 1; i++)
        {
            var line = lines[i];
            if (!line.StartsWith(Prefix)) continue;

            var player = PrintInfoPlayer.Parse(line.Substring(Prefix.Length));

            if (player is not null) output.Players.Add(player);
        }

        return output;
    }

    public PrintInfo Parse(Group group) => throw new NotSupportedException();
}
