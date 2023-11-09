namespace ConanExilesHelper.Helpers;

public interface ICommandThrottler
{
    bool TryCanRunCommand();
}
