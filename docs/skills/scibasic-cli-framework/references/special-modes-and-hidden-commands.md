# Special Modes and Hidden Commands

Source: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\CommandLine\Interpreters\Interpreter.vb` and `g:\...\src\My\UNIX\LinuxRunHelper.vb`. These behaviors are built into the framework and are not declared as user subcommands.

## `invokeSpecial` (`Interpreter.vb:208-241`)

Dispatch: `Execute` (:150-165) → `apiInvoke` (:198-206) → unknown command → `invokeSpecial` → `apiInvokeEtc`.

| Command | Lines | Behavior |
|---|---|---|
| `??vars` | 210-211 | `ExecuteImpl.PrintVariables()` — prints `App.GetAppVariables` via `ConfigEngine.Prints` (`ExecuteImpl.vb:129-137`). |
| `??history` | 212-213 | `ExecuteImpl.HandleShellHistory(args)` (`ExecuteImpl.vb:139-169`): reads `App.LogErrDIR.ParentPath & "/.shells.log"`; no args → print all; `/clear` → clear; `/search` → filter by `InStr`; else `Unknown command!`. |
| `?` / `??` / `--help` | 214-221 | ① no help args → `Help("")` lists all commands; ② first arg not a known command and `ExecuteQuery` set → `ExecuteQuery(args)`; ③ else `Help(help_argvs.First)`. `HasCommandName` at `View\CommandHelp.vb:67-68`. |
| `~` | 222-223 | `Console.WriteLine(App.ExecutablePath)` (Linux HOME-like semantic). |
| `man` | 224-225 | `ExecuteImpl.HandleProgramManual(Me, args)` (`ExecuteImpl.vb:171-190`): gets `cli_app.SDKdocs()`; `/out` default `"./"`; without `--file`: `--print` prints directly, else `SDKManual.LaunchManual`; always writes `{output}/{App.AssemblyName}.md` (UTF8). |
| `/linux-bash` | 226-227 | `My.UNIX.BashShell(install_root:=args("--install_root"))`. |
| `/cli.dev` | 228-229 | `Me.CreateCLIPipelineFile(args)` (`ExecuteImpl.vb:76-96`): `/namespace` default `"CLI"`, generates VB pipeline source via `New VB(...).GetSourceCode`; `---echo` prints to `App.StdOut`, else writes `{App.HOME}/{cli_app.Type.Assembly.Location.BaseName}.vb` (UTF8 no BOM, cleared first). |
| `??<term>` (prefix) | 231-234 | R-style fuzzy help: `InStr(args.Name, "??") = 1` → `Mid(args.Name, 3).DoCall(AddressOf Help)`. |
| other | 236 | `apiInvokeEtc(args.Name, args)`. |

All `Call ...` branches ignore callee return values; `invokeSpecial` returns 0 by default (:240).

## `executeFile` Mode — Script Host

`apiInvokeEtc` (:320-351) triggers when `ExecuteFile` is set **and** the first positional token (`args.Name`) resolves to an existing file/directory/URL:

- `IsPossibleRequestFile` (:312-318): `target.FileExists OrElse target.DirectoryExists`, else `target.isURL` (`http`/`https`/`ftp`).
- `runShellScriptFile` (:243-262): sets `App.InputFile = filename`; `argv.IsTrue("--debug")` runs synchronously; otherwise wrapped in try/catch returning `500` on error.
- `exec_shell_script_internal` (:270-303): calls `ExecuteFile()(path:=filename, args:=argv)` on a `Thread` + `ManualResetEvent`; `max_stack_size` env var can set stack size via `Unit.ParseByteSize` (:279, :291-296); debug mode runs inline (:282).
- Not a file and `ExecuteNotFound` set → `ExecuteNotFound()(cli)` (:324-335); else fuzzy similar-command list or `BAD_COMMAND_NAME` (:353).

This is the mechanism for `Rscript /path/to/run.R /arg1=value1 /flag`. Reference: `Rscript\Program.vb:86` passes `executeFile:=AddressOf RunRscriptFile`.

## Empty Arguments — Help or REPL

- `doExecuteNonCLIInput` (:172-190): `ExecuteEmptyCli` set → invoke parameterless callback; otherwise `Help("")` (:188, "when the user inputs nothing, print all command names").
- `Help(CommandName)` (:410-422): empty name → `Console.WriteLine(HelpSummary(False))` (:414); non-empty → `PrintCommandHelp` (:418).
- `?` / `??` / `--help` with no help args also calls `Help("")` (:216).

REPL reference: `g:\GCModeller\src\R-sharp\studio\R-terminal\Program.vb:75-84` sets `executeEmpty:=AddressOf Terminal.RunTerminal` (parameterless `Public Function RunTerminal() As Integer`, `Terminal.vb:273-323`). It builds a `LineEditor` with IntelliSense, loads startup packages, registers Ctrl+C cancel, and runs a `Shell` loop.

## `/i` Interactive Console

`RunCLIInternal` (`App.vb:1357-1359`) routes `args.Name.TextEquals("/i")` to `New InteractiveConsole(App).RunApp` before constructing `Interpreter`. This is a built-in interactive mode distinct from user-provided `executeEmpty`.

## `/linux-bash` and `/usr/local/bin` Installation

`BashShell(Optional install_root As Boolean = False) As Integer` in `g:\...\src\My\UNIX\LinuxRunHelper.vb:109-137`:

1. `path = App.ExecutablePath.TrimSuffix`; `bash_run = BashRun().LineTokens.JoinBy(ASCII.LF)`; `dir = path.ParentPath`; `bashfile = dir & "/help"` (the write to `bashfile` is commented out at :129).
2. **Install branch** (:115-121): when `install_root` is true → `path = $"/usr/local/bin/{App.ExecutablePath.BaseName}"` (:116), and the script content is replaced with two lines (:117-120):
   ```
   #!/bin/bash
   dotnet "{App.HOME}/{App.AssemblyName}.dll" $@
   ```
   (`$@` forwards all CLI arguments.)
3. Prints `Bash script save at:` and `path` (:123-124).
4. Writes `bash_run` to `path` using `Encodings.UTF8WithoutBOM.CodePage` (:131-134), no extension (:126).
5. **No chmod is performed** — docs at :99-103 state the user must run `sudo chmod 777 cmd.sh` manually.

Non-install script source `BashRun()` (:86-96): `My.Resources.bashRunner` + `My.Resources.runNet5` (`#If NETCOREAPP`) or `runMono`, replacing `{appName}` with `App.AssemblyName` (:90-93).

CLI invocation: `program /linux-bash --install_root true` (or bare `--install_root` flag via `IsTrue` semantics, `CommandLine.vb:487-492`). Use `executeQuery` to customize `?`/`??`/`--help` when the first argument is a search term rather than a command.

## Built-in Behavior Checklist for Reviews

- No args: expect help unless `executeEmpty` is wired.
- `--help`, `?`, `??`: expect command list; `??term` fuzzy help; `ExecuteQuery` hook possible.
- First arg an existing file/URL: expect `executeFile` to run (script-host programs must set this hook).
- `/i`: interactive console via framework, not user code.
- `man`: generates a Markdown manual file next to the executable/current dir.
- `/cli.dev`: generates reflection pipeline VB source; non-user-visible.
- `~`, `??vars`, `??history`: informational hidden commands.
