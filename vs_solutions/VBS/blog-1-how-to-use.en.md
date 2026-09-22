# Running VB.NET Scripts Directly with vbs: A User Guide to the VBS Script Engine (How to Use)

> This is the first article in the VBS script engine series, covering **how to use** the engine to run VB.NET scripts.
> For how the engine works internally, see the second article: *How the VBS Script Engine Runs a VB.NET Script (How to Implement)*.
> All examples in this article come from the repository's `tutorials/VBS/scripts/` folder and can be run directly.

## 1. What is VBS?

`vbs` is a **VB.NET script engine host program** written in VB.NET (source code lives in `vs_solutions/VBS/VBS.vbproj`). It lets you run a `.vb` script file directly, without:

- Creating a project in Visual Studio first;
- Manually managing `.vbproj`, references, and NuGet dependencies;
- Compiling to an exe before running.

You just write a `.vb` file and run:

```bash
vbs ./run.vb
```

At runtime, the engine parses and restructures the script source code textually, compiles it in memory into an assembly via Roslyn, and executes it through reflection — nothing is ever written to disk.

On top of standard VB.NET syntax, the engine provides a series of syntax extensions that make scripts feel more "scripty":

- **Top-level statements**: code can be written directly at the top level of the file, no `Sub Main` wrapper needed;
- **Command-line argument syntax**: `?"--a"` reads an argument in one line;
- **`#include` directive**: reference DLLs, other scripts, and even NuGet packages;
- **`let` dynamic typing**: Python-like dynamic variables;
- **Tuple destructuring**: `Dim (a, b) = GetTuple()`;
- **Default parameter expressions**: optional parameter defaults can be arbitrary expressions;
- **Vectorized computation**: write `x * y + 1` on numeric arrays and the engine automatically expands it into SIMD element-wise operations;
- **`@` array projection**: `list@x` projects a member of an object array into a new array (Perl style);
- **`print` data printing**: prints vectors and tables in GNU R style.

## 2. Building and Running

### 2.1 Build

```bash
cd G:\GCModeller\src\runtime\sciBASIC#
dotnet build vs_solutions/VBS/VBS.vbproj -c Release
```

The build output goes to `.nuget\net10.0\`, and the entry program is `vbs.exe`.

### 2.2 Run Your First Script

```bash
cd .nuget\net10.0
vbs.exe G:\GCModeller\src\runtime\sciBASIC#\tutorials\VBS\scripts\tuple\tuple.vb
```

Command-line format:

```bash
vbs.exe <script path> [--name=value ...] [--verbose] [--no-vectorize]
```

| Option | Description |
|--------|-------------|
| `--verbose` | Print the full refactored, compilable code to the console and compile in Debug mode. Very handy for troubleshooting syntax issues |
| `--no-vectorize` | Disable automatic vectorization rewriting of numeric vectors |
| `make-project <script.vb>` | Sub-command: convert a script in place into a proper vbproj project |

## 3. A Minimal Runnable Script

Start with a minimal hello world (top-level statements):

```vbnet
' hello.vb — no Module, no Sub Main needed
Call Console.WriteLine("hello world!")
```

Run it:

```bash
vbs hello.vb
```

This is the biggest difference between VBS and a "proper" VB.NET program: **executable statements can be written directly at the top level of the file**. In standard VB.NET, statements must live inside a type container (e.g. `Sub Main` inside a `Module`); in VBS, the engine automatically places these top-level statements into a virtual `Main` method during parsing.

## 4. Reading Command-Line Arguments

Scripts can read command-line arguments directly with the `?"--name"` syntax:

```vbnet
' vbs ./run.vb --a=123 --flag
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"

