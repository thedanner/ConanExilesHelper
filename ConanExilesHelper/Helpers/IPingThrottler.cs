namespace ConanExilesHelper.Helpers;

public interface IPingThrottler
{
    bool TryCanPing();
}
