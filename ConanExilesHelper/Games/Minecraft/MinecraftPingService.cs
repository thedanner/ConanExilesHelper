using ConanExilesHelper.Games.Minecraft.Models;
using ConanExilesHelper.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConanExilesHelper.Games.Minecraft;

// Adapted from https://gist.github.com/csh/2480d14fbbb33b4bbae3
public class MinecraftPingService : IMinecraftPingService
{
    private static readonly object _lock = new();

    private const int ProtocolVersion = 47;
    private const int StateStatus = 1;

    private readonly ILogger<MinecraftPingService> _logger;
    private readonly IPingThrottler _pingThrottler;

    public MinecraftPingService(ILogger<MinecraftPingService> logger, IPingThrottler pingThrottler)
    {
        _logger = logger;
        _pingThrottler = pingThrottler;
    }

    public async Task<MinecraftPingPayload?> PingAsync(string hostname, ushort port)
    {
        lock (_lock)
        {
            if (!_pingThrottler.TryCanPing()) return null;
        }

        using var client = new TcpClient();
        await client.ConnectAsync(hostname, port);

        if (!client.Connected)
        {
            throw new Exception("Client failed to connect to Minecraft server.");
        }

        using var stream = client.GetStream();

        // https://wiki.vg/Server_List_Ping

        SendHandshakePacket(hostname, port, stream);

        SendStatusRequestPacket(stream);

        var length = ReadVarInt(stream);
        var packet = ReadVarInt(stream);
        var jsonLength = ReadVarInt(stream);
        var json = ReadString(stream, jsonLength);
        var ping = JsonSerializer.Deserialize<MinecraftPingPayload>(json);

        return ping;
    }

    private void SendHandshakePacket(string hostname, ushort port, NetworkStream stream)
    {
        // http://wiki.vg/Server_List_Ping#Ping_Process

        var requestData = new MemoryStream();

        WriteVarInt(requestData, ProtocolVersion); // Protocol Version
        WriteString(requestData, hostname); // Server Address
        WriteUShort(requestData, port); // Server Port
        WriteVarInt(requestData, StateStatus); // Next State. 1 for status, 2 for login.
        Flush(requestData, stream, 0);
    }

    private void SendStatusRequestPacket(NetworkStream stream)
    {
        // Send a "Status Request" packet
        // http://wiki.vg/Server_List_Ping#Ping_Process
        Flush(null, stream, 0);
    }

    internal static byte[] Read(Stream stream, int length)
    {
        var data = new byte[length];
        var totalRead = 0;
        while (length - totalRead > 0)
        {
            var leftToRead = length - totalRead;
            var readCount = stream.Read(data, totalRead, leftToRead);
            totalRead += readCount;
        }
        return data;
    }

    internal static int ReadVarInt(Stream stream)
    {
        var value = 0;
        var size = 0;
        int b;
        while (((b = stream.ReadByte()) & 0x80) == 0x80)
        {
            value |= (b & 0x7F) << size++ * 7;
            if (size > 5)
            {
                throw new IOException("This VarInt is an imposter!");
            }
        }
        return value | (b & 0x7F) << size * 7;
    }

    internal static string ReadString(Stream stream, int length)
    {
        var data = Read(stream, length);
        return Encoding.UTF8.GetString(data);
    }

    internal static void WriteVarInt(MemoryStream inputBuffer, int value)
    {
        while ((value & 128) != 0)
        {
            inputBuffer.WriteByte((byte)(value & 127 | 128));
            value = (int)(uint)value >> 7;
        }
        inputBuffer.WriteByte((byte)value);
    }

    internal static void WriteUShort(MemoryStream inputBuffer, ushort value)
    {
        var bytes = BitConverter.GetBytes(value);
        for (var i = 0; i < bytes.Length; i++)
        {
            inputBuffer.WriteByte(bytes[i]);
        }
    }

    internal static void WriteString(MemoryStream inputBuffer, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        WriteVarInt(inputBuffer, bytes.Length);
        for (var i = 0; i < bytes.Length; i++)
        {
            inputBuffer.WriteByte(bytes[i]);
        }
    }

    internal static void Write(MemoryStream inputBuffer, byte b)
    {
        inputBuffer.WriteByte(b);
    }

    internal static void Flush(MemoryStream? requestData, NetworkStream stream, int id)
    {
        // https://wiki.vg/Protocol

        // Uncompressed packet format
        // Length	VarInt	Length of Packet ID + Data
        // Packet ID   VarInt
        // Data    Byte Array  Depends on the connection state and packet ID, see the sections below

        var requestDataLength = 0;
        if (requestData is not null)
        {
            requestDataLength = (int)requestData.Length;
        }

        using var packetIdBuffer = new MemoryStream(4);

        var add = 0;
        if (id >= 0)
        {
            WriteVarInt(packetIdBuffer, id);
            add = (int)packetIdBuffer.Length;
        }
        else
        {
            packetIdBuffer.WriteByte(0x00);
        }

        using var lengthBuffer = new MemoryStream(4);
        WriteVarInt(lengthBuffer, requestDataLength + add);
        // Write length part.
        lengthBuffer.Position = 0;
        lengthBuffer.CopyTo(stream, 4);

        // Write PacketId part.
        packetIdBuffer.Position = 0;
        packetIdBuffer.CopyTo(stream, 4);

        // Write payload (or nothing if there is no payload).
        if (requestData is not null)
        {
            requestData.Position = 0;
            requestData.CopyTo(stream);

            requestData.Position = 0;
            requestData.SetLength(0);
        }
        else
        {
            stream.Write(Array.Empty<byte>(), 0, 0);
        }
    }
}
