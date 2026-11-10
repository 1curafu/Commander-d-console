using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MoveCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";
    private const string NotFoundMessage = "The system cannot find the file specified.";
    private const string DuplicateMessage = "A duplicate file name exists, or the file cannot be found.";
    private const string MultipleToFileMessage = "Cannot move multiple files to a single file.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly string _work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-move-tests", "work"));
    private readonly FakeFileSystemService _fileSystem;
    private readonly TestOutputWriter _outputWriter;
    private readonly MoveCommand _command;

    public MoveCommandTests()
    {
        _fileSystem = new FakeFileSystemService(_work);
        _outputWriter = new TestOutputWriter();
        _command = new MoveCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("move", arguments));
    }

    private string InWork(params string[] parts)
    {
        return Path.Combine(new[] { _work }.Concat(parts).ToArray());
    }

    private static string Line(string text)
    {
        return text + Environment.NewLine;
    }

    [TestMethod]
    public void Move_SingleFileToFolder_FileIsInTheNewFolder()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddDirectory(InWork("target"));

        Run("a.txt", "target");

        Assert.IsFalse(_fileSystem.FileExists(InWork("a.txt")));
        Assert.IsTrue(_fileSystem.FileExists(InWork("target", "a.txt")));
        Assert.AreEqual(1, _fileSystem.MoveCalls);
    }

    [TestMethod]
    public void Move_SingleFileToFolder_ReportsOneFileMoved()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddDirectory(InWork("target"));

        Run("a.txt", "target");

        Assert.AreEqual(Line("        1 file(s) moved."), _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Move_FileToNewPath_MovesAndRenamesIt()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddDirectory(InWork("target"));

        Run("a.txt", Path.Combine("target", "b.txt"));

        Assert.IsFalse(_fileSystem.FileExists(InWork("a.txt")));
        Assert.IsTrue(_fileSystem.FileExists(InWork("target", "b.txt")));
        Assert.AreEqual(Line("        1 file(s) moved."), _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Move_MultipleFilesWithCommas_MovesAllOfThem()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddFile(InWork("b.txt"));
        _fileSystem.AddFile(InWork("c.txt"));
        _fileSystem.AddDirectory(InWork("target"));

        Run("a.txt,b.txt,c.txt", "target");

        foreach (string name in new[] { "a.txt", "b.txt", "c.txt" })
        {
            Assert.IsFalse(_fileSystem.FileExists(InWork(name)));
            Assert.IsTrue(_fileSystem.FileExists(InWork("target", name)));
        }

        Assert.AreEqual(Line("        3 file(s) moved."), _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Move_MultipleFilesWithSpaces_LastNameIsTheFolder()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddFile(InWork("b.txt"));
        _fileSystem.AddDirectory(InWork("target"));

        Run("a.txt", "b.txt", "target");

        Assert.IsTrue(_fileSystem.FileExists(InWork("target", "a.txt")));
        Assert.IsTrue(_fileSystem.FileExists(InWork("target", "b.txt")));
        Assert.AreEqual(2, _fileSystem.MoveCalls);
        Assert.AreEqual(Line("        2 file(s) moved."), _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Move_MultipleFilesToSingleFile_ShowsMessageAndMovesNothing()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddFile(InWork("b.txt"));

        Run("a.txt,b.txt", "new.txt");

        Assert.AreEqual(Line(MultipleToFileMessage), _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.MoveCalls);
        Assert.IsTrue(_fileSystem.FileExists(InWork("a.txt")));
        Assert.IsTrue(_fileSystem.FileExists(InWork("b.txt")));
    }

    [TestMethod]
    public void Move_MissingFile_ShowsCannotFindMessage()
    {
        _fileSystem.AddDirectory(InWork("target"));

        Run("missing.txt", "target");

        Assert.AreEqual(Line(NotFoundMessage), _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.MoveCalls);
    }

    [TestMethod]
    public void Move_FileAlreadyExistsInFolder_ShowsDuplicateMessageAndKeepsBothFiles()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddFile(InWork("target", "a.txt"));
        _fileSystem.AddDirectory(InWork("target"));

        Run("a.txt", "target");

        Assert.AreEqual(Line(DuplicateMessage), _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(InWork("a.txt")));
        Assert.IsTrue(_fileSystem.FileExists(InWork("target", "a.txt")));
    }

    [TestMethod]
    public void Move_NoArguments_ShowsSyntaxMessage()
    {
        Run();

        Assert.AreEqual(Line(SyntaxMessage), _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.MoveCalls);
    }

    [TestMethod]
    public void Move_OnlyOneArgument_ShowsSyntaxMessage()
    {
        _fileSystem.AddFile(InWork("a.txt"));

        Run("a.txt");

        Assert.AreEqual(Line(SyntaxMessage), _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.MoveCalls);
    }

    [TestMethod]
    public void Move_ProtectedFile_IsPreventedWithAccessDenied()
    {
        string file = InWork("system.sys");
        _fileSystem.AddProtectedFile(file);
        _fileSystem.AddDirectory(InWork("target"));

        Run("system.sys", "target");

        Assert.AreEqual(Line(AccessDeniedMessage), _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.MoveCalls);
        Assert.IsTrue(_fileSystem.FileExists(file));
    }

    [TestMethod]
    public void Move_OverwritingProtectedFile_IsPreventedWithAccessDenied()
    {
        _fileSystem.AddFile(InWork("a.txt"));
        _fileSystem.AddProtectedFile(InWork("system.sys"));

        Run("a.txt", "system.sys");

        Assert.AreEqual(Line(AccessDeniedMessage), _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.MoveCalls);
        Assert.IsTrue(_fileSystem.FileExists(InWork("a.txt")));
    }
}