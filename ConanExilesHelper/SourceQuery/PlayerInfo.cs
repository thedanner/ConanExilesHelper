using System;
using System.IO;
using System.Text;

namespace ConanExilesHelper.SourceQuery;

[Serializable]
public class PlayerInfo
{
    public string? Name;
    public int Score;
    public TimeSpan TimeConnected;

    public static PlayerInfo FromBinaryReader(BinaryReader br)
    {
        var playerInfo = new PlayerInfo();
        byte index = br.ReadByte();
        playerInfo.Name = br.ReadNullTerminatedString();
        playerInfo.Score = br.ReadInt32();
        playerInfo.TimeConnected = TimeSpan.FromSeconds(br.ReadSingle());
        return playerInfo;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name: " + Name);
        sb.AppendLine("Score: " + Score);
        sb.AppendLine("TimeConnected: " + TimeConnected);
        
        return sb.ToString();
    }
}
