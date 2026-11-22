# Core Framework and Parsing

Source: `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\CommandLine\` and `...\ApplicationServices\App.vb`.

## Directory Layout (70 .vb files)

Root `...\src\CommandLine\`:

| File | Role |
|---|---|
| `CommandLine.vb` | Core `CommandLine` data model (parsed result object) |
| `CLITools.vb` | Extensions: `GetTokens` (:197), `TryParse` (:298), path/object helpers |
| `CLIMapper.vb` | CLI args → object property mapping (`ContainsParameter`) |
| `CliResCommon.vb` | Resource/export info base class |
| `GitBashEnvironment.vb` | `GetCommandLineArgs()` (:77), Windows network path fix |
| `Stream.vb` | stdin/stdout/pipe helpers |

Subdirectories:

- `Parsers\` — `CLIParser.vb` (`TryParse` entry + `ParseCLIInternal`), `CliArgumentParsers.vb` (`CreateParameterValues`, `GetLogicalFlags`, `IsPossibleLogicFlag`), `CommandLineTokenlizer.vb` (tokenizer/escaping), `DictionaryParser.vb` (`/@set k=v;k=v`).
- `Reflection\` — `CLIToken.vb` (`CLIToken`/`Argv`/`Prefix`), `CLITypes.vb`, `ArgumentCollection.vb`, `ExportApiInformation.vb`, `XmlDocHelp.vb` (XML doc completion).
  - `Reflection\Attributes\` — `ArgumentAttribute.vb`, `ExportAPI.vb`, `Namespace.vb`, `NoteAttribute.vb`, `UsageAttribute.vb`, `ExampleAttribute.vb`, `OutputAttribute.vb`, `Flags.vb`, `Grouping.vb`, `LastUpdatedAttribute.vb`, `IExportAPI.vb`.
  - `Reflection\EntryPoints\` — `APIEntryPoint.vb`, `Delegate.vb` (`APIDelegate`).
  - `Reflection\SetOpt\` — `OptAttribute.vb`, `SetOpt.vb`.
- `Interpreters\` — `Interpreter.vb`, `Abstract.vb` (4 delegate definitions), `ExecuteImpl.vb`, `InteractiveConsole.vb`, `View\` (`SDKManual.vb`, `ManualBuilder.vb`, `CommandHelp.vb`, `ExceptionHandler.vb`).
- `InteropService\` — `CLIBuilder.vb`, `CLIAbstractor.vb`, `Pipeline\`, `SharedORM\` (includes `CLIAttribute` at `SharedORM\CLI.vb:63-64`).
- `CLI\` — `IORedirect*.vb`, `PipelineProcess.vb`, `ProcessEx.vb`, `Scripting.vb`.
- `POSIX\` — `POSIXParser.vb` (`JoinTokens`), `POSIXArguments.vb`.

## `CommandLine` Class

Declared `Public Class CommandLine`, implements `ICollection(Of NamedValue(Of String))`, `INamedValue`, `IStringGetter` (`CommandLine.vb:100-103`). Internal `arguments As List(Of NamedValue(Of String))` (:105).

Key members (line numbers in `CommandLine.vb`):

- `Name` (:121-129) — command/subcommand name (first token).
- `Tokens` (:139), `ParameterList` (:145-150), `Keys` (:156-161) — keys from `arguments.Select(Name)`, **does not include BoolFlags**.
- `Parameters` (:171-176) — `Tokens.Skip(1)`.
- `BoolFlags` (:183) — bare switches, lower-cased; switches start with `--`/`-`/`/`/`\` and carry no value (comment :178-181).
- `EnvironmentVariables` (:191-202) — from `/@set`, via `DictionaryParser.TryParse`.
- `cli` (:212-217) — original command-line string.
- **Indexer** `Default Public ReadOnly Property Item(paramName As String) As DefaultString` (:235-272) — case-insensitive; auto `TrimNamePrefix` (:306-308 strips `\ / -`); if `HavebFlag` true returns `"TRUE"` (:255-256); value interpolated via `Interpolate(environment, escape:=False)` (:267, env from `App.GetVariable` :274).
- `HavebFlag` (:283-290), `SingleValue` (:276), `IsNullOrEmpty`/`IsNothing` (:384-400).
- `ContainsParameter(parameterName, Optional trim)` (:410-420) — case-insensitive, optional `TrimParamPrefix`.
- `GetValue(Of T)(name, default, cast)` (:627-646) — returns default when absent (Boolean special case), otherwise `InputHandler.CTypeDynamic`. `GetObject(Of T)` (:612-617), `GetString` (:597-603).
- `IsTrue` (:487-492) — true if flag present, else `DefaultValue.ParseBoolean`.
- `Required` (:335-343) — throws `InvalidProgramException` on empty.
- `GetDictionary` (:465-477), `OpenStreamInput`/`OpenStreamOutput`/`ReadInput` (:511-571), `GetFullFilePath`/`DIRPath` (:317-333).
- `ToArgumentVector` (:818-837), `CreateOpts` → `SetOpt.CreateOpt(Of T)` (:839-841).
- Operator overloads `+`, `<=`, `^`, `-`, `>`, `>=` (:849-926) — mostly value extraction/re-parsing.
- `Parse` (:931-933) → `Parsers.TryParse(GetTokens(...), False, str)`; `ParseTokens` (:935-937).
- `BuildFromArguments` (:939-952): with `NoSubCommand=True` prepends `"RUN"` (:940-942), then `Parsers.TryParse(args, False, rawInput:=argStr)`.

Note: `CommandLine` has **no** `TryGetValue`; `TryGetValue` in `Interpreter` (:786-788) is its `IDictionary` implementation. Values are read via indexer, `GetValue`, `GetString`, operators.

## `CommandLineArgs` Is an Alias

`g:\...\ApplicationServices\App.vb:106`:

```vbnet
Imports CommandLineArgs = Microsoft.VisualBasic.CommandLine.CommandLine
```

So `App.CommandLine As CommandLineArgs` and `RunCLI(args As CommandLineArgs, ...)` all use the same class. `GitBashEnvironment.GetCommandLineArgs()` returns `CommandLine` (:77-85). `CommandLineArgs.BuildFromArguments` is really `CommandLine.BuildFromArguments` (:939-952).

## Parsing Pipeline

`Parsers` = namespace `CommandLine.Parsers`; `TryParse` is in module `CLIParser` (`Parsers\CLIParser.vb:69`):

**String overload** (:189-208): `TryParse(CLI$, Optional duplicateAllowed=False)`. Empty string → empty `CommandLine` (:194-196); else `CLITools.GetTokens(CLI).TryParse(duplicateAllowed, rawInput:=CLI)` (:203-205).

**Token overload** (:104-125): `TryParse(args As StringList, allowDuplicated=False, rawInput$=Nothing)`.
- UNIX: `POSIX.JoinTokens(args)` merges split tokens (docker case) (:110-112).
- `.fixWindowsNetworkDirectory` → `extract` (:78-94: splits `name=value` tokens) → `ParseCLIInternal` (:119-123).

`GetTokens` (`CLITools.vb:196-248`): regex `"\s+(?=(?:[^""]|""[^""]*"")*$)"` (:186) preserves spaces inside quotes; strips outer double quotes (:229-231); `tupleParser` (:256-283) splits on first unescaped `=` for non-URL tokens, except tokens after `/@set` (:235-241).

`ParseCLIInternal` (`CLIParser.vb:128-161`):
- `bools = tokens.Skip(1).GetLogicalFlags(singleValue)` (:130).
- Builds `CommandLine` with `Name = tokens(0)`, `Tokens`, `BoolFlags = bools`, `cliCommandArgvs = rawInput` (:131-139).
- Single parameter with no `singleValue` → `SingleValue` (:141-145).
- `cli.arguments = tokens.Skip(1).CreateParameterValues(False)` (:148).
- `checkKeyDuplicated` (:165-177) — duplicate keys throw by default (:150-157).
- `IsPossibleLogicFlag` (`CliArgumentParsers.vb:187-202`): starts with `-`/`/`, no spaces, not numeric, `/` count ≤ 1; :197-201 special-cases Linux absolute paths. `GetLogicalFlags` (:134-179) lower-cases flag names (:178).

## Metadata Attributes

- **`ArgumentAttribute`** — `Reflection\Attributes\ArgumentAttribute.vb:71`, `Inherits CLIToken`, `<AttributeUsage(Method Or Parameter, AllowMultiple:=True, Inherited:=True)>` (:70). Properties: `Name` (:79-83), `BriefName` (POSIX short, :89), `Description` (:101-115, `"\n"` → `vbCrLf`), `Example` (:127-135), `Usage` (:143), `Optional`/`TokenType`/`Pipeline` (:151-153), `Out` (:159), `AcceptTypes` (:164), `Extensions` (:177). Constructors: `Sub New(name$, Optional [optional]=False, Optional type As CLITypes=CLITypes.String, Optional pip As PipelineTypes=PipelineTypes.undefined)` (:185-195). `ToString` (:202-231) formats help lines.
- **`CLIToken`** base — `Reflection\CLIToken.vb:80` (`MustInherit`, `Inherits Attribute`), `Name` (:87); also `Argv` (:105-149, on Property: `Type`/`IsOptional`/`Format`) and `Prefix` (:154-166, on Class/Property).
- **`Parameter` attribute (alias)** — `g:\...\src\Scripting\MetaData\Parameter.vb:69`, `<AttributeUsage(Parameter)>`. Properties: `ParameterInfo` (:78), `[Alias]` (:88-99), `Description` (:107), `TypeInfo` (:170); `GetAliasNameView`/`GetParameterNameAlias` (:129/143/159). Used by v2 binding (see `parameter-binding-v2.md`).
- **`ExportAPIAttribute`** — `Attributes\ExportAPI.vb:107`, marks command entry; `Name` (:115).
- **`[Namespace]`** — `Attributes\Namespace.vb:68`, `Description` (:77) / `Namespace` (:86).
- **`UsageAttribute`** — `Attributes\UsageAttribute.vb:62`, `UsageInfo` (:64).
- **`ExampleAttribute`** — `Attributes\ExampleAttribute.vb:62`, `ExampleInfo` (:64).
- **`NoteAttribute`** — `Attributes\NoteAttribute.vb:70`, `noteText` (:72).
- **Others**: `OutputAttribute`, `Flags`, `Grouping`, `LastUpdated`, `ActiveViews`, `IExportAPI`, `OptAttribute`.
- **`CLIAttribute`** — `InteropService\SharedORM\CLI.vb:63-64`, marks the container module (`<CLI> Module CLI`).
- **`DescriptionAttribute`** — from `System.ComponentModel`; no custom version in the framework. Read on methods via `methodInfo.Description([default]:="")` (`Interpreter.vb:549`). Help prefers `Description`, falls back to XML `<summary>` (`Interpreter.vb:551-553`).

## Interpreter: Reflection & Help Generation

`g:\...\CommandLine\Interpreters\Interpreter.vb:98`, `Public Class Interpreter`, `Implements IDisposable, IDictionary(Of String, APIEntryPoint)`; `apiTable As Dictionary(Of String, APIEntryPoint)` (:106, keys lower-case).

**Discovery**: constructor `Sub New(type As Type, <CallerMemberName> caller)` (:447-462) iterates `__getsAllCommands(type, False)`, lower-cases command names into `apiTable` (:449-455), sets root namespace/info. `GetAllCommands` (:486-504) calls `type.GetMethods(BindingFlags.Public Or BindingFlags.Static)` (:491), filters methods with `ExportAPIAttribute` (:492/517) → `getAPI` (:513-537) → `doLoadApiInternal` (:543-563). Help info: `Description` first, XML summary fallback (:549-553); `Usage`/`Example` via `methodInfo.Usage`/`ExampleInfo` (:559-560).

**Parameter help**: `ArgumentCollection` (`Reflection\ArgumentCollection.vb:185-215`) reflects `ArgumentAttribute` (:186) and completes from XML docs (:203-204, `CompleteFromXmlDocs` :221+).

**Factory**: `CreateEmptyCLIObject` (:575), `CreateInstance(type)` (:588), `CreateInstance(Of T)` (:600), `CreateInstance(assmPath)` (:615-632, loads assembly, finds type with `RunDllEntryPoint`).

**Help output**: `Help(CommandName)` (:406-422) — empty name → `HelpSummary(False)` (:414), else `PrintCommandHelp` (`View\CommandHelp.vb:72`). `SDKdocs()` (:362-364) → `MarkdownDoc` (`View\SDKManual.vb:133-203`) assembles assembly title, `App.HelpSummary(True)`, per-command `APIEntryPoint.HelpInformation(md:=True)` (:174-199). `APIEntryPoint.HelpInformation` (`Reflection\EntryPoints\APIEntryPoint.vb:175-191`) appends `argumentNote` (:193-221) and `NoteAttribute.noteText` (:182-188).
