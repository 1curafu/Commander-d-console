using System.Globalization;
using CommanderD.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class DirCommandTests
{
    private readonly string _testDir;
    private readonly CultureInfo _originalCulture;

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
        var command = new DirCommand();
        var context = new CommandContext("dir", new[] { _testDir });

        var result = command.Execute(context);

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.OutputMessage, $" Verzeichnis von {_testDir}");
        StringAssert.Contains(result.OutputMessage, "test.txt");
        StringAssert.Contains(result.OutputMessage, "1 Datei(en)              5 Bytes");
        StringAssert.Contains(result.OutputMessage, "0 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_FileWithOneFile_ListsFileAndSummary()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo ich heisse florian und ich mag karate");
        var command = new DirCommand();
        var context = new CommandContext("dir",new[] { _testDir });
        var result = command.Execute(context);

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.OutputMessage, $" Verzeichnis von {_testDir}");
        StringAssert.Contains(result.OutputMessage, "test.txt");
        StringAssert.Contains(result.OutputMessage, "1 Datei(en)             43 Bytes");
        StringAssert.Contains(result.OutputMessage, "0 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_DirectoryWithOneFile_ListsFileAndSummary()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo ich heisse florian und ich mag karate");
        Directory.CreateDirectory(Path.Combine(_testDir, "Unterordner"));
        var command = new DirCommand();
        var context = new CommandContext("dir",new[] { _testDir });
        var result = command.Execute(context);

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.OutputMessage, "<DIR>          Unterordner");
        StringAssert.Contains(result.OutputMessage, "test.txt");
        StringAssert.Contains(result.OutputMessage, "1 Datei(en)             43 Bytes");
        StringAssert.Contains(result.OutputMessage, "1 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_EmptyDirectory_ShowsZeroFilesAndFolders()
    {
        var command = new DirCommand();
        var context = new CommandContext("dir", new[] { _testDir });
        var result = command.Execute(context);

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.OutputMessage, "0 Datei(en)");
        StringAssert.Contains(result.OutputMessage, "0 Verzeichnis(se)");
    }

    [TestMethod]
    public void Execute_NoArgument_ListsCurrentDirectory()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo");
        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_testDir);

        try
        {
            var command = new DirCommand();
            var context = new CommandContext("dir", new string[0]);
            var result = command.Execute(context);

            Assert.IsTrue(result.Success);
            StringAssert.Contains(result.OutputMessage, $" Verzeichnis von {_testDir}");
            StringAssert.Contains(result.OutputMessage, "test.txt");
        }
        finally
        {
            // Sonst kann der Temp-Ordner im Cleanup nicht gelöscht werden
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    [TestMethod]
    public void Execute_DirectoryDoesNotExist_ReturnsFail()
    {
        string missing = Path.Combine(_testDir, "gibtsnicht");
        var command = new DirCommand();
        var context = new CommandContext("dir", new[] { missing });
        var result = command.Execute(context);

        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.OutputMessage, "Das System kann den angegebenen Pfad nicht finden");
    }

    [TestMethod]
    public void Execute_FileAsArgument_ListsOnlyThisFile()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo");
        File.WriteAllText(Path.Combine(_testDir, "andere.txt"), "Hallo");
        var command = new DirCommand();
        var context = new CommandContext("dir", new[] { Path.Combine(_testDir, "test.txt") });
        var result = command.Execute(context);

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.OutputMessage, "test.txt");
        Assert.IsFalse(result.OutputMessage.Contains("andere.txt"));
        StringAssert.Contains(result.OutputMessage, "1 Datei(en)");
    }

    [TestMethod]
    public void Execute_PathInQuotes_RemovesQuotes()
    {
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Hallo");
        var command = new DirCommand();
        var context = new CommandContext("dir", new[] { $"\"{_testDir}\"" });
        var result = command.Execute(context);

        Assert.IsTrue(result.Success);
        StringAssert.Contains(result.OutputMessage, $" Verzeichnis von {_testDir}");
        StringAssert.Contains(result.OutputMessage, "test.txt");
    }
}
