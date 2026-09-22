# How the VBS Script Engine Runs a VB.NET Script (How to Implement)

> This is the second article in the VBS script engine series. From a **code implementation** perspective, it dissects how the engine in `vs_solutions/VBS/VBS.vbproj` runs a VB.NET script, and how the new syntax features — vectorization, dynamic typing, tuple destructuring, default parameter expressions — are implemented.
> For usage, see the first article: *Running VB.NET Scripts Directly with vbs: A User Guide*.

## 1. Overall Architecture: A Four-Stage Pipeline

Here is the global view. From the moment you type `vbs ./run.vb` until the script finishes, the pipeline has four stages:

```
 .vb source file
      │
      ▼
 ┌──────────────────────────────────────────────────┐
 │ ① Parse  ParseScript (VBScript.vb)                │
 │    - #include resolution (dll/scripts/nuget)       │
 │    - metadata directives (#package/#author/...)    │
 │    - textual preprocessing (sugar expansion,       │
 │      vectorization rewriting)                      │
 └──────────────────────────────────────────────────┘
      │  ScriptParseResult (preprocessed code + deps)
      ▼
 ┌──────────────────────────────────────────────────┐
 │ ② Restructure  ScriptRefactor                     │
 │    - line-by-line block scanning                  │
 │      (ScriptStructure.Scan)                       │
 │    - top-level functions → anonymous functions    │
 │      + slot resolution                            │
 │    - assemble the Namespace+Module+Main container │
 └──────────────────────────────────────────────────┘
      │  GeneratedCode (complete compilable VB.NET)
      ▼
 ┌──────────────────────────────────────────────────┐
 │ ③ In-memory compile  DynamicDll.CompileScript     │
 │    - Roslyn: parse → compile → emit               │
 │    - IL/PDB emitted into MemoryStreams            │
 └──────────────────────────────────────────────────┘
      │  Assembly (exists only in memory)
      ▼
 ┌──────────────────────────────────────────────────┐
 │ ④ Reflect & run  ScriptRuntime.Run                │
 │    - ScriptLoadContext collectible load context   │
 │    - reflection call DynamicDll.Program.Main      │
 │    - Unload the whole context on Dispose          │
 └──────────────────────────────────────────────────┘
```

Mapped to source files, each module's responsibility is:

| File | Responsibility |
|------|----------------|
| `Program.vb` | CLI entry: sub-command dispatch, wire parse → compile → run |
| `src/VBScript/VBScript.vb` | Parse entry: `#include` → metadata → preprocessing → refactoring |
| `src/VBScript/IncludeDirective/` | `#include` directive model and resolver (three target kinds, recursive expansion) |
| `src/VBScript/ScriptStructure.vb` | Static structure model and line-by-line block scanner |
| `src/VBScript/Syntax/ScriptRefactor.vb` | Textual preprocessing + runtime code emitter |
| `src/VBScript/Syntax/LetStatement.vb` | `let` dynamic type expansion |
| `src/VBScript/Syntax/TupleDestructuring.vb` | Tuple destructuring expansion |
| `src/VBScript/Syntax/DefaultParameterExpression.vb` | Default parameter expression rewriting |
| `src/VBScript/Syntax/Vectorization/` | Vectorization rewriter + `@` projection |
| `src/DynamicDll.vb` | Roslyn in-memory compilation and reflection invocation |
| `src/VBScript/ScriptRuntime.vb` | Runtime wrapper: execution and unloading |

The entry point `Program.vb` is very thin:

```vbnet
Public Function Main(args As String()) As Integer
    ' Sub-command dispatch: any first token other than make-project
    ' is treated as a script file path
    If String.Equals(args(0), "make-project", StringComparison.OrdinalIgnoreCase) Then
        Return MakeProject.Run(args)
    End If

    Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
    Dim scriptFile As String = args(0)
    Dim verbose As Boolean = cmdl("--verbose")
    Dim vectorize As Boolean = Not cmdl("--no-vectorize")

    Dim vbs As ScriptParseResult = VBScript.ParseScript(scriptFile, verbose:=verbose, vectorize:=vectorize)

    Using script As ScriptRuntime = vbs.CompileScript(debug:=verbose)
        Return script.Run(args)   ' the return value is the process exit code
    End Using
End Function
```

