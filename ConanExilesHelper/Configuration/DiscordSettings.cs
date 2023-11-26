using ConanExilesHelper.Scheduling.Infrastructure;
using System.Collections.Generic;

namespace ConanExilesHelper.Configuration;

public class DiscordSettings
{
    public string BotToken { get; set; } = "";
    public ConanExilesSettings ConanExilesSettings { get; set; } = new ConanExilesSettings();
    public List<TaskDefinition> TaskDefinitions { get; set; } = new List<TaskDefinition>();
}
