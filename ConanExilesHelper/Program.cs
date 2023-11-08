using CoreRCON;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using ConanExilesHelper.Discord.Handlers;
using ConanExilesHelper.Discord.Interfaces;
using ConanExilesHelper.Discord.Interfaces.Events;
using ConanExilesHelper.Games.ConanExiles;
using ConanExilesHelper.Games.Minecraft;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Models.Configuration;
using ConanExilesHelper.Scheduling;
using ConanExilesHelper.Services;
using ConanExilesHelper.Wrappers.Rcon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ConanExilesHelper;

public class Program
{
    public enum ExitCode
    {
        Success = 0,
        ErrorUnknown = 10,
        InvalidArgs = 20,
        ErrorException = 30,
    }



    // Authorize the bot by visiting:
    // https://discord.com/api/oauth2/authorize?client_id=778014691804577844&permissions=16885760&scope=bot%20applications.commands
    // If that link doesn't work, the generators are:
    // https://discord.com/developers/applications/778014691804577844/oauth2/url-generator
    // or just the bit calculator:
    // https://discord.com/developers/applications/778014691804577844/bot at the bottom

    public static int Main(string[] args)
    {
        try
        {
            // When run as a service, the working directory is wrong,
            // and it can't be set from the service configuration.
            var exeLocation = Assembly.GetExecutingAssembly().Location;
            var exeDirectory = Path.GetDirectoryName(exeLocation);
            if (exeDirectory is not null)
            {
                Environment.CurrentDirectory = exeDirectory;
            }

            CreateHostBuilder(args).Build().Run();
        }
        catch (ObjectDisposedException ex) when (ex.ObjectName == "System.Net.Sockets.Socket") { }
        catch (Exception ex)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddNLog();
                builder.AddConsole();
            });
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(ex, "Error starting service or unhandled runtime error.");
        }

        // TODO some thread is probably still running somewhere preventing a normal shutdown. Find out where.
        // Environment.Exit() doesn't even cut it here.
        Environment.ExitCode = 0;
        Process.GetCurrentProcess().Kill();
        return 0;
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "ConanExilesHelper";
            })
            .ConfigureAppConfiguration((hostingContext, config) => ConfigureAppConfiguration(hostingContext, config, args))
            .ConfigureServices(ConfigureServices);

        return hostBuilder;
    }

    private static void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder config, string[] args)
    {
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#file-configuration-provider
        config.Sources.Clear();

        var env = hostContext.HostingEnvironment;

        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(prefix: "ConanExilesHelper_")
            .AddCommandLine(args);
    }

    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection serviceCollection)
    {
        var config = hostContext.Configuration!;

        serviceCollection.Configure<DiscordSettings>(config.GetSection("discordSettings"));
        serviceCollection.Configure<Left4DeadSettings>(config.GetSection("left4deadSettings"));
        serviceCollection.Configure<MinecraftSettings>(config.GetSection("minecraft"));
        serviceCollection.Configure<ConanExilesSettings>(config.GetSection("conanExiles"));

        serviceCollection.Configure<ShiftCodeSettings>(config.GetSection("shiftCodes"));
        serviceCollection.Configure<List<TaskDefinition>>(config.GetSection("tasks"));
        serviceCollection.Configure<List<UserMapping>>(config.GetSection("userMappings"));

        serviceCollection.AddSingleton(new HttpClient());

        serviceCollection.AddLogging(loggerBuilder =>
        {
            loggerBuilder.ClearProviders();
            loggerBuilder.SetMinimumLevel(LogLevel.Debug);
            loggerBuilder.AddNLog(config);
        });

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            serviceCollection.AddHostedService<Worker>()
                .Configure<EventLogSettings>(config =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        config.LogName = "ConanExilesHelper";
                        config.SourceName = "ConanExilesHelper - Discord Bot";
                    }
                });
        }

        serviceCollection.AddSingleton<IPingThrottler, PingThrottler>();
        serviceCollection.AddTransient<IMinecraftPingService, MinecraftPingService>();
        serviceCollection.AddTransient<IConanExilesPingService, ConanExilesPingService>();

        serviceCollection.AddTransient<IDiscordConnectionBootstrapper, DiscordConnectionBootstrapper>();

        serviceCollection.AddTransient<IDiscordChatMover, DiscordChatMover>();

        serviceCollection.AddTransient<ISprayModuleCommandResolver, SprayModuleCommandResolver>();

        serviceCollection.AddSingleton<CommandService>();
        serviceCollection.AddSingleton<InteractionService>();

        serviceCollection.AddSingleton(sp =>
        {
            return new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
                    | GatewayIntents.GuildIntegrations
                    | GatewayIntents.GuildVoiceStates
                    | GatewayIntents.GuildPresences // Needed to know which users are in which voice channels
                    | GatewayIntents.GuildMessages
                    | GatewayIntents.GuildMessageReactions
                    | GatewayIntents.DirectMessages
                    | GatewayIntents.DirectMessageReactions
                    // Needed for the SHiFT code bridge.
                    // This also needs the Priveleged Gateway Intent for Message Content Intent enabled in the developer portal.
                    // https://discord.com/developers/applications
                    | GatewayIntents.MessageContent
            });
        });

        serviceCollection.AddSingleton<CommandAndEventHandler>();

        var serverInfo = config.GetSection("left4deadSettings")?.Get<Left4DeadSettings>()?.ServerInfo;

        if (serverInfo is not null)
        {
            serviceCollection.AddTransient(sp => new RCON(new IPEndPoint(IPAddress.Parse(serverInfo.Ip), serverInfo.Port), serverInfo.RconPassword));
            serviceCollection.AddTransient<IRCONWrapperFactory, RCONWrapperFactory>();
        }

        // More specific handlers
        var allLoadedTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .ToList()
            .AsReadOnly();

        BindEventHandlers(allLoadedTypes, serviceCollection);

        BindTasks(allLoadedTypes, serviceCollection);

        ConfigureScheduler(serviceCollection, config);
    }

    private static void BindEventHandlers(IReadOnlyList<Type> allLoadedTypes, IServiceCollection serviceCollection)
    {
        var handlerTypes = new List<Type>();
        var eventInterfaceTypes = new List<Type>();

        // Do all the filtering and sorting in one pass over the loaded types list.
        foreach (var type in allLoadedTypes)
        {
            if (typeof(IHandleDiscordEvents).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                handlerTypes.Add(type);
            }
            else if (type != typeof(IHandleDiscordEvents)
                && typeof(IHandleDiscordEvents).IsAssignableFrom(type)
                && type.IsInterface)
            {
                eventInterfaceTypes.Add(type);
            }
        }

        foreach (var handlerType in handlerTypes)
        {
            foreach (var implmenetedHandlerInterface in handlerType.GetInterfaces().Intersect(eventInterfaceTypes))
            {
                serviceCollection.AddTransient(implmenetedHandlerInterface, handlerType);
            }
        }
    }

    private static void BindTasks(IReadOnlyList<Type> allLoadedTypes, IServiceCollection serviceCollection)
    {
        var handlerTypes = new List<Type>();

        // Do all the filtering and sorting in one pass over the loaded types list.
        foreach (var type in allLoadedTypes)
        {
            if (typeof(ITask).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                handlerTypes.Add(type);
            }
        }

        var duplicatedNames = handlerTypes
            .GroupBy(t => t.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicatedNames.Any())
        {
            // If this comes up, we may need to create an attribute that allows a custom name to be specified or something.
            throw new Exception(
                "Found tasks with duplicate names (note that only the class name and no part of the namespace is " +
                "used as the task name, and therefore class names must be unique: " +
                string.Join(", ", duplicatedNames));
        }

        foreach (var handlerType in handlerTypes)
        {
            serviceCollection.AddTransient(handlerType);
        }

        // Technique borrowed from
        // https://dejanstojanovic.net/aspnet/2018/december/registering-multiple-implementations-of-the-same-interface-in-aspnet-core/
        serviceCollection.AddTransient<Func<string, ITask>>(serviceProvider => key =>
        {
            var type = handlerTypes.FirstOrDefault(t => t.Name == key);
            if (type is null)
            {
                throw new Exception($"Couldn't find an ITask with class name of '{key}'.");
            }
            var task = (ITask)serviceProvider.GetRequiredService(type);
            return task;
        });
    }

    private static void ConfigureScheduler(IServiceCollection serviceCollection, IConfiguration config)
    {
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        var tasks = serviceProvider.GetRequiredService<IOptions<List<TaskDefinition>>>().Value;

        var tasksGroupedByName = tasks.GroupBy(t => t.Name);
        var replicatedNamed = tasksGroupedByName.Where(g => g.Count() > 1);
        if (replicatedNamed.Any())
        {
            throw new Exception("Multiple tasks were found with each of the following names: " +
                string.Join(", ", replicatedNamed.Select(n => $"\"{n.Key}\"")));
        }

        serviceCollection.AddQuartz(q =>
        {
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 1;
            });

            foreach (var taskSetting in tasks)
            {
                if (string.IsNullOrEmpty(taskSetting.Name))
                {
                    throw new Exception($"A task is missing a {nameof(taskSetting.Name)}.");
                }

                logger.LogInformation("Creating task in scheduler for \"{taskName}\".", taskSetting.Name);

                var jobKey = new JobKey("JobRunner");

                q.AddJob<JobRunner>(jobKey, j => j
                    .WithDescription("Job runner")
                );

                if (string.IsNullOrEmpty(taskSetting.ClassName))
                {
                    throw new Exception($"The task definition for the task \"{0}\" is missing {nameof(taskSetting.ClassName)}.");
                }

                var jobData = new JobDataMap();
                jobData.Put(JobRunner.KeyTaskName, taskSetting.Name);
                jobData.Put(JobRunner.KeyClassName, taskSetting.ClassName);

                q.AddTrigger(t => t
                    .WithIdentity(taskSetting.Name)
                    .WithCronSchedule(taskSetting.CronSchedule)
                    .ForJob(jobKey)
                    .UsingJobData(jobData)
                );
            }
        });
        serviceCollection.AddTransient<JobRunner>();

        serviceCollection.AddQuartzHostedService(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;
        });
    }
}
