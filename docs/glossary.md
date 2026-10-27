# Glossary

The terms of Commander D, so that all of Team D mean the same thing.
Example line used in the table, as in the assignment "Command Struktur von Windows commands":

```
C:\>dir c:\windows /A:R
```

| Term | Meaning | In our code |
|---|---|---|
| **Prompt** | The text in front of the cursor. It shows the current directory and ends with `>`, here `C:\>`. | `CommandShell` writes it from `CurrentDirectory` (CD-9) |
| **Input** (command line) | Everything typed after the prompt until Enter, here `dir c:\windows /A:R`. | `CommandContext.RawInput` |
| **Command** | The first word of the input. It names the operation, here `dir`. Not case sensitive: `dir`, `DIR` and `Dir` are the same. | `CommandContext.CommandName`, `ICommand` |
| **Parameter** | Everything after the command name, here `c:\windows /A:R`. | `CommandContext.Arguments` (the whole list) |
| **Argument** | One item of the parameter, separated by spaces, here `c:\windows` and `/A:R`. Text with spaces is put in double quotes, the parser removes the quotes. | One entry of `CommandContext.Arguments` |
| **Directory** | An argument that is a path, here `c:\windows`. | An argument, read as `string` and resolved with `ResolvePath` |
| **Flag:Attribute** (switch, option) | An argument that starts with `/`. It changes how the command works. Here `/A` is the flag and `R` is its attribute. A flag can also stand alone, like `/w`. | An argument, read by the command |
| **Placeholder** | The name in the description of a command, for example `MKDIR <path>`. `<path>` is the placeholder, `C:\temp` is the argument you give for it. | Only in Jira and documentation |
| **Internal command** | A command built into the shell (DIR, CD, ...). All 12 commands of this project are internal. | One class per command |
| **External command** | A separate program such as `ping.exe`. Not part of this project, so it counts as unknown. | `CommandFactory` returns `null` |
| **Path** | The location of a file or directory. *Absolute* paths start at the drive (`C:\Users`), *relative* paths start at the current directory (`src\Shell`). `.` is the current directory, `..` is the parent. | `string` |
| **Drive** | A letter with a colon, for example `C:`. `C:\` is the root of that drive. | Only in paths |
| **Current directory** (working directory) | The directory where relative paths start. It is shown in the prompt and changed by `CD`. | `IFileSystemService.CurrentDirectory` |
| **Output message** | The text a command prints, for results and for errors. | `CommandResult.OutputMessage` |
| **Exit code** | Number that tells if a command worked: `0` is success. In our code this is the `Success` flag. | `CommandResult.Success` |
| **Shell** | The program that repeats prompt, input, execution. | `CommandShell` |
| **Invoker** | Command Pattern: the class that executes commands without knowing them. | `CommandShell` |
| **Receiver** | Command Pattern: the class that does the real work. | `IFileSystemService`, `IConsoleService` |
| **Factory** | Creates the matching command from the command name. | `CommandFactory` |

## One note about macOS

Drive letters exist only on Windows. On macOS a path looks like `/Users/name`.
So our code must never assume `C:`. It uses `Path.Combine` and the `Path` class.
How the shell looks is compared with the real `cmd.exe` on Windows.
