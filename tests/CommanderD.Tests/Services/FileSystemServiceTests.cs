using CommanderD.Filesystem;
using CommanderD.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Services;

[TestClass]
public class FileSystemServiceTests
{
    private string _directory = string.Empty;
    private string _jsonPath = string.Empty;
    private PersistenceService _persistence = null!;
    private ShellContext _context = null!;
    private FileSystemService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _directory = Path.Combine(Path.GetTempPath(), "commanderd-vfs-tests-" + Guid.NewGuid());
        _jsonPath = Path.Combine(_directory, "commanderd.json");
        _persistence = new PersistenceService(_jsonPath);
        _context = new ShellContext(_persistence);
        _service = new FileSystemService(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [TestMethod]
    public void CurrentDirectory_AtStart_IsRootOfDriveC()
    {
        Assert.AreEqual(@"C:\", _service.CurrentDirectory);
    }

    [TestMethod]
    public void CreateDirectory_NewFolder_ExistsAndIsListedInContext()
    {
        _service.CreateDirectory("docs");

        Assert.IsTrue(_service.DirectoryExists(@"C:\docs"));
        Assert.IsFalse(_service.FileExists(@"C:\docs"));
        Assert.HasCount(1, _context.Drives[0].RootFolder.GetChildItems());
    }

    [TestMethod]
    public void CreateDirectory_NestedPath_CreatesMissingParents()
    {
        _service.CreateDirectory(@"a\b\c");

        Assert.IsTrue(_service.DirectoryExists(@"C:\a"));
        Assert.IsTrue(_service.DirectoryExists(@"C:\a\b"));
        Assert.IsTrue(_service.DirectoryExists(@"C:\a\b\c"));
    }

    [TestMethod]
    public void CreateDirectory_NameIsNotCaseSensitive()
    {
        _service.CreateDirectory("Docs");

        Assert.IsTrue(_service.DirectoryExists("docs"));
        Assert.IsTrue(_service.DirectoryExists("DOCS"));
    }

    [TestMethod]
    public void CreateDirectory_InvalidName_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateDirectory("a*b"));
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateDirectory("a|b"));
    }

    [TestMethod]
    public void CreateDirectory_NameOfExistingFile_ThrowsIOException()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("notes.txt", @"C:\notes.txt"));

        Assert.ThrowsExactly<IOException>(() => _service.CreateDirectory("notes.txt"));
    }

    [TestMethod]
    public void CreateDirectory_UnknownDrive_ThrowsDirectoryNotFoundException()
    {
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.CreateDirectory(@"Z:\docs"));
    }

    [TestMethod]
    public void CreateDirectory_NeverCreatesAFolderOnTheRealDisk()
    {
        string name = "commanderd-virtual-" + Guid.NewGuid();

        _service.CreateDirectory(name);

        Assert.IsTrue(_service.DirectoryExists(name));
        Assert.IsFalse(Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), name)));
        Assert.IsFalse(Directory.Exists(Path.Combine(Path.GetTempPath(), name)));
    }

    [TestMethod]
    public void ChangeDirectory_ExistingFolder_ChangesCurrentDirectoryAndContext()
    {
        _service.CreateDirectory(@"docs\work");

        _service.ChangeDirectory(@"docs\work");

        Assert.AreEqual(@"C:\docs\work", _service.CurrentDirectory);
        Assert.AreEqual("work", _context.CurrentFolder.Name);
    }

    [TestMethod]
    public void ChangeDirectory_UsesCaseOfTheExistingFolder()
    {
        _service.CreateDirectory("Docs");

        _service.ChangeDirectory("DOCS");

        Assert.AreEqual(@"C:\Docs", _service.CurrentDirectory);
    }

    [TestMethod]
    public void ChangeDirectory_ParentAndRoot_Work()
    {
        _service.CreateDirectory(@"a\b");
        _service.ChangeDirectory(@"a\b");

        _service.ChangeDirectory("..");
        Assert.AreEqual(@"C:\a", _service.CurrentDirectory);

        _service.ChangeDirectory(@"b");
        _service.ChangeDirectory(@"\");
        Assert.AreEqual(@"C:\", _service.CurrentDirectory);
    }

    [TestMethod]
    public void ChangeDirectory_ParentOfRoot_StaysInRoot()
    {
        _service.ChangeDirectory("..");

        Assert.AreEqual(@"C:\", _service.CurrentDirectory);
    }

    [TestMethod]
    public void ChangeDirectory_MissingFolder_ThrowsAndKeepsDirectory()
    {
        _service.CreateDirectory("docs");
        _service.ChangeDirectory("docs");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.ChangeDirectory("gibtsnicht"));

        Assert.AreEqual(@"C:\docs", _service.CurrentDirectory);
    }

    [TestMethod]
    public void ChangeDirectory_File_Throws()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("notes.txt", @"C:\notes.txt"));

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.ChangeDirectory("notes.txt"));
    }

    [TestMethod]
    [DataRow("docs", @"C:\docs")]
    [DataRow(@"docs\work", @"C:\docs\work")]
    [DataRow("docs/work", @"C:\docs\work")]
    [DataRow(@"\docs", @"C:\docs")]
    [DataRow(@"c:\docs", @"C:\docs")]
    [DataRow(@"C:\docs\..\other", @"C:\other")]
    [DataRow(@".\docs", @"C:\docs")]
    [DataRow(@"..\..", @"C:\")]
    [DataRow(@"D:\x", @"D:\x")]
    public void ResolvePath_FromRoot_ReturnsAbsoluteWindowsPath(string input, string expected)
    {
        Assert.AreEqual(expected, _service.ResolvePath(input));
    }

    [TestMethod]
    public void ResolvePath_Relative_UsesCurrentDirectory()
    {
        _service.CreateDirectory("docs");
        _service.ChangeDirectory("docs");

        Assert.AreEqual(@"C:\docs\work", _service.ResolvePath("work"));
        Assert.AreEqual(@"C:\other", _service.ResolvePath(@"..\other"));
    }

    [TestMethod]
    public void GetEntries_Folder_ReturnsFoldersFirstThenFilesSortedByName()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("b.txt", @"C:\b.txt") { Content = "abc" });
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));
        _service.CreateDirectory("zeta");
        _service.CreateDirectory("alpha");

        IReadOnlyList<DirectoryEntry> entries = _service.GetEntries(@"C:\");

        CollectionAssert.AreEqual(new[] { "alpha", "zeta", "a.txt", "b.txt" }, entries.Select(e => e.Name).ToArray());
        Assert.IsTrue(entries[0].IsDirectory);
        Assert.AreEqual(3L, entries[3].Size);
    }

    [TestMethod]
    public void GetEntries_File_ReturnsOnlyThisFile()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));
        _context.CurrentFolder.AddChildItem(new FsFile("b.txt", @"C:\b.txt"));

        IReadOnlyList<DirectoryEntry> entries = _service.GetEntries("a.txt");

        Assert.HasCount(1, entries);
        Assert.AreEqual("a.txt", entries[0].Name);
    }

    [TestMethod]
    public void GetEntries_MissingPath_ThrowsDirectoryNotFoundException()
    {
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.GetEntries("gibtsnicht"));
    }

    [TestMethod]
    public void CreateDirectory_WithSaveOnChange_WritesJsonFile()
    {
        var service = new FileSystemService(_context, saveOnChange: true);

        service.CreateDirectory("docs");

        Assert.IsTrue(System.IO.File.Exists(_jsonPath));
        StringAssert.Contains(System.IO.File.ReadAllText(_jsonPath), "docs");
    }

    [TestMethod]
    public void CreateDirectory_WithoutSaveOnChange_WritesNothing()
    {
        _service.CreateDirectory("docs");

        Assert.IsFalse(System.IO.File.Exists(_jsonPath));
    }

    [TestMethod]
    public void SavedFolders_AreAvailableAfterRestart()
    {
        new FileSystemService(_context, saveOnChange: true).CreateDirectory(@"docs\work");

        var restartedContext = new ShellContext(new PersistenceService(_jsonPath));
        restartedContext.Load();
        var restarted = new FileSystemService(restartedContext);

        Assert.IsTrue(restarted.DirectoryExists(@"C:\docs\work"));
    }

    [TestMethod]
    public void DeleteFile_ExistingFile_RemovesItFromTheFolder()
    {
        _service.CreateDirectory("docs");
        var docs = (Folder)_context.CurrentFolder.GetChildItems()[0];
        docs.AddChildItem(new FsFile("a.txt", @"C:\docs\a.txt"));

        _service.DeleteFile(@"docs\a.txt");

        Assert.IsFalse(_service.FileExists(@"C:\docs\a.txt"));
        Assert.IsTrue(_service.DirectoryExists(@"C:\docs"));
    }

    [TestMethod]
    public void DeleteFile_MissingFileOrFolder_Throws()
    {
        _service.CreateDirectory("docs");

        Assert.ThrowsExactly<FileNotFoundException>(() => _service.DeleteFile("gibtsnicht.txt"));
        Assert.ThrowsExactly<FileNotFoundException>(() => _service.DeleteFile("docs"));
    }

    [TestMethod]
    public void DeleteFile_WithSaveOnChange_SavesTheJsonFile()
    {
        var service = new FileSystemService(_context, saveOnChange: true);
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));

        service.DeleteFile("a.txt");

        Assert.IsTrue(System.IO.File.Exists(_jsonPath));
        Assert.IsFalse(System.IO.File.ReadAllText(_jsonPath).Contains("a.txt"));
    }

    [TestMethod]
    public void IsProtectedFile_VirtualFile_IsNotProtected()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));

        Assert.IsFalse(_service.IsProtectedFile("a.txt"));
    }
}
