# VBS - VisualBasic.NET Scripting Host

`vbs` 是一个 VB.NET 脚本引擎宿主程序。它允许直接运行 `.vb` 脚本文件，而不需要事先创建 VS 工程并编译为 exe：引擎在运行时对脚本源代码做文本解析与结构重构，然后基于 Roslyn 在内存中编译为 assembly 并通过反射执行，全程不落盘。

- 目标框架: `net10.0`
- 输出程序集名称: `vbs`
- 核心依赖: `Microsoft.CodeAnalysis.VisualBasic`(Roslyn 编译器)、`Microsoft.VisualBasic.Core`(sciBASIC 运行时)

## 快速上手

```bash
# 基本用法: 第一个参数为脚本文件路径, 之后的参数会原样传递给脚本
vbs ./run.vb --a=123 --flag

# 调试模式: 在控制台打印重构后生成的完整可编译代码, 并以 Debug 模式编译
vbs ./run.vb --verbose
```

脚本宿主的入口逻辑(`Program.vb`)：

```vbnet
Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
Dim scriptFile As String = args(0)
Dim verbose As Boolean = cmdl("--verbose")
Dim vbs As ScriptParseResult = VBScript.ParseScript(scriptFile, verbose:=verbose)

Using script As ScriptRuntime = vbs.CompileScript(debug:=verbose)
    Return script.Run(args)   ' 返回值即脚本的进程退出码
End Using
```

## 实现原理

脚本引擎会通过脚本文件中的元数据解析，动态编译生成 assembly，具体过程为：引擎会首先从脚本中解析出``#include``元数据，得到引用的程序集。然后通过正则表达式提取出类型定义（class，structure，interface，enum）代码块，处理顶层函数为匿名函数，最后将得到的代码生成下面的固定命名空间以及类型的完整代码。

整个执行流水线分为四个阶段：

1. **解析**(`VBScript.ParseScript`)：读取源码，提取 `#include` 引用的外部程序集，并进行文本预处理（移除 `#include` 行、展开命令行参数语法、展开元组分解语法）；
2. **结构重构**(`ScriptRefactor.Refactor`)：逐行扫描代码，利用块栈分离出类型定义块、顶层函数、顶层控制流块与顶层语句，将顶层函数重写为匿名函数并求解其在 `Main` 中的落位，最终组装为固定容器结构；
3. **内存编译**(`DynamicDll.CompileScript`)：基于 Roslyn 将生成的代码编译为 `DynamicallyLinkedLibrary`，IL 与 PDB 均直接发射到内存流中；
4. **反射执行**(`ScriptRuntime.Run`)：在可回收的 `ScriptLoadContext`(自定义 `AssemblyLoadContext`) 中加载 assembly，通过反射调用 `DynamicDll.Program.Main(args As CommandLine)`，`Dispose` 时卸载整个加载上下文。

例如，从输入的脚本代码：

```vbnet
#include "abc.dll"

' vbs ./run.vb --a=123 --flag
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"

Public Class Test
    Public A As Integer

    Public Function ToString() As String
       ' 从外部dll模块 "abc.dll" 文件中导入
       Return ABC.Internal.Seed(A) 
    End Function
End Class

Call console.writeLine(new Test With {.A = A})

' 顶层函数
Public Function HelloWorld As String
    Return "hello world!"
End Function

If B Then
  Call Console.writeLine(HelloWorld())
End If
```

经过脚本引擎处理后，将会变为下面所示的正式代码，从而能够被正常的编译：

```vbnet
Namespace DynamicDll                                         ' 在脚本引擎中固定的顶层命令空间
    Module Program                                           ' 在脚本引擎中固定的代码容器
       Public Function Main(args As CommandLine) As Integer  ' 在脚本引擎中固定的执行函数
            Dim A As Integer = args("--a")
            Dim B As Boolean = args("--flag")

            Dim HelloWorld = Function() As String            ' 顶层函数会被重构为匿名函数
                                Return "hello world!"
                             End Function

            Call console.writeLine(new Test With {.A = A})

            If B Then
                Call Console.writeLine(HelloWorld())
            End If

            Return 0
       End Function
    End Module

    Public Class Test
        Public A As Integer

        Public Function ToString() As String
            ' imported from "abc.dll"
            Return ABC.Seed(A) 
        End Function
    End Class
End Namespace
```

