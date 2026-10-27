using CommanderD.Commands;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands
{
    [TestClass]
    public class MkDirCommandTests
    {
        private FakeFileSystemService _fileSystemService = null!;
        private MkDirCommand _command = null!;

        [TestInitialize]
        public void Setup()
        {
            _fileSystemService = new FakeFileSystemService();
            _command = new MkDirCommand(_fileSystemService);
        }

        [TestMethod]
        public void Execute_NewDirectory_CreatesDirectoryAndReturnsEmptyOk()
        {
            var dirName = "NewFolder";
            var context = new CommandContext("mkdir", new List<string> { dirName });

            var result = _command.Execute(context);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.OutputMessage);
            Assert.IsTrue(_fileSystemService.DirectoryExists(dirName));
        }

        [TestMethod]
        public void Execute_ExistingDirectory_ReturnsAlreadyExistsError()
        {
            var dirName = "ExistingFolder";
            _fileSystemService.AddDirectory(dirName);
            var context = new CommandContext("mkdir", new List<string> { dirName });

            var result = _command.Execute(context);

            Assert.IsFalse(result.Success);
            Assert.AreEqual($"A subdirectory or file {dirName} already exists.", result.OutputMessage);
        }

        [TestMethod]
        public void Execute_WithoutArguments_ReturnsSyntaxError()
        {
            var context = new CommandContext("mkdir", new List<string>());

            var result = _command.Execute(context);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("The syntax of the command is incorrect.", result.OutputMessage);
        }
    }
}