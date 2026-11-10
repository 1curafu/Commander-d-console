using CommanderD.Filesystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Filesystem;

[TestClass]
public class DriveTests
{
    [TestMethod]
    [DataRow("C", "C:\\")]
    [DataRow("D", "D:\\")]
    [DataRow("Z", "Z:\\")]
    public void Drive_DifferentLabels_RootFolderPathUsesLabel(string label, string expectedPath)
    {
        var drive = new Drive("testdrive", label);

        Assert.AreEqual(expectedPath, drive.RootFolder.Path);
    }

    [TestMethod]
    public void Drive_Initialized_RootFolderIsEmpty()
    {
        var drive = new Drive("testdrive", "C");

        Assert.AreEqual(0, drive.RootFolder.GetChildItems().Count);
    }
    

}