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

}