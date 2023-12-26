using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Services.ModComparison;

public interface IModVersionChecker
{
    Task<VersionCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken);
}
