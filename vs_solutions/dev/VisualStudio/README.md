# Visual Studio Solution, VBProject And Source Code Analysis Toolkit

Parses and writes Visual Studio solutions and VB.NET projects, builds VB source symbol trees, maps compiled assemblies and decodes IL for sciBASIC# code tooling.

## Overview
- Solution parsing/writing for both the classic text `.sln` format and the new XML `.slnx` format, with project trees, configurations and solution workspaces.
- VB.NET source analysis: a scanner plus recursive-descent parser producing namespace/class/module/member/variable symbol trees, and a `.vbproj` model exposing references, build configurations and NuGet metadata.
- Compiled-asset analysis: read-only metadata reflection mapping an assembly into the same symbol model, an IL reader/decompiler (CFG, SSA, structure recovery, syntax writer) and source-map (base64 VLQ) decoding.
- Project tooling helpers: code statistics, license banner insertion/removal, resx/xsd models and git/svn log and diff parsing.

## Key Types
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln.Solution` — unified solution model with `Load`, `Save` and project/configuration queries.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln.File.Parser` — entry parser dispatching to the legacy `.sln` or `.slnx` reader.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln.File.SlnxWriter` — writes a solution model back out as `.slnx`.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.Project.VBProject` — `vbproj` file model (documents, references, configurations).
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.CodeDOM.Syntax.VBParser` — recursive descent parser turning VB.NET source text into a symbol tree.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.Reflection.AssemblySymbolLoader` — maps a compiled assembly into the VB symbol tree by metadata reflection.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL.Decompiler.MethodDecompiler` — decodes a method IL body into an expression/statement syntax tree.
- `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.SourceMap.sourceMap` — source-map encode/decode (base64 VLQ mappings).

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.CodeDOM.Syntax

Dim sln As Solution = Solution.Load("D:\repo\MyApp.sln")

For Each p In sln.Projects
    Console.WriteLine($"{p.Name} -> {p.RelativePath}")
Next

Dim tree = VBParser.Parse(IO.File.ReadAllText("Module1.vb"))
```

## Package
- Assembly: `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio`
- TargetFramework: `net10.0`
- Tags: `scibasic;visual-studio;solution-parser;code-analysis;vbnet`

## License
GPL-3.0-or-later
