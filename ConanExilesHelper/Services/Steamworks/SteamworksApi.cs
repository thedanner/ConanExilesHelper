using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Services.Steamworks;

public class SteamworksApi : ISteamworksApi
{
    private readonly HttpClient _http;

    public SteamworksApi(HttpClient http)
    {
        _http = http;
    }

    public async Task<PublishedFileDetailsResponse?> GetPublishedFileDetails(
        IList<long> publishedFileIds, CancellationToken cancellationToken)
    {
        var formValues = new Dictionary<string, string>(publishedFileIds.Count + 1)
        {
            { "itemcount", publishedFileIds.Count.ToString() }
        };

        var i = 0;
        foreach (var publishedFileId in publishedFileIds)
        {
            formValues.Add($"publishedfileids[{i++}]", publishedFileId.ToString());
        }

        var payload = new FormUrlEncodedContent(formValues);

        var response = await _http.PostAsync(
            @"https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", payload, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseStr = await response.Content.ReadAsStringAsync(cancellationToken);

        var publisedFileDetails = JsonSerializer.Deserialize<PublishedFileDetailsResponse>(responseStr);

        return publisedFileDetails;
    }
}
