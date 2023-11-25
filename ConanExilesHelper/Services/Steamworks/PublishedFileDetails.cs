using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConanExilesHelper.Services.Steamworks;

public class SteamworksResponse<T>
{
    [JsonPropertyName("response")]
    public T? Response { get; set; } = default;
}

public class PublishedFileDetailsWrapper
{
    [JsonPropertyName("result")]
    public int Result { get; set; }

    [JsonPropertyName("resultcount")]
    public int ResultCount { get; set; }

    [JsonPropertyName("publishedfiledetails")]
    public List<PublishedFileDetails> PublishedFileDetails { get; set; } = new List<PublishedFileDetails>();
}

public class PublishedFileDetails
{
    [JsonPropertyName("publishedfileid")]
    public string PublishedFileIdStr { get; set; } = "";

    public long PublishedFileId =>
        long.TryParse(PublishedFileIdStr, out var publishedFileId)
            ? publishedFileId
            : -1;

    [JsonPropertyName("result")]
    public int ResultNumber { get; set; }

    [JsonPropertyName("creator")]
    public string CreatorIdStr { get; set; } = "";

    public long CreatorId =>
        long.TryParse(CreatorIdStr, out var creatorId)
            ? creatorId
            : -1;

    [JsonPropertyName("creator_app_id")]
    public long CreatorAppId { get; set; }

    [JsonPropertyName("consumer_app_id")]
    public long Consumer { get; set; }

    [JsonPropertyName("filename")]
    public string FileName { get; set; } = "";

    // At least I'm guessing it's in bytes.
    [JsonPropertyName("file_size")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = "";

    [JsonPropertyName("hcontent_file")]
    public string HContentFileIdStr { get; set; } = "";

    public long HContentFileId =>
        long.TryParse(HContentFileIdStr, out var hContentFileId)
            ? hContentFileId
            : -1;


    [JsonPropertyName("preview_url")]
    public string PreviewUrl { get; set; } = "";

    [JsonPropertyName("hcontent_preview")]
    public string HContentPreviewStr { get; set; } = "";

    public long HContentPreview =>
        long.TryParse(HContentPreviewStr, out var hContentPreview)
            ? hContentPreview
            : -1;



    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("time_created")]
    public long TimeCreatedUnixSeconds { get; set; }
    
    public DateTimeOffset TimeCreated => DateTimeOffset.FromUnixTimeSeconds(TimeCreatedUnixSeconds);

    [JsonPropertyName("time_updated")]
    public long TimeUpdatedUnixSeconds { get; set; }
    
    public DateTimeOffset TimeUpdated => DateTimeOffset.FromUnixTimeSeconds(TimeUpdatedUnixSeconds);

    [JsonPropertyName("visibility")]
    public int VisibilityInt { get; set; }

    public bool IsVisible => VisibilityInt == 0;

    [JsonPropertyName("banned")]
    public int BannedInt { get; set; }

    public bool IsBanned => BannedInt != 0;

    [JsonPropertyName("ban_reason")]
    public string BanReason { get; set; } = "";

    [JsonPropertyName("subscriptions")]
    public long SubscriptionsCount { get; set; }

    [JsonPropertyName("favorited")]
    public long FavoritedCount { get; set; }

    [JsonPropertyName("lifetime_subscriptions")]
    public long LifetimeSubscriptionsCount { get; set; }
    
    [JsonPropertyName("lifetime_favorited")]
    public long LifetimeFavoritedCount { get; set; }

    [JsonPropertyName("views")]
    public long ViewCount { get; set; }

    // Note that keys may be duplicated.
    [JsonPropertyName("tags")]
    public List<KeyValuePair<string, string>> Tags { get; set; } = new List<KeyValuePair<string, string>>();
}