Call Console.WriteLine($"a = {A}, flag = {B}")
```

Run:

```bash
vbs run.vb --a=123 --flag
```

`?"--a"` is essentially a reference to the `args` variable of the virtual `Main(args As CommandLine)` method created by the engine; during preprocessing it is rewritten to `args("--a")`. `CommandLine` performs type conversion automatically based on the declared type (numbers, boolean switches, etc.) — the `--flag` switch above is parsed as a `Boolean`.

## 5. Top-Level Functions

Top-level `Function` / `Sub` definitions have no type container, so the engine automatically rewrites them as local anonymous functions inside `Main`:

```vbnet
Dim msg = "hello"

Public Function HelloWorld As String
    Return msg & " world!"
End Function

Public Sub Foo(a As Integer)
    Call Console.WriteLine(a)
End Sub

Call Console.WriteLine(HelloWorld())
Call Foo(123)
```

Don't worry about declaration order — the engine computes the insertion position of each anonymous function inside `Main` based on the top-level variables it captures and the other top-level functions it calls, so the code above can be written in any order.

## 6. Referencing External Dependencies (#include)

The `#include` preprocessor directive supports three kinds of targets, all with the unified syntax `#include "<target>"`.

### 6.1 Referencing External DLL Assemblies

```vbnet
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "./libs/mylib.dll"    ' relative paths work too
```

Paths may be absolute or relative. Relative paths are probed in the following directories, in order:

1. The folder containing the script file;
2. The engine program directory `App.HOME`;
3. `App.HOME/libs`;
4. `libs/` under the parent of `App.HOME`.

A complete example — `tutorials/VBS/scripts/linear_regression/linear_regression.vb`:

```vbnet
#include "Microsoft.VisualBasic.Data.Bootstrapping.Fittings.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Drawing.dll"

imports Microsoft.VisualBasic.Data
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

' 1. Build a noisy linear dataset y = 2x + 3 + noise
dim n = 100
dim y(n - 1) as double

for i = 0 to n - 1
    dim x = i * 0.1
    dim noise = ((i mod 7) - 3) * 0.1
    y(i) = 2.0 * x + 3.0 + noise
next

' 2. Fit, predict, plot, export csv
dim model = table.LinearFit(y := "y")
call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Plot(DataSerials(xs, ys, class_id).tolist())
    plt.SavePng(here("linear-regression.png"), 300)
End Using
```

Running it produces the regression fit chart `linear-regression.png` and the result table `linear-regression.csv`. As you can see, a script can run a complete data science pipeline: "read data → machine learning → visualization → export files".

### 6.2 Including Other VB Scripts

When the `#include` target ends with `.vb`, the target script's code is copied directly into the top-level namespace of the main script:

```vbnet
' ./lib/Helper.vb
Public Class LibraryHelper
    Public Shared Function Greet(name As String) As String
        Return $"hello, {name}!"
    End Function
End Class
```

```vbnet
' main script main.vb
#include "./lib/Helper.vb"

Call Console.WriteLine(LibraryHelper.Greet("vbs"))
```

Key rules:

- An included script may `#include` further scripts (the engine expands recursively with deduplication; circular references produce an error);
- Included scripts should only contain `Imports` and type definitions — top-level executable statements, top-level functions, magic methods, and `#package`-style metadata directives are not allowed; violations are reported with the file and reason.

### 6.3 Referencing NuGet Packages

```vbnet
#include "Newtonsoft.Json"               ' latest stable version
#include "Newtonsoft.Json@13.0.3"        ' exact version
#include "Microsoft.Extensions.Logging@[8.0,9.0)"   ' version range
```

NuGet referencing capabilities include:

- **Automatic transitive dependency resolution**: breadth-first expansion following the nuspec dependency groups;
- **Version conflict resolution**: when the same package is required in multiple places, the lowest version satisfying all constraints wins;
- **Local caching**: packages are extracted to `~/.nuget/packages/`, matching the official NuGet layout, so packages already installed by Visual Studio / `dotnet restore` are reused directly; cache hits cost zero network traffic.

See `test/test_include_nuget.vb` for an example.

## 7. Assembly Metadata Directives

The script header can declare metadata for the dynamically compiled assembly:

