using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ConanExilesHelper.Discord.Interfaces.Events;
using ConanExilesHelper.Discord.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConanExilesHelper.Discord.Handlers;

public class CommandAndEventHandler : IDisposable
{
    private readonly ILogger<CommandAndEventHandler> _logger;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    private bool _disposedValue;

    // Retrieve client and CommandService instance via ctor
    public CommandAndEventHandler(
        ILogger<CommandAndEventHandler> logger, DiscordSocketClient client,
        InteractionService interactionService,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task InitializeAsync()
    {
        _client.ButtonExecuted += HandleButtonExecutedAsync;
        _client.MessageReceived += HandleMessageReceivedAsync;
        _client.ReactionAdded += HandleReactionAddedAsync;

        await _interactionService.AddModulesAsync(typeof(MinecraftPlayersInteractiveModule).Assembly, _serviceProvider);

        _client.InteractionCreated += HandleInteraction;
    }

    private async Task HandleButtonExecutedAsync(SocketMessageComponent interaction)
    {
        var ctx = new SocketInteractionContext<SocketMessageComponent>(_client, interaction);
        await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
    }

    private async Task HandleMessageReceivedAsync(SocketMessage message)
    {
        var implementingServices = _serviceProvider.GetServices<IHandleMessageReceivedAsync>();

        await Task.WhenAll(implementingServices.Select(s => s.HandleMessageReceivedAsync(message)));
    }

    private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> maybeCachedMessage,
        Cacheable<IMessageChannel, ulong> maybeCachedChannel, SocketReaction reaction)
    {
           var implementingServices = _serviceProvider.GetServices<IHandleReactionAddedAsync>();
    
        await Task.WhenAll(implementingServices.Select(s => s.HandleReactionAddedAsync(maybeCachedMessage, maybeCachedChannel, reaction)));
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_client, arg);
            await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trying to handle interaction (e.g., from a slash command).");

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist.
            // It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
            {
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _client.ButtonExecuted -= HandleButtonExecutedAsync;
                _client.MessageReceived -= HandleMessageReceivedAsync;
                _client.ReactionAdded -= HandleReactionAddedAsync;
                _client.InteractionCreated -= HandleInteraction;

                _interactionService?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