### 编译引用的收集

`CompileScript` 会按下面的优先级收集 `MetadataReference`（自动去重）：

1. `#include` 所引用的外部程序集（文件不存在时直接抛出 `FileNotFoundException`）；
2. 引擎宿主自身 assembly 与 `CommandLine` 所在的 sciBASIC 库（提供 `CommandLine` 类型定义）；
3. VB 运行时（支持脚本使用 `Microsoft.VisualBasic` 内置函数）；
4. 调用方传入的额外引用程序集；
5. 当前 `AppDomain` 中所有已加载的 BCL 程序集。

### 运行时依赖解析与卸载

编译产物通过自定义的 `ScriptLoadContext`（`isCollectible:=True`）加载，其 `Load` 回调按如下顺序解析脚本运行期的依赖程序集：

1. 引擎宿主自身程序集——强制共享，保证宿主与脚本之间的类型同一性（否则反射传参必然类型分裂）；
2. Default ALC 中已加载的程序集——优先共享返回（`CommandLine` 所在的 sciBASIC 库即属此类）；
3. `#include` 文件的简单名称精确映射；
4. 在 `#include` 所在文件夹中按 `<名称>.dll` 探测，解决"依赖的依赖"这类传递依赖问题。

脚本执行完毕、`ScriptRuntime` 被 `Dispose` 后，`ScriptLoadContext.Unload()` 会卸载整个加载上下文，脚本 assembly 与其依赖均可被 GC 回收。

## 脚本语法

支持的vb.net脚本的语法与正常的vb.net源代码的语法保持一致，除了下面的一些不同：

#### 1. 获取命令行参数

VB.NET脚本可以直接通过下面的语法来获取得到命令行参数：

```vbnet
' vbs ./run.vb --a=123 --flag
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"
```

本质上是脚本引擎所创建的虚拟函数Main方法中的args变量的引用：

```vbnet
Dim A As Integer = args("--a")
Dim B As Boolean = args("--flag")
```

`CommandLine` 会根据命令行自动完成类型转换（数值、布尔开关等），例如上面的 `--flag` 开关会被解析为 `Boolean`。

#### 2. 引用外部.NET CLR DLL程序集文件

