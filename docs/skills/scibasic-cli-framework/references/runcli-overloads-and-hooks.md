# RunCLI Overloads and Hooks

Source: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\App.vb` (lines 1180-1433).

## Delegates

Four hook delegate types are declared in `...\src\CommandLine\Interpreters\Abstract.vb`:

```vbnet
' Abstract.vb:77
Public Delegate Function ExecuteFile(path As String, args As CommandLine) As Integer
' Abstract.vb:82
Public Delegate Function ExecuteEmptyCLI() As Integer
' Abstract.vb:89
Public Delegate Function ExecuteNotFound(args As CommandLine) As Integer
' Abstract.vb:96
Public Delegate Function ExecuteQuery(args As CommandLine) As Integer
```

`ExecuteEmptyCLI` takes **no parameters** — pass a parameterless `Function() As Integer`.

## Public RunCLI Overloads

All overloads take `Interpreter As Type` as the first argument and carry `<ExportAPI("RunCLI")>` (many also `<Extension>` / `<MethodImpl(AggressiveInlining)>`). Hook shorthand: E=`executeEmpty`, N=`executeNotFound`, F=`executeFile`, Q=`executeQuery`.

| Line | Signature (after `Interpreter As Type`) | Hooks passed internally |
|---|---|---|
| 1191 | `RunCLI(args$, <CallerMemberName> caller$ = Nothing)` | none |
| 1196 | `RunCLI(args As String(), Optional E, N, F, Q)` | all four; `caller` hardcoded `"Main"` |
| 1214 | `RunCLI(args As CommandLineArgs, <CallerMemberName> caller$)` | none |
| 1229 | `RunCLI(args As CommandLineArgs, E, <CallerMemberName> caller$)` | E |
| 1243 | `RunCLI(args$, E, <CallerMemberName> caller$)` | E |
| 1265 | `RunCLI(args$, E, N, <CallerMemberName> caller$)` | E + N |
| 1287 | `RunCLI(args As CommandLineArgs, E, F, N, <CallerMemberName> caller$)` | E + N + F (declared order E,F,N; reordered to E,N,F at call) |
| 1313 | `RunCLI(args As CommandLineArgs, E, F, N, Q, <CallerMemberName> caller$)` | all four (standard full version) |
| 1340 | `RunCLI(args As CommandLineArgs, E, N, <CallerMemberName> caller$)` | E + N |
| 1381 | `RunCLI(args$, F, <CallerMemberName> caller$)` | F |
| 1399 | `RunCLI(args As CommandLineArgs, F, <CallerMemberName> caller$)` | F |
| 1412 | `RunCLI(args$, F, E, <CallerMemberName> caller$)` | delegates to 1429 overload via `Interpreter.RunCLI(Parsers.TryParse(args), executeFile, executeEmpty, caller)` |
| 1429 | `RunCLI(args As CommandLineArgs, F, E, <CallerMemberName> caller$)` | E + F (N, Q = Nothing) |

Note the argument **order trap**: for the `(F, E)` overload (1412/1429), `executeFile` comes before `executeEmpty` on the public API, but internal `RunCLIInternal` takes `(E, N, F, Q)`. Use named arguments (`executeFile:=`, `executeEmpty:=`) to avoid mistakes.

`args$` overloads call `Parsers.TryParse(args)`; `args As String()` overloads call `CommandLineArgs.BuildFromArguments(args)`.

## `RunCLIInternal` (:1346-1370)

```vbnet
Private Function RunCLIInternal(App As Type, args As CommandLineArgs, caller$,
                                executeEmpty As ExecuteEmptyCLI,
                                executeNotFound As ExecuteNotFound,
                                executeFile As ExecuteFile,
                                executeQuery As ExecuteQuery) As Integer
```

- `#If DEBUG` → `args.debug` (:1352-1354).
- `args.InitDebuggerEnvir(caller)` (:1355).
- If `args.Name.TextEquals("/i")` → interactive terminal mode: `finalizeCLI(New InteractiveConsole(App).RunApp)` — **bypasses `Interpreter` construction** (:1357-1359).
- Otherwise `New Interpreter(App, caller:=caller) With {.ExecuteEmptyCli=E, .ExecuteNotFound=N, .ExecuteFile=F, .ExecuteQuery=Q}`, then `finalizeCLI(program.Execute(args))` (:1361-1368).

## Dispatch Flow in `Interpreter`

- `Execute(args As CommandLine)` (:150-165): non-empty → `apiInvoke(args, args.Parameters)`; empty → `doExecuteNonCLIInput()`.
- `apiInvoke` (:198-206): `command = args.Name.ToLower`; if found in `apiTable` → `apiTable(command).Execute(args)`; else `invokeSpecial` → `apiInvokeEtc`.
- `apiInvokeEtc` (:320-351): if `ExecuteFile` set and command is a file/dir/URL (`IsPossibleRequestFile` :312-318) → `runShellScriptFile` (:243-262); else if `ExecuteNotFound` set → `ExecuteNotFound()(cli)` (:324-335); else Levenshtein-similar command list (`ExecuteImpl.vb:99-127`) or `BAD_COMMAND_NAME` (:353).
- `doExecuteNonCLIInput` (:172-190): if `ExecuteEmptyCli` set → invoke it (DEBUG direct, Release wrapped in Try/Catch, failure logs and returns `-100`); else `Help("")` (:188).

## Choosing an Overload / Hook Set

| Goal | Call |
|---|---|
| Normal CLI, help on no args | `GetType(MyCLI).RunCLI(App.CommandLine)` |
| Pass explicit `String()` command line | `GetType(MyCLI).RunCLI(args)` |
| REPL on no args | `GetType(MyCLI).RunCLI(App.CommandLine, executeEmpty:=AddressOf MyRepl)` |
| Script host (file/URL first arg) | `GetType(MyCLI).RunCLI(App.CommandLine, executeFile:=AddressOf RunScript)` |
| REPL + script host + custom unknown + help query | Full overload at :1313: `RunCLI(args, E, F, N, Q, caller)` |
| Custom unknown-command handling only | `RunCLI(args, E, N, caller)` |

`R-terminal\Program.vb:75-84` passes all four hooks:

```vbnet
Return GetType(CLI).RunCLI(
    args:=App.CommandLine,
    executeFile:=AddressOf RunScript,
    executeEmpty:=AddressOf Terminal.RunTerminal,
    executeNotFound:=AddressOf RunExpression,
    executeQuery:=AddressOf QueryCommandLineArgvs
)
```
