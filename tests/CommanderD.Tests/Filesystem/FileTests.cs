using CommanderD.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using File = CommanderD.Filesystem.File;

namespace CommanderD.Tests.Filesystem;

[TestClass]
public class FileTests
{
    [TestMethod]
    public void File_Initialized_IsAFileSystemItem()
    {
        var file = new File("notes.txt", "C:\\Documents\\notes.txt");

        Assert.IsInstanceOfType(file, typeof(FileSystemItem));
    }

}