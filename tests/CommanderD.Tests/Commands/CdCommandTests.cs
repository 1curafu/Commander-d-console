using CommanderD.Commands;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class CdCommandTests
{
    private const string PathNotFoundMessage = "The system cannot find the path specified.";

    private readonly string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-cd-tests"));
    private readonly string work;
    private readonly string src;
    private readonly string spaced;
    private readonly FakeFileSystemService fileSystem;
    private readonly CdCommand command;

    public CdCommandTests()
    {
        work = Path.Combine(root, "work");
        src = Path.Combine(work, "src");
        spaced = Path.Combine(work, "My Folder");

        fileSystem = new FakeFileSystemService(work);
        fileSystem.AddDirectory(root);
        fileSystem.AddDirectory(src);
        fileSystem.AddDirectory(spaced);

        command = new CdCommand(fileSystem);
    }

    private CommandResult Run(params string[] arguments)
    {
        return command.Execute(new CommandContext("cd", arguments));
    }

    [TestMethod]
    public void Cd_HasNameAndDescription()
    {
        Assert.AreEqual("cd", command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(command.Description));
    }

    [TestMethod]
    public void Cd_NoArgument_ShowsCurrentDirectory()
    {
        var result = Run();

        Assert.IsTrue(result.Success);
        Assert.AreEqual(work, result.OutputMessage);
        Assert.IsFalse(result.ShouldExit);
    }

    [TestMethod]
    public void Cd_NoArgument_DoesNotChangeDirectory()
    {
        Run();

        Assert.AreEqual(0, fileSystem.ChangeDirectoryCalls);
        Assert.AreEqual(work, fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_RelativeDirectory_ChangesDirectory()
    {
        var result = Run("src");

        Assert.IsTrue(result.Success);
        Assert.AreEqual("", result.OutputMessage);
        Assert.AreEqual(src, fileSystem.ChangedTo);
        Assert.AreEqual(src, fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_AbsoluteDirectory_ChangesDirectory()
    {
        var result = Run(src);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("", result.OutputMessage);
        Assert.AreEqual(src, fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_ParentDirectory_ChangesToParent()
    {
        var result = Run("..");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(root, fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_CurrentDirectoryDot_StaysInDirectory()
    {
        var result = Run(".");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(work, fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_RelativePathWithParent_ResolvesPath()
    {
        fileSystem.CurrentDirectory = src;

        var result = Run(Path.Combine("..", "My Folder"));

        Assert.IsTrue(result.Success);
        Assert.AreEqual(spaced, fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_QuotedPathWithSpaces_ChangesDirectory()
    {
        var result = Run("\"My Folder\"");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(spaced, fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_QuotedPathSplitByParser_JoinsArgumentsAndRemovesQuotes()
    {
        var result = Run("\"My", "Folder\"");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(spaced, fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_UnquotedPathWithSpaces_ChangesDirectory()
    {
        var result = Run("My", "Folder");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(spaced, fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_DirectoryDoesNotExist_ShowsPathNotFound()
    {
        var result = Run("missing");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
        Assert.IsFalse(result.ShouldExit);
    }

    [TestMethod]
    public void Cd_DirectoryDoesNotExist_KeepsCurrentDirectory()
    {
        Run("missing");

        Assert.AreEqual(0, fileSystem.ChangeDirectoryCalls);
        Assert.AreEqual(work, fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_TargetIsFileNotDirectory_ShowsPathNotFound()
    {
        var result = Run("notes.txt");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
        Assert.AreEqual(0, fileSystem.ChangeDirectoryCalls);
    }

    [TestMethod]
    public void Cd_ExecutedTwice_ChangesRelativeToNewDirectory()
    {
        fileSystem.AddDirectory(Path.Combine(src, "inner"));

        Run("src");
        var result = Run("inner");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(Path.Combine(src, "inner"), fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_ThenNoArgument_ShowsNewDirectory()
    {
        Run("src");

        var result = Run();

        Assert.AreEqual(src, result.OutputMessage);
    }

    [TestMethod]
    public void Cd_ResolvePathThrowsArgumentException_ShowsPathNotFound()
    {
        fileSystem.ResolvePathException = new ArgumentException("invalid path");

        var result = Run("???");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
        Assert.AreEqual(0, fileSystem.ChangeDirectoryCalls);
    }

    [TestMethod]
    public void Cd_ResolvePathThrowsNotSupportedException_ShowsPathNotFound()
    {
        fileSystem.ResolvePathException = new NotSupportedException();

        var result = Run("a:b:c");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
    }

    [TestMethod]
    public void Cd_ChangeDirectoryThrowsUnauthorized_ShowsPathNotFound()
    {
        fileSystem.ChangeDirectoryException = new UnauthorizedAccessException();

        var result = Run("src");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
        Assert.AreEqual(work, fileSystem.CurrentDirectory);
    }

    [TestMethod]
    public void Cd_ChangeDirectoryThrowsIOException_ShowsPathNotFound()
    {
        fileSystem.ChangeDirectoryException = new IOException();

        var result = Run("src");

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PathNotFoundMessage, result.OutputMessage);
    }

    [TestMethod]
    public void Cd_UnexpectedException_IsNotSwallowed()
    {
        fileSystem.ChangeDirectoryException = new InvalidOperationException();

        Assert.ThrowsExactly<InvalidOperationException>(() => Run("src"));
    }

    private CommandResult RunLine(string input)
    {
        CommandContext? context = new CommandParser().Parse(input);

        Assert.IsNotNull(context);
        Assert.AreEqual("cd", context.Name.ToLowerInvariant());
        return command.Execute(context);
    }

    [TestMethod]
    public void Cd_WithoutSpaceBeforeDoubleDot_WorksLikeCdDoubleDot()
    {
        var result = RunLine("CD..");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(root, fileSystem.ChangedTo);
    }

    [TestMethod]
    public void Cd_WithoutSpaceBeforeBackslash_ParsesLikeCdBackslash()
    {
        var withoutSpace = new CommandParser().Parse("CD\\");
        var withSpace = new CommandParser().Parse("CD \\");

        Assert.IsNotNull(withoutSpace);
        Assert.IsNotNull(withSpace);
        CollectionAssert.AreEqual(withSpace.Arguments, withoutSpace.Arguments);
    }

    [TestMethod]
    public void Cd_Root_ChangesToRootOfCurrentDrive()
    {
        string separator = Path.DirectorySeparatorChar.ToString();
        string driveRoot = Path.GetPathRoot(work)!;
        fileSystem.AddDirectory(driveRoot);

        var result = Run(separator);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(driveRoot, fileSystem.ChangedTo);
    }
}