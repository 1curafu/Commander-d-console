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
    public void CreateDirectory_InvalidName_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateDirectory("a*b"));
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateDirectory("a|b"));
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
    public void CreateFile_NewFile_ExistsAndIsListedInContext()
    {
        _service.CreateFile("notes.txt", string.Empty);

        Assert.IsTrue(_service.FileExists(@"C:\notes.txt"));
        Assert.IsFalse(_service.DirectoryExists(@"C:\notes.txt"));
        Assert.HasCount(1, _context.Drives[0].RootFolder.GetChildItems());
        Assert.IsInstanceOfType(_context.Drives[0].RootFolder.GetChildItems()[0], typeof(FsFile));
    }

    [TestMethod]
    public void CreateFile_WithContent_StoresContent()
    {
        _service.CreateFile("notes.txt", "Hello World");

        DirectoryEntry entry = _service.GetEntries("notes.txt")[0];
        Assert.AreEqual("Hello World", entry.Content);
        Assert.AreEqual(11, entry.Size);
    }

    [TestMethod]
    public void CreateFile_RelativePath_UsesCurrentDirectory()
    {
        _service.CreateDirectory("docs");
        _service.ChangeDirectory("docs");

        _service.CreateFile("notes.txt", string.Empty);

        Assert.IsTrue(_service.FileExists(@"C:\docs\notes.txt"));
        Assert.IsFalse(_service.FileExists(@"C:\notes.txt"));
    }

    [TestMethod]
    public void CreateFile_ExistingFile_ThrowsIOExceptionAndKeepsContent()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("notes.txt", @"C:\notes.txt") { Content = "keep me" });

        Assert.ThrowsExactly<IOException>(() => _service.CreateFile("NOTES.TXT", "new content"));

        Assert.HasCount(1, _context.CurrentFolder.GetChildItems());
        Assert.AreEqual("keep me", _service.GetEntries("notes.txt")[0].Content);
    }

    [TestMethod]
    public void CreateFile_MissingFolder_ThrowsDirectoryNotFoundExceptionAndCreatesNothing()
    {
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.CreateFile(@"gibtsnicht\notes.txt", string.Empty));

        Assert.IsFalse(_service.DirectoryExists("gibtsnicht"));
        Assert.HasCount(0, _context.Drives[0].RootFolder.GetChildItems());
    }

    [TestMethod]
    public void CreateFile_NeverCreatesAFileOnTheRealDisk()
    {
        string name = "commanderd-virtual-" + Guid.NewGuid() + ".txt";

        _service.CreateFile(name, string.Empty);

        Assert.IsTrue(_service.FileExists(name));
        Assert.IsFalse(System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), name)));
        Assert.IsFalse(System.IO.File.Exists(Path.Combine(Path.GetTempPath(), name)));
    }

    [TestMethod]
    public void CreateFile_WithSaveOnChange_WritesJsonFile()
    {
        var service = new FileSystemService(_context, saveOnChange: true);

        service.CreateFile("notes.txt", "saved content");

        Assert.IsTrue(System.IO.File.Exists(_jsonPath));
        string json = System.IO.File.ReadAllText(_jsonPath);
        StringAssert.Contains(json, "notes.txt");
        StringAssert.Contains(json, "saved content");
    }

    [TestMethod]
    public void SavedFiles_AreAvailableAfterRestart()
    {
        var service = new FileSystemService(_context, saveOnChange: true);
        service.CreateDirectory("docs");
        service.CreateFile(@"docs\notes.txt", "still here");

        var restartedContext = new ShellContext(new PersistenceService(_jsonPath));
        restartedContext.Load();
        var restarted = new FileSystemService(restartedContext);

        Assert.IsTrue(restarted.FileExists(@"C:\docs\notes.txt"));
        Assert.AreEqual("still here", restarted.GetEntries(@"C:\docs\notes.txt")[0].Content);
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
    public void ChangeDirectory_MissingFolder_ThrowsAndKeepsDirectory()
    {
        _service.CreateDirectory("docs");
        _service.ChangeDirectory("docs");

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.ChangeDirectory("gibtsnicht"));

        Assert.AreEqual(@"C:\docs", _service.CurrentDirectory);
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
    public void GetEntries_MissingPath_ThrowsDirectoryNotFoundException()
    {
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.GetEntries("gibtsnicht"));
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
    public void Rename_ExistingFile_GetsNewNameAndPath()
    {
        var file = new FsFile("a.txt", @"C:\a.txt");
        _context.CurrentFolder.AddChildItem(file);

        _service.Rename("a.txt", "b.txt");

        Assert.IsFalse(_service.FileExists(@"C:\a.txt"));
        Assert.IsTrue(_service.FileExists(@"C:\b.txt"));
        Assert.AreEqual("b.txt", file.Name);
        Assert.AreEqual(@"C:\b.txt", file.Path);
    }

    [TestMethod]
    public void Rename_ExistingDirectory_UpdatesPathsOfChildren()
    {
        _service.CreateDirectory(@"docs\sub");
        var docs = (Folder)_context.CurrentFolder.GetChildItems()[0];
        var sub = (Folder)docs.GetChildItems()[0];
        var file = new FsFile("a.txt", @"C:\docs\sub\a.txt");
        sub.AddChildItem(file);

        _service.Rename("docs", "papers");

        Assert.IsTrue(_service.DirectoryExists(@"C:\papers\sub"));
        Assert.IsTrue(_service.FileExists(@"C:\papers\sub\a.txt"));
        Assert.IsFalse(_service.DirectoryExists(@"C:\docs"));
        Assert.AreEqual(@"C:\papers\sub\a.txt", file.Path);
    }

    [TestMethod]
    public void Rename_MissingItem_ThrowsFileNotFoundException()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => _service.Rename("gibtsnicht.txt", "b.txt"));
    }

    [TestMethod]
    public void Rename_NewNameAlreadyExists_ThrowsIOException()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));
        _context.CurrentFolder.AddChildItem(new FsFile("b.txt", @"C:\b.txt"));

        Assert.ThrowsExactly<IOException>(() => _service.Rename("a.txt", "B.TXT"));
        Assert.IsTrue(_service.FileExists(@"C:\a.txt"));
    }

    [TestMethod]
    public void Move_FileIntoFolder_ChangesParentAndPath()
    {
        _service.CreateDirectory("docs");
        var file = new FsFile("a.txt", @"C:\a.txt");
        _context.CurrentFolder.AddChildItem(file);

        _service.Move("a.txt", "docs");

        Assert.IsFalse(_service.FileExists(@"C:\a.txt"));
        Assert.IsTrue(_service.FileExists(@"C:\docs\a.txt"));
        Assert.AreEqual(@"C:\docs\a.txt", file.Path);
        Assert.AreEqual("a.txt", file.Name);
    }

    [TestMethod]
    public void Move_FileToNewPath_UsesTheNewName()
    {
        _service.CreateDirectory("docs");
        var file = new FsFile("a.txt", @"C:\a.txt");
        _context.CurrentFolder.AddChildItem(file);

        _service.Move("a.txt", @"docs\b.txt");

        Assert.IsFalse(_service.FileExists(@"C:\a.txt"));
        Assert.IsTrue(_service.FileExists(@"C:\docs\b.txt"));
        Assert.AreEqual("b.txt", file.Name);
        Assert.AreEqual(@"C:\docs\b.txt", file.Path);
    }

    [TestMethod]
    public void Move_Directory_UpdatesPathsOfChildren()
    {
        _service.CreateDirectory(@"docs\sub");
        _service.CreateDirectory("target");
        var sub = (Folder)((Folder)_context.CurrentFolder.GetChildItems()[0]).GetChildItems()[0];
        var file = new FsFile("a.txt", @"C:\docs\sub\a.txt");
        sub.AddChildItem(file);

        _service.Move("docs", "target");

        Assert.IsFalse(_service.DirectoryExists(@"C:\docs"));
        Assert.IsTrue(_service.DirectoryExists(@"C:\target\docs\sub"));
        Assert.IsTrue(_service.FileExists(@"C:\target\docs\sub\a.txt"));
        Assert.AreEqual(@"C:\target\docs\sub\a.txt", file.Path);
    }

    [TestMethod]
    public void Move_TargetAlreadyExists_ThrowsIOExceptionAndMovesNothing()
    {
        _service.CreateDirectory("docs");
        var docs = (Folder)_context.CurrentFolder.GetChildItems()[0];
        docs.AddChildItem(new FsFile("a.txt", @"C:\docs\a.txt"));
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));

        Assert.ThrowsExactly<IOException>(() => _service.Move("a.txt", "docs"));

        Assert.IsTrue(_service.FileExists(@"C:\a.txt"));
        Assert.IsTrue(_service.FileExists(@"C:\docs\a.txt"));
    }

    [TestMethod]
    public void Move_MissingSource_ThrowsFileNotFoundException()
    {
        _service.CreateDirectory("docs");

        Assert.ThrowsExactly<FileNotFoundException>(() => _service.Move("gibtsnicht.txt", "docs"));
    }

    [TestMethod]
    public void Move_DirectoryIntoItself_ThrowsUnauthorizedAccessException()
    {
        _service.CreateDirectory(@"docs\sub");

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => _service.Move("docs", @"docs\sub"));
        Assert.IsTrue(_service.DirectoryExists(@"C:\docs\sub"));
    }

}