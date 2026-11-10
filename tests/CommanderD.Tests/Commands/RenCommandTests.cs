using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class RenCommandTests
{
    private const string NotFoundMessage = "The system cannot find the file specified.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly string _work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-ren-tests", "work"));
    private readonly FakeFileSystemService _fileSystem;
    private readonly TestOutputWriter _outputWriter;
    private readonly RenCommand _command;

    public RenCommandTests()
    {
        _fileSystem = new FakeFileSystemService(_work);
        _outputWriter = new TestOutputWriter();
        _command = new RenCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("ren", arguments));
    }

    [TestMethod]
    public void Ren_ExistingFile_GetsNewName()
    {
        _fileSystem.AddFile(Path.Combine(_work, "old.txt"));

        Run("old.txt", "new.txt");

        Assert.IsFalse(_fileSystem.FileExists(Path.Combine(_work, "old.txt")));
        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "new.txt")));
        Assert.AreEqual(1, _fileSystem.RenameCalls);
    }

    [TestMethod]
    public void Ren_ExistingDirectory_GetsNewName()
    {
        _fileSystem.AddDirectory(Path.Combine(_work, "olddir"));

        Run("olddir", "newdir");

        Assert.IsFalse(_fileSystem.DirectoryExists(Path.Combine(_work, "olddir")));
        Assert.IsTrue(_fileSystem.DirectoryExists(Path.Combine(_work, "newdir")));
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Ren_MissingFile_ShowsCannotFindMessage()
    {
        Run("missing.txt", "new.txt");

        Assert.AreEqual(NotFoundMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Ren_ProtectedFile_IsNotRenamedAndShowsAccessDenied()
    {
        string file = Path.Combine(_work, "system.sys");
        _fileSystem.AddProtectedFile(file);

        Run("system.sys", "other.sys");

        Assert.AreEqual(AccessDeniedMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(file));
        Assert.AreEqual(0, _fileSystem.RenameCalls);
    }

    [TestMethod]
    public void Ren_SystemLevelError_ShowsAccessDenied()
    {
        _fileSystem.AddDirectory(Path.Combine(_work, "current"));
        _fileSystem.RenameException = new UnauthorizedAccessException();

        Run("current", "other");

        Assert.AreEqual(AccessDeniedMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }
}