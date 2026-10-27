using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MkDirCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";
    private const string PathNotFoundMessage = "The system cannot find the path specified.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly string work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-mkdir-tests", "work"));
    private readonly FakeFileSystemService fileSystem;
    private readonly MkDirCommand command;

    public MkDirCommandTests()
    {
        fileSystem = new FakeFileSystemService(work);
        command = new MkDirCommand(fileSystem);
    }

    private CommandResult Run(params string[] arguments)
    {
        return command.Execute(new CommandContext("mkdir", arguments));
    }

    [TestMethod]
    public void MkDir_HasNameAndDescription()
    {
        Assert.AreEqual("mkdir", command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(command.Description));
    }


    [TestMethod]
    public void MkDir_NewDirectory_CreatesDirectory()
    {
        string expected = Path.Combine(work, "NewFolder");

        var result = Run("NewFolder");

        Assert.IsTrue(result.Success);
        Assert.IsTrue(fileSystem.DirectoryExists(expected));
        Assert.AreEqual(1, fileSystem.CreateDirectoryCalls);
    }

    [TestMethod]
    public void MkDir_NewDirectory_PrintsNothing()
    {
        var result = Run("NewFolder");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("", result.OutputMessage);
        Assert.IsFalse(result.ShouldExit);
    }

    [TestMethod]
    public void MkDir_NewDirectoryWithAbsolutePath_CreatesItThere()
    {
        string absolute = Path.Combine(work, "other", "Absolute");

        var result = Run(absolute);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("", result.OutputMessage);
        Assert.IsTrue(fileSystem.DirectoryExists(absolute));
    }

    [TestMethod]
    public void MkDir_NameInQuotes_CreatesDirectoryWithoutQuotes()
    {
        var result = Run("\"My Folder\"");

        Assert.IsTrue(result.Success);
        Assert.IsTrue(fileSystem.DirectoryExists(Path.Combine(work, "My Folder")));
    }

    [TestMethod]
    public void MkDir_SecondCallWithSameName_ShowsAlreadyExists()
    {
        Run("Twice");

        var result = Run("Twice");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("A subdirectory or file Twice already exists.", result.OutputMessage);
    }


    [TestMethod]
    public void MkDir_ExistingDirectory_ShowsAlreadyExistsMessage()
    {
        fileSystem.AddDirectory(Path.Combine(work, "ExistingFolder"));

        var result = Run("ExistingFolder");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("A subdirectory or file ExistingFolder already exists.", result.OutputMessage);
    }

    [TestMethod]
    public void MkDir_ExistingDirectory_DoesNotCreateAgain()
    {
        fileSystem.AddDirectory(Path.Combine(work, "ExistingFolder"));

        Run("ExistingFolder");

        Assert.AreEqual(0, fileSystem.CreateDirectoryCalls);
    }

    [TestMethod]
    public void MkDir_ExistingDirectory_MessageContainsNameAsTyped()
    {
        string absolute = Path.Combine(work, "Existing");
        fileSystem.AddDirectory(absolute);

        var result = Run(absolute);

        Assert.AreEqual($"A subdirectory or file {absolute} already exists.", result.OutputMessage);
    }


    [TestMethod]
    public void MkDir_SeveralNames_CreatesAllOfThem()
    {
        var result = Run("one", "two");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("", result.OutputMessage);
        Assert.IsTrue(fileSystem.DirectoryExists(Path.Combine(work, "one")));
        Assert.IsTrue(fileSystem.DirectoryExists(Path.Combine(work, "two")));
    }

    [TestMethod]
    public void MkDir_SeveralNamesOneExists_CreatesOthersAndReportsExisting()
    {
        fileSystem.AddDirectory(Path.Combine(work, "one"));

        var result = Run("one", "two");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("A subdirectory or file one already exists.", result.OutputMessage);
        Assert.IsTrue(fileSystem.DirectoryExists(Path.Combine(work, "two")));
    }


    [TestMethod]
    public void MkDir_NoArgument_ShowsSyntaxMessage()
    {
        var result = Run();

        Assert.IsFalse(result.Success);
        Assert.AreEqual(SyntaxMessage, result.OutputMessage);
        Assert.AreEqual(0, fileSystem.CreateDirectoryCalls);
    }

    [TestMethod]
    public void MkDir_IoError_ShowsPathNotFound()
    {
        fileSystem.CreateDirectoryException = new IOException("disk error");

        var result = Run("NewFolder");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
    }

    [TestMethod]
    public void MkDir_InvalidPath_ShowsPathNotFound()
    {
        fileSystem.ResolvePathException = new ArgumentException("invalid");

        var result = Run("bad|name");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
    }

    [TestMethod]
    public void MkDir_NoPermission_ShowsAccessDenied()
    {
        fileSystem.CreateDirectoryException = new UnauthorizedAccessException();

        var result = Run("NewFolder");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(AccessDeniedMessage, result.OutputMessage);
    }


    [TestMethod]
    [DataRow("mkdir Typed")]
    [DataRow("MKDIR Typed")]
    [DataRow("MkDir Typed")]
    public void MkDir_TypedIntoShell_CreatesDirectoryAndPrintsNothing(string input)
    {
        var invoker = CreateInvoker();

        var result = invoker.ExecuteCommand(input);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("", result.OutputMessage);
        Assert.IsTrue(fileSystem.DirectoryExists(Path.Combine(work, "Typed")));
    }

    [TestMethod]
    public void MkDir_TypedIntoShellTwice_ShowsAlreadyExists()
    {
        var invoker = CreateInvoker();
        invoker.ExecuteCommand("MKDIR Typed");

        var result = invoker.ExecuteCommand("MKDIR Typed");

        Assert.IsFalse(result.Success);
        Assert.AreEqual("A subdirectory or file Typed already exists.", result.OutputMessage);
    }

    private CommandInvoker CreateInvoker()
    {
        var factory = new CommandFactory();
        factory.Register("mkdir", () => new MkDirCommand(fileSystem));
        return new CommandInvoker(new CommandParser(), factory);
    }
}