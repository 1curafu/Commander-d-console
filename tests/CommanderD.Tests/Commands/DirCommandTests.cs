using System.Globalization;
using CommanderD.Commands;
using CommanderD.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class DirCommandTests
{
    private readonly string _testDir;
    private readonly CultureInfo _originalCulture;
    private readonly TestOutputWriter _outputWriter = new();

    public DirCommandTests()
    {
       
        _testDir = Path.Combine(Path.GetTempPath(), "DirCommandTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);

   
        _originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-CH");
    }

    [TestCleanup]
    public void Cleanup()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        Directory.Delete(_testDir, true);
    }

    [TestMethod]
    public void Execute_FolderWithOneFile_ListsFileAndSummary()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo");
        var command = new DirCommand(_outputWriter);
        var context = new CommandContext("dir", new[] { _testDir });

        command.Execute(context);

        StringAssert.Contains(_outputWriter.output, $" Verzeichnis von {_testDir}");
        StringAssert.Contains(_outputWriter.output, "test.txt");
        StringAssert.Contains(_outputWriter.output, "1 Datei(en)              5 Bytes");
        StringAssert.Contains(_outputWriter.output, "0 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_FileWithOneFile_ListsFileAndSummary()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo ich heisse florian und ich mag karate");
        var command = new DirCommand(_outputWriter);
        var context = new CommandContext("dir",new[] { _testDir });
        command.Execute(context);

        StringAssert.Contains(_outputWriter.output, $" Verzeichnis von {_testDir}");
        StringAssert.Contains(_outputWriter.output, "test.txt");
        StringAssert.Contains(_outputWriter.output, "1 Datei(en)             43 Bytes");
        StringAssert.Contains(_outputWriter.output, "0 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_DirectoryWithOneFile_ListsFileAndSummary()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo ich heisse florian und ich mag karate");
        Directory.CreateDirectory(Path.Combine(_testDir, "Unterordner"));
        var command = new DirCommand(_outputWriter);
        var context = new CommandContext("dir",new[] { _testDir });
        command.Execute(context);

        StringAssert.Contains(_outputWriter.output, "<DIR>          Unterordner");
        StringAssert.Contains(_outputWriter.output, "test.txt");
        StringAssert.Contains(_outputWriter.output, "1 Datei(en)             43 Bytes");
        StringAssert.Contains(_outputWriter.output, "1 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_EmptyDirectory_ShowsZeroFilesAndFolders()
    {
        var command = new DirCommand(_outputWriter);
        var context = new CommandContext("dir", new[] { _testDir });
        command.Execute(context);

        StringAssert.Contains(_outputWriter.output, "0 Datei(en)");
        StringAssert.Contains(_outputWriter.output, "0 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_NoArgument_ListsCurrentDirectory()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo");
        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_testDir);

        try
        {
            var command = new DirCommand(_outputWriter);
            var context = new CommandContext("dir", new string[0]);
            command.Execute(context);

            StringAssert.Contains(_outputWriter.output, $" Verzeichnis von {_testDir}");
            StringAssert.Contains(_outputWriter.output, "test.txt");
        }
        finally
        {
            // Sonst kann der Temp-Ordner im Cleanup nicht gelöscht werden
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [TestMethod]
    public void Execute_DirectoryDoesNotExist_WritesError()
    {
        string missing = Path.Combine(_testDir, "gibtsnicht");
        var command = new DirCommand(_outputWriter);
        var context = new CommandContext("dir", new[] { missing });
        command.Execute(context);

        StringAssert.Contains(_outputWriter.output, "Das System kann den angegebenen Pfad nicht finden");
    }

    [TestMethod]
    public void Execute_FileAsArgument_ListsOnlyThisFile()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo");
        File.WriteAllText(Path.Combine(_testDir, "andere.txt"), "Hallo");
        var command = new DirCommand(_outputWriter);
        var context = new CommandContext("dir", new[] { Path.Combine(_testDir, "test.txt") });
        command.Execute(context);

        StringAssert.Contains(_outputWriter.output, "test.txt");
        Assert.IsFalse(_outputWriter.output.Contains("andere.txt"));
        StringAssert.Contains(_outputWriter.output, "1 Datei(en)");
    }

    [TestMethod]
    public void Execute_PathInQuotes_RemovesQuotes()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo");
        var command = new DirCommand(_outputWriter);
        var context = new CommandContext("dir", new[] { $"\"{_testDir}\"" });
        command.Execute(context);

        StringAssert.Contains(_outputWriter.output, $" Verzeichnis von {_testDir}");
        StringAssert.Contains(_outputWriter.output, "test.txt");
    }
}
