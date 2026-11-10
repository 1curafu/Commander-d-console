using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class CdCommandTests
{
    private const string PathNotFoundMessage = "The system cannot find the path specified.";

    private readonly string _root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-cd-tests"));
    private readonly string _work;
    private readonly string _src;
    private readonly string _spaced;
    private readonly FakeFileSystemService _fileSystem;
    private readonly CdCommand _command;
    private readonly TestOutputWriter _outputWriter;

    public CdCommandTests()
    {
        _work = Path.Combine(_root, "work");
        _src = Path.Combine(_work, "src");
        _spaced = Path.Combine(_work, "My Folder");

        _fileSystem = new FakeFileSystemService(_work);
        _fileSystem.AddDirectory(_root);
        _fileSystem.AddDirectory(_src);
        _fileSystem.AddDirectory(_spaced);

        _outputWriter = new TestOutputWriter();

        _command = new CdCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("cd", arguments));
    }

    [TestMethod]
    public void Cd_NoArgument_ShowsCurrentDirectory()
    {
        Run();

        Assert.AreEqual(_work + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Cd_RelativeDirectory_ChangesDirectory()
    {
        Run("src");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.AreEqual(_src, _fileSystem.ChangedTo);
        Assert.AreEqual(_src, _fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_AbsoluteDirectory_ChangesDirectory()
    {
        Run(_src);

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.AreEqual(_src, _fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_ParentDirectory_ChangesToParent()
    {
        Run("..");

        Assert.AreEqual(_root, _fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_DirectoryDoesNotExist_ShowsPathNotFound()
    {
        Run("missing");

        Assert.AreEqual(PathNotFoundMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Cd_DirectoryDoesNotExist_KeepsCurrentDirectory()
    {
        var directoryBefore = _fileSystem.CurrentDirectory;
        
        Run("missing");

        Assert.AreEqual(directoryBefore, _fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_ChangeDirectoryThrowsUnauthorized_ShowsPathNotFound()
    {
        _fileSystem.ChangeDirectoryException = new UnauthorizedAccessException();

        Run("src");

        Assert.AreEqual(PathNotFoundMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(_work, _fileSystem.CurrentDirectory);
    }

    private void RunLine(string input)
    {
        CommandContext? context = new Parser().Parse(input);

        Assert.IsNotNull(context);
        Assert.AreEqual("cd", context.Name.ToLowerInvariant());
        _command.Execute(context);
    }

    [TestMethod]
    public void Cd_WithoutSpaceBeforeDoubleDot_WorksLikeCdDoubleDot()
    {
        RunLine("CD..");

        Assert.AreEqual(_root, _fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_WithoutSpaceBeforeBackslash_ParsesLikeCdBackslash()
    {
        var withoutSpace = new Parser().Parse("CD\\");
        var withSpace = new Parser().Parse("CD \\");

        Assert.IsNotNull(withoutSpace);
        Assert.IsNotNull(withSpace);
        CollectionAssert.AreEqual(withSpace.Arguments, withoutSpace.Arguments);
    }

    [TestMethod]
    public void Cd_Root_ChangesToRootOfCurrentDrive()
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        string driveRoot = Path.GetPathRoot(_work)!;
        _fileSystem.AddDirectory(driveRoot);

        Run(separator);

        Assert.AreEqual(driveRoot, _fileSystem.ChangedTo);
    }
}