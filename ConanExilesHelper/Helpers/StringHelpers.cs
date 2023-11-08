using System;
using System.Text.RegularExpressions;

namespace ConanExilesHelper.Helpers;

public static class StringHelpers
{
    private static readonly Regex SafeCharsForAttachmentFileNames = new Regex(@"[^a-zA-Z0-9_\-\.]",
        RegexOptions.Compiled|RegexOptions.Multiline);
    private static readonly Regex MultipleSubstitutionCharactersPattern = new Regex(@"\-+",
        RegexOptions.Compiled);

    public static string SanitizeFileNameForDiscordAttachment(string fileName)
    {
        if (fileName is null) throw new ArgumentNullException(nameof(fileName));

        fileName = SafeCharsForAttachmentFileNames.Replace(fileName, "-");
        fileName = MultipleSubstitutionCharactersPattern.Replace(fileName, "-");

        return fileName;
    }
}
