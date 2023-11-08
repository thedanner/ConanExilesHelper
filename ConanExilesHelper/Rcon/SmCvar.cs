using CoreRCON.Parsers;
using System.Text.RegularExpressions;

namespace ConanExilesHelper.Rcon;

// Based on https://github.com/Challengermode/CoreRcon/blob/master/src/CoreRCON/Parsers/Standard/Status.cs .

public class SmCvar : IParseable
{
    public SmCvar()
    {
        Name = "";
        Value = "";
    }

    public string Name { get; set; }
    public string Value { get; set; }
}

public class SmCvarParser : IParser<SmCvar>
{
    public string Pattern => throw new System.NotImplementedException();

    public bool IsMatch(string input)
    {
        return input.StartsWith("[SM] Value of cvar \"");
    }

    public SmCvar Load(GroupCollection groups)
    {
        throw new System.NotImplementedException();
    }

    public SmCvar Parse(string input)
    {
        // E.g., [SM] Value of cvar "mp_gamemode": "versus"
        var cvarNameStart = input.IndexOf('"') + 1;
        var cvarNameEnd = input.IndexOf('"', cvarNameStart);
        var cvarName = input.Substring(cvarNameStart, cvarNameEnd - cvarNameStart);

        var cvarValueStart = cvarNameEnd + 4;
        var cvarValueEnd = input.Length - 1;
        var cvarValue = input.Substring(cvarValueStart, cvarValueEnd - cvarValueStart);

        return new SmCvar
        {
            Name = cvarName,
            Value = cvarValue
        };
    }

    public SmCvar Parse(Group group)
    {
        throw new System.NotImplementedException();
    }
}
