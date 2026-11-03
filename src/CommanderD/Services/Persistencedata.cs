using System.Text.Json;
using CommanderD.Filesystem;
using CommanderD.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Services;

[TestClass]
public class PersistenceDriveTests
{
    private string _directory = string.Empty;
    private PersistenceService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _directory = Path.Combine(Path.GetTempPath(), "commanderd-tests-" + Guid.NewGuid());
        _service = new PersistenceService(Path.Combine(_directory, "state.json"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    private static DateTime Utc(int minute) => new DateTime(2026, 11, 3, 10, minute, 0, DateTimeKind.Utc);

    private static DriveData CreateDriveData(string label)
    {
        return new DriveData
        {
            Name = label,
            Label = label,
            RootCreatedOn = Utc(0),
            Items =
            {
                new FolderData
                {
                    Name = "docs",
                    Path = label + ":\\docs",
                    CreatedOn = Utc(1),
                    Items =
                    {
                        new FileData { Name = "a.txt", Path = label + ":\\docs\\a.txt", CreatedOn = Utc(2), Content = "hello" },
                    },
                },
                new FileData { Name = "b.txt", Path = label + ":\\b.txt", CreatedOn = Utc(3), Content = null },
            },
        };
    }

    private static string AsJson(DriveData data) => JsonSerializer.Serialize(data);

    [TestMethod]
    public void LoadDrives_NoFile_ReturnsEmptyList()
    {
        Assert.AreEqual(0, _service.LoadDrives().Count);
    }

    [TestMethod]
    public void LoadDrives_AfterSave_RestoresNameAndLabel()
    {
        _service.SaveDrives(new[] { PersistenceMapper.ToDrive(CreateDriveData("C")) });

        List<Drive> loaded = _service.LoadDrives();

        Assert.AreEqual(1, loaded.Count);
        Assert.AreEqual("C", loaded[0].Name);
        Assert.AreEqual("C", loaded[0].Label);
    }

    [TestMethod]
    public void LoadDrives_AfterSave_RestoresTreeWithCorrectTypes()
    {
        _service.SaveDrives(new[] { PersistenceMapper.ToDrive(CreateDriveData("C")) });

        Drive drive = _service.LoadDrives()[0];

        List<FileSystemItem> rootItems = drive.RootFolder.GetChildItems();
        Assert.AreEqual(2, rootItems.Count);
        Assert.IsInstanceOfType(rootItems[0], typeof(Folder));
        Assert.IsInstanceOfType(rootItems[1], typeof(FsFile));

        List<FileSystemItem> docsItems = ((Folder)rootItems[0]).GetChildItems();
        Assert.AreEqual(1, docsItems.Count);
        Assert.IsInstanceOfType(docsItems[0], typeof(FsFile));
    }

    [TestMethod]
    public void LoadDrives_AfterSave_RestoresPrivateValues()
    {
        DriveData original = CreateDriveData("C");
        _service.SaveDrives(new[] { PersistenceMapper.ToDrive(original) });

        DriveData restored = PersistenceMapper.ToData(_service.LoadDrives()[0]);

        Assert.AreEqual(AsJson(original), AsJson(restored));
    }

    [TestMethod]
    public void LoadDrives_SeveralDrives_KeepsOrderAndContent()
    {
        _service.SaveDrives(new[]
        {
            PersistenceMapper.ToDrive(CreateDriveData("C")),
            PersistenceMapper.ToDrive(new DriveData { Name = "D", Label = "D", RootCreatedOn = Utc(5) }),
        });

        List<Drive> loaded = _service.LoadDrives();

        Assert.AreEqual(2, loaded.Count);
        Assert.AreEqual("C", loaded[0].Label);
        Assert.AreEqual(2, loaded[0].RootFolder.GetChildItems().Count);
        Assert.AreEqual("D", loaded[1].Label);
        Assert.AreEqual(0, loaded[1].RootFolder.GetChildItems().Count);
    }

    [TestMethod]
    public void SaveDrives_Twice_ReplacesOldContent()
    {
        _service.SaveDrives(new[] { PersistenceMapper.ToDrive(CreateDriveData("C")) });
        _service.SaveDrives(new[] { PersistenceMapper.ToDrive(new DriveData { Name = "D", Label = "D" }) });

        List<Drive> loaded = _service.LoadDrives();

        Assert.AreEqual(1, loaded.Count);
        Assert.AreEqual("D", loaded[0].Label);
    }

    [TestMethod]
    public void SaveDrives_AddedFolder_IsPersisted()
    {
        Drive drive = PersistenceMapper.ToDrive(CreateDriveData("C"));
        drive.RootFolder.AddChildItem(new Folder("new", "C:\\new"));

        _service.SaveDrives(new[] { drive });

        Assert.AreEqual(3, _service.LoadDrives()[0].RootFolder.GetChildItems().Count);
    }
}