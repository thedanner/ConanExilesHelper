using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using ConanExilesHelper.Discord.Handlers;
using ConanExilesHelper.Games.ConanExiles;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Models.Configuration;
using ConanExilesHelper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
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
        serviceCollection.Configure<ConanExilesSettings>(config.GetSection("conanExilesSettings"));

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

        serviceCollection.AddSingleton<ICommandThrottler, CommandThrottler>();
        serviceCollection.AddTransient<IPingService, PingService>();
        serviceCollection.AddTransient<IRestartService, RestartService>();

        serviceCollection.AddTransient<IDiscordConnectionBootstrapper, DiscordConnectionBootstrapper>();

        serviceCollection.AddSingleton<CommandService>();
        serviceCollection.AddSingleton<InteractionService>();

        serviceCollection.AddSingleton(sp =>
        {
            return new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
                    | GatewayIntents.GuildIntegrations
            });
        });

        serviceCollection.AddSingleton<CommandAndEventHandler>();
    }
}
