using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MkDirCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";
    private const string AccessDeniedMessage = "Access is denied.";

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
    public void MkDir_HasNameAndDescription()
    {
        Assert.AreEqual("mkdir", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }

    // ---- Criterion 1: directory does not exist -> created, nothing printed ----

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

        Assert.IsNull(_outputWriter.output);
    }

    [TestMethod]
    public void MkDir_NewDirectoryWithAbsolutePath_CreatesItThere()
    {
        string absolute = Path.Combine(_work, "other", "Absolute");

        Run(absolute);

        Assert.IsNull(_outputWriter.output);
        Assert.IsTrue(_fileSystem.DirectoryExists(absolute));
    }

    [TestMethod]
    public void MkDir_NameInQuotes_CreatesDirectoryWithoutQuotes()
    {
        Run("\"My Folder\"");

        Assert.IsNull(_outputWriter.output);
        Assert.IsTrue(_fileSystem.DirectoryExists(Path.Combine(_work, "My Folder")));
    }

    [TestMethod]
    public void MkDir_QuotedNameSplitByParser_JoinsArguments()
    {
        Run("\"My", "Folder\"");

        Assert.IsNull(_outputWriter.output);
        Assert.IsTrue(_fileSystem.DirectoryExists(Path.Combine(_work, "My Folder")));
    }

    // ---- Criterion 2: directory already exists -> message ----

    [TestMethod]
    public void MkDir_ExistingDirectory_ShowsAlreadyExistsMessage()
    {
        _fileSystem.AddDirectory(Path.Combine(_work, "ExistingFolder"));

        Run("ExistingFolder");

        Assert.AreEqual("A subdirectory or file ExistingFolder already exists.", _outputWriter.output);
    }

    [TestMethod]
    public void MkDir_ExistingDirectory_DoesNotCreateAgain()
    {
        _fileSystem.AddDirectory(Path.Combine(_work, "ExistingFolder"));

        Run("ExistingFolder");

        Assert.AreEqual(0, _fileSystem.CreateDirectoryCalls);
    }

    [TestMethod]
    public void MkDir_ExistingDirectory_MessageContainsNameAsTyped()
    {
        string absolute = Path.Combine(_work, "Existing");
        _fileSystem.AddDirectory(absolute);

        Run(absolute);

        Assert.AreEqual($"A subdirectory or file {absolute} already exists.", _outputWriter.output);
    }

    [TestMethod]
    public void MkDir_SecondCallWithSameName_ShowsAlreadyExists()
    {
        Run("Twice");
        Assert.IsNull(_outputWriter.output);

        Run("Twice");

        Assert.AreEqual("A subdirectory or file Twice already exists.", _outputWriter.output);
        Assert.AreEqual(1, _fileSystem.CreateDirectoryCalls);
    }

    // ---- Errors ----

    [TestMethod]
    public void MkDir_NoArgument_ShowsSyntaxMessage()
    {
        Run();

        Assert.AreEqual(SyntaxMessage, _outputWriter.output);
        Assert.AreEqual(0, _fileSystem.CreateDirectoryCalls);
    }

    [TestMethod]
    public void MkDir_OnlyQuotes_ShowsSyntaxMessage()
    {
        Run("\"\"");

        Assert.AreEqual(SyntaxMessage, _outputWriter.output);
        Assert.AreEqual(0, _fileSystem.CreateDirectoryCalls);
    }

    [TestMethod]
    public void MkDir_InvalidPath_ShowsSyntaxMessage()
    {
        _fileSystem.ResolvePathException = new ArgumentException("invalid");

        Run("bad|name");

        Assert.AreEqual(SyntaxMessage, _outputWriter.output);
    }

    [TestMethod]
    public void MkDir_IoError_ShowsSyntaxMessage()
    {
        _fileSystem.CreateDirectoryException = new IOException("disk error");

        Run("NewFolder");

        Assert.AreEqual(SyntaxMessage, _outputWriter.output);
    }

    [TestMethod]
    public void MkDir_NoPermission_ShowsAccessDenied()
    {
        _fileSystem.CreateDirectoryException = new UnauthorizedAccessException();

        Run("NewFolder");

        Assert.AreEqual(AccessDeniedMessage, _outputWriter.output);
    }

    [TestMethod]
    public void MkDir_UnexpectedException_IsNotSwallowed()
    {
        _fileSystem.CreateDirectoryException = new InvalidOperationException();

        Assert.ThrowsExactly<InvalidOperationException>(() => Run("NewFolder"));
    }

    // ---- Through parser, factory and invoker (like typing it into the shell) ----

    private CommandInvoker CreateInvoker(TestOutputWriter writer)
    {
        var factory = new CommandFactory();
        factory.Register("mkdir", (outputWriter) => new MkDirCommand(_fileSystem, outputWriter));
        return new CommandInvoker(new CommandParser(), factory, writer);
    }

    [TestMethod]
    [DataRow("mkdir Typed")]
    [DataRow("MKDIR Typed")]
    [DataRow("MkDir Typed")]
    public void MkDir_TypedIntoShell_CreatesDirectoryAndPrintsNothing(string input)
    {
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand(input);

        Assert.IsNull(writer.output);
        Assert.IsTrue(_fileSystem.DirectoryExists(Path.Combine(_work, "Typed")));
    }

    [TestMethod]
    public void MkDir_TypedIntoShellTwice_ShowsAlreadyExists()
    {
        var writer = new TestOutputWriter();
        var invoker = CreateInvoker(writer);
        invoker.ExecuteCommand("MKDIR Typed");

        invoker.ExecuteCommand("MKDIR Typed");

        Assert.AreEqual("A subdirectory or file Typed already exists.", writer.output);
    }

    [TestMethod]
    public void MkDir_RegisteredInDefaultFactory_AsMkdirAndMd()
    {
        var factory = new CommandFactory();

        Assert.IsInstanceOfType(factory.Create("MKDIR", new TestOutputWriter()), typeof(MkDirCommand));
        Assert.IsInstanceOfType(factory.Create("md", new TestOutputWriter()), typeof(MkDirCommand));
    }
}