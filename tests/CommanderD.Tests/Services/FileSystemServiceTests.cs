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
    public void CreateFile_NewFile_ExistsAndIsListedInContext()
    {
        _service.CreateFile("notes.txt", string.Empty);

        Assert.IsTrue(_service.FileExists(@"C:\notes.txt"));
        Assert.IsFalse(_service.DirectoryExists(@"C:\notes.txt"));
        Assert.HasCount(1, _context.Drives[0].RootFolder.GetChildItems());
        Assert.IsInstanceOfType(_context.Drives[0].RootFolder.GetChildItems()[0], typeof(FsFile));
    }

    [TestMethod]
    public void CreateFile_EmptyContent_IsEmpty()
    {
        _service.CreateFile("notes.txt", string.Empty);

        DirectoryEntry entry = _service.GetEntries("notes.txt")[0];
        Assert.AreEqual(string.Empty, entry.Content);
        Assert.AreEqual(0, entry.Size);
        Assert.IsFalse(entry.IsDirectory);
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
    public void CreateFile_MultilineContent_StoresItUnchanged()
    {
        string content = "line 1\r\nline 2\r\n\r\nline 4";

        _service.CreateFile("notes.txt", content);

        Assert.AreEqual(content, _service.GetEntries("notes.txt")[0].Content);
    }

    [TestMethod]
    public void CreateFile_ContentWithUmlauts_SizeIsUtf8Bytes()
    {
        _service.CreateFile("notes.txt", "Grüezi");

        Assert.AreEqual(7, _service.GetEntries("notes.txt")[0].Size);
    }

    [TestMethod]
    public void CreateFile_TwoFiles_KeepTheirOwnContent()
    {
        _service.CreateFile("a.txt", "content of a");
        _service.CreateFile("b.txt", "content of b");

        Assert.AreEqual("content of a", _service.GetEntries("a.txt")[0].Content);
        Assert.AreEqual("content of b", _service.GetEntries("b.txt")[0].Content);
    }

    [TestMethod]
    public void CreateFile_InExistingSubfolder_CreatesFileThere()
    {
        _service.CreateDirectory("docs");

        _service.CreateFile(@"docs\notes.txt", string.Empty);

        Assert.IsTrue(_service.FileExists(@"C:\docs\notes.txt"));
        Assert.IsFalse(_service.FileExists(@"C:\notes.txt"));
    }

    [TestMethod]
    public void CreateFile_AbsolutePath_CreatesFileThere()
    {
        _service.CreateDirectory(@"docs\work");
        _service.ChangeDirectory("docs");

        _service.CreateFile(@"C:\docs\work\notes.txt", string.Empty);

        Assert.IsTrue(_service.FileExists(@"C:\docs\work\notes.txt"));
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
    public void CreateFile_PathUsesCaseOfTheExistingFolder()
    {
        _service.CreateDirectory("Docs");

        _service.CreateFile(@"DOCS\notes.txt", string.Empty);

        var docs = (Folder)_context.Drives[0].RootFolder.GetChildItems()[0];
        var file = (FsFile)docs.GetChildItems()[0];
        Assert.AreEqual("notes.txt", file.Name);
        Assert.AreEqual(@"C:\Docs\notes.txt", file.Path);
    }

    [TestMethod]
    public void CreateFile_NameIsNotCaseSensitive()
    {
        _service.CreateFile("Notes.txt", string.Empty);

        Assert.IsTrue(_service.FileExists("notes.txt"));
        Assert.IsTrue(_service.FileExists("NOTES.TXT"));
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
    public void CreateFile_NameOfExistingFolder_ThrowsIOException()
    {
        _service.CreateDirectory("docs");

        Assert.ThrowsExactly<IOException>(() => _service.CreateFile("docs", string.Empty));
        Assert.IsTrue(_service.DirectoryExists("docs"));
    }

    [TestMethod]
    public void CreateFile_MissingFolder_ThrowsDirectoryNotFoundExceptionAndCreatesNothing()
    {
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.CreateFile(@"gibtsnicht\notes.txt", string.Empty));

        Assert.IsFalse(_service.DirectoryExists("gibtsnicht"));
        Assert.HasCount(0, _context.Drives[0].RootFolder.GetChildItems());
    }

    [TestMethod]
    public void CreateFile_ParentIsAFile_ThrowsDirectoryNotFoundException()
    {
        _service.CreateFile("notes.txt", string.Empty);

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.CreateFile(@"notes.txt\other.txt", string.Empty));
    }

    [TestMethod]
    public void CreateFile_UnknownDrive_ThrowsDirectoryNotFoundException()
    {
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => _service.CreateFile(@"Z:\notes.txt", string.Empty));
    }

    [TestMethod]
    public void CreateFile_InvalidName_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateFile("a*b.txt", string.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateFile("a|b.txt", string.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateFile("a?.txt", string.Empty));
    }

    [TestMethod]
    [DataRow(@"C:\")]
    [DataRow(@"\")]
    [DataRow("..")]
    public void CreateFile_PathWithoutFileName_ThrowsArgumentException(string path)
    {
        Assert.ThrowsExactly<ArgumentException>(() => _service.CreateFile(path, string.Empty));
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
    public void CreateFile_WithoutSaveOnChange_WritesNothing()
    {
        _service.CreateFile("notes.txt", string.Empty);

        Assert.IsFalse(System.IO.File.Exists(_jsonPath));
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
    public void Rename_OnlyChangesCase_Works()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));

        _service.Rename("a.txt", "A.txt");

        Assert.AreEqual("A.txt", _context.CurrentFolder.GetChildItems()[0].Name);
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
    public void Rename_InvalidNewName_ThrowsArgumentException()
    {
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));

        Assert.ThrowsExactly<ArgumentException>(() => _service.Rename("a.txt", @"sub\b.txt"));
        Assert.ThrowsExactly<ArgumentException>(() => _service.Rename("a.txt", "b|c.txt"));
        Assert.ThrowsExactly<ArgumentException>(() => _service.Rename("a.txt", ".."));
    }

    [TestMethod]
    public void Rename_RootOfDrive_ThrowsFileNotFoundException()
    {
        Assert.ThrowsExactly<FileNotFoundException>(() => _service.Rename(@"C:\", "D"));
    }

    [TestMethod]
    public void Rename_CurrentDirectory_IsDenied()
    {
        _service.CreateDirectory(@"docs\sub");
        _service.ChangeDirectory(@"docs\sub");

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => _service.Rename(@"C:\docs\sub", "other"));
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => _service.Rename(@"C:\docs", "other"));
        Assert.IsTrue(_service.DirectoryExists(@"C:\docs\sub"));
    }

    [TestMethod]
    public void Rename_SiblingOfCurrentDirectory_Works()
    {
        _service.CreateDirectory(@"docs\sub");
        _service.CreateDirectory(@"docs\other");
        _service.ChangeDirectory(@"docs\sub");

        _service.Rename(@"C:\docs\other", "renamed");

        Assert.IsTrue(_service.DirectoryExists(@"C:\docs\renamed"));
    }

    [TestMethod]
    public void Rename_WithSaveOnChange_SavesTheJsonFile()
    {
        var service = new FileSystemService(_context, saveOnChange: true);
        _context.CurrentFolder.AddChildItem(new FsFile("a.txt", @"C:\a.txt"));

        service.Rename("a.txt", "b.txt");

        string json = System.IO.File.ReadAllText(_jsonPath);
        Assert.IsTrue(json.Contains("b.txt"));
        Assert.IsFalse(json.Contains("a.txt"));
    }
}