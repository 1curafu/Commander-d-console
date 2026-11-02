using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Shell;

[TestClass]
public class TestOutputWriterTests
{
    private readonly TestOutputWriter _writer = new();

    [TestMethod]
    public void OutputBuffer_AtStart_IsEmpty()
    {
        Assert.AreEqual(string.Empty, _writer.OutputBuffer);
    }

    [TestMethod]
    public void Write_AppendsWithoutNewLine()
    {
        _writer.Write("a");
        _writer.Write("b");

        Assert.AreEqual("ab", _writer.OutputBuffer);
    }

    [TestMethod]
    public void WriteLn_AppendsLineAndKeepsEarlierOutput()
    {
        _writer.WriteLn("one");
        _writer.WriteLn("two");

        Assert.AreEqual("one" + Environment.NewLine + "two" + Environment.NewLine, _writer.OutputBuffer);
    }

    [TestMethod]
    public void ClearOutputBuffer_RemovesAllOutput()
    {
        _writer.WriteLn("one");

        _writer.ClearOutputBuffer();

        Assert.AreEqual(string.Empty, _writer.OutputBuffer);
    }
}
