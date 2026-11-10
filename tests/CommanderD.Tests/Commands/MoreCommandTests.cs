using CommanderD.Commands;
using CommanderD.Services;
using CommanderD.Shell;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands;

[TestClass]
public class MoreCommandTests
{
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
    public void More_MissingFile_PrintsCannotAccess()
    {
        string target = Path.Combine(_work, "missing.txt");

        Run("missing.txt");

        Assert.AreEqual($"Cannot access file {target}" + Environment.NewLine, _outputWriter.OutputBuffer);
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