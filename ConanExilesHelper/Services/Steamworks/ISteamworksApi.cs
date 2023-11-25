using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Services.Steamworks;

public interface ISteamworksApi
{
    Task<PublishedFileDetailsResponse?> GetPublishedFileDetails(IEnumerable<long> publishedFileIds, CancellationToken cancellationToken);
}
