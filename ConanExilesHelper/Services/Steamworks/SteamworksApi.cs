using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Services.Steamworks;

public class SteamworksApi : ISteamworksApi
{
    private readonly HttpClient _http;
    private readonly ILogger<SteamworksApi> _logger;

    public SteamworksApi(HttpClient http, ILogger<SteamworksApi> logger)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SteamworksResponse<PublishedFileDetailsWrapper?>?> GetPublishedFileDetails(
        List<long> publishedFileIds, CancellationToken cancellationToken)
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

        var publishedFileDetails = JsonSerializer.Deserialize<SteamworksResponse<PublishedFileDetailsWrapper?>>(responseStr);

        return publishedFileDetails;
    }
}
