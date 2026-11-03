using CommanderD.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using File = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Filesystem;

[TestClass]
public class FolderTests
{
    private Folder _folder = null!;

    [TestInitialize]
    public void Setup()
    {
        _folder = new Folder("Documents", "C:\\Documents");
    }

    [TestMethod]
    public void Folder_Initialized_StoresNameAndPath()
    {
        Assert.AreEqual("Documents", _folder.Name);
        Assert.AreEqual("C:\\Documents", _folder.Path);
    }

    [TestMethod]
    public void Folder_Initialized_IsAFileSystemItem()
    {
        Assert.IsInstanceOfType(_folder, typeof(FileSystemItem));
    }

    [TestMethod]
    public void Folder_Initialized_HasNoChildItems()
    {
        Assert.AreEqual(0, _folder.GetChildItems().Count);
    }

    // ---- AddChildItem ----

    [TestMethod]
    public void AddChildItem_File_FolderContainsFile()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");

        _folder.AddChildItem(file);

        Assert.AreEqual(1, _folder.GetChildItems().Count);
        Assert.AreSame(file, _folder.GetChildItems()[0]);
    }

    [TestMethod]
    public void AddChildItem_Folder_FolderContainsSubfolder()
    {
        var subfolder = new Folder("Projects", "C:\\Documents\\Projects");

        _folder.AddChildItem(subfolder);

        Assert.AreEqual(1, _folder.GetChildItems().Count);
        Assert.AreSame(subfolder, _folder.GetChildItems()[0]);
    }

    [TestMethod]
    public void AddChildItem_SeveralItems_KeepsOrderOfAdding()
    {
        var first = new File("a.txt", "C:\\Documents\\a.txt");
        var second = new Folder("B", "C:\\Documents\\B");
        var third = new File("c.txt", "C:\\Documents\\c.txt");

        _folder.AddChildItem(first);
        _folder.AddChildItem(second);
        _folder.AddChildItem(third);

        CollectionAssert.AreEqual(new FileSystemItem[] { first, second, third }, _folder.GetChildItems());
    }

    [TestMethod]
    public void AddChildItem_ToSubfolder_DoesNotChangeParent()
    {
        var subfolder = new Folder("Projects", "C:\\Documents\\Projects");
        var file = new File("readme.md", "C:\\Documents\\Projects\\readme.md");
        _folder.AddChildItem(subfolder);

        subfolder.AddChildItem(file);

        Assert.AreEqual(1, _folder.GetChildItems().Count);
        Assert.AreSame(subfolder, _folder.GetChildItems()[0]);
        Assert.AreSame(file, subfolder.GetChildItems()[0]);
    }

    // ---- RemoveChildItem ----

    [TestMethod]
    public void RemoveChildItem_ExistingItem_ItemIsRemoved()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");
        _folder.AddChildItem(file);

        _folder.RemoveChildItem(file);

        Assert.AreEqual(0, _folder.GetChildItems().Count);
    }

    [TestMethod]
    public void RemoveChildItem_OneOfSeveral_OtherItemsStay()
    {
        var first = new File("a.txt", "C:\\Documents\\a.txt");
        var second = new File("b.txt", "C:\\Documents\\b.txt");
        var third = new File("c.txt", "C:\\Documents\\c.txt");
        _folder.AddChildItem(first);
        _folder.AddChildItem(second);
        _folder.AddChildItem(third);

        _folder.RemoveChildItem(second);

        CollectionAssert.AreEqual(new FileSystemItem[] { first, third }, _folder.GetChildItems());
    }

    [TestMethod]
    public void RemoveChildItem_ItemNotInFolder_ChangesNothing()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");
        var otherFile = new File("other.txt", "C:\\Documents\\other.txt");
        _folder.AddChildItem(file);

        _folder.RemoveChildItem(otherFile);

        Assert.AreEqual(1, _folder.GetChildItems().Count);
        Assert.AreSame(file, _folder.GetChildItems()[0]);
    }

    [TestMethod]
    public void RemoveChildItem_FromEmptyFolder_DoesNotThrow()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");

        _folder.RemoveChildItem(file);

        Assert.AreEqual(0, _folder.GetChildItems().Count);
    }

    [TestMethod]
    public void RemoveChildItem_SameNameButDifferentInstance_IsNotRemoved()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");
        var sameNameFile = new File("notes.txt", "C:\\Documents\\notes.txt");
        _folder.AddChildItem(file);

        _folder.RemoveChildItem(sameNameFile);

        Assert.AreEqual(1, _folder.GetChildItems().Count);
    }

    [TestMethod]
    public void RemoveChildItem_Subfolder_RemovesItWithItsContent()
    {
        var subfolder = new Folder("Projects", "C:\\Documents\\Projects");
        subfolder.AddChildItem(new File("readme.md", "C:\\Documents\\Projects\\readme.md"));
        _folder.AddChildItem(subfolder);

        _folder.RemoveChildItem(subfolder);

        Assert.AreEqual(0, _folder.GetChildItems().Count);
    }
}
