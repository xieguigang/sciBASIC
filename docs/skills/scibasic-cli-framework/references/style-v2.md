# v2 CLI Style — Typed Function Parameters

Reference implementation: `g:\GCModeller\src\GCModeller\engine\vCella\Program.vb`.

## Container Structure

- `Module Program` (`Program.vb:22`) with 7 subcommands: `/compile` (:41), `/run` (:129), `/petri` (:259), `/context` (:405), `/export` (:557), `/optimize` (:735), `/optimize-consortium` (:1045).
- Every subcommand is `Public Function ... As Integer` with **concrete typed parameters** (`String`, `Boolean`, `Integer`, `Double`, nullable `Integer?`/`Double?`). No `args As CommandLine` parameter.
- Each subcommand is declared with `<ExportAPI("/...")>` + `<Usage("... --flag=<default=...> ...")>`. No `<Argument>` attributes are used.

## Main Entry Point

`Program.vb:24-26` — minimal, no callbacks:

```vbnet
Public Function Main(args As String()) As Integer
    Return GetType(Program).RunCLI(App.CommandLine)
End Function
```

(The `args` parameter is ignored; the framework global `App.CommandLine` is used. To use the passed-in `String()` instead, call `GetType(Program).RunCLI(args)` — see `runcli-overloads-and-hooks.md`.)

## XML Doc Comments Become Help Text

Each method carries full `''' <summary>` / `''' <param name="...">` / `<paramref>` / `<c>` / `<see cref="..."/>` / `<returns>` documentation. Example (`Program.vb:28-44`):

```vbnet
''' <summary>
''' Compile a BioCyc database directory into a GCMarkup virtual cell model xml file.
''' </summary>
''' <param name="data">
''' Source BioCyc database directory. When <paramref name="output"/> is omitted the model file is
''' written next to it as <c>&lt;data&gt;.vcella.xml</c>.
''' </param>
''' <param name="output">Target model xml path; defaults to <c>&lt;data&gt;.vcella.xml</c>.</param>
''' <param name="verbose">Print per-step compilation logs to the console.</param>
''' <param name="report">Write a compilation report next to the generated model file.</param>
''' <returns>Zero on success; a non-zero exit code when the compilation fails.</returns>
<ExportAPI("/compile")>
<Usage("/compile --data='/path/to/model/data/' [--output='/path/to/output.xml' --verbose --report]")>
```

The framework completes parameter help from XML docs (`ArgumentCollection.vb:203-204, 221+`, `XmlDocHelp.vb`). A `DescriptionAttribute` on the method takes precedence for the summary (`Interpreter.vb:549-553`).

## Representative Method Signatures

```vbnet
' Program.vb:41-44
Public Function Compile(data As String,
                        Optional output As String = Nothing,
                        Optional verbose As Boolean = False,
                        Optional report As Boolean = False) As Integer
```

```vbnet
' Program.vb:405-409
Public Function Context(models As String,
                        Optional output As String = Nothing,
                        Optional init_mass As Double = 100,
                        Optional with_context As Boolean = False,
                        Optional report As Boolean = False) As Integer
```

`/run` has 21 typed parameters (`Program.vb:129-150`, e.g. `voxel_resolution As Integer = 128`, `no_state As Boolean = False`, `passive_permeability As Double = -1`); `/petri` at `:259-288`. Method bodies only validate and assemble options objects then call the engine (`:327-364` builds `PetriOptions`, `:570-583` builds `ExportOptions`).

## Nullable Options Distinguish "Not Supplied" from "Supplied Zero"

In `/optimize`, `Integer?`/`Double?` express the difference between an omitted option and an explicit value (`:593-594`, `:735-749`):

```vbnet
''' Every option is optional except <paramref name="model"/>: an option that is not supplied keeps the
''' value loaded from <paramref name="config"/>, and an option that is supplied always wins.
Public Function Optimize(model As String,
                         Optional output As String = Nothing,
                         Optional config As String = Nothing,
                         Optional steps As Integer? = Nothing,
                         Optional dt As Double? = Nothing, ...)
```

## Naming Rules (Critical)

Parameter names are matched against CLI token names by reflection. The exact algorithm is in `parameter-binding-v2.md`. Key points:

- Write VB identifiers with `_` where the CLI uses `-`: CLI `--aa-bb` binds to VB parameter `aa_bb` (`Delegate.vb:245-248`).
- Prefixes (`--`, `-`, `/`, `\`) are stripped from the CLI name before comparison; matching is case-insensitive.
- Use `<Parameter("--alias")>` (`Scripting\MetaData\Parameter.vb`) to bind a VB parameter to a differently-named CLI token.
- A bare boolean switch must be declared `Optional flag As Boolean = False`; binding reads `BoolFlags` as well as `Keys`.
- A required parameter (no `Optional`/default) that is missing throws `MissingMemberException` at invocation.
- A `CommandLine`-typed parameter receives the whole parsed object — this is the v1/v2 bridge. The check happens before the `IsOptional` check (`Delegate.vb:189-196`).

## When to Use v2

- New CLI programs with a fixed, known option set.
- When self-documenting signatures and XML help generation are desired.
- When migrating from v1: keep the same command names; replace `args As CommandLine` with typed parameters, remove `<Argument>`, add XML docs.

## Style Comparison Table

| Style | Subcommand signature | Discovery & docs | Main → RunCLI |
|---|---|---|---|
| v1 | `Function Compile(args As CommandLine) As Integer`, reads `args("/src")`, `args <= "--target"` | `<CLI> Module CLI` + `<ExportAPI("--x")>` + `<Description>`/`<Usage>`/`<Argument>` | `GetType(CLI).RunCLI(App.CommandLine, executeFile:=..., executeEmpty:=...)` |
| v2 | `Function Compile(data As String, Optional output As String = Nothing, ...) As Integer` | `<ExportAPI("/x")>` + `<Usage>`, typed parameters bound by reflection; XML `''' <param>` docs | `GetType(Program).RunCLI(App.CommandLine)` |
