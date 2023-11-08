using Discord.WebSocket;
using ConanExilesHelper.Models.Configuration;
using ConanExilesHelper.Rcon;
using ConanExilesHelper.Wrappers.Rcon;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesHelper.Services;

public class DiscordChatMover : IDiscordChatMover
{
    private readonly ILogger<DiscordChatMover> _logger;
    private readonly DiscordSettings _discordSettings;
    private readonly List<UserMapping> _userMappings;

    public DiscordChatMover(ILogger<DiscordChatMover> logger,
        IOptions<DiscordSettings>? discordSettings,
        IOptions<List<UserMapping>>? userMappings)
    {
        _logger = logger;
        _discordSettings = discordSettings?.Value ?? throw new ArgumentNullException(nameof(discordSettings));
        _userMappings = userMappings?.Value ?? throw new ArgumentNullException(nameof(userMappings));
    }

    public async Task<MoveResult> MovePlayersToCorrectChannelsAsync(
        IRCONWrapper rcon,
        DiscordSocketClient client,
        SocketGuild guild,
        SocketVoiceChannel usedInChannel,
        CancellationToken cancellationToken)
    {
        if (rcon is null) throw new ArgumentNullException(nameof(rcon));
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (usedInChannel is null) throw new ArgumentNullException(nameof(usedInChannel));

        var guildSettings = _discordSettings.GuildSettings.FirstOrDefault(g => g.Id == guild.Id);
        if (guildSettings is null)
        {
            throw new Exception($"Unable to find guild setting with ID {guild.Id} in the configuration.");
        }

        var primaryVoiceChannel = guild.GetVoiceChannel(guildSettings.Channels.Primary.Id);
        if (primaryVoiceChannel is null) throw new Exception("Bad primary channel ID in config.");
        var secondaryVoiceChannel = guild.GetVoiceChannel(guildSettings.Channels.Secondary.Id);
        if (secondaryVoiceChannel is null) throw new Exception("Bad secondary channel ID in config.");
        
        // Used by someone in a different voice channel from the ones indicated in settings.
        // Treat that as the primary, and the first of the empty of the two channels by setting ID as the secondary.
        if (usedInChannel.Id != primaryVoiceChannel.Id && usedInChannel.Id != secondaryVoiceChannel.Id)
        {
            if (primaryVoiceChannel.ConnectedUsers.Count == 0)
            {
                secondaryVoiceChannel = primaryVoiceChannel;
                primaryVoiceChannel = usedInChannel;
            }
            else if (secondaryVoiceChannel.ConnectedUsers.Count == 0)
            {
                // The primary channel stays the same.
                secondaryVoiceChannel = usedInChannel;
            }
            else
            {
                return new MoveResult { FailureReason = MoveResult.MoveFailureReason.NotEnoughEmptyVoiceChannels };
            }
        }

        var result = new MoveResult();

        var printInfo = await rcon.SendCommandAsync<PrintInfo>("sm_printinfo");

        var currentPlayersOnServer = printInfo.Players
             .Where(p => !"BOT".Equals(p.SteamId, StringComparison.CurrentCultureIgnoreCase))
             .ToList();

        _logger.LogDebug("Getting current voice channel users.");

        var usersInPrimaryChannel = primaryVoiceChannel.ConnectedUsers;
        var usersInSecondaryChannel = secondaryVoiceChannel.ConnectedUsers;

        _logger.LogDebug("Current players per PrintInfo results ({playerCount}):", currentPlayersOnServer.Count);
        for (var i = 0; i < currentPlayersOnServer.Count; i++)
        {
            _logger.LogDebug("  {index}: {steamId} - {name}", i, currentPlayersOnServer[i].SteamId, currentPlayersOnServer[i].Name);
        }

        var discordUsersToMove = new List<(SocketGuildUser user, SocketVoiceChannel intendedChannel)>();

        var currentlyPlayingSteamIds = currentPlayersOnServer
            .Select(p => p.SteamId)
            .ToList();

        var currentPlayerMappings = _userMappings
            .Where(d => currentlyPlayingSteamIds.Intersect(d.SteamIds, StringComparer.CurrentCultureIgnoreCase).Any())
            .ToList();

        _logger.LogDebug("Current players found in mapping data ({playerCount}):", currentPlayerMappings.Count);
        for (var i = 0; i < currentPlayerMappings.Count; i++)
        {
            _logger.LogDebug("  {index}: {steamIds} - {discordId} - {name}",
                i, string.Join(",", currentPlayerMappings[i].SteamIds), currentPlayerMappings[i].DiscordId, currentPlayerMappings[i].Name);
        }

        var allSteamIdsFromUserMappings = _userMappings.SelectMany(um => um.SteamIds).ToList();
        var missingSteamMappings = currentPlayersOnServer
            .Where(p => !allSteamIdsFromUserMappings.Contains(p.SteamId, StringComparer.CurrentCultureIgnoreCase))
            .ToList();

        var currentPlayerDiscordSnowflakes = currentPlayerMappings
            .Select(p => p.DiscordId)
            .ToList();

        var discordAccountsForCurrentPlayers =
            (primaryVoiceChannel.ConnectedUsers.Concat(secondaryVoiceChannel.ConnectedUsers))
            .Where(u => currentPlayerDiscordSnowflakes.Contains(u.Id))
            .ToList();

        _logger.LogDebug("Discord accounts found from mappings ({matchCount}):", discordAccountsForCurrentPlayers.Count);
        for (var i = 0; i < discordAccountsForCurrentPlayers.Count; i++)
        {
            _logger.LogDebug("  {index}: {discordId} - {username}",
                i, discordAccountsForCurrentPlayers[i].Id, discordAccountsForCurrentPlayers[i].Username);
        }

        var discordSnowflakesInVoice = usersInPrimaryChannel.Select(u => u.Id)
            .Concat(usersInSecondaryChannel.Select(u => u.Id))
            .ToList();

        var missingDiscordMappings = discordAccountsForCurrentPlayers.Where(d =>
                !discordSnowflakesInVoice.Contains(d.Id))
            .ToList();

        if (missingSteamMappings.Any())
        {
            _logger.LogDebug("Current Steam users MISSING from mapping ({missingUserCount}):", missingSteamMappings.Count);
            for (var i = 0; i < missingSteamMappings.Count; i++)
            {
                result.UnmappedSteamUsers.Add(new UnmappedSteamUser(missingSteamMappings[i].Name, missingSteamMappings[i].SteamId));
                _logger.LogDebug("  {index}: {steamId} - {name}", i, missingSteamMappings[i].SteamId, missingSteamMappings[i].Name);
            }
        }

        if (missingDiscordMappings.Any())
        {
            _logger.LogDebug("Current Discord users MISSING from mapping ({missingUserCount}):", missingDiscordMappings.Count);
            for (var i = 0; i < missingDiscordMappings.Count; i++)
            {
                _logger.LogDebug("  {index}: {id} - \"{nickname}\" (\"{username}\")",
                    i,
                    missingDiscordMappings[i].Id,
                    missingDiscordMappings[i].Nickname,
                    missingDiscordMappings[i].Username);
            }
        }

        foreach (var discordAccount in discordAccountsForCurrentPlayers)
        {
            SocketVoiceChannel currentVoiceChannel;

            if (usersInPrimaryChannel is not null && usersInPrimaryChannel.Any(u => u.Id == discordAccount.Id))
            {
                currentVoiceChannel = primaryVoiceChannel;

                _logger.LogDebug("{username} ({id}) found in primary channel.", discordAccount.Username, discordAccount.Id);
            }
            else if (usersInSecondaryChannel is not null && usersInSecondaryChannel.Any(u => u.Id == discordAccount.Id))
            {
                currentVoiceChannel = secondaryVoiceChannel;

                _logger.LogDebug("{username} ({id}) found in secondary channel.", discordAccount.Username, discordAccount.Id);
            }
            else
            {
                _logger.LogDebug("Skipping {username} ({id}): not in voice chat.",
                    discordAccount.Username, discordAccount.Id);
                continue;
            }

            if (primaryVoiceChannel.Id != currentVoiceChannel.Id && secondaryVoiceChannel.Id != currentVoiceChannel.Id)
            {
                _logger.LogDebug("Skipping {username} ({id}): not in a designated voice channel.",
                    discordAccount.Username, discordAccount.Id);
                continue;
            }

            var userMappingsFromDiscordId = _userMappings.Where(um => um.DiscordId == discordAccount.Id).ToList();
            if (!userMappingsFromDiscordId.Any())
            {
                _logger.LogDebug("Skipping {username} ({id}): Couldn't find user mapping.",
                    discordAccount.Username, discordAccount.Id);
                continue;
            }

            var allSteamIdsFromDiscordId = userMappingsFromDiscordId.SelectMany(s => s.SteamIds);
            var usersPrintInfo = printInfo.Players.FirstOrDefault(pi =>
                allSteamIdsFromDiscordId.Any(sid => sid.Equals(pi.SteamId, StringComparison.CurrentCultureIgnoreCase)));
            if (usersPrintInfo is null)
            {
                _logger.LogDebug("Skipping {username} ({id}): Couldn't find user's Steam ID ({userMappingSteamIds}) in PrintInfo results.",
                    discordAccount.Username, discordAccount.Id, string.Join(", ", allSteamIdsFromDiscordId));
                continue;
            }

            SocketVoiceChannel intendedChannel;
            if (usersPrintInfo.TeamIndex == PrintInfo.TeamIndexSurvivor)
            {
                intendedChannel = primaryVoiceChannel;
            }
            else if (usersPrintInfo.TeamIndex == PrintInfo.TeamIndexInfected)
            {
                intendedChannel = secondaryVoiceChannel;
            }
            else
            {
                continue;
            }

            discordUsersToMove.Add((discordAccount, intendedChannel));
        }

        foreach (var (user, intendedChannel) in discordUsersToMove)
        {
            SocketVoiceChannel currentVoiceChannel;

            if (usersInPrimaryChannel.Any(u => u.Id == user.Id))
            {
                currentVoiceChannel = primaryVoiceChannel;
            }
            else if (usersInSecondaryChannel.Any(u => u.Id == user.Id))
            {
                currentVoiceChannel = secondaryVoiceChannel;
            }
            else
            {
                // Not in voice chat
                continue;
            }

            if (currentVoiceChannel.Id != intendedChannel.Id)
            {
                _logger.LogDebug("Moving {username} ({id}) to other voice channel (from {fromChannelName} to {toChannelName}).",
                    user.Username, user.Id,
                    currentVoiceChannel.Name, intendedChannel.Name);

                await user.ModifyAsync(p => p.ChannelId = intendedChannel.Id);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                result.MoveCount++;
            }
        }

        return result;
    }