The core design decision: **the script is not "interpreted" — it is restructured into valid VB.NET source and handed to the real compiler.** The engine itself only does text-level front-end processing; the back-end is entirely Roslyn, so semantics stay maximally consistent with VB.NET.

## 2. Stage One: Parsing (ParseScript)

`VBScript.ParseScript` is the parse-stage entry point and does three things:

### 2.1 #include Resolution (IncludeResolver)

`#include` supports three kinds of targets with a simple discrimination rule:

| Target form | Kind (`IncludeKind`) | Handling |
|-------------|----------|----------|
| `xxx.dll` | Assembly | Resolved to an absolute path, added to compile references |
| `xxx.vb` | Other script | Read its source, extract its `Imports` and type definition blocks, recursively expand its own `#include`s |
| `pkg@version` | NuGet package | Download/reuse cache, resolve transitive dependencies, collect lib assets |

Script includes are expanded recursively with deduplication (circular references produce an error). The included script's type definition blocks (`IncludeSet.TypeBlocks`) are injected into the main script's top-level namespace — note this is "copy the source in place," not compiling it as a separate module, so included scripts may only contain `Imports` and type definitions.

DLL paths are probed across a search-directory list: script directory → engine directory → `libs/` directories. NuGet packages go through a separate `VBProj.NuGet` namespace (a self-contained NuGet client: `NuGetClient` for version indexing and download, `NuGetResolver` for dependency expansion and version conflict resolution, `NuGetVersion`/`NuGetFramework` for version and framework models), reusing the `~/.nuget/packages/` global cache.

### 2.2 Metadata Parsing (ScriptMetadata)

The four directives `#package/#author/#title/#version` are parsed into a `ScriptMetadata` object, which later serves both as the assembly name for Roslyn compilation and as the source for emitted assembly-level attributes (`AssemblyCompanyAttribute` etc.).

### 2.3 Textual Preprocessing (PreprocessText)

This is where all **syntax extensions** happen. `ScriptRefactor.PreprocessText` is a purely textual transformation pipeline:

```vbnet
Public Shared Function PreprocessText(source As String, ...) As String
    ' remove #include lines
    Dim code As String = Regex.Replace(source, "^\s*#include\s+""[^""]*""\s*$", "", ...)

    ' ?"--a" => args("--a")
    code = Regex.Replace(code, "\?""(?<name>[^""]+)""", "args(""${name}"")")

    ' let x = ... => Dim x As Object = ... (skip Let clauses in LINQ queries)
    code = LetStatement.Expand(code)

    ' Dim (a, b) = ... => several independent Dim statements
    code = TupleDestructuring.Expand(code)

    ' non-constant default parameters: bridge functions for Functions,
    ' inline expansion at Sub call sites
    code = DefaultParameterExpression.Expand(code, defaultReport)

    ' @ projection expansion + numeric vector arithmetic => Vec* SIMD calls
    code = Vectorization.Expand(code, enabled:=vectorize, report:=report, ...)
    Return code
End Function
```

The order matters: default parameter expansion **must run before** vectorization, so that generated bridge function bodies and temporary variable lines can also be processed by `@` projection and SIMD rewriting.

## 3. How the New Syntax Is Implemented

Before structural refactoring, let's dissect four syntax extensions. They all happen during preprocessing and share one essence: **rewriting non-standard VB.NET syntax into valid VB.NET syntax**.

### 3.1 `let` Dynamic Typing (LetStatement.vb)

The simplest one: rewrite `let name = expr` into `Dim name As Object = expr`. Runtime "dynamic member access" (`b.a`, `b.b`) requires nothing from the engine — under VB's `Option Strict Off`, member access on an `Object` variable is naturally late-bound.

The only technically interesting part is **not rewriting `Let` clauses inside LINQ queries by mistake**:

```vbnet
Dim squares = From n In numbers
              Let sq = n * n     ' this Let must NOT be rewritten!
              Where sq > 9
              Select sq
```

`LetStatement.Expand` solves this with a line-by-line **query state machine**: `From ... In ...` enters query state; a line starting with `Select`/`Group` exits it; the `let` rewrite only happens outside query state. To keep natural-language strings containing "from ... in ..." from confusing the state machine, detection first strips string literal contents and comments (`DetectText`).

### 3.2 Tuple Destructuring (TupleDestructuring.vb)

`Dim (a, b as double) = (1, 2)` is expanded into independent `Dim` statements:

