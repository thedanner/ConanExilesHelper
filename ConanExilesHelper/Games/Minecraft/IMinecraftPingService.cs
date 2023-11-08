using ConanExilesHelper.Games.Minecraft.Models;
using System.Threading.Tasks;

namespace ConanExilesHelper.Games.Minecraft;

public interface IMinecraftPingService
{
    Task<MinecraftPingPayload?> PingAsync(string hostname, ushort port);
}
