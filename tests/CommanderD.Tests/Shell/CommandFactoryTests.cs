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
    public void GetCommands_ContainsRegisteredName()
    {
        Assert.IsTrue(CreateFactory().GetCommands().Contains("fake"));
    }
}