可以使用下面的语法来引用dll文件(采用``#include``预编译命令)：

```vbnet
#include "/path/to/assembly.dll"
```

所导入的外部dll程序集的路径可以为绝对路径，或者相对于脚本文件的相对路径。在脚本引擎中会将这里引用的dll路径统一解析为绝对路径。

对于相对路径，引擎会依次在下列目录中探测：

1. 脚本文件所在的文件夹；
2. 引擎程序目录 `App.HOME`；
3. `App.HOME/libs`；
4. `App.HOME` 上级目录下的 `libs/`。

#### 3. 顶层代码语句

对于正规的vb.net代码而言，代码语句（例如函数，方法，表达式）必须要放入到一个类型容器中，例如必须要放入到Class申明代码块之中。而在当前的这个脚本引擎中，代码语句可以直接出现在最顶层，例如

```vbnet
' 顶层语句
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"

' 顶层函数
Public Function HelloWorld As String
    Return "hello world!"
End Function
```

这些顶层表达式，在解析阶段，编译之前会被解析出来，并放入到一个虚拟的Main方法之中。

#### 4. 顶层函数会被重构为匿名函数

顶层 `Function`/`Sub` 定义没有类型容器，因此在重构阶段会被改写为 `Main` 内部的局部匿名函数变量：

```vbnet
' 脚本中的顶层函数
Public Function HelloWorld As String
    Return "hello world!"
End Function

Public Sub Foo(a As Integer)
    Console.WriteLine(a)
End Sub
```

```vbnet
' 重构后 Main 内部的等价代码
Dim HelloWorld = Function() As String
                     Return "hello world!"
                 End Function

Dim Foo = Sub(a As Integer)
              Console.WriteLine(a)
          End Sub
```

由于 VB 要求"先声明后使用"，引擎会根据每个匿名函数所捕获的顶层变量、以及所调用的其它顶层函数，自动求解其在 `Main` 中的插入位置（不依赖任何变量与函数的匿名函数放在所有语句之前），多个函数之间保持源码中的相对顺序。

#### 5. 元组分解语法

脚本支持标准的 VB.NET 元组分解语法，引擎会在预处理阶段将其展开为多条独立的 `Dim` 语句：

```vbnet
Dim (a, b) = GetTuple()                    ' 基本分解
Dim (x As Integer, y As String) = GetT()   ' 带类型声明
Dim (a, , c) = GetTuple()                  ' 跳过中间项
Dim (p, (q, r)) = GetNested()              ' 嵌套分解
Dim (name:=v1, score:=v2) = GetTuple()     ' 命名别名
```

例如 `Dim (a, b) = GetTuple()` 会被展开为：

```vbnet
Dim __tuple2 = GetTuple()
Dim a = __tuple2.Item1
Dim b = __tuple2.Item2
```

#### 6. 头部 Imports 与 Option 语句

脚本文件顶层的 `Imports ...` 与 `Option ...` 语句会被引擎识别并保留到生成代码的头部。除此之外，引擎还会自动注入下列常用命名空间，脚本内无需重复声明：

- `Microsoft.VisualBasic.CommandLine`
- `Microsoft.VisualBasic`
- `System`, `System.Linq`, `System.Collections`, `System.Collections.Generic`
- `System.Data`, `System.Diagnostics`, `System.Threading.Tasks`, `System.Xml.Linq`

生成的代码固定包含 `Option Strict Off` / `Option Explicit On` / `Option Infer On`。

## 退出码

脚本对应的虚拟 `Main` 函数返回 `Integer` 作为进程退出码，默认 `Return 0`。若脚本执行过程中抛出未捕获的异常，宿主进程会以异常终止。

## 以库的方式嵌入使用

`VBS.vbproj` 项目除了提供 `vbs` 命令行宿主外，还可以作为类库被其他 .NET 程序引用，直接内嵌脚本执行能力：

```vbnet
Imports VBScriptHost.Script

' 1. 解析脚本(会同时解析#include与重构代码结构)
Dim vbs As ScriptParseResult = VBScript.ParseScript("./run.vb", verbose:=False)

' 2. 编译为内存assembly, 可通过extraRefs传入额外的引用程序集
Using runtime As ScriptRuntime = vbs.CompileScript(
    asmName:="MyScript",
    extraRefs:={"./extra.dll"},
    debug:=False)

    ' 3. 执行脚本, 返回值为脚本的退出码
    Dim exitCode As Integer = runtime.Run({"--a", "123"})
End Using   ' Dispose后动态加载的assembly会被卸载
```

## 项目源码结构

| 文件 | 职责 |
|------|------|
| `Program.vb` | `vbs` 命令行入口：解析参数，串联 解析 -> 编译 -> 执行 流程 |
| `src/DynamicDll.vb` | Roslyn 内存编译（引用收集、编译、IL 发射与加载）与反射调用 |
| `src/VBScript/VBScript.vb` | 脚本解析入口：`#include` 提取与路径解析、代码重构调度 |
| `src/VBScript/ScriptRefactor.vb` | 代码结构重构核心：文本预处理、逐行块扫描、顶层函数落位、代码组装 |
| `src/VBScript/TupleDestructuring.vb` | 元组分解语法展开 |
| `src/VBScript/ScriptParseResult.vb` | 解析结果数据对象 |
| `src/VBScript/ScriptRuntime.vb` | 脚本运行时：封装动态 assembly 的执行与卸载(`IDisposable`) |