```vbnet
Dim __tuple2 = (1, 2)
Dim a = __tuple2.Item1
Dim b As Double = __tuple2.Item2
```

Supported forms include skip positions (`Dim (a, , c)`: skipped positions emit no declaration), nesting (`Dim (p, (q, r))`: recursive expansion), named aliases (`Dim (name:=v1)`), and iteration destructuring `For Each (str, int) in tuples`.

### 3.3 Default Parameter Expressions (DefaultParameterExpression.vb + DefaultParameterSignature.vb)

VB only allows constants as `Optional` parameter defaults, but scripts want to write:

```vbnet
Function test(a As String, Optional b As Boolean = True,
              Optional c As testdata = If(b, New testdata(a), New testdata("default")))
```

The default expression references earlier parameters `b` and `a` — which VB syntax forbids. The engine's solution has two parts:

**Declaration rewriting**: parameters with defaults lose `Optional` and `= default`, becoming required parameters. Why is removing `Optional` mandatory? On the runtime path, top-level functions are rewritten as anonymous functions inside `Main`, and VB does not allow lambda parameters declared `Optional` (`BC33010`).

**Call-site rewriting**, split by whether the callee is a Function or a Sub:

- **Function path — bridge functions**: for each distinct "subset of actually provided arguments", generate a bridge function; call sites only replace the callee's name:

```vbnet
Call print(test(a:="xxxxx", b:=False))
' => Call print(test_defaults(__vbs_test_a:="xxxxx", __vbs_test_b:=False))

' bridge function appended after the original function:
Function test_defaults(__vbs_test_a As String, __vbs_test_b As Boolean) As String
    Dim __vbs_test_c = If(__vbs_test_b, New testdata(__vbs_test_a), New testdata("default"))
    Return test(__vbs_test_a, __vbs_test_b, __vbs_test_c)
End Function
```

Because it is a pure name substitution, any expression position (`While test(a, b)`, `If test(...) Then`, nested calls) can be rewritten without changing evaluation order or named-argument semantics.

- **Sub path — inline expansion**: a Sub call is a statement, so it is expanded in place into several temporary-variable lines, avoiding an extra function call on hot paths:

```vbnet
Call emit(a:="inline")
' =>
Dim __vbs_emit_a_2 = "inline"
Dim __vbs_emit_c_3 = New testdata(__vbs_emit_a_2)
Call emit(__vbs_emit_a_2, __vbs_emit_c_3)
```

When the surrounding structure can't hold multiple statements (single-line lambdas `Sub() Call emit(...)`, single-line `If ... Then ...`), it is first expanded into a full block before injection.

The whole rewriter is **text-level** (regex + token substitution; no Roslyn semantic model). Any uncertain situation (`ParamArray`, generic parameter lists, signatures continued across lines, overloaded same-name functions...) is simply skipped — the parameter becomes required and the compiler reports the missing argument instead of failing silently. This embodies an important design philosophy: **better unsupported than wrongly rewritten**.

### 3.4 Vectorized Computation (the Vectorization/ folder)

Vectorization is the most complex syntax extension, involving five cooperating files:

| File | Responsibility |
|------|----------------|
| `Vectorization.vb` | Entry: `#no-vectorize` directive handling, line-by-line driving, rewrite report |
| `VectorType.vb` | Numeric type model and VB element-wise type promotion rules |
| `VectorExpressionRewriter.vb` | Core: Roslyn-based statement parsing with shallow type inference |
| `SimdVocabulary.vb` | Operator/function → `Vec*` call mapping and text emission |
| `PropertyProjection.vb` + `ObjectMemberTable.vb` | `@` projection expansion and member table parsing |

The workflow:

1. **Vector variable registration**: scan `Dim` declarations line by line and identify numeric vectors — `Dim x As Integer()`, `Dim x(10) As Integer`, `Dim x = {1, 2, 3}` (inferred from literals), function parameters of type `Double()`, and vector results produced by earlier rewrites; all are registered into a vector symbol table. Only five element types are supported: `Short/Integer/Long/Single/Double`;
2. **Expression parsing**: parse each line's expression into a syntax tree with Roslyn (single line only; no whole-file semantic model);
3. **Type inference**: perform **shallow** inference on identifiers — look up the vector symbol table and literal types; on unknown identifiers, member access `obj.Value`, or indexed access `x(0)`, give up rewriting the whole line;
4. **Rewrite emission**: look up `SimdVocabulary` by operator semantics, map the expression tree to `Vec*` calls, and write the result back into the original line by character range. Type promotion is decided by `VectorType`; e.g. for `/`, `Single / Single` yields `Single`, everything else yields `Double` (a `VecConvert` is inserted automatically); when element types differ (`Short() + Integer()`), a `VecConvert` is inserted first so the result exactly matches "running the same expression element-wise in scalar VB".

