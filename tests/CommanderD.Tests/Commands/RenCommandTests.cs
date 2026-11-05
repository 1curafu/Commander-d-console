using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class RenCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";
    private const string NotFoundMessage = "The system cannot find the file specified.";
    private const string DuplicateMessage = "A duplicate file name exists, or the file cannot be found.";
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
    public void Ren_HasNameAndDescription()
    {
        Assert.AreEqual("ren", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
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
    public void Ren_ExistingFile_PrintsNothing()
    {
        _fileSystem.AddFile(Path.Combine(_work, "old.txt"));

        Run("old.txt", "new.txt");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
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
    public void Ren_FileWithAbsolutePath_RenamesInItsOwnFolder()
    {
        string folder = Path.Combine(_work, "other");
        _fileSystem.AddFile(Path.Combine(folder, "old.txt"));

        Run("\"" + Path.Combine(folder, "old.txt") + "\"", "new.txt");

        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(folder, "new.txt")));
        Assert.IsFalse(_fileSystem.FileExists(Path.Combine(_work, "new.txt")));
    }

    [TestMethod]
    public void Ren_OnlyRenamesTheNamedItem()
    {
        string other = Path.Combine(_work, "other.txt");
        _fileSystem.AddFile(Path.Combine(_work, "old.txt"));
        _fileSystem.AddFile(other);

        Run("old.txt", "new.txt");

        Assert.IsTrue(_fileSystem.FileExists(other));
    }

    [TestMethod]
    public void Ren_OnlyChangesTheCase_Works()
    {
        _fileSystem.AddFile(Path.Combine(_work, "file.txt"));

        Run("file.txt", "FILE.txt");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "FILE.txt")));
    }

    [TestMethod]
    public void Ren_NamesInQuotes_AreUsedWithoutQuotes()
    {
        _fileSystem.AddFile(Path.Combine(_work, "my old file.txt"));

        Run("\"my", "old", "file.txt\"", "\"my", "new", "file.txt\"");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.IsFalse(_fileSystem.FileExists(Path.Combine(_work, "my old file.txt")));
        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "my new file.txt")));
    }


    [TestMethod]
    public void Ren_MissingFile_ShowsCannotFindMessage()
    {
        Run("missing.txt", "new.txt");

        Assert.AreEqual(NotFoundMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Ren_MissingFile_RenamesNothing()
    {
        Run("missing.txt", "new.txt");

        Assert.AreEqual(0, _fileSystem.RenameCalls);
    }

    [TestMethod]
    public void Ren_NewNameAlreadyExists_ShowsDuplicateMessage()
    {
        string old = Path.Combine(_work, "old.txt");
        _fileSystem.AddFile(old);
        _fileSystem.AddFile(Path.Combine(_work, "new.txt"));

        Run("old.txt", "new.txt");

        Assert.AreEqual(DuplicateMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(old));
    }

    [TestMethod]
    public void Ren_NewNameIsExistingDirectory_ShowsDuplicateMessage()
    {
        _fileSystem.AddFile(Path.Combine(_work, "old.txt"));
        _fileSystem.AddDirectory(Path.Combine(_work, "new"));

        Run("old.txt", "new");

        Assert.AreEqual(DuplicateMessage + Environment.NewLine, _outputWriter.OutputBuffer);
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

    [TestMethod]
    public void Ren_UnexpectedException_IsNotSwallowed()
    {
        _fileSystem.AddFile(Path.Combine(_work, "old.txt"));
        _fileSystem.RenameException = new InvalidOperationException();

        Assert.ThrowsExactly<InvalidOperationException>(() => Run("old.txt", "new.txt"));
    }


    [TestMethod]
    public void Ren_NoArguments_ShowsSyntaxMessage()
    {
        Run();

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.RenameCalls);
    }

    [TestMethod]
    public void Ren_OnlyOneArgument_ShowsSyntaxMessage()
    {
        _fileSystem.AddFile(Path.Combine(_work, "old.txt"));

        Run("old.txt");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.RenameCalls);
    }

    [TestMethod]
    public void Ren_TooManyArguments_ShowsSyntaxMessage()
    {
        Run("a.txt", "b.txt", "c.txt");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.RenameCalls);
    }

    [TestMethod]
    public void Ren_EmptyQuotes_ShowsSyntaxMessage()
    {
        Run("old.txt", "\"\"");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.RenameCalls);
    }

    [TestMethod]
    public void Ren_NewNameWithPath_ShowsSyntaxMessage()
    {
        string file = Path.Combine(_work, "old.txt");
        _fileSystem.AddFile(file);

        Run("old.txt", @"sub\new.txt");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(file));
    }

    [TestMethod]
    public void Ren_InvalidPath_ShowsSyntaxMessage()
    {
        _fileSystem.ResolvePathException = new ArgumentException("invalid");

        Run("bad|name", "new.txt");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }


    private CommandInvoker CreateInvoker(TestOutputWriter writer)
    {
        var factory = new CommandFactory();
        factory.Register("ren", outputWriter => new RenCommand(_fileSystem, outputWriter));
        return new CommandInvoker(new Parser(), factory, writer);
    }

    [TestMethod]
    [DataRow("ren old.txt new.txt")]
    [DataRow("REN old.txt new.txt")]
    [DataRow("Ren old.txt new.txt")]
    public void Ren_TypedIntoShell_RenamesFileAndPrintsNothing(string input)
    {
        var writer = new TestOutputWriter();
        _fileSystem.AddFile(Path.Combine(_work, "old.txt"));

        CreateInvoker(writer).ExecuteCommand(input);

        Assert.AreEqual(string.Empty, writer.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "new.txt")));
    }

    [TestMethod]
    public void Ren_TypedIntoShellForMissingFile_ShowsCannotFindMessage()
    {
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand("REN missing.txt new.txt");

        Assert.AreEqual(NotFoundMessage + Environment.NewLine, writer.OutputBuffer);
    }

    [TestMethod]
    public void Ren_RegisteredInDefaultFactory()
    {
        var factory = new CommandFactory();

        Assert.IsInstanceOfType(factory.Create("REN", new TestOutputWriter()), typeof(RenCommand));
        Assert.IsInstanceOfType(factory.Create("rename", new TestOutputWriter()), typeof(RenCommand));
    }
}