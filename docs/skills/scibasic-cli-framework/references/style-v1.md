# v1 CLI Style — `args As CommandLine`

Reference implementation: `g:\GCModeller\src\R-sharp\Rscript\` (project `Rscript.NET5.vbproj`).

## Container Structure

- `CLI\CLI.vb:66`: `<CLI> Module CLI` — the whole module is marked with the framework's `<CLI>` attribute (defined `g:\...\src\CommandLine\InteropService\SharedORM\CLI.vb:63-64`).
- `CLI\IPC.vb:75`, `CLI\Lambda.vb:74`, `CLI\Parallel.vb:64` use `Partial Module CLI` to extend the same type. One logical CLI type spans multiple files; each subcommand is a module-level `Public Function`.
- `Program.vb:73` is a separate `Module Program` responsible only for the entry point and script-execution callback.

## Subcommand Declaration

Subcommands reflect over `Public Shared` methods marked with `<ExportAPI("name")>`. All parameters are described by method-level attributes; the only VB parameter is `args As CommandLine`.

```vbnet
' CLI\CLI.vb:68-81
<ExportAPI("--build")>
<Description("build a R# package source dir as package assembly file.")>
<Usage("--build [/src <folder, default=./> ... ]")>
<Argument("/src", False, CLITypes.File, PipelineTypes.std_in,
          AcceptTypes:={GetType(String)},
          Description:="A folder path that contains the R source files ...")>
Public Function Compile(args As CommandLine) As Integer
```

```vbnet
' CLI\CLI.vb:109-112
<ExportAPI("--check")>
<Usage("--check --target <package.zip> [--debug]")>
<Description("Verify a packed R# package is damaged or not ...")>
Public Function Check(args As CommandLine) As Integer
```

```vbnet
' CLI\Parallel.vb:71-77
<ExportAPI("--parallel")>
<Usage("--parallel --master <master_port> [--host <localhost> ...]")>
<Description("Create a new parallel thread process for running a new parallel task.")>
<Argument("--master", False, CLITypes.Integer, AcceptTypes:={GetType(Integer)}, Description:="the TCP port of the master node.")>
<Argument("--task", True, CLITypes.String, Description:="set the task name ...")>
<Argument("--delegate", True, CLITypes.String, Description:="the delegate function name ...")>
Public Function parallelMode(args As CommandLine) As Integer
```

```vbnet
' CLI\IPC.vb:116,135,168 — multiple <Argument>, includes CLITypes.Base64 + std_in pipeline
<ExportAPI("--slave")>
<Argument("/exec", False, CLITypes.File, AcceptTypes:={GetType(String)}, Extensions:="*.R", ...)>
<Argument("/argvs", False, CLITypes.Base64, PipelineTypes.std_in, ...)>
Public Function slaveMode(args As CommandLine) As Integer
```

`ArgumentAttribute` can annotate `Method` or `Parameter` (`ArgumentAttribute.vb:70`). Here it annotates the method to describe each switch. Constructor: `Sub New(name$, Optional [optional]=False, Optional type As CLITypes=CLITypes.String, Optional pip As PipelineTypes=PipelineTypes.undefined)` (`ArgumentAttribute.vb:185-188`).

## Reading Parameter Values

All values are read manually from `CommandLine` via indexer/operators — no typed parameters:

```vbnet
' CLI\CLI.vb:82-89
Dim src$ = RPackage.sourceHelper(args("/src") Or App.CurrentDirectory)
Dim save$ = args("/save") Or $"{src}/../{meta.Package}_{meta.Version}.zip"
Dim skipSourceBuild As Boolean = args("--skip-src-build")
Dim r_syntax As String = args("--github-page") Or "../../_assets/R_syntax.js"
Dim enableDebugSymbols As Boolean = args("--debug-symbols")
```

```vbnet
' CLI\CLI.vb:113 — <= operator extracts a value
Dim target As String = args <= "--target"
Dim is_debug As Boolean = args("--debug")
```

Useful accessors (see `core-framework-and-parsing.md`): `args("--flag")`, `args <= "--flag"`, `args.GetValue(Of T)(name, default)`, `args.GetString`, `args.IsTrue`, `args.ContainsParameter`, `args.Keys`, `args.BoolFlags`, `args.Parameters`.

## Main Entry Point

`Program.vb:80-82`:

```vbnet
Public Function Main() As Integer
    Return GetType(CLI).RunCLI(App.CommandLine, executeFile:=AddressOf RunRscriptFile, executeEmpty:=AddressOf Run)
End Function
```

`Main()` has no parameters; command line comes from `App.CommandLine`. `executeEmpty` falls back to `Run()` (reads a script from stdin), and `Run()` re-invokes `GetType(CLI).RunCLI(App.CommandLine)` to show help when stdin is empty (`Program.vb:88-102`).

## When to Use v1

- The parameter set is dynamic or large and reading from a container is simpler than many typed parameters.
- Preserving an existing codebase written in v1 style.
- Parameters need pipeline behavior (`CLITypes.File` with `PipelineTypes.std_in`, `CLITypes.Base64`) described with `<Argument>`.
- Runtime-built arguments via `CreateOpts` / `SetOpt`.

## Conventions

- Group related subcommands into `Partial Module` files sharing one `<CLI>` module.
- Use `<Usage>` to document the exact command form; `<Description>` from `System.ComponentModel` for the summary; one `<Argument>` per switch.
- Prefix command names consistently (this project uses `--build`, `--check`, `--parallel`; vCella uses `/compile`, `/run`).
- Return `Integer` exit codes from every subcommand.
