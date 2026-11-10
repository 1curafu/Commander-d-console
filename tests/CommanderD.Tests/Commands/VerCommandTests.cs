using System.Text.RegularExpressions;
using CommanderD.Commands;
using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class VerCommandTests
{
    private readonly TestOutputWriter _outputWriter = new TestOutputWriter();

    // ---- Criterion: same format as the real VER on Windows ----

    [TestMethod]
    public void Ver_KnownVersion_ShowsWindowsFormat()
    {
        var command = new VersionCommand(_outputWriter, new Version(10, 0, 22631, 4460));

        command.Execute(new CommandContext("ver", []));

        Assert.AreEqual("Microsoft Windows [Version 10.0.22631.4460]" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    // ---- Criterion: the current version number of the OS is shown ----

    [TestMethod]
    public void Ver_TypedIntoShell_ShowsCurrentOsVersion()
    {
        Version current = Environment.OSVersion.Version;
        var command = new CommandFactory().Create("VER", _outputWriter);

        command!.Execute(new CommandContext("ver", []));

        string output = _outputWriter.OutputBuffer.TrimEnd();
        StringAssert.Matches(output, new Regex(@"^Microsoft Windows \[Version \d+\.\d+\.\d+\.\d+\]$"));
        StringAssert.StartsWith(output, $"Microsoft Windows [Version {current.Major}.{current.Minor}.");
    }
}
