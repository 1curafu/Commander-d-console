using CommanderD.Commands;
using CommanderD.Filesystem;
using CommanderD.Services;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MkFileCommandTests
{
    private readonly string _work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-mkfile-tests", "work"));
    private readonly FakeFileSystemService _fileSystem;
    private readonly TestOutputWriter _outputWriter;
    private readonly MkFileCommand _command;

    public MkFileCommandTests()
    {
        _fileSystem = new FakeFileSystemService(_work);
        _outputWriter = new TestOutputWriter();
        _command = new MkFileCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("mkfile", arguments));
    }

    [TestMethod]
    public void MkFile_OnlyName_CreatesEmptyFile()
    {
        Run("notes.txt");

        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "notes.txt")));
        Assert.AreEqual(1, _fileSystem.CreateFileCalls);
        Assert.AreEqual(string.Empty, _fileSystem.CreatedFileContent);
    }

    [TestMethod]
    public void MkFile_NewFile_PrintsNothing()
    {
        Run("notes.txt");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void MkFile_NameAndSeveralWords_JoinsContentWithSpaces()
    {
        Run("notes.txt", "Hello", "World", "!");

        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "notes.txt")));
        Assert.AreEqual("Hello World !", _fileSystem.CreatedFileContent);
    }

    [TestMethod]
    public void MkFile_ExistingFile_ShowsAlreadyExistsMessage()
    {
        _fileSystem.AddFile(Path.Combine(_work, "notes.txt"));

        Run("notes.txt", "new content");

        Assert.AreEqual("A subdirectory or file notes.txt already exists." + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.CreateFileCalls);
    }
}
