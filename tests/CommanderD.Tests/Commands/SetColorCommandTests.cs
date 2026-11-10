using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class SetColorCommandTests
{
    private readonly FakeConsoleService _console = new FakeConsoleService();
    private readonly TestOutputWriter _outputWriter = new TestOutputWriter();
    private readonly SetColorCommand _command;

    public SetColorCommandTests()
    {
        _command = new SetColorCommand(_console, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("color", arguments));
    }

    [TestMethod]
    public void Color_Green_ChangesOnlyForeground()
    {
        Run("green");

        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    [DataRow("white", "on", "darkblue", ConsoleColor.DarkBlue, ConsoleColor.White)]
    [DataRow("black", "on", "white", ConsoleColor.White, ConsoleColor.Black)]
    [DataRow("yellow", "on", "darkred", ConsoleColor.DarkRed, ConsoleColor.Yellow)]
    [DataRow("red", "on", "darkgreen", ConsoleColor.DarkGreen, ConsoleColor.Red)]
    public void Color_ForegroundOnBackground_SetsBoth(string foreground, string keyword, string background, ConsoleColor expectedBackground, ConsoleColor expectedForeground)
    {
        Run(foreground, keyword, background);

        Assert.AreEqual(expectedBackground, _console.BackgroundColor);
        Assert.AreEqual(expectedForeground, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_ValidName_AppliesColorsOnceAndPrintsNothing()
    {
        Run("green");

        Assert.AreEqual(1, _console.SetColorsCalls);
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Color_SameBackgroundAndForeground_IsPrevented()
    {
        Run("green", "on", "green");

        Assert.AreEqual(0, _console.SetColorsCalls);
        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Gray, _console.ForegroundColor);
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Color_ForegroundEqualToCurrentBackground_IsPrevented()
    {
        Run("black");

        Assert.AreEqual(0, _console.SetColorsCalls);
    }
}