```vbnet
#package "MyScript.Package"     ' assembly name
#author  "xieguigang"           ' AssemblyCompany
#title   "My First Script"      ' AssemblyTitle
#version "1.2.3.4"              ' AssemblyVersion
```

These values can be read back in the script via magic methods (next section).

## 8. Magic Methods

During preprocessing, the engine bakes script context information (script path, metadata, dependency search directories) into the generated code as constants. Scripts can call these without any imports:

| Group | Method | Description |
|-------|--------|-------------|
| Script context | `ScriptDir()` | Folder containing the script |
| | `ScriptFile()` | Absolute path of the script |
| | `ScriptName()` | Script file name |
| | `Here(relpath)` | Resolve a relative path against the script folder |
| | `ScriptText()` / `ScriptLines()` | Read the script's own source |
| | `Self()` | The Assembly compiled from the script itself |
| Metadata | `Package()` / `Author()` / `Title()` / `Version()` / `Meta(key)` | Read back the `#` directive values |
| Dependencies | `Includes()` | Absolute paths of all `#include` assemblies |
| | `Locate(name)` | Locate a file using the same search order as `#include` |

The most commonly used is `Here()` — when a script needs to read/write data files in its own directory:

```vbnet
dim file = here("../../data/bezdekIris.csv")
dim table = NumericTableIO.ReadCsv(file, columns := {"D1","D2","D3","D4"})
```

That line opens `tutorials/VBS/scripts/kmeans/kmeans.vb`: it loads the classic Iris dataset, runs KMeans clustering + PCA dimension reduction + scatter plot visualization.

## 9. `let` Dynamic Typing

With a `Dim` declaration, Roslyn infers the type automatically, giving you a strongly typed variable. A `let` declaration is preprocessed by the engine into an `Object` declaration, and the variable can be rebound to any type at runtime with dynamic member access:

```vbnet
Dim strong = "hello"     ' inferred as String, strongly typed
let dynamic = "hello"    ' preprocessed to Dim dynamic As Object = "hello"
```

Full example (`tutorials/VBS/scripts/dynamic_type/dynamic_type.vb`):

```vbnet
Dim a as integer
a = 123
console.writeline($"a := {a}")

let b = 456
console.writeline($"b := {b}")

' b is rebound to an anonymous type, with dynamic field access
b = new with {.a = 123, .b = 456}

console.writeline($"b.a := {b.a}")
console.writeline($"b.b := {b.b}")

' rebound once more, this time as a Boolean
b = false

console.writeline($"check boolean := {b}")
```

Output:

```text
a := 123
b := 456
b.a := 123
b.b := 456
check boolean := False
```

One variable acts as a number, then an object, then a boolean — that's the dynamic typing experience `let` provides. The engine automatically distinguishes `Let` clauses inside LINQ query expressions, so queries are never rewritten by mistake.

## 10. Tuple Destructuring

Scripts support tuple destructuring syntax, including nested destructuring and destructuring inside `For Each`. Full example (`tutorials/VBS/scripts/tuple/tuple.vb`):

```vbnet
' Deconstruct a tuple into two variables in one line (type annotations allowed)
Dim (a, b as double) = (1, 2)

console.writeline($"a := {a}")
console.writeline($"b := {b}")
console.writeline($"(a+b) := {a + b}")

' An array literal of tuples
dim tuples = {
    ("a", 123), ("b", 456), ("c", 789)
}

' Deconstruct each item while iterating
for each (str as string, int) in tuples
    call console.writeline($"{str} => {int:F4}")
next
```

Output:

```text
demo test result of variable tuple deconstruct in vb:
a := 1
b := 2
(a+b) := 3
a => 123.0000
b => 456.0000
c => 789.0000
```

Other supported forms: skip-position destructuring `Dim (a, , c) = GetTuple()`, nested destructuring `Dim (p, (q, r)) = GetNested()`, and named aliases `Dim (name:=v1, score:=v2) = GetTuple()`.

## 11. Vectorized Computation