The line `x + 5` is finally rewritten to `VecAddScalar(Of Integer)(x, 5)`. The generated code only gets `Imports Microsoft.VisualBasic.Math.SIMD.Vectorization` when a rewrite actually happened — the vocabulary deliberately uses the `Vec*` prefix in its own sub-namespace to dodge VB's `BC30562` "name is ambiguous" error for same-named members in two imported modules.

The runtime back end is the `Vectorized` module in the sciBASIC runtime, built on `System.Numerics.Vector` (SSE2/AVX/ARM SIMD) and `SimdMath` hardware kernels.

### 3.5 The `@` Array Projection (PropertyProjection.vb + ObjectMemberTable.vb)

Expanding `list@x` requires one crucial piece of information: the type of `x`. The engine uses Roslyn to parse `Class/Structure` definition blocks in the script (including included scripts) and builds a "type name → member name → member type" member table (`ObjectMemberTable`), which decides the element type of `list@x`. Only members with an explicit `As` type clause are registered.

Expansion rules:

| Notation | Expansion |
|----------|-----------|
| `list@x` | `list.Select(Function(__vbs_o) __vbs_o.x).ToArray()` |
| `list@{x, y}` | `...New With {.x = ..., .y = ...}.ToArray()` (anonymous-type array) |
| `list@inner@x` | chained, level-by-level projection |

Expansion is equally conservative: if the left side's element type can't be determined, the type comes from a DLL assembly, the member doesn't exist, or the left side isn't an identifier dot-chain — any single uncertainty means **no speculative code is generated**; the `@` stays as-is and the script reports its usual syntax error. String literals, comments, and `1.5@` (a Decimal type character position) are never mis-expanded either.

## 4. Stage Two: Structural Refactoring (ScriptRefactor + ScriptStructure)

After preprocessing, the code is still a mix of "top-level statements + top-level functions + type definition blocks" and can't compile directly. Structural refactoring has two steps:

### 4.1 Line-by-Line Block Scanning (ScriptStructure.Scan)

`Scan` walks the code line by line with a **block stack**: block-opening keywords (`Class/Structure/Interface/Enum/Module/Function/Sub/If/For/...`) push, matching `End XXX` pops, splitting the code into four kinds of slots:

- **Header statements**: `Imports` and `Option` statements;
- **Type definition blocks**: complete type declaration blocks (kept verbatim);
- **Top-level function blocks**: `Function/Sub` without a type container;
- **Top-level statements**: everything else, entering the `Slots` list in source order.

The scan is text-level and must correctly handle pseudo-keywords inside string literals and comments.

### 4.2 Code Emission (BuildCode)

The emitter assembles the four slot kinds into a **fixed container structure**:

```vbnet
Option Strict Off
Option Explicit On
Option Infer On
Imports ...              ' the script's own Imports + auto-injected common namespaces

<AssemblyAttributes()>   ' attributes generated from #package etc.

Namespace DynamicDll                          ' fixed top-level namespace
    Module VBScriptHostMagics                 ' magic method injection point
        Public Function ScriptDir() As String ...
    End Module

    Module Program
        Public Sub print(data As Object, ...) ' print forwarder
        Public Function Main(args As CommandLine) As Integer
            ' dependency-free anonymous functions come first (slot = -1)
            Dim HelloWorld = Function() As String ... End Function

            ' top-level statements in source order
            Dim A As Integer = args("--a")
            Call Console.WriteLine(new Test With {.A = A})

            ' after each statement, anonymous functions whose
            ' dependencies are now declared
            Dim Foo = Sub(a As Integer) ... End Sub

            Return 0
        End Function
    End Module

    Public Class Test                         ' type blocks as namespace members
        ...
    End Class
End Namespace
```

Several key implementation details:

