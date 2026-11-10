using CommanderD.Commands;
using CommanderD.Filesystem;
using CommanderD.Services;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Commands;

[TestClass]
public class TypeCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.\n";
    private const string FileNotFoundMessage = "The system cannot find the file specified.\n";

    private ShellContext _context = null!;
    private FileSystemService _fileSystem = null!;
    private TestOutputWriter _outputWriter = null!;
    private TypeCommand _command = null!;

    [TestInitialize]
    public void Setup()
    {

        string unusedJson = Path.Combine(Path.GetTempPath(), "commanderd-type-tests-" + Guid.NewGuid(), "state.json");
        _context = new ShellContext(new PersistenceService(unusedJson));
        _fileSystem = new FileSystemService(_context);
        _outputWriter = new TestOutputWriter();
        _command = new TypeCommand(_fileSystem, _outputWriter);
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("type", arguments));
    }

    private Folder Root => _context.Drives[0].RootFolder;

    private static FsFile AddFile(Folder folder, string name, string? content)
    {
        var file = new FsFile(name, name) { Content = content };
        folder.AddChildItem(file);
        return file;
    }

    private static Folder AddFolder(Folder parent, string name)
    {
        var folder = new Folder(name, name);
        parent.AddChildItem(folder);
        return folder;
    }

    [TestMethod]
    public void Type_HasNameAndDescription()
    {
        Assert.AreEqual("type", _command.Name, ignoreCase: true);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }


    [TestMethod]
    public void Type_ExistingFile_PrintsContent()
    {
        AddFile(Root, "hello.txt", "Hello World");

        Run("hello.txt");

        Assert.AreEqual("Hello World", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_MultilineContent_PrintsContentUnchanged()
    {
        string content = "line 1\r\nline 2\r\n\r\nline 4";
        AddFile(Root, "lines.txt", content);

        Run("lines.txt");

        Assert.AreEqual(content, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_EmptyContent_PrintsNothing()
    {
        AddFile(Root, "empty.txt", string.Empty);

        Run("empty.txt");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    [DataRow("hello.txt")]
    [DataRow("HELLO.TXT")]
    [DataRow("Hello.Txt")]
    public void Type_FileNameInOtherCase_PrintsContent(string name)
    {
        AddFile(Root, "hello.txt", "Hello World");

        Run(name);

        Assert.AreEqual("Hello World", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_SeveralFiles_PrintsOnlyTheRequestedOne()
    {
        AddFile(Root, "a.txt", "content of a");
        AddFile(Root, "b.txt", "content of b");
        AddFile(Root, "c.txt", "content of c");

        Run("b.txt");

        Assert.AreEqual("content of b", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_DoesNotChangeTheFile()
    {
        FsFile file = AddFile(Root, "hello.txt", "Hello World");

        Run("hello.txt");

        Assert.AreEqual("Hello World", file.Content);
        Assert.HasCount(1, Root.GetChildItems());
    }

    [TestMethod]
    public void Type_CalledTwice_PrintsContentTwice()
    {
        AddFile(Root, "hello.txt", "Hi");

        Run("hello.txt");
        Run("hello.txt");

        Assert.AreEqual("HiHi", _outputWriter.OutputBuffer);
    }


    [TestMethod]
    public void Type_RelativePathIntoSubfolder_PrintsContent()
    {
        Folder docs = AddFolder(Root, "docs");
        AddFile(docs, "notes.txt", "my notes");

        Run(@"docs\notes.txt");

        Assert.AreEqual("my notes", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_ForwardSlashes_PrintsContent()
    {
        Folder docs = AddFolder(Root, "docs");
        AddFile(docs, "notes.txt", "my notes");

        Run("docs/notes.txt");

        Assert.AreEqual("my notes", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_AbsolutePath_PrintsContent()
    {
        Folder docs = AddFolder(Root, "docs");
        AddFile(docs, "notes.txt", "my notes");

        Run(@"C:\docs\notes.txt");

        Assert.AreEqual("my notes", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_RelativeToCurrentDirectory_PrintsContent()
    {
        Folder docs = AddFolder(Root, "docs");
        AddFile(docs, "notes.txt", "my notes");
        _fileSystem.ChangeDirectory(@"C:\docs");

        Run("notes.txt");

        Assert.AreEqual("my notes", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_ParentFolderPath_PrintsContent()
    {
        AddFolder(Root, "docs");
        AddFile(Root, "readme.txt", "read me");
        _fileSystem.ChangeDirectory(@"C:\docs");

        Run(@"..\readme.txt");

        Assert.AreEqual("read me", _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_FileOnOtherDrive_PrintsContent()
    {
        var driveD = new Drive("D", "Data");
        _context.Drives.Add(driveD);
        AddFile(driveD.RootFolder, "data.txt", "on drive D");

        Run(@"D:\data.txt");

        Assert.AreEqual("on drive D", _outputWriter.OutputBuffer);
    }


    [TestMethod]
    public void Type_MissingFile_ShowsFileNotFound()
    {
        Run("missing.txt");

        Assert.AreEqual(FileNotFoundMessage, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_FileInOtherFolder_ShowsFileNotFound()
    {
        Folder docs = AddFolder(Root, "docs");
        AddFile(docs, "notes.txt", "my notes");

        Run("notes.txt");

        Assert.AreEqual(FileNotFoundMessage, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_MissingFolderInPath_ShowsFileNotFound()
    {
        Run(@"nowhere\notes.txt");

        Assert.AreEqual(FileNotFoundMessage, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_UnknownDrive_ShowsFileNotFound()
    {
        Run(@"X:\notes.txt");

        Assert.AreEqual(FileNotFoundMessage, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_Folder_ShowsFileNotFound()
    {
        AddFolder(Root, "docs");

        Run("docs");

        Assert.AreEqual(FileNotFoundMessage, _outputWriter.OutputBuffer);
    }


    [TestMethod]
    public void Type_NoArgument_ShowsSyntaxMessage()
    {
        Run();

        Assert.AreEqual(SyntaxMessage, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_TwoArguments_ShowsSyntaxMessage()
    {
        AddFile(Root, "a.txt", "content of a");
        AddFile(Root, "b.txt", "content of b");

        Run("a.txt", "b.txt");

        Assert.AreEqual(SyntaxMessage, _outputWriter.OutputBuffer);
    }


    private CommandInvoker CreateInvoker(TestOutputWriter writer)
    {
        var factory = new CommandFactory(_fileSystem, new FakeConsoleService());
        return new CommandInvoker(new Parser(), factory, writer);
    }

    [TestMethod]
    [DataRow("type hello.txt")]
    [DataRow("TYPE hello.txt")]
    [DataRow("Type hello.txt")]
    [DataRow("  type   hello.txt  ")]
    public void Type_TypedIntoShell_PrintsContent(string input)
    {
        AddFile(Root, "hello.txt", "Hello World");
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand(input);

        Assert.AreEqual("Hello World", writer.OutputBuffer);
    }

    [TestMethod]
    public void Type_TypedIntoShellAfterCd_PrintsContent()
    {
        Folder docs = AddFolder(Root, "docs");
        AddFile(docs, "notes.txt", "my notes");
        var writer = new TestOutputWriter();
        var invoker = CreateInvoker(writer);
        invoker.ExecuteCommand("cd docs");
        writer.ClearOutputBuffer();

        invoker.ExecuteCommand("type notes.txt");

        Assert.AreEqual("my notes", writer.OutputBuffer);
    }

    [TestMethod]
    public void Type_TypedIntoShellWithMissingFile_ShowsFileNotFound()
    {
        var writer = new TestOutputWriter();

        CreateInvoker(writer).ExecuteCommand("type missing.txt");

        Assert.AreEqual(FileNotFoundMessage, writer.OutputBuffer);
    }

    [TestMethod]
    public void Type_RegisteredInDefaultFactory()
    {
        var factory = new CommandFactory(_fileSystem, new FakeConsoleService());

        Assert.IsInstanceOfType(factory.Create("TYPE", new TestOutputWriter()), typeof(TypeCommand));
    }
}