This is VBS's most distinctive feature: **when variables declared as numeric arrays participate in math operations, the engine automatically expands scalar notation into equivalent element-wise SIMD calls** — script authors never need to hand-write `For` loops:

```vbnet
Dim x = {1, 2, 3, 4, 5}
Dim y = {2, 3, 4, 5, 1}

Dim sum = x + 5                 ' => VecAddScalar(Of Integer)(x, 5)
Dim z = (x * y + 6) / (x + y)   ' the whole expression tree expanded at once
```

With `--verbose` you can see the refactoring result:

```vbnet
Dim sum = VecAddScalar(Of Integer)(x, 5)
Dim z = VecDivide(
            VecConvert(Of Integer, Double)(VecAddScalar(Of Integer)(VecMultiply(Of Integer)(x, y), 6)),
            VecConvert(Of Integer, Double)(VecAdd(Of Integer)(x, y)))
```

Key points:

- Supported element types: one-dimensional numeric arrays of `Short` / `Integer` / `Long` / `Single` / `Double`;
- Type promotion follows VB element-wise semantics, e.g. `Integer() / Integer()` yields `Double()`, and `^` always yields `Double`;
- Math functions are vectorized too: `Math.Sqrt(v)` → `VecSqrt`, `Math.Abs(v)` → `VecAbs`;
- Aggregations: `x.Sum()`, `x.Average()`, `x.Min()`, `x.Max()`, `x.Product()`;
- The backend is hardware-accelerated via `System.Numerics.Vector` (SSE2/AVX/ARM SIMD);
- Whenever the engine is unsure how to rewrite an expression it conservatively gives up (keeps the original code) — it never generates speculative code;
- Can be disabled with the `--no-vectorize` command-line switch or a `#no-vectorize` directive in the script header.

## 12. The `@` Array Projection Operator

For **object arrays**, the Perl-style `@` operator projects a member directly into a new array:

```vbnet
Class CLRObjectType
    Property x As Double
    Property y As String
End Class

Dim list As CLRObjectType() = { ... }

Dim x = list@x          ' => Double()
Dim y = list@y          ' => String()

' Projection results plug straight into vectorization
Dim z = list@x + {2, 3, 4, 5, 6, 7, 8, 9}
```

`list@x` is equivalent to `list.Select(Function(o) o.x).ToArray()`, but it's first-class syntactic sugar, and the projected array can immediately participate in vectorized operations. Multi-member projection `list@{x, y}` produces an anonymous-type array; `list@inner@x` supports chained step-by-step projection.

## 13. `print` Data Printing

Scripts can call the engine-injected `print(...)` directly, which formats data in **GNU R style** — very handy for debugging:

```vbnet
Dim x = {1, 3, 24, 23, 4, 23}
Call print(x)
' [1]  1  3 24 23  4 23

Call print(42)          ' [1] 42
Call print(1.0 / 3)     ' [1] 0.3333333
Call print("done")      ' [1] "done"
```

The bracketed number is the index of the first element on the line (matching R); lines wrap automatically beyond the width limit (80 chars by default); `width` and `digits` are optional parameters; `Nothing` elements print as `NA`; a `NumericTable` (equivalent to an R data.frame) prints as a bordered table.

## 14. Default Parameter Expressions

Standard VB.NET only allows constants as optional parameter defaults; VBS relaxes this to **arbitrary expressions**:

```vbnet
Dim prefix = "[vbs] "

Function test(a As String, Optional b As Boolean = True,
              Optional c As testdata = If(b, New testdata(a), New testdata("default"))) As String
    Return c.text
End Function

Call print(test(a:="hello"))            ' => "hello"
Call print(test(a:="xxxxx", b:=False))  ' => "default"
```

Default value expressions may reference earlier parameters, module-level variables, `New` constructions, and function calls.

## 15. Converting to a Proper Project (make-project)

Once a script is debugged, the `make-project` sub-command converts it **in place** into a proper VB.NET project:

```bash
vbs make-project ./run.vb [--verbose] [--no-build] [--force]
```

