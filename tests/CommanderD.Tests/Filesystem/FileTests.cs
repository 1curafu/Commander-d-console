using CommanderD.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using File = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Filesystem;

[TestClass]
public class FileTests
{
    [TestMethod]
    public void File_Initialized_StoresNameAndPath()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");

        Assert.AreEqual("notes.txt", file.Name);
        Assert.AreEqual("C:\\Documents\\notes.txt", file.Path);
    }

    [TestMethod]
    public void File_Initialized_IsAFileSystemItem()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");

        Assert.IsInstanceOfType(file, typeof(FileSystemItem));
    }

    [TestMethod]
    public void File_Initialized_IsNotAFolder()
    {
        FileSystemItem file = new File("notes.txt", "C:\\Documents\\notes.txt");

        Assert.IsNotInstanceOfType(file, typeof(Folder));
    }

    [TestMethod]
    public void File_Renamed_HasNewName()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");

        file.Name = "todo.txt";
        file.Path = "C:\\Documents\\todo.txt";

        Assert.AreEqual("todo.txt", file.Name);
        Assert.AreEqual("C:\\Documents\\todo.txt", file.Path);
    }
}