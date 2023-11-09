using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;

namespace ConanExilesHelper.Helpers;

public static class ProcessHelpers
{
    [SupportedOSPlatform("windows")]
    public static string? GetCommandLine(this Process process)
    {
        using var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
        using var objects = searcher.Get();
        return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
    }
}
