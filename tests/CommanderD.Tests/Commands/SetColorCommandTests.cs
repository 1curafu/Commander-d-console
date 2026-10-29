using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class SetColorCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";

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
    public void Color_HasNameAndDescription()
    {
        Assert.AreEqual("color", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }


    [TestMethod]
    public void Color_0A_BlackBackgroundGreenText()
    {
        Run("0A");

        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    [DataRow("1F", ConsoleColor.DarkBlue, ConsoleColor.White)]
    [DataRow("F0", ConsoleColor.White, ConsoleColor.Black)]
    [DataRow("4E", ConsoleColor.DarkRed, ConsoleColor.Yellow)]
    [DataRow("07", ConsoleColor.Black, ConsoleColor.Gray)]
    [DataRow("2C", ConsoleColor.DarkGreen, ConsoleColor.Red)]
    public void Color_TwoDigits_FirstIsBackgroundSecondIsForeground(string code, ConsoleColor background, ConsoleColor foreground)
    {
        Run(code);

        Assert.AreEqual(background, _console.BackgroundColor);
        Assert.AreEqual(foreground, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_LowerCaseCode_Works()
    {
        Run("0a");

        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_OneDigit_ChangesOnlyForeground()
    {
        Run("1F");
        Run("A");

        Assert.AreEqual(ConsoleColor.DarkBlue, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_NoArgument_RestoresDefaultColors()
    {
        Run("1F");

        Run();

        Assert.AreEqual(1, _console.ResetColorsCalls);
        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Gray, _console.ForegroundColor);
    }


    [TestMethod]
    public void Color_ValidCode_AppliesColorsOnceAndPrintsNothing()
    {
        Run("0A");

        Assert.AreEqual(1, _console.SetColorsCalls);
        Assert.IsNull(_outputWriter.output);
    }


    [TestMethod]
    [DataRow("00")]
    [DataRow("AA")]
    [DataRow("ff")]
    public void Color_SameBackgroundAndForeground_IsPrevented(string code)
    {
        Run(code);

        Assert.AreEqual(0, _console.SetColorsCalls);
        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Gray, _console.ForegroundColor);
        Assert.IsNull(_outputWriter.output);
    }

    [TestMethod]
    public void Color_OneDigitEqualToCurrentBackground_IsPrevented()
    {
        Run("0");

        Assert.AreEqual(0, _console.SetColorsCalls);
    }


    [TestMethod]
    [DataRow("zz")]
    [DataRow("G0")]
    [DataRow("123")]
    [DataRow("-1")]
    [DataRow("")]
    public void Color_InvalidCode_ShowsSyntaxMessageAndChangesNothing(string code)
    {
        Run(code);

        Assert.AreEqual(SyntaxMessage, _outputWriter.output);
        Assert.AreEqual(0, _console.SetColorsCalls);
    }

    [TestMethod]
    public void Color_TooManyArguments_ShowsSyntaxMessage()
    {
        Run("0A", "1F");

        Assert.AreEqual(SyntaxMessage, _outputWriter.output);
        Assert.AreEqual(0, _console.SetColorsCalls);
    }


    private CommandInvoker CreateInvoker(TestOutputWriter writer)
    {
        var factory = new CommandFactory();
        factory.Register("color", (outputWriter) => new SetColorCommand(_console, outputWriter));
        return new CommandInvoker(new CommandParser(), factory, writer);
    }

    [TestMethod]
    [DataRow("color 0A")]
    [DataRow("COLOR 0A")]
    [DataRow("Color 0a")]
    public void Color_TypedIntoShell_ChangesColor(string input)
    {
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand(input);

        Assert.IsNull(writer.output);
        Assert.AreEqual(ConsoleColor.Black, _console.BackgroundColor);
        Assert.AreEqual(ConsoleColor.Green, _console.ForegroundColor);
    }

    [TestMethod]
    public void Color_RegisteredInDefaultFactory()
    {
        var factory = new CommandFactory();

        Assert.IsInstanceOfType(factory.Create("COLOR", new TestOutputWriter()), typeof(SetColorCommand));
    }
}