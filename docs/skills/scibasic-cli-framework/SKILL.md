---
name: scibasic-cli-framework
description: Develop, review, or modify command-line programs built on the sciBASIC# / Microsoft.VisualBasic.Core reflection-based CLI framework (the GCModeller / R# ecosystem). This skill should be used when a task involves a VB.NET CLI entry module that calls GetType(...).RunCLI(...), declares ExportAPI subcommands, reads parameters from a CommandLine object, binds CLI tokens to typed function parameters, registers executeEmpty/executeFile/executeNotFound/executeQuery hooks, or relies on dynamic help generation and built-in hidden commands such as ?/??/--help, man, /linux-bash, /cli.dev and /i. Trigger on files like Program.vb / CLI.vb referencing Microsoft.VisualBasic.CommandLine or Core.vbproj.
---

# sciBASIC# CLI Framework

## Overview

The sciBASIC# CLI framework is a reflection-driven command-line engine in `Microsoft.VisualBasic.Core` (`Core.vbproj`). A CLI "container" is a VB.NET `Module` (or `Type`) whose `Public Shared` methods are marked with `<ExportAPI("name")>`; each method becomes a subcommand. `GetType(Container).RunCLI(...)` reflects over the type, dispatches the first token to a subcommand, and generates help documentation from attributes and XML doc comments.

Two authoring styles exist and are fully compatible:

- **v1** — each subcommand takes a single `args As CommandLine` and reads values manually (`args("/src")`, `args <= "--target"`). Metadata comes from `<Argument>`, `<Usage>`, `<Description>` attributes. Reference: `g:\GCModeller\src\R-sharp\Rscript\CLI\CLI.vb`.
- **v2** — each subcommand uses concrete typed parameters (`data As String`, `Optional verbose As Boolean = False`, `steps As Integer? = Nothing`). Parameter names are matched to CLI tokens by reflection; docs come from .NET XML `''' <param>` comments. Reference: `g:\GCModeller\src\GCModeller\engine\vCella\Program.vb`.

Both are invoked the same way: `GetType(MyCLI).RunCLI(App.CommandLine, ...)`.

## Choosing an Authoring Style (Decision Tree)

Follow this tree before writing code:

1. **Need a script host / interpreter** where the first argument is a file or URL to execute? → Use `executeFile` hook. See `references/special-modes-and-hidden-commands.md`.
2. **Need a REPL** when no arguments are supplied (e.g. an interpreter terminal)? → Use `executeEmpty` hook pointing to a parameterless `Function() As Integer`. See `references/special-modes-and-hidden-commands.md`.
3. **Subcommands read a variable set of flags, or need runtime-built parameters?** → Use **v1** (single `CommandLine` parameter). See `references/style-v1.md`.
4. **Subcommands have a known, fixed set of typed options and XML documentation is desired?** → Use **v2** (typed parameters). See `references/style-v2.md`.
5. **Custom handling of unknown commands or `--help` queries?** → Add `executeNotFound` / `executeQuery` hooks. See `references/runcli-overloads-and-hooks.md`.

v2 is generally preferred for new code because signatures are self-documenting and XML comments become help text automatically; v1 remains for compatibility and dynamic parameter scenarios.

## Core Concepts (must know)

Read `references/core-framework-and-parsing.md` for full detail. Essential rules:

- The single core data class is `CommandLine`. `CommandLineArgs` is only an alias (`Imports CommandLineArgs = Microsoft.VisualBasic.CommandLine.CommandLine` in `App.vb:106`), not a separate class.
- CLI token parsing chain: `CLI string → CLITools.GetTokens → CLIParser.TryParse(String) → CLIParser.TryParse(StringList) → ParseCLIInternal → CliArgumentParsers.CreateParameterValues / GetLogicalFlags`.
- Argument prefixes `-`, `--`, `/`, `\` are accepted. Keys are case-insensitive. `key=value` is split into two tokens. Duplicate keys throw by default.
- Bare switches (no value) are stored in `CommandLine.BoolFlags`, **not** in `Keys`. This distinction is critical for v2 boolean binding.
- `/@set key=value;key2=value2` defines environment variables; values support `App.GetVariable` interpolation.
- Mark every subcommand with `<ExportAPI("command-name")>` as a `Public Shared` method returning `Integer`. Help text prefers `DescriptionAttribute`, falling back to XML `<summary>`.

## Quick Start: Minimal v2 CLI

Create a module and an entry point:

```vbnet
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine

