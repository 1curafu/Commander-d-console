using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MkDirCommandTests
{
    private readonly string _work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-mkdir-tests", "work"));
    private readonly FakeFileSystemService _fileSystem;
    private readonly TestOutputWriter _outputWriter;
    private readonly MkDirCommand _command;

    public MkDirCommandTests()
    {
        _fileSystem = new FakeFileSystemService(_work);
        _outputWriter = new TestOutputWriter();
        _command = new MkDirCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("mkdir", arguments));
    }

    [TestMethod]
    public void MkDir_NewDirectory_CreatesDirectory()
    {
        Run("NewFolder");

        Assert.IsTrue(_fileSystem.DirectoryExists(Path.Combine(_work, "NewFolder")));
        Assert.AreEqual(1, _fileSystem.CreateDirectoryCalls);
    }

    [TestMethod]
    public void MkDir_NewDirectory_PrintsNothing()
    {
        Run("NewFolder");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void MkDir_ExistingDirectory_ShowsAlreadyExistsMessage()
    {
        _fileSystem.AddDirectory(Path.Combine(_work, "ExistingFolder"));

        Run("ExistingFolder");

        Assert.AreEqual("A subdirectory or file ExistingFolder already exists." + Environment.NewLine, _outputWriter.OutputBuffer);
    }
}