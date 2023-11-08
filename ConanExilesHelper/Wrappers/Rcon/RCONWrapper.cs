using CoreRCON;
using CoreRCON.Parsers;
using System;
using System.Threading.Tasks;

namespace ConanExilesHelper.Wrappers.Rcon;

public class RCONWrapper : IRCONWrapper
{
    private readonly RCON _rcon;

    public RCONWrapper(RCON rcon)
    {
        _rcon = rcon ?? throw new ArgumentNullException(nameof(rcon));
    }

    public virtual event Action OnDisconnected
    {
        add { _rcon.OnDisconnected += value; }
        remove { _rcon.OnDisconnected -= value; }
    }

    public virtual Task ConnectAsync()
    {
        return _rcon.ConnectAsync();
    }

    public virtual void Dispose()
    {
        _rcon.Dispose();
    }

    public virtual Task<T> SendCommandAsync<T>(string command) where T : class, IParseable, new()
    {
        return _rcon.SendCommandAsync<T>(command);
    }

    public virtual Task<string> SendCommandAsync(string command)
    {
        return _rcon.SendCommandAsync(command);
    }
}
