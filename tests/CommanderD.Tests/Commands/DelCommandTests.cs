using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class DelCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";
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
    public void Del_HasNameAndDescription()
    {
        Assert.AreEqual("del", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
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
    public void Del_ExistingFileWithAbsolutePath_DeletesIt()
    {
        string absolute = Path.Combine(_work, "other", "absolute.txt");
        _fileSystem.AddFile(absolute);

        Run(absolute);

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.IsFalse(_fileSystem.FileExists(absolute));
    }

    [TestMethod]
    public void Del_NameInQuotes_DeletesFileWithoutQuotes()
    {
        string file = Path.Combine(_work, "my file.txt");
        _fileSystem.AddFile(file);

        Run("\"my file.txt\"");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.IsFalse(_fileSystem.FileExists(file));
    }

    [TestMethod]
    public void Del_QuotedNameSplitByParser_JoinsArguments()
    {
        string file = Path.Combine(_work, "my file.txt");
        _fileSystem.AddFile(file);

        Run("\"my", "file.txt\"");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.IsFalse(_fileSystem.FileExists(file));
    }

    [TestMethod]
    public void Del_OnlyDeletesTheNamedFile()
    {
        string other = Path.Combine(_work, "other.txt");
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"));
        _fileSystem.AddFile(other);

        Run("test.txt");

        Assert.IsTrue(_fileSystem.FileExists(other));
    }


    [TestMethod]
    public void Del_MissingFile_ShowsCouldNotFindWithFullPath()
    {
        Run("missing.txt");

        string expected = "Could Not Find " + Path.Combine(_work, "missing.txt") + Environment.NewLine;
        Assert.AreEqual(expected, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Del_MissingFile_DeletesNothing()
    {
        Run("missing.txt");

        Assert.AreEqual(0, _fileSystem.DeleteFileCalls);
    }

    [TestMethod]
    public void Del_Directory_IsNotDeletedAndShowsCouldNotFind()
    {
        string directory = Path.Combine(_work, "folder");
        _fileSystem.AddDirectory(directory);

        Run("folder");

        Assert.AreEqual("Could Not Find " + directory + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.DirectoryExists(directory));
        Assert.AreEqual(0, _fileSystem.DeleteFileCalls);
    }

    [TestMethod]
    public void Del_SecondCallWithSameName_ShowsCouldNotFind()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"));
        Run("test.txt");
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);

        Run("test.txt");

        Assert.AreEqual("Could Not Find " + Path.Combine(_work, "test.txt") + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(1, _fileSystem.DeleteFileCalls);
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
    public void Del_NoPermission_ShowsAccessDenied()
    {
        _fileSystem.AddFile(Path.Combine(_work, "readonly.txt"));
        _fileSystem.DeleteFileException = new UnauthorizedAccessException();

        Run("readonly.txt");

        Assert.AreEqual(AccessDeniedMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Del_LockedFile_ShowsFileInUseMessage()
    {
        _fileSystem.AddFile(Path.Combine(_work, "locked.txt"));
        _fileSystem.DeleteFileException = new IOException("locked");

        Run("locked.txt");

        Assert.AreEqual(FileInUseMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }


    [TestMethod]
    public void Del_NoArgument_ShowsSyntaxMessage()
    {
        Run();

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.DeleteFileCalls);
    }

    [TestMethod]
    public void Del_OnlyQuotes_ShowsSyntaxMessage()
    {
        Run("\"\"");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.DeleteFileCalls);
    }

    [TestMethod]
    public void Del_InvalidPath_ShowsSyntaxMessage()
    {
        _fileSystem.ResolvePathException = new ArgumentException("invalid");

        Run("bad|name");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Del_UnexpectedException_IsNotSwallowed()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"));
        _fileSystem.DeleteFileException = new InvalidOperationException();

        Assert.ThrowsExactly<InvalidOperationException>(() => Run("test.txt"));
    }


    private CommandInvoker CreateInvoker(TestOutputWriter writer)
    {
        var factory = new CommandFactory();
        factory.Register("del", (outputWriter) => new DelCommand(_fileSystem, outputWriter));
        return new CommandInvoker(new Parser(), factory, writer);
    }

    [TestMethod]
    [DataRow("del test.txt")]
    [DataRow("DEL test.txt")]
    [DataRow("Del test.txt")]
    public void Del_TypedIntoShell_DeletesFileAndPrintsNothing(string input)
    {
        var writer = new TestOutputWriter();
        string file = Path.Combine(_work, "test.txt");
        _fileSystem.AddFile(file);

        CreateInvoker(writer).ExecuteCommand(input);

        Assert.AreEqual(string.Empty, writer.OutputBuffer);
        Assert.IsFalse(_fileSystem.FileExists(file));
    }

    [TestMethod]
    public void Del_TypedIntoShellForMissingFile_ShowsCouldNotFind()
    {
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand("DEL missing.txt");

        Assert.AreEqual("Could Not Find " + Path.Combine(_work, "missing.txt") + Environment.NewLine, writer.OutputBuffer);
    }

    [TestMethod]
    public void Del_RegisteredInDefaultFactory()
    {
        var factory = new CommandFactory();

        Assert.IsInstanceOfType(factory.Create("DEL", new TestOutputWriter()), typeof(DelCommand));
    }
}