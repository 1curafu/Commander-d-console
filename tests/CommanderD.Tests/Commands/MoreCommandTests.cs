using CommanderD.Commands;
using CommanderD.Services;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MoreCommandTests
{
    private const string SyntaxMessage = "The syntax of the command is incorrect.";

    private static readonly ConsoleKeyInfo Space = new(' ', ConsoleKey.Spacebar, false, false, false);
    private static readonly ConsoleKeyInfo Enter = new('\r', ConsoleKey.Enter, false, false, false);
    private static readonly ConsoleKeyInfo Q = new('q', ConsoleKey.Q, false, false, false);
    private static readonly ConsoleKeyInfo CtrlC = new('\u0003', ConsoleKey.C, false, false, true);
    private static readonly ConsoleKeyInfo X = new('x', ConsoleKey.X, false, false, false);

    private readonly string _work = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "commanderd-more-tests", "work"));
    private readonly MoreFakeFileSystem _fileSystem;
    private readonly TestOutputWriter _outputWriter;
    private readonly FakeConsoleService _console = new();
    private readonly MoreCommand _command;

    public MoreCommandTests()
    {
        _fileSystem = new MoreFakeFileSystem(_work);
        _outputWriter = new TestOutputWriter();
        _command = new MoreCommand(_fileSystem, _console, _outputWriter);
    }

    private void PressKeys(params ConsoleKeyInfo[] keys)
    {
        foreach (ConsoleKeyInfo key in keys)
        {
            _console.Keys.Enqueue(key);
        }
    }

    private void Run(params string[] arguments)
    {
        _command.Execute(new CommandContext("more", arguments));
    }

    /// <summary>Creates long.txt with lines "Zeile 01" ... "Zeile NN".</summary>
    private void AddLongFile(int lineCount)
    {
        string content = string.Join("\n", Enumerable.Range(1, lineCount).Select(i => $"Zeile {i:D2}"));
        _fileSystem.AddFile(Path.Combine(_work, "long.txt"), content);
    }

    private bool WasShown(int lineNumber)
    {
        return _outputWriter.OutputBuffer.Contains($"Zeile {lineNumber:D2}" + Environment.NewLine);
    }

    [TestMethod]
    public void More_HasNameAndDescription()
    {
        Assert.AreEqual("more", _command.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(_command.Description));
    }

    [TestMethod]
    public void More_NoArguments_PrintsSyntaxError()
    {
        Run();

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_EmptyQuotes_PrintsSyntaxError()
    {
        Run("\"\"");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_InvalidPath_PrintsSyntaxError()
    {
        _fileSystem.ResolvePathException = new ArgumentException("bad path");

        Run("x");

        Assert.AreEqual(SyntaxMessage + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_ExistingFile_PrintsEachLine()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"), "Zeile1\nZeile2");

        Run("test.txt");

        Assert.AreEqual("Zeile1" + Environment.NewLine + "Zeile2" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_WindowsLineEndings_PrintsLinesWithoutCarriageReturn()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"), "a\r\nb");

        Run("test.txt");

        Assert.AreEqual("a" + Environment.NewLine + "b" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_TrailingNewline_DoesNotPrintExtraEmptyLine()
    {
        _fileSystem.AddFile(Path.Combine(_work, "test.txt"), "a\nb\n");

        Run("test.txt");

        Assert.AreEqual("a" + Environment.NewLine + "b" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_EmptyFile_PrintsNothing()
    {
        _fileSystem.AddFile(Path.Combine(_work, "empty.txt"), null);

        Run("empty.txt");

        Assert.AreEqual(string.Empty, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_FileNameWithSpaces_ReadsFile()
    {
        _fileSystem.AddFile(Path.Combine(_work, "my notes.txt"), "hallo");

        Run("\"my", "notes.txt\"");

        Assert.AreEqual("hallo" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_MissingFile_PrintsCannotAccess()
    {
        string target = Path.Combine(_work, "missing.txt");

        Run("missing.txt");

        Assert.AreEqual($"Cannot access file {target}" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_Directory_PrintsCannotAccess()
    {
        string folder = Path.Combine(_work, "docs");
        _fileSystem.AddDirectory(folder);

        Run("docs");

        Assert.AreEqual($"Cannot access file {folder}" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_UnreadableFile_PrintsCannotAccess()
    {
        string target = Path.Combine(_work, "locked.txt");
        _fileSystem.AddFile(target, "geheim");
        _fileSystem.GetEntriesException = new UnauthorizedAccessException();

        Run("locked.txt");

        Assert.AreEqual($"Cannot access file {target}" + Environment.NewLine, _outputWriter.OutputBuffer);
    }

    [TestMethod]
    public void More_LongFile_ShowsOnlyFirstPage()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);

        Run("long.txt");

        Assert.IsTrue(WasShown(1));
        Assert.IsTrue(WasShown(3));
        Assert.IsFalse(WasShown(4));
    }

    [TestMethod]
    public void More_LongFile_FirstPageFillsWindowHeightMinusStatusRow()
    {
        _console.WindowHeight = 6;
        AddLongFile(20);

        Run("long.txt");

        Assert.IsTrue(WasShown(5));
        Assert.IsFalse(WasShown(6));
    }

    [TestMethod]
    public void More_LongFile_ShowsStatusWithPercentage()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);

        Run("long.txt");

        StringAssert.Contains(_outputWriter.OutputBuffer, "-- More (30%) --");
    }

    [TestMethod]
    public void More_ShortFile_ShowsNoStatusAndWaitsForNoKey()
    {
        AddLongFile(5);

        Run("long.txt");

        Assert.IsTrue(WasShown(5));
        Assert.IsFalse(_outputWriter.OutputBuffer.Contains("-- More"));
        Assert.AreEqual(0, _console.ReadKeyCalls);
    }

    [TestMethod]
    public void More_Space_AdvancesOnePage()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);
        PressKeys(Space, Q);

        Run("long.txt");

        Assert.IsTrue(WasShown(6));
        Assert.IsFalse(WasShown(7));
        StringAssert.Contains(_outputWriter.OutputBuffer, "-- More (60%) --");
    }

    [TestMethod]
    public void More_Enter_AdvancesOneLine()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);
        PressKeys(Enter, Q);

        Run("long.txt");

        Assert.IsTrue(WasShown(4));
        Assert.IsFalse(WasShown(5));
        StringAssert.Contains(_outputWriter.OutputBuffer, "-- More (40%) --");
    }

    [TestMethod]
    public void More_Q_EndsCommand()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);
        PressKeys(Q, Space);

        Run("long.txt");

        Assert.IsFalse(WasShown(4));
        Assert.AreEqual(1, _console.ReadKeyCalls);
    }

    [TestMethod]
    public void More_CtrlC_EndsCommand()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);
        PressKeys(CtrlC, Space);

        Run("long.txt");

        Assert.IsFalse(WasShown(4));
        Assert.AreEqual(1, _console.ReadKeyCalls);
    }

    [TestMethod]
    public void More_OtherKey_IsIgnored()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);
        PressKeys(X, Q);

        Run("long.txt");

        Assert.IsFalse(WasShown(4));
        Assert.AreEqual(2, _console.ReadKeyCalls);
    }

    [TestMethod]
    public void More_SpaceUntilEnd_ShowsWholeFileAndStops()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);
        PressKeys(Space, Space, Space);

        Run("long.txt");

        Assert.AreEqual(3, _console.ReadKeyCalls);
        Assert.IsTrue(_outputWriter.OutputBuffer.EndsWith("Zeile 10" + Environment.NewLine));
    }

    [TestMethod]
    public void More_AfterKeyPress_StatusIsErased()
    {
        _console.WindowHeight = 4;
        AddLongFile(10);
        PressKeys(Enter, Q);

        Run("long.txt");

        string status = "-- More (30%) --";
        StringAssert.Contains(_outputWriter.OutputBuffer, status + "\r" + new string(' ', status.Length) + "\r" + "Zeile 04");
    }

    /// <summary>
    /// Small file system for MORE tests. Unlike the shared FakeFileSystemService it stores file contents
    /// and returns them through GetEntries, which is how MORE reads a file.
    /// </summary>
    private sealed class MoreFakeFileSystem(string currentDirectory) : IFileSystemService
    {
        private readonly Dictionary<string, string?> _files = new(StringComparer.Ordinal);
        private readonly HashSet<string> _directories = new(StringComparer.Ordinal) { currentDirectory };

        public string CurrentDirectory { get; } = currentDirectory;

        public Exception? ResolvePathException { get; set; }

        public Exception? GetEntriesException { get; set; }

        public void AddFile(string path, string? content) => _files[path] = content;

        public void AddDirectory(string path) => _directories.Add(path);

        public string ResolvePath(string path)
        {
            if (ResolvePathException != null)
            {
                throw ResolvePathException;
            }

            return Path.GetFullPath(path, CurrentDirectory);
        }

        public bool DirectoryExists(string path) => _directories.Contains(path);

        public bool FileExists(string path) => _files.ContainsKey(path);

        public IReadOnlyList<DirectoryEntry> GetEntries(string path)
        {
            if (GetEntriesException != null)
            {
                throw GetEntriesException;
            }

            if (_files.TryGetValue(path, out string? content))
            {
                return [new DirectoryEntry(Path.GetFileName(path), false, content?.Length ?? 0, DateTime.Now, content)];
            }

            throw new DirectoryNotFoundException(path);
        }

        public bool IsProtectedFile(string path) => false;

        public void ChangeDirectory(string path) => throw new NotSupportedException();

        public void CreateDirectory(string path) => throw new NotSupportedException();

        public void CreateFile(string path, string content) => throw new NotSupportedException();

        public void DeleteFile(string path) => throw new NotSupportedException();

        public void Rename(string path, string newName) => throw new NotSupportedException();

        public void Move(string source, string destination) => throw new NotSupportedException();
    }
}