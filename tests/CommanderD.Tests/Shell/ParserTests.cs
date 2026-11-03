using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class ParserTests
{
    private readonly Parser parser = new Parser();

    [TestMethod]
    public void Parse_NameOnly_HasNoArguments()
    {
        var context = parser.Parse("ver");

        Assert.IsNotNull(context);
        Assert.AreEqual("ver", context.Name);
        Assert.AreEqual(0, context.Arguments.Length);
    }

    [TestMethod]
    public void Parse_NameWithFlagAndPath_SplitsArguments()
    {
        var context = parser.Parse(@"dir c:\windows /A:R");

        Assert.IsNotNull(context);
        Assert.AreEqual("dir", context.Name);
        CollectionAssert.AreEqual(new[] { @"c:\windows", "/A:R" }, context.Arguments);
    }

    [TestMethod]
    [DataRow("cd..", "..")]
    [DataRow(@"cd\", @"\")]
    [DataRow("cd/", "/")]
    public void Parse_CommandWithoutSpace_CutsNameAtSeparator(string input, string argument)
    {
        var context = parser.Parse(input);

        Assert.IsNotNull(context);
        Assert.AreEqual("cd", context.Name);
        CollectionAssert.AreEqual(new[] { argument }, context.Arguments);
    }

    [TestMethod]
    public void Parse_ExtraWhitespace_IsIgnored()
    {
        var context = parser.Parse("  mkdir    test  ");

        Assert.IsNotNull(context);
        Assert.AreEqual("mkdir", context.Name);
        CollectionAssert.AreEqual(new[] { "test" }, context.Arguments);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Parse_EmptyInput_ReturnsNull(string input)
    {
        Assert.IsNull(parser.Parse(input));
    }

    [TestMethod]
    [DataRow("ver", "ver")]
    [DataRow("  CD  src ", "CD")]
    [DataRow("cd..", "cd")]
    [DataRow("cd\\", "cd")]
    public void ParseCommand_ReturnsCommandName(string input, string expected)
    {
        Assert.AreEqual(expected, parser.ParseCommand(input));
    }

    [TestMethod]
    public void ParseParameters_ReturnsArguments()
    {
        CollectionAssert.AreEqual(new[] { "one", "two" }, parser.ParseParameters("fake one two"));
    }

    [TestMethod]
    public void ParseParameters_NoArguments_ReturnsEmptyArray()
    {
        Assert.AreEqual(0, parser.ParseParameters("ver").Length);
    }
}
