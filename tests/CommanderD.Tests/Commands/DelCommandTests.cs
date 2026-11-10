using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class DelCommandTests
{
    private const string AccessDeniedMessage = "Access is denied.";
    private const string FileInUseMessage = "The process cannot access the file because it is being used by another process.";

    private readonly string _work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-del-tests", "work"));
    private readonly FakeFileSystemService _fileSystem;
    private readonly TestOutputWriter _outputWriter;
    private readonly DelCommand _command;

    public DelCommandTests()
    {
        _fileSystem = new FakeFileSystemService(_work);
        _outputWriter = new TestOutputWriter();
        _command = new DelCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("del", arguments));
    }

    [TestMethod]
    public void Del_ExistingFile_DeletesFile()
    {
        string file = Path.Combine(_work, "test.txt");
        _fileSystem.AddFile(file);

        Run("test.txt");

        Assert.IsFalse(_fileSystem.FileExists(file));
        Assert.AreEqual(1, _fileSystem.DeleteFileCalls);
    }

    [TestMethod]
    public void Del_ExistingFile_PrintsNothing()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"));

        Run("test.txt");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Del_MissingFile_ShowsCouldNotFindWithFullPath()
    {
        Run("missing.txt");

        string expected = "Could Not Find " + Path.Combine(_work, "missing.txt") + Environment.NewLine;
        Assert.AreEqual(expected, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Del_ProtectedFile_IsNotDeletedAndShowsAccessDenied()
    {
        string file = Path.Combine(_work, "system.sys");
        _fileSystem.AddProtectedFile(file);

        Run("system.sys");

        Assert.AreEqual(AccessDeniedMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(file));
        Assert.AreEqual(0, _fileSystem.DeleteFileCalls);
    }

    [TestMethod]
    public void Del_LockedFile_ShowsFileInUseMessage()
    {
        _fileSystem.AddFile(Path.Combine(_work, "locked.txt"));
        _fileSystem.DeleteFileException = new IOException("locked");

        Run("locked.txt");

        Assert.AreEqual(FileInUseMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }
}