**Top-level functions → anonymous functions + slot resolution.** VB requires "declare before use", and anonymous functions capture top-level variables and other top-level functions, so they can't simply be dumped at the top of `Main`. The engine computes each function's **dependency closure** (captured variables + called functions) and derives its placement slot (`ResolveFunctionSlots`): functions depending on nothing go before all statements (slot = -1); the rest are inserted after the top-level statement whose completion makes their dependencies declared. Relative source order is preserved between functions.

**Type blocks must not go inside a Module.** VB forbids declaring a `Module` inside another `Module` (`BC30617`), and included scripts often contribute `Module` definitions, so type blocks are always emitted as direct namespace members.

**Magic method baking.** Methods like `ScriptDir()` and `Here()` depend on script context (file path, metadata, search directories). During preprocessing, the engine writes that context directly into the generated `VBScriptHostMagics` module source **as constant literals** — for example, `ScriptDir()`'s body is literally `Return "G:\path\to\script\dir"`. This is far simpler than passing a context object at runtime, and it lets a script's `MakeProject` output run standalone without the engine.

**The print forwarder and name shadowing.** The script's `print` is a forwarder injected by the host into `Module Program`. Why not simply `Imports Microsoft.VisualBasic.Printing`? Because in the generated code's import scope, the name `Print` is already exported by both the VB runtime's `FileSystem.Print` (file-number overloads) and this framework's `CLITools`; VB reports `BC30561` ("name is ambiguous") for same-named members in two imported modules — meaning `print` was already unusable in the script scope. Adding another import would only turn a two-way ambiguity into a three-way one. The fix exploits VB's name lookup order the other way: "members of the type itself" take precedence over "members of imported namespaces". Declaring `print` as a member of the script's own `Module Program` shadows all of the conflicting names at once, and the forwarding target is a fully qualified call to avoid introducing new ambiguity.

## 5. Stage Three: Roslyn In-Memory Compilation (DynamicDll.vb)

At this point the script has become **complete, valid VB.NET source code**. The `DynamicDll.CompileScript` extension method takes over compilation:

### 5.1 Reference Collection and Deduplication

`MetadataReference`s are collected in the following priority order:

1. External assemblies referenced by `#include` (dlls / NuGet assets / dependencies forwarded by included scripts);
2. The engine host's own assembly and the sciBASIC library containing `CommandLine`;
3. The VB runtime;
4. Extra references passed in by the caller;
5. All BCL assemblies currently loaded in the `AppDomain`.

The crucial detail is **deduplication by assembly simple name**: NuGet dependency trees very often contain "another copy of the same-named assembly", and having two copies as `MetadataReference`s causes duplicate-type-definition compile errors. The strategy prefers the version already loaded in the current `AppDomain`.

### 5.2 Compilation and In-Memory Emission

```vbnet
Dim compilation As VisualBasicCompilation = VisualBasicCompilation.Create(
    assemblyName:=finalAsmName,      ' explicit arg > #package directive > script file name
    syntaxTrees:=trees,
    references:=references,
    options:=options)                ' OutputKind.DynamicallyLinkedLibrary

Using ms As New MemoryStream(), pdb As New MemoryStream()
    Dim result As EmitResult = compilation.Emit(ms, pdb)

    If Not result.Success Then
        ' aggregate all Error-severity diagnostics and throw
        Throw New InvalidOperationException("Script compilation failed!" & ...)
    End If

    Call ms.Seek(0, SeekOrigin.Begin)

    Dim ctx As New ScriptLoadContext("script-" & Guid.NewGuid().ToString("N"), script.Imports)
    Return New ScriptRuntime(ctx, ctx.LoadFromStream(ms))
End Using
```

IL and PDB are both emitted into memory streams — **nothing touches disk**. On failure, Roslyn diagnostics are aggregated into a readable error; since the generated code's structure is fixed, reported line numbers map directly onto the code printed by `--verbose` for quick debugging.

## 6. Stage Four: Reflection Execution and Unloading (ScriptRuntime.vb + ScriptLoadContext)

### 6.1 Dependency Resolution in the Load Context

The compiled assembly is loaded through a custom `ScriptLoadContext` (derived from `AssemblyLoadContext`, `isCollectible:=True`). Its `Load` callback resolves the script's **runtime** dependencies in this order:

1. The engine host's own assembly — forcibly shared to guarantee type identity between host and script (otherwise reflection arguments would inevitably split types);
2. Assemblies already loaded in the Default ALC — shared preferentially;
3. Exact simple-name mapping of `#include` files;
4. Probing `<name>.dll` in the folders of the `#include` assemblies, solving "dependencies of dependencies" (transitive dependencies).

