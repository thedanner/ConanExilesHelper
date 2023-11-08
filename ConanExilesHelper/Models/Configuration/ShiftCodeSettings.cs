namespace ConanExilesHelper.Models.Configuration;

public class ShiftCodeSettings
{
    public bool IsRepostEnabled { get; set; }
    public ulong? SourceUserId { get; set; }
    public ulong? SourceChannelId { get; set; }
    public ulong? RepostChannelId { get; set; }
    public bool? DeleteMessageInSourceChannelAfterRepost { get; set; }
}
