using System.Globalization;
using CommanderD.Commands;
using CommanderD.Filesystem;
using CommanderD.Services;
using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Commands;

[TestClass]
public class DirCommandTests
{
    private readonly CultureInfo _originalCulture;
    private readonly TestOutputWriter _outputWriter = new();
    private readonly ShellContext _context;
    private readonly FileSystemService _fileSystem;
    private readonly DirCommand _command;

    public DirCommandTests()
    {
        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-CH");

        _context = new ShellContext(new PersistenceService(Path.Combine(Path.GetTempPath(), "commanderd-dir-tests-unused.json")));
        _fileSystem = new FileSystemService(_context);
        _command = new DirCommand(_fileSystem, _outputWriter);
    }

    [TestCleanup]
    public void Cleanup()
    {
        CultureInfo.CurrentCulture = _originalCulture;
    }

    private void AddFile(Folder folder, string name, string content)
    {
        folder.AddChildItem(new FsFile(name, @"C:\" + name) { Content = content });
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("dir", arguments));
    }

    [TestMethod]
    public void Execute_FolderWithOneFile_ListsFileAndSummary()
    {
        AddFile(_context.CurrentFolder, "test.txt", "Hallo");

        Run();

        StringAssert.Contains(_outputWriter.OutputBuffer, @" Directory of C:\");
        StringAssert.Contains(_outputWriter.OutputBuffer, "test.txt");
        StringAssert.Contains(_outputWriter.OutputBuffer, "1 File(s)              5 bytes");
        StringAssert.Contains(_outputWriter.OutputBuffer, "0 Dir(s)");
    }

    [TestMethod]
    public void Execute_DirectoryWithFileAndSubfolder_ListsBothAndSummary()
    {
        AddFile(_context.CurrentFolder, "test.txt", "Hallo ich heisse florian und ich mag karate");
        _fileSystem.CreateDirectory("Unterordner");

        Run();

        StringAssert.Contains(_outputWriter.OutputBuffer, "<DIR>          Unterordner");
        StringAssert.Contains(_outputWriter.OutputBuffer, "test.txt");
        StringAssert.Contains(_outputWriter.OutputBuffer, "1 File(s)             43 bytes");
        StringAssert.Contains(_outputWriter.OutputBuffer, "1 Dir(s)");
    }

    [TestMethod]
    public void Execute_EmptyDirectory_ShowsZeroFilesAndFolders()
    {
        Run();

        StringAssert.Contains(_outputWriter.OutputBuffer, "0 File(s)");
        StringAssert.Contains(_outputWriter.OutputBuffer, "0 Dir(s)");
    }

    [TestMethod]
    public void Execute_NoArgument_ListsCurrentDirectory()
    {
        _fileSystem.CreateDirectory("docs");
        _fileSystem.ChangeDirectory("docs");
        AddFile(_context.CurrentFolder, "inside.txt", "x");

        Run();

        StringAssert.Contains(_outputWriter.OutputBuffer, @" Directory of C:\docs");
        StringAssert.Contains(_outputWriter.OutputBuffer, "inside.txt");
    }

    [TestMethod]
    public void Execute_RelativeFolderAsArgument_ListsThatFolder()
    {
        _fileSystem.CreateDirectory("docs");
        _fileSystem.ChangeDirectory("docs");
        AddFile(_context.CurrentFolder, "inside.txt", "x");
        _fileSystem.ChangeDirectory(@"\");

        Run("docs");

        StringAssert.Contains(_outputWriter.OutputBuffer, @" Directory of C:\docs");
        StringAssert.Contains(_outputWriter.OutputBuffer, "inside.txt");
    }

    [TestMethod]
    public void Execute_DirectoryDoesNotExist_WritesError()
    {
        Run("gibtsnicht");

        StringAssert.Contains(_outputWriter.OutputBuffer, "The system cannot find the path specified.");
    }

    [TestMethod]
    public void Execute_FileAsArgument_ListsOnlyThisFile()
    {
        AddFile(_context.CurrentFolder, "test.txt", "Hallo");
        AddFile(_context.CurrentFolder, "andere.txt", "Hallo");

        Run("test.txt");

        StringAssert.Contains(_outputWriter.OutputBuffer, @" Directory of C:\");
        StringAssert.Contains(_outputWriter.OutputBuffer, "test.txt");
        Assert.IsFalse(_outputWriter.OutputBuffer.Contains("andere.txt"));
        StringAssert.Contains(_outputWriter.OutputBuffer, "1 File(s)");
    }

    [TestMethod]
    public void Execute_PathInQuotes_RemovesQuotes()
    {
        _fileSystem.CreateDirectory("docs");

        Run("\"docs\"");

        StringAssert.Contains(_outputWriter.OutputBuffer, @" Directory of C:\docs");
    }

    [TestMethod]
    public void Execute_FoldersAreListedBeforeFiles()
    {
        AddFile(_context.CurrentFolder, "a.txt", "x");
        _fileSystem.CreateDirectory("zeta");

        Run();

        int folderIndex = _outputWriter.OutputBuffer.IndexOf("zeta", StringComparison.Ordinal);
        int fileIndex = _outputWriter.OutputBuffer.IndexOf("a.txt", StringComparison.Ordinal);
        Assert.IsTrue(folderIndex < fileIndex);
    }

    [TestMethod]
    public void Execute_DoesNotReadTheRealDisk()
    {
        string realFolder = Path.Combine(Directory.GetCurrentDirectory(), "commanderd-real-" + Guid.NewGuid());

        try
        {
            Directory.CreateDirectory(realFolder);

            Run();

            Assert.IsFalse(_outputWriter.OutputBuffer.Contains(Path.GetFileName(realFolder)));
        }
        finally
        {
            Directory.Delete(realFolder, true);
        }
    }

    [TestMethod]
    [DataRow("de-CH", "29.10.2026  14:10")]
    [DataRow("en-US", "10/29/2026  2:10 PM")]
    [DataRow("ja-JP", "2026/10/29  14:10")]
    public void Execute_DifferentRegionalSettings_UsesRegionalDateTimeFormat(string culture, string expected)
    {
      
        CultureInfo.CurrentCulture = new CultureInfo(culture, false);
        var date = new DateTime(2026, 10, 29, 14, 10, 0);
        _context.CurrentFolder.AddChildItem(new FsFile("test.txt", @"C:\test.txt") { Content = "Hallo", CreatedOn = date });
        _context.CurrentFolder.AddChildItem(new Folder("Unterordner", @"C:\Unterordner") { CreatedOn = date });

        Run();

        string output = _outputWriter.OutputBuffer.Replace('\u202f', ' ');

        StringAssert.Contains(output, $"{expected}    <DIR>          Unterordner");
        StringAssert.Contains(output, $"{expected}    {"5",14} test.txt");
    }

    [TestMethod]
    public void Execute_OutputEnds_ShowsFileAndDirectoryCount()
    {
        AddFile(_context.CurrentFolder, "a.txt", "Hallo");
        AddFile(_context.CurrentFolder, "b.txt", "Hallo");
        _fileSystem.CreateDirectory("Unterordner");

        Run();

        string[] lines = _outputWriter.OutputBuffer.TrimEnd().Split(Environment.NewLine);

        Assert.AreEqual($"{2,16} File(s) {10,14:N0} bytes", lines[^2]);
        Assert.AreEqual($"{1,16} Dir(s)", lines[^1]);
    }
}
