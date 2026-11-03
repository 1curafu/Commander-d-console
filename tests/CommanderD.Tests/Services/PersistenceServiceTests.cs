using System.Text.Json;
using CommanderD.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Services;

[TestClass]
public class PersistenceServiceTests
{
    private class Node
    {
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public long Size { get; set; }

        public List<Node> Children { get; set; } = new();
    }

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

    private static Node CreateTree()
    {
        return new Node
        {
            Name = "C:",
            CreatedOn = new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc),
            Children =
            {
                new Node { Name = "readme.txt", Size = 5, CreatedOn = new DateTime(2026, 11, 3, 10, 5, 0, DateTimeKind.Utc) },
                new Node { Name = "docs" },
            },
        };
    }

    [TestMethod]
    public void Constructor_EmptyPath_Throws()
    {
        Assert.Throws<ArgumentException>(() => new PersistenceService(" "));
    }

    [TestMethod]
    public void Constructor_Default_UsesDefaultFileName()
    {
        var service = new PersistenceService();

        Assert.AreEqual(PersistenceService.DefaultFileName, Path.GetFileName(service.FilePath));
    }

    [TestMethod]
    public void Save_CreatesJsonFile()
    {
        _service.Save(CreateTree());

        Assert.IsTrue(System.IO.File.Exists(_path));
    }

    [TestMethod]
    public void Save_MissingDirectory_CreatesIt()
    {
        Assert.IsFalse(Directory.Exists(_directory));

        _service.Save(CreateTree());

        Assert.IsTrue(Directory.Exists(_directory));
    }

    [TestMethod]
    public void Save_ExistingFile_IsOverwritten()
    {
        _service.Save(new Node { Name = "first" });
        _service.Save(new Node { Name = "second" });

        Assert.AreEqual("second", _service.Load<Node>()!.Name);
    }

    [TestMethod]
    public void Save_LeavesNoTemporaryFile()
    {
        _service.Save(CreateTree());

        Assert.IsFalse(System.IO.File.Exists(_path + ".tmp"));
    }

    [TestMethod]
    public void Load_NoFile_ReturnsNull()
    {
        Assert.IsNull(_service.Load<Node>());
    }

    [TestMethod]
    public void Load_AfterSave_RestoresNestedData()
    {
        _service.Save(CreateTree());

        Node? loaded = _service.Load<Node>();

        Assert.IsNotNull(loaded);
        Assert.AreEqual("C:", loaded.Name);
        Assert.AreEqual(new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc), loaded.CreatedOn);
        Assert.AreEqual(2, loaded.Children.Count);
        Assert.AreEqual("readme.txt", loaded.Children[0].Name);
        Assert.AreEqual(5, loaded.Children[0].Size);
        Assert.AreEqual("docs", loaded.Children[1].Name);
    }

    [TestMethod]
    public void Load_AfterSave_WorksForLists()
    {
        _service.Save(new List<Node> { new Node { Name = "a" }, new Node { Name = "b" } });

        List<Node>? loaded = _service.Load<List<Node>>();

        Assert.IsNotNull(loaded);
        Assert.AreEqual(2, loaded.Count);
        Assert.AreEqual("b", loaded[1].Name);
    }

    [TestMethod]
    public void Load_InvalidJson_ThrowsJsonException()
    {
        Directory.CreateDirectory(_directory);
        System.IO.File.WriteAllText(_path, "this is not json");

        Assert.Throws<JsonException>(() => _service.Load<Node>());
    }
}