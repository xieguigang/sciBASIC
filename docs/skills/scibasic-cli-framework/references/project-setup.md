# Project Setup

Reference: `g:\GCModeller\src\R-sharp\Rscript\Rscript.NET5.vbproj` and `g:\GCModeller\src\GCModeller\engine\vCella\`.

## Core Project Reference

Every CLI program must reference the core runtime library that provides the framework:

```xml
<ProjectReference Include="..\..\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\Core.vbproj" />
```

This project provides `Microsoft.VisualBasic.CommandLine.*` (`RunCLI`, `CommandLine`, `ExportAPI`/`Usage`/`Argument` attributes). Adjust the relative path to the actual location of `Core.vbproj` at `g:\GCModeller\src\runtime\sciBASIC#\Microsoft.VisualBasic.Core\src\Core.vbproj`.

## Essential `.vbproj` Properties (Rscript example, lines 4-12)

```xml
<OutputType>Exe</OutputType>
<RootNamespace>Rsharp.NetCore5</RootNamespace>
<TargetFrameworks>net10.0</TargetFrameworks>
<AssemblyName>Rscript</AssemblyName>
<StartupObject>Rsharp.NetCore5.Program</StartupObject>
<RuntimeIdentifiers>win-x64;linux-x64;linux-arm64;osx-x64</RuntimeIdentifiers>
```

- `OutputType` must be `Exe`.
- `StartupObject` must point to the module containing `Main` (fully qualified with `RootNamespace`).
- `TargetFrameworks` / `TargetFramework` should match the solution's runtime (vCella targets .NET; Rscript here targets `net10.0`).
- `RuntimeIdentifiers` may list the desired deployment targets, especially if the program will use `/linux-bash` on Linux.

Other typical references seen in Rscript (`Rscript.NET5.vbproj:585-606`):

```xml
<PackageReference Include="SkiaSharp" Version="4.157.0" />
<PackageReference Include="System.Configuration.ConfigurationManager" Version="11.0.0" />
<ProjectReference Include="..\..\runtime\Darwinism\src\networking\networking.vbproj" />
<ProjectReference Include="..\R#\Rsharp-netcore5.vbproj" />
```

Only the `Core.vbproj` reference and framework-appropriate packages are required for a minimal CLI.

## Imports

A CLI module typically starts with:

```vbnet
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine
Imports System.ComponentModel   ' for <Description> in v1 style
```

`GetType(MyCLI).RunCLI(...)` and `App.CommandLine` come from `Microsoft.VisualBasic` (`ApplicationServices\App.vb`). `CommandLine`, `ExportAPI`, `Argument`, `Usage`, `CLITypes`, `PipelineTypes` come from `Microsoft.VisualBasic.CommandLine`.

## Container and Entry Point Placement

- Keep the container `Module` (with all `<ExportAPI>` subcommands) in `Program.vb` for v2 (vCella), or split across `Partial Module` files under `CLI\` for v1 (Rscript).
- Define `Main` in the module named by `<StartupObject>`. Two common forms:
  - `Public Function Main(args As String()) As Integer` → pass `args` to `RunCLI(args, ...)` or ignore it and use `App.CommandLine` (vCella).
  - `Public Function Main() As Integer` → use `App.CommandLine` (Rscript, R-terminal).
- Return `Integer` from `Main`.

## Startup Behavior to Preserve

Because the framework prints help on no arguments and provides hidden commands, avoid pre-parsing `args` in `Main` in a way that bypasses `RunCLI`. Let `RunCLI` own parsing so that `?`, `??`, `man`, `/i`, `executeFile`, `executeEmpty`, etc. continue to work.
