using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Services.Steamworks;

public interface ISteamworksApi
{
    Task<SteamworksResponse<PublishedFileDetailsWrapper?>?> GetPublishedFileDetailsAsync(
        List<long> publishedFileIds, CancellationToken cancellationToken);
}
