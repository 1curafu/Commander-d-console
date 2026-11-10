using CommanderD.Filesystem;
using CommanderD.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Filesystem;

[TestClass]
public class ShellContextTests
{
    private string _directory = string.Empty;
    private string _path = string.Empty;
    private PersistenceService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _directory = Path.Combine(Path.GetTempPath(), "commanderd-tests-" + Guid.NewGuid());
        _path = Path.Combine(_directory, "state.json");
        _service = new PersistenceService(_path);
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
    public void Constructor_CreatesDefaultDriveC()
    {
        var context = new ShellContext(_service);

        Assert.HasCount(1, context.Drives);
        Assert.AreEqual("C", context.Drives[0].Name);
    }

    [TestMethod]
    public void Constructor_CurrentFolderIsRootOfDefaultDrive()
    {
        var context = new ShellContext(_service);

        Assert.AreSame(context.Drives[0].RootFolder, context.CurrentFolder);
    }

    [TestMethod]
    public void Load_NoSaveFile_KeepsDefaultDrive()
    {
        var context = new ShellContext(_service);
        Drive defaultDrive = context.Drives[0];

        context.Load();

        Assert.HasCount(1, context.Drives);
        Assert.AreSame(defaultDrive, context.Drives[0]);
        Assert.AreSame(defaultDrive.RootFolder, context.CurrentFolder);
    }

    [TestMethod]
    public void SaveThenLoad_RestoresDrives()
    {
        var original = new ShellContext(_service);
        original.Drives.Add(new Drive("D", "Data"));
        original.Save();

        var loaded = new ShellContext(_service);
        loaded.Load();

        Assert.HasCount(2, loaded.Drives);
        Assert.AreEqual("C", loaded.Drives[0].Name);
        Assert.AreEqual("D", loaded.Drives[1].Name);
        Assert.AreEqual("Data", loaded.Drives[1].Label);
    }

    [TestMethod]
    public void SaveThenLoad_RestoresFoldersAndFiles()
    {
        var original = new ShellContext(_service);
        var docs = new Folder("docs", "C:\\docs");
        docs.AddChildItem(new CommanderD.Filesystem.File("readme.txt", "C:\\docs\\readme.txt"));
        original.CurrentFolder.AddChildItem(docs);
        original.Save();

        var loaded = new ShellContext(_service);
        loaded.Load();

        List<FileSystemItem> rootItems = loaded.Drives[0].RootFolder.GetChildItems();
        Assert.HasCount(1, rootItems);
        Assert.IsInstanceOfType<Folder>(rootItems[0]);
        var loadedDocs = (Folder)rootItems[0];
        Assert.HasCount(1, loadedDocs.GetChildItems());
        Assert.IsInstanceOfType<CommanderD.Filesystem.File>(loadedDocs.GetChildItems()[0]);
    }

    [TestMethod]
    public void SaveThenLoad_RestoresFileContentAndCreationDate()
    {
        var original = new ShellContext(_service);
        var date = new DateTime(2026, 10, 29, 14, 10, 0);
        original.CurrentFolder.AddChildItem(
            new CommanderD.Filesystem.File("a.txt", "C:\\a.txt") { Content = "Hallo", CreatedOn = date });
        original.Save();

        var loaded = new ShellContext(_service);
        loaded.Load();

        var file = (CommanderD.Filesystem.File)loaded.Drives[0].RootFolder.GetChildItems()[0];
        Assert.AreEqual("Hallo", file.Content);
        Assert.AreEqual(date, file.CreatedOn);
    }
}
