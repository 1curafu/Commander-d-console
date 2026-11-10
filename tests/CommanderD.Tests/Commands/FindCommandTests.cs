using CommanderD.Commands;
using CommanderD.Filesystem;
using CommanderD.Services;
using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Commands;

[TestClass]
public class FindCommandTests
{
    private readonly TestOutputWriter _outputWriter = new();
    private readonly ShellContext _context;
    private readonly FileSystemService _fileSystem;
    private readonly FindCommand _command;

    public FindCommandTests()
    {
        _context = new ShellContext(new PersistenceService(Path.Combine(Path.GetTempPath(), "commanderd-find-tests-unused.json")));
        _fileSystem = new FileSystemService(_context);
        _command = new FindCommand(_fileSystem, _outputWriter);
    }

    private void AddFile(string name)
    {
        string path = $"{_fileSystem.CurrentDirectory.TrimEnd('\\')}\\{name}";
        _context.CurrentFolder.AddChildItem(new FsFile(name, path) { Content = "x" });
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("find", arguments));
    }

    private string[] OutputLines()
    {
        return _outputWriter.OutputBuffer.TrimEnd().Split(Environment.NewLine);
    }

    [TestMethod]
    public void Execute_FileExistsInCurrentDirectory_ShowsFullPath()
    {
        AddFile("readme.txt");

        Run("readme.txt");

        CollectionAssert.AreEqual(new[] { @"C:\readme.txt" }, OutputLines());
    }

    [TestMethod]
    public void Execute_DirectoryExists_ShowsFullPath()
    {
        _fileSystem.CreateDirectory("docs");

        Run("docs");

        CollectionAssert.AreEqual(new[] { @"C:\docs" }, OutputLines());
    }

    [TestMethod]
    public void Execute_MatchesInSubdirectories_ShowsEveryMatch()
    {
        AddFile("readme.txt");
        _fileSystem.CreateDirectory("docs");
        _fileSystem.ChangeDirectory("docs");
        AddFile("readme.txt");
        _fileSystem.CreateDirectory("deep");
        _fileSystem.ChangeDirectory("deep");
        AddFile("readme.txt");
        _fileSystem.ChangeDirectory(@"\");

        Run("readme.txt");

        string[] lines = OutputLines();
        Assert.AreEqual(3, lines.Length);
        CollectionAssert.Contains(lines, @"C:\readme.txt");
        CollectionAssert.Contains(lines, @"C:\docs\readme.txt");
        CollectionAssert.Contains(lines, @"C:\docs\deep\readme.txt");
    }

    [TestMethod]
    public void Execute_OnlySearchesFromCurrentDirectory()
    {
        AddFile("readme.txt");
        _fileSystem.CreateDirectory("docs");
        _fileSystem.ChangeDirectory("docs");
        AddFile("readme.txt");

        Run("readme.txt");

        CollectionAssert.AreEqual(new[] { @"C:\docs\readme.txt" }, OutputLines());
    }

    [TestMethod]
    public void Execute_OnlyPartialNameMatches_IsNotFound()
    {
        AddFile("readme.txt");

        Run("readme");

        Assert.AreEqual("File not found - readme", _outputWriter.OutputBuffer.TrimEnd());
    }

    [TestMethod]
    public void Execute_NameDoesNotExist_ShowsFileNotFound()
    {
        AddFile("other.txt");

        Run("readme.txt");

        Assert.AreEqual("File not found - readme.txt", _outputWriter.OutputBuffer.TrimEnd());
    }

    [TestMethod]
    public void Execute_NoName_ShowsSyntaxError()
    {
        Run();

        Assert.AreEqual("The syntax of the command is incorrect.", _outputWriter.OutputBuffer.TrimEnd());
    }

    [TestMethod]
    public void Execute_OnlyWhitespaceAsName_ShowsSyntaxError()
    {
        Run("   ");

        Assert.AreEqual("The syntax of the command is incorrect.", _outputWriter.OutputBuffer.TrimEnd());
    }

    [TestMethod]
    [DataRow("README")]
    [DataRow("readme")]
    [DataRow("ReadMe")]
    public void Execute_DifferentCase_StillFindsMatch(string searchName)
    {
        AddFile("Readme");

        Run(searchName);

        CollectionAssert.AreEqual(new[] { @"C:\Readme" }, OutputLines());
    }

    [TestMethod]
    public void Execute_NameInQuotes_RemovesQuotes()
    {
        AddFile("my file.txt");

        Run("\"my", "file.txt\"");

        CollectionAssert.AreEqual(new[] { @"C:\my file.txt" }, OutputLines());
    }
}
