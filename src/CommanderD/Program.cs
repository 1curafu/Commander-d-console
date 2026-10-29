using CommanderD.Commands;
using CommanderD.Shell;

CommandParser parser = new CommandParser();
CommandFactory factory = new CommandFactory();
IOutputWriter outputWriter = new ConsoleOutputWriter();
CommandInvoker invoker = new CommandInvoker(parser, factory, outputWriter);
CommandShell shell = new CommandShell(invoker);

shell.Run();