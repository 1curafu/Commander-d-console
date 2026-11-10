using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class CommandFactoryTests
{
    private readonly IOutputWriter _outputWriter = new TestOutputWriter();
    private static CommandFactory CreateFactory()
    {
        var factory = new CommandFactory();
        factory.Register("fake", (outputWriter) => new FakeCommand("fake",outputWriter));
        return factory;
    }

    [TestMethod]
    [DataRow("fake")]
    [DataRow("FAKE")]
    [DataRow("Fake")]
    public void Create_KnownName_IgnoresCase(string name)
    {
        var command = CreateFactory().Create(name, _outputWriter);

        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(FakeCommand));
    }

    [TestMethod]
    public void Create_UnknownName_ReturnsNull()
    {
        Assert.IsNull(CreateFactory().Create("unknown", _outputWriter));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    public void Create_EmptyName_ReturnsNull(string name)
    {
        Assert.IsNull(CreateFactory().Create(name, new TestOutputWriter()));
    }

    [TestMethod]
    public void GetCommands_ContainsRegisteredName()
    {
        Assert.IsTrue(CreateFactory().GetCommands().Contains("fake"));
    }

    // Moved here from the command test classes (CD-15: every registered name creates the right command).
    [TestMethod]
    [DataRow("EXIT", typeof(ExitCommand))]
    [DataRow("CD", typeof(CdCommand))]
    [DataRow("VER", typeof(VersionCommand))]
    [DataRow("MKDIR", typeof(MkDirCommand))]
    [DataRow("MD", typeof(MkDirCommand))]
    [DataRow("MKFILE", typeof(MkFileCommand))]
    [DataRow("MF", typeof(MkFileCommand))]
    [DataRow("DIR", typeof(DirCommand))]
    [DataRow("DEL", typeof(DelCommand))]
    [DataRow("DELETE", typeof(DelCommand))]
    [DataRow("COLOR", typeof(SetColorCommand))]
    [DataRow("CLS", typeof(ClsCommand))]
    [DataRow("CLEAR", typeof(ClsCommand))]
    [DataRow("HELP", typeof(HelpCommand))]
    [DataRow("TYPE", typeof(TypeCommand))]
    [DataRow("REN", typeof(RenCommand))]
    [DataRow("RENAME", typeof(RenCommand))]
    [DataRow("MORE", typeof(MoreCommand))]
    [DataRow("FIND", typeof(FindCommand))]
    [DataRow("MOVE", typeof(MoveCommand))]
    public void Create_EveryRegisteredName_ReturnsTheRightCommand(string name, Type expectedType)
    {
        var factory = new CommandFactory(new FakeFileSystemService(@"C:\"), new FakeConsoleService());

        Command? command = factory.Create(name, _outputWriter);

        Assert.IsInstanceOfType(command, expectedType);
    }
}