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
    public void RemoveChildItem_ExistingItem_ItemIsRemoved()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");
        _folder.AddChildItem(file);

        _folder.RemoveChildItem(file);

        Assert.AreEqual(0, _folder.GetChildItems().Count);
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
