using Discord;
using ConanExilesHelper.Helpers.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ConanExilesHelper.Helpers;

public class SprayModuleCommandResolver : ISprayModuleCommandResolver
{
    private readonly ILogger<SprayModuleCommandResolver> _logger;

    public SprayModuleCommandResolver(ILogger<SprayModuleCommandResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool TryResolve(string? arg1, string? arg2, IUserMessage message, [NotNullWhen(true)] out SprayModuleParseResult? result)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));

        var referencedMessage = message.ReferencedMessage;

        // Supported scenarios:
        // 1. Message content starts with a URL.
        // 2. Message content starts with a filename and URL.
        // 3. Message content starts with a filename and has an attachment.
        // 4. Message content starts with a filename and is a reply to a message with only a URL as its content.
        // 5. Message content starts with a filename and is a reply to a message with an attachement.
        // 6. Message content is empty and has an attachment.
        // 7. Message content is empty and is a reply to a message with only a URL as its content.
        // 8. Message content is empty and is a reply to a message with an attachment.

        if (!string.IsNullOrEmpty(arg1))
        {
            // Case 1. Message content starts with a URL.
            if (Uri.TryCreate(arg1, UriKind.Absolute, out var uri)
                && uri is not null)
            {
                if (arg1.StartsWithHttpProtocol())
                {
                    result = new SprayModuleParseResult(uri);
                    return true;
                }
                result = null;
                return false;
            }
            else
            {
                var fileName = arg1;

                // Case 2. Message content starts with a filename and URL.
                if (!string.IsNullOrEmpty(arg2)
                    && Uri.TryCreate(arg2, UriKind.Absolute, out uri)
                    && uri is not null)
                {
                    if (arg2.StartsWithHttpProtocol())
                    {
                        result = new SprayModuleParseResult(uri, fileName);
                        return true;
                    }
                    result = null;
                    return false;
                }

                // Case 3. Message content starts with a filename and attachment.
                if (message.Attachments.Any()
                    && Uri.TryCreate(message.Attachments.First().ProxyUrl, UriKind.Absolute, out uri)
                    && uri is not null)
                {
                    result = new SprayModuleParseResult(uri, fileName);
                    return true;
                }

                // 4. Message content starts with a filename and is a reply to a message with only a URL as its content.
                // 5. Message content starts with a filename and is a reply to a message with an attachement.
                if (referencedMessage is not null)
                {
                    return TryHandleReferencedMessage(referencedMessage, fileName, out result);
                }
            }
        }
        else 
        {
            // 6. Message content is empty and has an attachment.
            if (message.Attachments.Any()
                && Uri.TryCreate(message.Attachments.First().ProxyUrl, UriKind.Absolute, out var uri)
                && uri is not null)
            {
                result = new SprayModuleParseResult(uri, null);
                return true;
            }

            // 7. Message content is empty and is a reply to a message with only a URL as its content.
            // 8. Message content is empty and is a reply to a message with an attachment.
            if (referencedMessage is not null)
            {
                return TryHandleReferencedMessage(referencedMessage, null, out result);
            }
        }

        result = null;
        return false;
    }

    public bool TryHandleReferencedMessage(IMessage referencedMessage, string? fileName, [NotNullWhen(true)] out SprayModuleParseResult? result)
    {
        if (referencedMessage is null) throw new ArgumentNullException(nameof(referencedMessage));
        
        if (!string.IsNullOrEmpty(referencedMessage.Content)
            && Uri.TryCreate(referencedMessage.Content, UriKind.Absolute, out var uri)
            && uri is not null)
        {
            if (referencedMessage.Content.StartsWithHttpProtocol())
            {
                result = new SprayModuleParseResult(uri, fileName);
                return true;
            }
            result = null;
            return false;
        }

        if (referencedMessage.Attachments.Any()
            && Uri.TryCreate(referencedMessage.Attachments.First().ProxyUrl, UriKind.Absolute, out uri)
            && uri is not null)
        {
            result = new SprayModuleParseResult(uri, fileName);
            return true;
        }

        result = null;
        return false;
    }
}
