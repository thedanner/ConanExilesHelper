using System;

namespace ConanExilesHelper.Helpers;

public class SprayModuleParseResult
{
    public SprayModuleParseResult(Uri sourceImageUri) : this(sourceImageUri, null)
    {

    }
    public SprayModuleParseResult(Uri sourceImageUri, string? fileName)
    {
        SourceImageUri = sourceImageUri ?? throw new ArgumentNullException(nameof(sourceImageUri));
        FileName = fileName;
    }

    public Uri SourceImageUri { get; }
    public string? FileName { get; set; }
}
