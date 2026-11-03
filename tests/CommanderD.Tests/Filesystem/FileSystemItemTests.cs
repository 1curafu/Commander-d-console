using CommanderD.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Filesystem;

[TestClass]
public class FileSystemItemTests
{
    // FileSystemItem is abstract, so the tests use this small subclass.
    private class TestItem(string name, string path) : FileSystemItem(name, path);

    [TestMethod]
    public void FileSystemItem_Initialized_StoresNameAndPath()
    {
        var item = new TestItem("item", "C:\\item");

        Assert.AreEqual("item", item.Name);
        Assert.AreEqual("C:\\item", item.Path);
    }

    [TestMethod]
    public void FileSystemItem_Initialized_CreatedOnIsNow()
    {
        var before = DateTime.Now;

        var item = new TestItem("item", "C:\\item");

        var after = DateTime.Now;
        Assert.IsTrue(item.CreatedOn >= before && item.CreatedOn <= after,
            $"CreatedOn {item.CreatedOn:O} is not between {before:O} and {after:O}.");
    }

    [TestMethod]
    public void FileSystemItem_SetName_ChangesName()
    {
        var item = new TestItem("item", "C:\\item");

        item.Name = "renamed";

        Assert.AreEqual("renamed", item.Name);
    }

    [TestMethod]
    public void FileSystemItem_SetPath_ChangesPath()
    {
        var item = new TestItem("item", "C:\\item");

        item.Path = "D:\\item";

        Assert.AreEqual("D:\\item", item.Path);
    }

}