using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConanExilesHelper.Scheduling.Infrastructure;

public class JobRunner : IJob
{
    public const string KeyTaskName = "taskName";
    public const string KeyClassName = "className";

    private readonly IServiceProvider _serviceProvider;

    public JobRunner(IServiceProvider serviceProvider)
    {
        if (serviceProvider is null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _serviceProvider = serviceProvider;
    }


    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        var taskName = context.MergedJobDataMap.GetString(KeyTaskName);
        if (string.IsNullOrWhiteSpace(taskName))
        {
            throw new Exception($"The JobDataMap must have a non-empty value for the entry with key \"{KeyTaskName}\".");
        }

        var taskClassName = context.MergedJobDataMap.GetString(KeyClassName);
        if (string.IsNullOrWhiteSpace(taskClassName))
        {
            throw new Exception($"The JobDataMap must have a non-empty value for the entry with key \"{KeyClassName}\".");
        }

        var taskResolver = _serviceProvider.GetRequiredService<Func<string, ITask>>();
        var task = taskResolver(taskClassName);

        var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

        var allTasksSettings = _serviceProvider.GetRequiredService<IOptions<List<TaskDefinition>>>().Value;

        var taskDefinition = allTasksSettings.FirstOrDefault(t => t.Name == taskName);

        if (taskDefinition is null)
        {
            throw new Exception($"Couldn't find a task definition in app settings for task \"{context.Trigger.JobKey.Name}\".");
        }

        await task.RunTaskAsync(client, taskDefinition.Settings, cancellationToken);
    }
}