Because the probing directories are derived from all `#include` assembly locations, NuGet package extraction directories and their native asset directories are automatically covered — scripts using types from NuGet packages need no extra runtime configuration at all.

### 6.2 Reflection Invocation and Unloading

`ScriptRuntime.Run` does two things: first it reflectively constructs the `CommandLine` argument object visible to the script (also via reflection on `CommandLine.BuildFromArguments`, since that type lives in the shared sciBASIC library — host and script see the same type), then it reflectively invokes `DynamicDll.Program.Main(args As CommandLine)`:

```vbnet
Public Function Run(dynamicAsm As Assembly, args As Object) As Integer
    Dim targetType As Type = dynamicAsm.GetType($"{NameOf(DynamicDll)}.Program")
    Dim methodInfo As MethodInfo = targetType.GetMethod("Main", BindingFlags.Public Or BindingFlags.Static)
    Dim result As Object = methodInfo.Invoke(Nothing, New Object() {args})
    Return CInt(result)
End Function
```

The virtual `Main`'s return value is the process exit code. `ScriptRuntime` implements `IDisposable`; `Dispose` calls `ScriptLoadContext.Unload()` to unload the whole load context — the script assembly and its dependencies become GC-collectible. This matters greatly when VBS is embedded as a library in a host program: repeatedly running scripts does not leak assemblies.

## 7. Two Emission Paths: Runtime and Project

Notably, code emission has **two paths** sharing the same preprocessing and structure scanning:

- **Runtime** (`ScriptRefactor`): top-level functions rewritten as anonymous functions, assembled into `Namespace+Module+Main`, compiled in memory and executed;
- **Project** (`ProjectCodeBuilder`, used by `make-project`): top-level functions restored as module-level `Private Function/Sub`, captured top-level variables promoted to module-level fields, magic methods materialized into a standalone `VBScriptHostMagics.vb` source file, `#include` rewritten into `<Reference HintPath>` / `<PackageReference>` / standalone source files.

`PreprocessText` is a purely textual transformation, so both paths share it entirely — guaranteeing that "how a script behaves at runtime" matches "how the promoted project behaves".

## 8. Design Philosophy

Looking back at the whole implementation, a few design decisions run through everything:

1. **Text-level front end + Roslyn back end.** The engine is not a full compiler front end (no SemanticModel, no dataflow analysis); it only does text-level parsing and rewriting, then hands "generating valid source" — the hardest, most error-prone part — to Roslyn. The host stays tiny, and semantics remain highly consistent with VB.NET;
2. **Conservative rewriting: better missing than wrong.** Every rewriter (vectorization, `@` projection, default parameters) follows the same iron rule: any single uncertain inference aborts the rewrite and keeps the original code. Such code couldn't compile before the extension existed anyway, so there is no behavioral regression — the worst case is "the new syntax didn't kick in", never "wrong code was generated";
3. **Compile-time errors beat silent runtime failures.** Call sites that can't be covered degrade the parameter to required, letting Roslyn report "argument not specified" rather than silently passing `Nothing`;
4. **Reporting and observability.** Every rewriter emits a report (rewrite counts, generated bridge names, skipped lines); combined with `--verbose` printing the generated code, any "why didn't it work" can be located quickly;
5. **Collectible by design.** A collectible `AssemblyLoadContext` plus `IDisposable` means scripts unload completely after execution — well suited for long-running host programs.

## Summary

VBS is essentially a sandwich of **a source-to-source rewriter + a Roslyn in-memory compiler + a collectible reflection executor**:

- Syntax extensions (`let`, tuples, default parameters, vectorization, `@`) are "de-sugared" into standard VB.NET via textual rewriting during preprocessing;
- Top-level code is fitted into the fixed `Namespace+Module+Main` container via block scanning and slot resolution;
- Roslyn does the actual compilation, with IL staying in memory throughout;
- A custom `AssemblyLoadContext` handles runtime dependency resolution and complete unloading after execution.

This design gives VB.NET a "scripting language" experience while fully preserving the performance of a statically compiled language and the whole .NET ecosystem. If you're curious about any module, dive into the sources under `vs_solutions/VBS/src/` — behind every conservative strategy, the comments explain the "why".