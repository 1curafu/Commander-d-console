using System.Runtime.Versioning;
using CommanderD.Shell;
using Microsoft.Win32;

namespace CommanderD.Commands;

public class VersionCommand : Command
{
    private const string CurrentVersionKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    private readonly Version _osVersion;

    public VersionCommand(IOutputWriter outputWriter)
        : this(outputWriter, GetOsVersion())
    {
    }

    public VersionCommand(IOutputWriter outputWriter, Version osVersion)
        : base("ver", "Shows the version number.", outputWriter)
    {
        _osVersion = osVersion;
    }

    public override void Execute(CommandContext context)
    {
        OutputWriter.WriteLn(Format(_osVersion));
    }

    public static string Format(Version version)
    {
        int build = Math.Max(version.Build, 0);
        int revision = Math.Max(version.Revision, 0);

        return $"Microsoft Windows [Version {version.Major}.{version.Minor}.{build}.{revision}]";
    }

    private static Version GetOsVersion()
    {
        Version version = Environment.OSVersion.Version;


        if (OperatingSystem.IsWindows() && TryReadUpdateBuildRevision(out int revision))
        {
            return new Version(version.Major, version.Minor, Math.Max(version.Build, 0), revision);
        }

        return version;
    }

    [SupportedOSPlatform("windows")]
    private static bool TryReadUpdateBuildRevision(out int revision)
    {
        revision = 0;

        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(CurrentVersionKey);

            if (key?.GetValue("UBR") is int value)
            {
                revision = value;
                return true;
            }
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
        }

        return false;
    }
}
