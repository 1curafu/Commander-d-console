using CommanderD.Commands;
using CommanderD.Services;
using CommanderD.Shell;

IFileSystemService fileSystemService = new FileSystemService();

CommandParser parser = new CommandParser();
CommandFactory factory = new CommandFactory();
factory.Register("mkdir", () => new MkDirCommand(fileSystemService));

CommandInvoker invoker = new CommandInvoker(parser, factory);
CommandShell shell = new CommandShell(invoker);

shell.Run();