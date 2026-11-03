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
    public void Drive_Initialized_RootFolderNameIsBackslash()
    {
        var drive = new Drive("testdrive", "C");

        Assert.AreEqual("\\", drive.RootFolder.Name);
    }

    [TestMethod]
    public void Drive_Initialized_StoresNameAndLabel()
    {
        var drive = new Drive("testdrive", "C");

        Assert.AreEqual("testdrive", drive.Name);
        Assert.AreEqual("C", drive.Label);
    }

    [TestMethod]
    public void Drive_Initialized_RootFolderIsEmpty()
    {
        var drive = new Drive("testdrive", "C");

        Assert.AreEqual(0, drive.RootFolder.GetChildItems().Count);
    }
    

    [TestMethod]
    public void Drive_TwoDrives_HaveSeparateRootFolders()
    {
        var driveC = new Drive("system", "C");
        var driveD = new Drive("data", "D");

        driveC.RootFolder.AddChildItem(new Folder("Windows", "C:\\Windows"));

        Assert.AreNotSame(driveC.RootFolder, driveD.RootFolder);
        Assert.AreEqual(1, driveC.RootFolder.GetChildItems().Count);
        Assert.AreEqual(0, driveD.RootFolder.GetChildItems().Count);
    }

    [TestMethod]
    public void Drive_ItemAddedToRootFolder_IsReachableThroughDrive()
    {
        var drive = new Drive("testdrive", "C");
        var folder = new Folder("Users", "C:\\Users");

        drive.RootFolder.AddChildItem(folder);

        Assert.AreSame(folder, drive.RootFolder.GetChildItems()[0]);
    }
}