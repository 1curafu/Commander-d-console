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
    public void Type_MissingFile_ShowsFileNotFound()
    {
        Run("missing.txt");

        Assert.AreEqual(FileNotFoundMessage, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_MissingFolderInPath_ShowsFileNotFound()
    {
        Run(@"nowhere\notes.txt");

        Assert.AreEqual(FileNotFoundMessage, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void Type_NoArgument_ShowsSyntaxMessage()
    {
        Run();

        Assert.AreEqual(SyntaxMessage, _outputWriter.OutputBuffer);
    }
}