    public async Task<ReuniteResult> RenuitePlayersAsync(
        DiscordSocketClient client,
        SocketGuild guild,
        SocketVoiceChannel usedInChannel,
        CancellationToken cancellationToken)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (usedInChannel is null) throw new ArgumentNullException(nameof(usedInChannel));

        var guildSettings = _discordSettings.GuildSettings.FirstOrDefault(g => g.Id == guild.Id);
        if (guildSettings is null)
        {
            throw new Exception($"Unable to find guild setting with ID {guild.Id} in the configuration.");
        }

        var primaryVoiceChannel = guild.GetVoiceChannel(guildSettings.Channels.Primary.Id);
        if (primaryVoiceChannel is null) throw new Exception("Bad primary channel ID in config.");
        var secondaryVoiceChannel = guild.GetVoiceChannel(guildSettings.Channels.Secondary.Id);
        if (secondaryVoiceChannel is null) throw new Exception("Bad secondary channel ID in config.");
        
        var result = new ReuniteResult();

        // Used by someone in a different voice channel from the ones indicated in settings.
        // Treat that as the primary, and the first of the empty of the two channels by setting ID as the secondary.
        if (usedInChannel.Id != primaryVoiceChannel.Id && usedInChannel.Id != secondaryVoiceChannel.Id)
        {
            if (primaryVoiceChannel.ConnectedUsers.Count > 0 && secondaryVoiceChannel.ConnectedUsers.Count > 0)
            {
                return new ReuniteResult { FailureReason = ReuniteResult.ReuniteFailureReason.TooManyPopulatedVoiceChannels };
            }

            if (primaryVoiceChannel.ConnectedUsers.Count == 0)
            {
                secondaryVoiceChannel = usedInChannel;
            }
            else if (secondaryVoiceChannel.ConnectedUsers.Count == 0)
            {
                primaryVoiceChannel = usedInChannel;
            }
            else
            {
                // Both empty - no one to reunite.
                return result;
            }
        }


        _logger.LogDebug("Getting current voice channel users.");

        var usersInPrimaryChannel = primaryVoiceChannel.ConnectedUsers;
        var usersInSecondaryChannel = secondaryVoiceChannel.ConnectedUsers;

        // Shouldn't happen. If so, it's a race condition where one channel empted between the command running and now.
        if (!usersInPrimaryChannel.Any() || !usersInSecondaryChannel.Any())
        {
            return result;
        }

        foreach (var user in usersInSecondaryChannel)
        {
            await user.ModifyAsync(p => p.ChannelId = primaryVoiceChannel.Id);
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            result.MoveCount++;
        }

        return result;
    }
}
