using CommanderD.Commands;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class CommandFactoryTests
{
    private static CommandFactory CreateFactory()
    {
        var factory = new CommandFactory();
        factory.Register("fake", () => new FakeCommand());
        return factory;
    }

    [TestMethod]
    [DataRow("fake")]
    [DataRow("FAKE")]
    [DataRow("Fake")]
    public void Create_KnownName_IgnoresCase(string name)
    {
        var command = CreateFactory().Create(name);

        Assert.IsNotNull(command);
        Assert.IsInstanceOfType(command, typeof(FakeCommand));
    }

    [TestMethod]
    public void Create_UnknownName_ReturnsNull()
    {
        Assert.IsNull(CreateFactory().Create("unknown"));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    public void Create_EmptyName_ReturnsNull(string name)
    {
        Assert.IsNull(CreateFactory().Create(name));
    }

    [TestMethod]
    public void GetCommandNames_ContainsRegisteredName()
    {
        CollectionAssert.Contains(CreateFactory().GetCommandNames(), "fake");
    }
}
