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
    private const string SyntaxMessage = "The syntax of the command is incorrect.";
    private const string AccessDeniedMessage = "Access is denied.";

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
    public void MkFile_HasNameAndDescription()
    {
        Assert.AreEqual("mkfile", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }

    [TestMethod]
    public void MkFile_Usage_DescribesMkFile()
    {
        StringAssert.Contains(_command.Usage, "MKFILE");
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
    public void MkFile_NameAndOneWord_CreatesFileWithContent()
    {
        Run("notes.txt", "Hello");

        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "notes.txt")));
        Assert.AreEqual("Hello", _fileSystem.CreatedFileContent);
    }

    [TestMethod]
    public void MkFile_NameAndSeveralWords_JoinsContentWithSpaces()
    {
        Run("notes.txt", "Hello", "World", "!");

        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "notes.txt")));
        Assert.AreEqual("Hello World !", _fileSystem.CreatedFileContent);
    }

    [TestMethod]
    public void MkFile_ContentIsNotUsedAsFileName()
    {
        Run("notes.txt", "Hello");

        Assert.IsFalse(_fileSystem.FileExists(Path.Combine(_work, "Hello")));
    }

    [TestMethod]
    public void MkFile_AbsolutePath_CreatesFileThere()
    {
        string absolute = Path.Combine(_work, "other", "notes.txt");

        Run(absolute, "content");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(absolute));
    }

    [TestMethod]
    public void MkFile_RelativePath_IsResolvedFromCurrentDirectory()
    {
        Run(Path.Combine("sub", "notes.txt"));

        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "sub", "notes.txt")));
    }


    [TestMethod]
    public void MkFile_ExistingFile_ShowsAlreadyExistsMessage()
    {
        _fileSystem.AddFile(Path.Combine(_work, "notes.txt"));

        Run("notes.txt", "new content");

        Assert.AreEqual("A subdirectory or file notes.txt already exists." + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.CreateFileCalls);
    }

    [TestMethod]
    public void MkFile_ExistingDirectory_ShowsAlreadyExistsMessage()
    {
        _fileSystem.AddDirectory(Path.Combine(_work, "docs"));

        Run("docs");

        Assert.AreEqual("A subdirectory or file docs already exists." + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.CreateFileCalls);
    }

    [TestMethod]
    public void MkFile_SecondCallWithSameName_ShowsAlreadyExists()
    {
        Run("notes.txt", "first");
        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);

        Run("notes.txt", "second");

        Assert.AreEqual("A subdirectory or file notes.txt already exists." + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(1, _fileSystem.CreateFileCalls);
        Assert.AreEqual("first", _fileSystem.CreatedFileContent);
    }


    [TestMethod]
    public void MkFile_NoArgument_ShowsSyntaxMessage()
    {
        Run();

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.CreateFileCalls);
    }

    [TestMethod]
    public void MkFile_InvalidPath_ShowsSyntaxMessage()
    {
        _fileSystem.ResolvePathException = new ArgumentException("invalid");

        Run("bad|name.txt");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
        Assert.AreEqual(0, _fileSystem.CreateFileCalls);
    }

    [TestMethod]
    public void MkFile_InvalidNameFromService_ShowsSyntaxMessage()
    {
        _fileSystem.CreateFileException = new ArgumentException("invalid");

        Run("bad|name.txt");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void MkFile_IoError_ShowsSyntaxMessage()
    {
        _fileSystem.CreateFileException = new IOException("disk error");

        Run("notes.txt");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void MkFile_NoPermission_ShowsAccessDenied()
    {
        _fileSystem.CreateFileException = new UnauthorizedAccessException();

        Run("notes.txt");

        Assert.AreEqual(AccessDeniedMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void MkFile_UnexpectedException_IsNotSwallowed()
    {
        _fileSystem.CreateFileException = new InvalidOperationException();

        Assert.ThrowsExactly<InvalidOperationException>(() => Run("notes.txt"));
    }


    private CommandInvoker CreateInvoker(TestOutputWriter writer)
    {
        var factory = new CommandFactory();
        factory.Register("mkfile", (outputWriter) => new MkFileCommand(_fileSystem, outputWriter));
        factory.Register("mf", (outputWriter) => new MkFileCommand(_fileSystem, outputWriter));
        return new CommandInvoker(new Parser(), factory, writer);
    }

    [TestMethod]
    [DataRow("mkfile notes.txt Hello World")]
    [DataRow("MKFILE notes.txt Hello World")]
    [DataRow("MkFile notes.txt Hello World")]
    [DataRow("mf notes.txt Hello World")]
    public void MkFile_TypedIntoShell_CreatesFileWithContent(string input)
    {
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand(input);

        Assert.AreEqual(string.Empty, writer.OutputBuffer);
        Assert.IsTrue(_fileSystem.FileExists(Path.Combine(_work, "notes.txt")));
        Assert.AreEqual("Hello World", _fileSystem.CreatedFileContent);
    }

    [TestMethod]
    public void MkFile_TypedIntoShellTwice_ShowsAlreadyExists()
    {
        var writer = new TestOutputWriter();
        var invoker = CreateInvoker(writer);
        invoker.ExecuteCommand("mkfile notes.txt");

        invoker.ExecuteCommand("mkfile notes.txt");

        Assert.AreEqual("A subdirectory or file notes.txt already exists." + Environment.NewLine, writer.OutputBuffer);
    }

    [TestMethod]
    public void MkFile_RegisteredInDefaultFactory_AsMkFileAndMf()
    {
        var factory = new CommandFactory();

        Assert.IsInstanceOfType(factory.Create("MKFILE", new TestOutputWriter()), typeof(MkFileCommand));
        Assert.IsInstanceOfType(factory.Create("mf", new TestOutputWriter()), typeof(MkFileCommand));
    }


    [TestMethod]
    public void MkFile_ThenType_ShowsTheContent()
    {
        string unusedJson = Path.Combine(Path.GetTempPath(), "commanderd-mkfile-tests-" + Guid.NewGuid(), "state.json");
        var fileSystem = new FileSystemService(new ShellContext(new PersistenceService(unusedJson)));
        var writer = new TestOutputWriter();
        var invoker = new CommandInvoker(new Parser(), new CommandFactory(fileSystem, new FakeConsoleService()), writer);

        invoker.ExecuteCommand("mkfile notes.txt Hello World");
        invoker.ExecuteCommand("type notes.txt");

        Assert.AreEqual("Hello World", writer.OutputBuffer);
    }
}
