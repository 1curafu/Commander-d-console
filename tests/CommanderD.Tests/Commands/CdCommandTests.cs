using CommanderD.Commands;
using CommanderD.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommanderD.Tests.Commands
{
    [TestClass]
    public class CdCommandTests
    {
        private FakeFileSystemService _fileSystemService = null!;
        private CdCommand _command = null!;

        [TestInitialize]
        public void Setup()
        {
            _fileSystemService = new FakeFileSystemService();
            _command = new CdCommand(_fileSystemService);
        }

        [TestMethod]
        public void Execute_WithoutArguments_ReturnsCurrentDirectory()
        {
            var initialDir = @"C:\TestDir";
            _fileSystemService.ChangeDirectory(initialDir);
            var context = new CommandContext("cd", new List<string>());

            var result = _command.Execute(context);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(initialDir, result.OutputMessage);
        }

        [TestMethod]
        public void Execute_ExistingDirectory_ChangesDirectoryAndReturnsEmptyOk()
        {
            var targetDir = @"C:\TestDir\SubFolder";
            _fileSystemService.AddDirectory(targetDir);
            var context = new CommandContext("cd", new List<string> { targetDir });

            var result = _command.Execute(context);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.OutputMessage);
            Assert.AreEqual(targetDir, _fileSystemService.GetCurrentDirectory());
        }

        [TestMethod]
        public void Execute_DotDot_MovesOneLevelUp()
        {
            _fileSystemService.ChangeDirectory(@"C:\TestDir\SubFolder");
            var context = new CommandContext("cd", new List<string> { ".." });

            var result = _command.Execute(context);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.OutputMessage);
        }

        [TestMethod]
        public void Execute_CdDotDotWithoutSpace_WorksLikeDotDot()
        {
            _fileSystemService.ChangeDirectory(@"C:\TestDir\SubFolder");
            var context = new CommandContext("cd..", new List<string> { "cd.." });

            var result = _command.Execute(context);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.OutputMessage);
        }

        [TestMethod]
        public void Execute_CdBackslashWithoutSpace_WorksLikeBackslash()
        {
            _fileSystemService.ChangeDirectory(@"C:\TestDir\SubFolder");
            var context = new CommandContext("cd\\", new List<string> { "cd\\" });

            var result = _command.Execute(context);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.OutputMessage);
        }

        [TestMethod]
        public void Execute_NonExistingDirectory_ReturnsFailureMessage()
        {
            var context = new CommandContext("cd", new List<string> { @"C:\NonExistingDir" });

            var result = _command.Execute(context);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("The system cannot find the path specified.", result.OutputMessage);
        }
    }
}