using System.Threading.Tasks;

namespace ConanExilesHelper.Games.ConanExiles;

public interface IRestartService
{
    Task<bool> TryRestartAsync();
}
