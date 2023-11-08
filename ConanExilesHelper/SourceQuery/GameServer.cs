using ConanExilesHelper.SourceQuery.Rules;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Checksum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.SourceQuery;

[Serializable]
public class GameServer<TRules> where TRules : new()
{
    [NonSerialized]
    private IPEndPoint? _endpoint;
    [NonSerialized]
    private UdpClient? _client;
    [NonSerialized]
    private readonly IRuleParser<TRules> _ruleParser;

    // TSource Engine Query
    [NonSerialized]
    private static readonly byte[] A2S_INFO = { 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
    [NonSerialized]
    private static readonly byte[] A2S_SERVERQUERY_GETCHALLENGE = { 0x55, 0xFF, 0xFF, 0xFF, 0xFF };
    [NonSerialized]
    private static readonly byte A2S_PLAYER = 0x55;
    [NonSerialized]
    private static readonly byte A2S_RULES = 0x56;
    [NonSerialized]
    private byte[]? _challengeBytes;

    public byte NetworkVersion;
    public string? Name;
    public string? Map;
    public string? GameDirectory;
    public string? GameDescription;
    public short AppId;
    public byte PlayerCount;
    public byte MaximumPlayerCount;
    public byte BotCount;
    public ServerType ServerType;
    public OperatingSystem OS;
    public bool RequiresPassword;
    public bool VACSecured;
    public string? GameVersion;
    public short Port = 27015;
    public string? SteamId;
    public short SpectatorPort;
    public string? SpectatorName;
    public string? GameTagData;
    public string? GameID;

    public List<PlayerInfo> Players { get; set; }

    public Dictionary<string, string> RawRules;

    public TRules Rules { get; set; }
    public string? Endpoint { get; set; }

    public GameServer(IRuleParser<TRules> ruleParser)
    {
        Players = new List<PlayerInfo>();
        RawRules = new Dictionary<string, string>();
        Rules = new();

        _ruleParser = ruleParser ?? throw new ArgumentNullException(nameof(ruleParser));
    }

    public async Task QueryAsync(IPAddress address, CancellationToken cancellationToken) =>
        await QueryAsync(new IPEndPoint(address, 27015), cancellationToken);

    public async Task QueryAsync(IPEndPoint endpoint, CancellationToken cancellationToken)
    {
        _endpoint = endpoint;
        Endpoint = endpoint.ToString();

        // BUG nulling this out without recreating the client will break if any of the RefreshX() methods are ever called
        // after the ctor returns.
        using (_client = new UdpClient())
        {
            _client.Client.SendTimeout = (int)500;
            _client.Client.ReceiveTimeout = (int)500;
            //_client.Connect(endpoint);

            await RefreshMainInfoAsync(cancellationToken);
            await RefreshPlayerInfoAsync(cancellationToken);
            await RefreshRulesAsync(cancellationToken);
        }
        _client = null;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name: " + Name);
        sb.AppendLine("Map: " + Map);
        sb.AppendLine("Players: " + PlayerCount + "/" + MaximumPlayerCount);
        sb.AppendLine("ServerType: " + ServerType);
        sb.AppendLine("OS: " + OS);
        sb.AppendLine("Port: " + Port);
        sb.AppendLine("GameData: " + GameTagData);
        return sb.ToString();
    }

    public async Task RefreshMainInfoAsync(CancellationToken cancellationToken)
    {
        await SendAsync(A2S_INFO, cancellationToken);
        var infoData = await ReceiveAsync(cancellationToken);

        // Challenge
        if (infoData[0] == 0x41)
        {
            var requestWithChallenge = new byte[A2S_INFO.Length + infoData.Length - 1];

            Buffer.BlockCopy(A2S_INFO, 0, requestWithChallenge, 0, A2S_INFO.Length);
            Buffer.BlockCopy(infoData, 1, requestWithChallenge, A2S_INFO.Length, infoData.Length - 1);

            await SendAsync(requestWithChallenge, cancellationToken);
            infoData = await ReceiveAsync(cancellationToken);
        }

        using (var br = new BinaryReader(new MemoryStream(infoData)))
        {
            br.ReadByte(); // type byte, not needed

            NetworkVersion = br.ReadByte();
            Name = br.ReadNullTerminatedString();
            Map = br.ReadNullTerminatedString();
            GameDirectory = br.ReadNullTerminatedString();
            GameDescription = br.ReadNullTerminatedString();
            AppId = br.ReadInt16();
            PlayerCount = br.ReadByte();
            MaximumPlayerCount = br.ReadByte();
            BotCount = br.ReadByte();
            ServerType = (ServerType)br.ReadByte();
            OS = (OperatingSystem)br.ReadByte();
            RequiresPassword = br.ReadByte() == 0x01;
            VACSecured = br.ReadByte() == 0x01;
            GameVersion = br.ReadNullTerminatedString();
            var edf = (ExtraDataFlags)br.ReadByte();

            if (edf.HasFlag(ExtraDataFlags.GamePort)) Port = br.ReadInt16();
            if (edf.HasFlag(ExtraDataFlags.SteamID)) SteamId = br.ReadUInt64().ToString();
            if (edf.HasFlag(ExtraDataFlags.SpectatorInfo))
            {
                SpectatorPort = br.ReadInt16();
                SpectatorName = br.ReadNullTerminatedString();
            }
            if (edf.HasFlag(ExtraDataFlags.GameTagData)) GameTagData = br.ReadNullTerminatedString();
            if (edf.HasFlag(ExtraDataFlags.GameID)) GameID = br.ReadUInt64().ToString();
        }
    }

    public async Task RefreshPlayerInfoAsync(CancellationToken cancellationToken)
    {
        Players.Clear();
        await GetChallengeDataAsync(cancellationToken);

        if (_challengeBytes is null) throw new Exception("_challengeBytes is null.");

        _challengeBytes[0] = A2S_PLAYER;
        await SendAsync(_challengeBytes, cancellationToken);
        var playerData = await ReceiveAsync(cancellationToken);
        
        using (var br = new BinaryReader(new MemoryStream(playerData)))
        {
            if (br.ReadByte() != 0x44) throw new Exception("Invalid data received in response to A2S_PLAYER request");
            var numPlayers = br.ReadByte();
            for (int index = 0; index < numPlayers; index++)
            {
                Players.Add(PlayerInfo.FromBinaryReader(br));
            }
        }
    }

    public async Task RefreshRulesAsync(CancellationToken cancellationToken)
    {
        RawRules.Clear();

        await GetChallengeDataAsync(cancellationToken);

        if (_challengeBytes is null) throw new Exception("_challengeBytes is null.");

        _challengeBytes[0] = A2S_RULES;
        await SendAsync(_challengeBytes, cancellationToken);
        var ruleData = await ReceiveAsync(cancellationToken);

        using (var ms = new MemoryStream(ruleData))
        using (var br = new BinaryReader(ms))
        {
            // skip padding
            var padding = new[]
            {
                br.ReadByte(),
                br.ReadByte(),
                br.ReadByte(),
                br.ReadByte(),
            };

            // Some games, like apparently Conan Exiles, don't return padding.
            if (padding[0] != 0xFF
                && padding[1] != 0xFF
                && padding[2] != 0xFF
                && padding[3] != 0xFF)
            {
                ms.Position -= padding.Length;
            }

            // Char value of 'E'
            if (br.ReadByte() != 0x45) throw new Exception("Invalid data received in response to A2S_RULES request");
            var numRules = br.ReadUInt16();

            for (int index = 0; index < numRules; index++)
            {
                RawRules.Add(br.ReadNullTerminatedString(), br.ReadNullTerminatedString());
            }
            Rules = _ruleParser.FromDictionary(RawRules);
        }
    }

    private async Task GetChallengeDataAsync(CancellationToken cancellationToken)
    {
        if (_challengeBytes is not null) return;
        
        await SendAsync(A2S_SERVERQUERY_GETCHALLENGE, cancellationToken);
        var challengeData = await ReceiveAsync(cancellationToken);
        if (challengeData[0] != 0x41) throw new Exception("Unable to retrieve challenge data");
        _challengeBytes = challengeData;            
    }

    private async Task SendAsync(byte[] message, CancellationToken cancellationToken)
    {
        var fullmessage = new byte[4 + message.Length];
        fullmessage[0] = fullmessage[1] = fullmessage[2] = fullmessage[3] = 0xFF;

        Buffer.BlockCopy(message, 0, fullmessage, 4, message.Length);
        
        if (_client is null) throw new Exception("_client is null.");

        var datagram = new ReadOnlyMemory<byte>(fullmessage);
        var bytesSent = await _client.SendAsync(datagram, _endpoint, cancellationToken);
    }

    private async Task<byte[]> ReceiveAsync(CancellationToken cancellationToken)
    {
        var packets = new List<byte[]>();
        var crc = 0;

        if (_client is null) throw new Exception("_client is null.");

        var result = await _client.ReceiveAsync(cancellationToken);
        var packet = result.Buffer;

        packets.Add(packet);

        using (var br = new BinaryReader(new MemoryStream(packet)))
        {
            // Adapted from https://github.com/ValvePython/steam/blob/6f955c4b4d28dc800c38f10a33f9a18e09b83c51/steam/game_servers.py#L248

            if (br.ReadInt32() == -2) // Multiple packets
            {
                var payloadOffset = -1;

                // locate first packet and handle out of order packets
                while (payloadOffset == -1)
                {
                    payloadOffset = packet.FindSequence(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, 18);

                    // locate payload offset in compressed packet
                    if (payloadOffset == -1)
                    {
                        payloadOffset = packet.FindSequence(Encoding.ASCII.GetBytes("BZh"), 0, 21);
                    }

                    //  if we still haven't found the offset receive the next packet
                    if (payloadOffset == -1)
                    {
                        packet = _client.Receive(ref _endpoint);
                        packets.Add(packet);
                    }
                }

                // read header
                (int packetIndex, int packetCount, bool isCompressed) = UnpackMultipakcetHeader(payloadOffset, packet);

                if (packetIndex != 0) throw new Exception("Unexpected first packet index");

                // Receive any remaining packets
                while (packets.Count < packetCount)
                {
                    var nextPacket = _client.Receive(ref _endpoint);
                    packets.Add(nextPacket);
                }

                // ensure packets are in correct order
                packets.Sort((a, b) =>
                {
                    var aData = UnpackMultipakcetHeader(payloadOffset, a);
                    var bData = UnpackMultipakcetHeader(payloadOffset, b);

                    return aData.packetIndex.CompareTo(bData.packetIndex);
                });

                // reconstruct full response
                var fullData = Combine(packets, payloadOffset);

                if (isCompressed)
                {
                    fullData = Decompress(fullData);
                    var crc32 = new Crc32();
                    crc32.Update(fullData);
                    if (crc32.Value != crc) throw new Exception("Invalid CRC for compressed packet data");
                }

                return fullData;
            }

            // Single packet
            var data = new byte[packet.Length - br.BaseStream.Position];
            Buffer.BlockCopy(packet, (int)br.BaseStream.Position, data, 0, data.Length);
            return data;
        }
    }

    private (int packetIndex, int packetCount, bool isCompressed) UnpackMultipakcetHeader(int payloadOffset, byte[] packet)
    {
        using var br = new BinaryReader(new MemoryStream(packet));

        if (payloadOffset == 9) // GoldSrc
        {
            var packetByte = br.ReadByte();
            return (packetByte >> 2, packetByte & 0xF, false); // packetIndex, packetCount, isCompressed
        }
        else if (new[] { 10, 12, 18 }.Contains(payloadOffset)) // Source
        {
            // Skip 4 bytes.
            br.ReadByte();
            br.ReadByte();
            br.ReadByte();
            br.ReadByte();

            var packetId = br.ReadInt32();
            var packetCount = br.ReadByte();
            var packetIndex = br.ReadByte();

            return (packetIndex, packetCount, (packetId & 0x80000000) != 0);
        }
        throw new Exception($"Unexpected payload_offset - {payloadOffset}");
    }

    private static byte[] Decompress(byte[] combinedData)
    {
        using var compressedData = new MemoryStream(combinedData);
        using var uncompressedData = new MemoryStream();
        BZip2.Decompress(compressedData, uncompressedData, true);
        return uncompressedData.ToArray();
    }

    private static byte[] Combine(List<byte[]> arrays, int payloadOffset)
    {
        var rv = new byte[arrays.Sum(a => Math.Max(a.Length - payloadOffset, 0))];
        var offset = 0;
        foreach (byte[] array in arrays)
        {
            var count = Math.Max(array.Length - payloadOffset, 0);
            Buffer.BlockCopy(array, payloadOffset, rv, offset, count);
            offset += count;
        }
        return rv;
    }
}

public class GameServer : GameServer<Dictionary<string, string>>
{
    public GameServer() : base(new DictionaryOnlyRuleParser())
    {
    }
}

public static class Extensions
{
    public static int FindSequence(this byte[] source, byte[] pattern)
    {
        return FindSequence(source, pattern, 0);
    }

