using CoreRCON.Parsers;
using System;
using System.Threading.Tasks;

namespace ConanExilesHelper.Wrappers.Rcon;

public interface IRCONWrapper : IDisposable
{
    Task ConnectAsync();
    Task<T> SendCommandAsync<T>(string command) where T : class, IParseable, new();
    Task<string> SendCommandAsync(string command);
}
