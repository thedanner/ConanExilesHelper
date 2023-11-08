using CoreRCON;

namespace ConanExilesHelper.Wrappers.Rcon;

public class RCONWrapperFactory : IRCONWrapperFactory
{
    private readonly RCON _rcon;

    public RCONWrapperFactory(RCON rcon)
    {
        _rcon = rcon;
    }

    public IRCONWrapper GetRcon() => new RCONWrapper(_rcon);
}