    public static int FindSequence(this byte[] source, byte[] pattern, int startIndex)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        return FindSequence(source, pattern, startIndex, source.Length);
    }

    public static int FindSequence(this byte[] source, byte[] pattern, int startIndex, int endIndex)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (source.Length == 0) throw new ArgumentException("Source cannot be empty.", nameof(source));
        if (pattern is null) throw new ArgumentNullException(nameof(pattern));
        if (pattern.Length == 0) throw new ArgumentException("Pattern cannot be empty.", nameof(pattern));
        if (startIndex < 0 || startIndex > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index must refer to an index in the source array.");
        }
        if (endIndex < 0 || endIndex > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(endIndex), "End index must refer to an index in the source array.");
        }
        if (pattern.Length > source.Length) throw new ArgumentException("The pattern length must be sorter than the source length.");
        if (startIndex >= endIndex) throw new ArgumentException("The start index must be before the end index.");

        var runLength = 0;
        for (var i = startIndex; i < endIndex; i++)
        {
            if (source[i] == pattern[runLength])
            {
                runLength++;
            }
            else
            {
                runLength = 0;
            }
            if (runLength == pattern.Length) return i - runLength + 1;
            if (i + pattern.Length - runLength >= endIndex) return -1;
        }
        return -1;
    }
}