Output (the original script is kept as a backup):

```
run.vb               ' original script (backup)
run.vbproj           ' generated SDK-style project
src/
 ├── Program.vb            ' refactored top-level statements/functions/type definitions
 ├── VBScriptHostMagics.vb ' materialized magic methods as plain source
 └── Helper.vb             ' one file per #include'd script
```

Conversion examples: top-level statements go into `Main`; top-level functions become module-level `Private Function`s; captured top-level variables are promoted to fields; `#include "pkg@ver"` becomes `<PackageReference>`; the `#package` family of directives maps to project properties. A `dotnet build` verification runs by default after generation.

In other words: **prototype quickly with scripts → one-click promotion to a proper project once verified** — a very smooth development workflow by design.

## 16. Demo Script Tour

Every sub-folder in `tutorials/VBS/scripts/` is a complete, runnable demo; most include `stdout.txt` (expected output) and generated images:

| Demo | Content | Features demonstrated |
|------|---------|----------------------|
| `tuple` | Tuple destructuring basics | Tuple syntax |
| `dynamic_type` | Dynamic variable rebinding | `let` dynamic typing |
| `linear_regression` | Linear regression + fit chart export | `#include` dll, `here()`, data science pipeline |
| `kmeans` | Iris clustering + PCA + scatter plot | `NumericTable`, `here()` data loading |
| `word2vec` | Word vector training + UMAP + KMeans | Full NLP + ML pipeline |
| `hola_layout` | HOLA orthogonal network layout | Complex type definitions, top-level functions, graphics rendering |
| `mnist_cnn` / `spiking_nn` / `tf-idf` / `hierarchical_clustering` / `mnist_umap` | Deep learning & statistical learning examples | Large `#include` dependency management |

Take `kmeans` as an example:

```bash
cd .nuget\net10.0
vbs.exe G:\GCModeller\src\runtime\sciBASIC#\tutorials\VBS\scripts\kmeans\kmeans.vb
```

After running, you'll find the PCA scatter plot `bezdekIris-pca-groups.png` and the cluster assignment `bezdekIris-kmeans.csv` in the script folder. All other demos run the same way.

## 17. Embedding as a Library

Besides the `vbs` command-line host, `VBS.vbproj` can also be referenced as a class library by other .NET programs to embed script execution:

```vbnet
Imports VBScriptHost.Script

' 1. Parse the script
Dim vbs As ScriptParseResult = VBScript.ParseScript("./run.vb")

' 2. Compile into an in-memory assembly
Using runtime As ScriptRuntime = vbs.CompileScript(
    asmName:="MyScript",
    extraRefs:={"./extra.dll"})

    ' 3. Execute; the return value is the script's exit code
    Dim exitCode As Integer = runtime.Run({"--a", "123"})
End Using   ' after Dispose, the dynamically loaded assembly is unloaded
```

## 18. Troubleshooting Tips

- **Compile errors unclear?** Run with `--verbose` to print the full refactored, compilable code — the reported line numbers refer to this generated code;
- **`#include` file not found?** Remember relative paths resolve against the script file's directory; you can also verify the search order with the `Locate(name)` magic method;
- **Vectorization not kicking in?** `--verbose` prints the rewrite count, recognized vector variables, and the list of lines that referenced known vectors but couldn't be rewritten;
- **Need traditional VB.NET semantics?** Use `--no-vectorize` to disable SIMD rewriting (`@` projection still applies).

## Summary

VBS turns VB.NET into a language you can "open and write" like Python or R, while fully retaining everything the .NET ecosystem offers: NuGet packages, external DLLs, strong typing, LINQ, async... Combined with syntax sugar such as vectorization, dynamic typing, and tuple destructuring, it's a great fit for data analysis, scientific computing, and rapid prototyping. And once a script is debugged, `make-project` promotes it to a proper project in one step.

In the next article, we'll go inside the engine to see how it turns a "not-quite-VB.NET script" into an assembly compiled and executed in memory.
