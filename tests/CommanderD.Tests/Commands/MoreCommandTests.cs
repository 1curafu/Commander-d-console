using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MoreCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";

    private readonly string _work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-more-tests", "work"));
    private readonly FakeFileSystemService _fileSystem;
    private readonly TestOutputWriter _outputWriter;
    private readonly MoreCommand _command;

    public MoreCommandTests()
    {
        _fileSystem = new FakeFileSystemService(_work);
        _outputWriter = new TestOutputWriter();
        _command = new MoreCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("more", arguments));
    }

    [TestMethod]
    public void More_HasNameAndDescription()
    {
        Assert.AreEqual("more", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }

    [TestMethod]
    public void More_NoArguments_PrintsSyntaxError()
    {
        Run();

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_EmptyQuotes_PrintsSyntaxError()
    {
        Run("\"\"");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_ExistingFile_PrintsEachLine()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"), "Zeile1\nZeile2");

        Run("test.txt");

        Assert.AreEqual("Zeile1" + Environment.NewLine + "Zeile2" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_WindowsLineEndings_PrintsLinesWithoutCarriageReturn()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"), "a\r\nb");

        Run("test.txt");

        Assert.AreEqual("a" + Environment.NewLine + "b" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_EmptyFile_PrintsNothing()
    {
        _fileSystem.AddFile(Path.Combine(_work, "empty.txt"));

        Run("empty.txt");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_FileNameWithSpaces_ReadsFile()
    {
        _fileSystem.AddFile(Path.Combine(_work, "my notes.txt"), "hallo");

        Run("\"my", "notes.txt\"");

        Assert.AreEqual("hallo" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_MissingFile_PrintsCannotAccess()
    {
        string target = Path.Combine(_work, "missing.txt");

        Run("missing.txt");

        Assert.AreEqual($"Cannot access file {target}" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_Directory_PrintsCannotAccess()
    {
        string folder = Path.Combine(_work, "docs");
        _fileSystem.AddDirectory(folder);

        Run("docs");

        Assert.AreEqual($"Cannot access file {folder}" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_InvalidPath_PrintsSyntaxError()
    {
        _fileSystem.ResolvePathException = new ArgumentException("bad path");

        Run("x");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }
}