<CLI>
Public Module Program
    ''' <summary>Print a greeting.</summary>
    ''' <param name="name">Who to greet.</param>
    ''' <param name="loud">Print in upper case.</param>
    ''' <returns>Zero on success.</returns>
    <ExportAPI("/greet")>
    <Usage("/greet --name <value> [--loud]")>
    Public Function Greet(name As String, Optional loud As Boolean = False) As Integer
        Dim text = $"Hello, {name}"
        If loud Then text = text.ToUpper
        Call Console.WriteLine(text)
        Return 0
    End Function

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function
End Module
```

Copy and adapt the ready-to-use skeletons in `assets/v2-template/` and `assets/v1-template/` when starting a new CLI program.

## Quick Start: Minimal v1 CLI

```vbnet
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine
Imports System.ComponentModel

<CLI>
Public Module CLI
    <ExportAPI("/greet")>
    <Description("Print a greeting.")>
    <Usage("/greet --name <value> [--loud]")>
    <Argument("--name", False, CLITypes.String, Description:="Who to greet.")>
    Public Function Greet(args As CommandLine) As Integer
        Dim name$ = args("--name")
        Dim loud As Boolean = args("--loud")
        Call Console.WriteLine($"Hello, {name}")
        Return 0
    End Function
End Module
```

## Development Workflow

When asked to build a new CLI program with this framework:

1. Confirm the target style (v1 vs v2) using the decision tree; default to v2 for new code.
2. Confirm hooks needed: `executeFile` (script host), `executeEmpty` (REPL), `executeNotFound`, `executeQuery`.
3. Create the VB.NET project referencing `Microsoft.VisualBasic.Core\src\Core.vbproj`; see `references/project-setup.md` for the `.vbproj` essentials.
4. Write the CLI container module; consult `references/style-v1.md` or `references/style-v2.md`.
5. For v2, follow the exact parameter-binding rules in `references/parameter-binding-v2.md` — especially the `-`→`_` name mapping, `BoolFlags` fallback, and the boolean special case. Incorrect parameter names silently keep defaults.
6. Wire `Main` to `RunCLI` using the correct overload from `references/runcli-overloads-and-hooks.md`.
7. Verify built-in hidden commands still work (`?`, `??`, `--help`, `man`, `/linux-bash`, `/cli.dev`, `/i`) per `references/special-modes-and-hidden-commands.md`.
8. Build and sanity-check: run the program with no args (help or REPL), with `--help`, and with each subcommand.

When reviewing or debugging an existing CLI, check these common failure points first:

- A v2 parameter declared with `-` in the CLI name must be written with `_` in VB (`--aa-bb` → `aa_bb`). See `references/parameter-binding-v2.md`.
- A bare boolean switch not appearing in `CommandLine.Keys` — boolean binding must scan `BoolFlags`.
- A required v2 parameter without a default throws `MissingMemberException` at invocation time, not at startup.
- `CommandLine` typed parameters in v2 must be checked for injection before the `IsOptional` check (`Delegate.vb:189-196`).
- `RunCLI` overload parameter order for the `(executeFile, executeEmpty)` variant is `(F, E)` on the public overload but `(E, F)` internally.

## Resources

### references/

| File | Contents |
|---|---|
| `core-framework-and-parsing.md` | Directory layout, `CommandLine` data model, tokenization/parsing pipeline, all metadata attributes, interpreter reflection and help generation. |
| `style-v1.md` | v1 authoring pattern, real `<Argument>`/`<Usage>` examples from Rscript, reading values from `CommandLine`. |
| `style-v2.md` | v2 authoring pattern, real typed-signature and XML-doc examples from vCella, nullable options. |
| `parameter-binding-v2.md` | Complete v2 reflection binding algorithm: `MatchArgumentName`, `FindMatch`, aliases, `-`→`_`, `BoolFlags`, boolean special case, type conversion. |
| `runcli-overloads-and-hooks.md` | All `RunCLI` overloads (App.vb:1180-1433), `RunCLIInternal`, the four delegate hooks and their signatures. |
| `special-modes-and-hidden-commands.md` | `invokeSpecial` hidden subcommands, `executeFile` file/URL mode, empty-arg help, REPL, `/linux-bash --install_root` and `/usr/local/bin` install behavior. |
| `project-setup.md` | `.vbproj` essentials: `OutputType`, `StartupObject`, `Core.vbproj` reference, namespaces/imports. |

### assets/

| Path | Contents |
|---|---|
| `v2-template/Program.vb` | Starter v2 CLI module with `/hello`, XML docs, and `Main`. |
| `v1-template/Program.vb` | Starter v1 CLI module with `<ExportAPI>` + `<Argument>` and `Main`. |

Load a reference file with a search/read tool when its topic is needed; do not load all references up front.
