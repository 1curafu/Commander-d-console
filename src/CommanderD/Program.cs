using CommanderD.Commands;
using CommanderD.Shell;

CommandParser parser = new CommandParser();
CommandFactory factory = new CommandFactory();
CommandInvoker invoker = new CommandInvoker(parser, factory);
CommandShell shell = new CommandShell(invoker);

shell.Run();