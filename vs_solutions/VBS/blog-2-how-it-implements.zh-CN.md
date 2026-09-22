# VBS 脚本引擎是如何运行一个 VB.NET 脚本的（How to Implement）

> 本文是 VBS 脚本引擎系列的第二篇，从**代码实现**的角度剖析 `vs_solutions/VBS/VBS.vbproj` 这个脚本引擎是如何运行一个 VB.NET 脚本的，以及向量化、动态类型、元组分解、默认参数表达式这些新语法是如何实现的。
> 使用方法请参见系列第一篇：《用 vbs 直接运行 VB.NET 脚本：VBS 脚本引擎使用指南》。

## 1. 总体架构：四阶段流水线

先给出全局视图。从用户敲下 `vbs ./run.vb` 到脚本执行完毕，整个流水线分为四个阶段：

```
 .vb 源码文件
      │
      ▼
 ┌─────────────────────────────────────────────┐
 │ ① 解析 ParseScript (VBScript.vb)             │
 │    - #include 解析 (dll/脚本/nuget)           │
 │    - 元数据指令解析 (#package/#author/...)     │
 │    - 文本级预处理(语法糖展开/向量化改写)        │
 └─────────────────────────────────────────────┘
      │  ScriptParseResult (预处理后代码 + 依赖清单)
      ▼
 ┌─────────────────────────────────────────────┐
 │ ② 结构重构 ScriptRefactor                    │
 │    - 逐行块扫描 (ScriptStructure.Scan)        │
 │    - 顶层函数 → 匿名函数 + 落位求解            │
 │    - 组装 Namespace+Module+Main 固定容器      │
 └─────────────────────────────────────────────┘
      │  GeneratedCode (完整可编译 VB.NET 源码)
      ▼
 ┌─────────────────────────────────────────────┐
 │ ③ 内存编译 DynamicDll.CompileScript           │
 │    - Roslyn: 解析 → 编译 → Emit               │
 │    - IL/PDB 直接发射到 MemoryStream           │
 └─────────────────────────────────────────────┘
      │  Assembly (仅存在于内存)
      ▼
 ┌─────────────────────────────────────────────┐
 │ ④ 反射执行 ScriptRuntime.Run                  │
 │    - ScriptLoadContext 可回收加载上下文        │
 │    - 反射调用 DynamicDll.Program.Main         │
 │    - Dispose 时 Unload 卸载整个上下文          │
 └─────────────────────────────────────────────┘
```

对应到源码文件，各模块职责如下：

| 文件 | 职责 |
|------|------|
| `Program.vb` | 命令行入口：子命令分发、串联 解析→编译→执行 |
| `src/VBScript/VBScript.vb` | 解析入口：`#include` → 元数据 → 预处理 → 重构 |
| `src/VBScript/IncludeDirective/` | `#include` 指令模型与解析器（dll/脚本/nuget 三类目标、递归展开） |
| `src/VBScript/ScriptStructure.vb` | 静态结构模型与逐行块扫描器 |
| `src/VBScript/Syntax/ScriptRefactor.vb` | 文本预处理 + 运行期代码发射器 |
| `src/VBScript/Syntax/LetStatement.vb` | `let` 动态类型展开 |
| `src/VBScript/Syntax/TupleDestructuring.vb` | 元组分解展开 |
| `src/VBScript/Syntax/DefaultParameterExpression.vb` | 默认参数表达式改写 |
| `src/VBScript/Syntax/Vectorization/` | 向量化改写器 + `@` 投影 |
| `src/DynamicDll.vb` | Roslyn 内存编译与反射调用 |
| `src/VBScript/ScriptRuntime.vb` | 运行时封装：执行与卸载 |

入口 `Program.vb` 非常薄：

```vbnet
Public Function Main(args As String()) As Integer
    ' 子命令分发: make-project 之外的第一个词元一律视为脚本文件路径
    If String.Equals(args(0), "make-project", StringComparison.OrdinalIgnoreCase) Then
        Return MakeProject.Run(args)
    End If

    Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
    Dim scriptFile As String = args(0)
    Dim verbose As Boolean = cmdl("--verbose")
    Dim vectorize As Boolean = Not cmdl("--no-vectorize")

    Dim vbs As ScriptParseResult = VBScript.ParseScript(scriptFile, verbose:=verbose, vectorize:=vectorize)

    Using script As ScriptRuntime = vbs.CompileScript(debug:=verbose)
        Return script.Run(args)   ' 返回值即脚本的进程退出码
    End Using
End Function
```

核心设计决策是：**脚本不是被"解释"的，而是被"重构为合法 VB.NET 源码后交给真正的编译器编译"**。引擎自己只做文本级的前端处理，后端完全复用 Roslyn，因此语义与 VB.NET 保持最大程度一致。

## 2. 阶段一：解析（ParseScript）

`VBScript.ParseScript` 是解析阶段的入口，做三件事：

### 2.1 #include 解析（IncludeResolver）

`#include` 支持三类目标，判别规则很简单：

| 目标形态 | 类别（`IncludeKind`） | 处理方式 |
|----------|----------|----------|
| `xxx.dll` | 程序集 | 解析为绝对路径，加入编译引用 |
| `xxx.vb` | 其它脚本 | 读取源码，提取其 `Imports` 与类型定义块，递归展开其自身的 `#include` |
| `pkg@version` | nuget 包 | 下载/复用缓存，解析传递依赖，收集 lib 资产 |

脚本类引用会递归展开并去重（循环引用报错），被引入脚本的类型定义块（`IncludeSet.TypeBlocks`）会被注入主脚本的顶层命名空间——注意是"原地复制源码"而不是编译为独立模块，因此被引入脚本只允许包含 `Imports` 与类型定义。

dll 路径解析按搜索目录列表依次探测：脚本所在目录 → 引擎目录 → `libs/` 目录；nuget 包则经由独立的 `VBProj.NuGet` 命名空间（自研的 NuGet 客户端：`NuGetClient` 版本索引与下载、`NuGetResolver` 依赖展开与版本冲突消解、`NuGetVersion`/`NuGetFramework` 版本与框架模型），复用 `~/.nuget/packages/` 全局缓存。

### 2.2 元数据解析（ScriptMetadata）

`#package/#author/#title/#version` 四条指令被解析为一个 `ScriptMetadata` 对象，后续既作为 Roslyn 编译的 assembly 名称来源，也会被发射为 assembly 级特性（`AssemblyCompanyAttribute` 等）。

### 2.3 文本预处理（PreprocessText）

这是所有**语法扩展**发生的地方。`ScriptRefactor.PreprocessText` 是一个纯文本变换管线：

```vbnet
Public Shared Function PreprocessText(source As String, ...) As String
    ' 移除 #include 行
    Dim code As String = Regex.Replace(source, "^\s*#include\s+""[^""]*""\s*$", "", ...)

    ' ?"--a" => args("--a")
    code = Regex.Replace(code, "\?""(?<name>[^""]+)""", "args(""${name}"")")

    ' let x = ... => Dim x As Object = ... (跳过 LINQ 查询中的 Let 子句)
    code = LetStatement.Expand(code)

    ' Dim (a, b) = ... => 多条独立 Dim 语句
    code = TupleDestructuring.Expand(code)

    ' 非常数默认参数: Function 生成桥接函数, Sub 调用点就地展开
    code = DefaultParameterExpression.Expand(code, defaultReport)

    ' @ 投影展开 + 数值向量算术 => Vec* SIMD 调用
    code = Vectorization.Expand(code, enabled:=vectorize, report:=report, ...)
    Return code
End Function
```

顺序是有讲究的：默认参数展开**必须早于**向量化，这样桥接函数体与临时变量行也能被 `@` 投影与 SIMD 处理。

## 3. 新语法是如何实现的

在进入结构重构之前，先详解四个语法扩展的实现——它们全部在预处理阶段完成，本质是"把非法的 VB.NET 语法改写为合法的 VB.NET 语法"。

### 3.1 let 动态类型（LetStatement.vb）

最简单的一个：把 `let name = expr` 重写为 `Dim name As Object = expr`。运行期的"动态成员访问"（`b.a`、`b.b`）不需要引擎做任何事——VB 的 `Option Strict Off` 下，`Object` 变量的成员访问天然是后期绑定（late binding）的。

实现上唯一有技术含量的地方是**不能误改 LINQ 查询中的 `Let` 子句**：

```vbnet
Dim squares = From n In numbers
              Let sq = n * n     ' 这个 Let 不能被改写！
              Where sq > 9
              Select sq
```

`LetStatement.Expand` 用一个逐行的**查询状态机**解决：遇到 `From ... In ...` 进入查询状态，遇到行首 `Select`/`Group` 退出；只在非查询状态下做 `let` 改写。为了防止自然语言字符串中的 "from ... in ..." 干扰状态机，检测前会先移除字符串字面量内容与注释（`DetectText`）。

### 3.2 元组分解（TupleDestructuring.vb）

`Dim (a, b as double) = (1, 2)` 被展开为多条独立的 `Dim` 语句：

```vbnet
Dim __tuple2 = (1, 2)
Dim a = __tuple2.Item1
Dim b As Double = __tuple2.Item2
```

支持跳位（`Dim (a, , c)`：跳过的位置不发声明）、嵌套（`Dim (p, (q, r))`：递归展开）、命名别名（`Dim (name:=v1)`）以及 `For Each (str, int) in tuples` 迭代分解。

### 3.3 默认参数表达式（DefaultParameterExpression.vb + DefaultParameterSignature.vb）

VB 只允许常数作为 `Optional` 参数默认值（`BC30201` 一类错误），而脚本希望写：

```vbnet
Function test(a As String, Optional b As Boolean = True,
              Optional c As testdata = If(b, New testdata(a), New testdata("default")))
```

默认值表达式引用了更靠前的参数 `b` 与 `a`——这是 VB 语法不允许的。引擎的解法分两条路径：

**声明改写**：带默认值的参数一律去掉 `Optional` 与 `= 默认值`，变为必填参数。之所以必须去掉 `Optional`：顶层函数在运行期路径会被重写为 `Main` 内的匿名函数，而 VB 不允许 lambda 参数声明为 `Optional`（`BC33010`）。

**调用点改写**，按被调用者是 Function 还是 Sub 分两路：

- **Function 路径——桥接函数**：为每一种"实际提供的参数子集"生成一个桥接函数，调用点只替换被调用名：

```vbnet
Call print(test(a:="xxxxx", b:=False))
' => Call print(test_defaults(__vbs_test_a:="xxxxx", __vbs_test_b:=False))

' 脚本末尾生成的桥接函数：
Function test_defaults(__vbs_test_a As String, __vbs_test_b As Boolean) As String
    Dim __vbs_test_c = If(__vbs_test_b, New testdata(__vbs_test_a), New testdata("default"))
    Return test(__vbs_test_a, __vbs_test_b, __vbs_test_c)
End Function
```

由于只做名字替换，任意表达式位置（`While test(a, b)`、`If test(...) Then`、嵌套调用）都能被改写，且不改变求值顺序与具名实参语义。

- **Sub 路径——就地展开**：Sub 调用只能是语句，直接展开为多行临时变量，不给热点调用增加函数调用开销：

```vbnet
Call emit(a:="inline")
' =>
Dim __vbs_emit_a_2 = "inline"
Dim __vbs_emit_c_3 = New testdata(__vbs_emit_a_2)
Call emit(__vbs_emit_a_2, __vbs_emit_c_3)
```

当 Sub 调用所在的结构容不下多行语句（如单行 lambda `Sub() Call emit(...)`、单行 `If ... Then ...`）时，先展开为完整块再注入。

整个改写器是**文本级**的（正则 + 词元替换，不建立 Roslyn 语义模型），任何不确定的情形（`ParamArray`、泛型参数表、续行签名、重载同名函数……）都直接放弃改写——参数退化为必填，编译期报错而不是静默出错。这是一个重要的设计哲学：**宁可不支持，也不能改错**。

### 3.4 向量化计算（Vectorization/ 文件夹）

向量化是最复杂的语法扩展，涉及五个文件协同：

| 文件 | 职责 |
|------|------|
| `Vectorization.vb` | 入口：`#no-vectorize` 指令处理、逐行驱动、改写报告 |
| `VectorType.vb` | 数值类型模型与 VB 逐元素类型提升规则 |
| `VectorExpressionRewriter.vb` | 核心：基于 Roslyn 的语句解析与浅层类型推断 |
| `SimdVocabulary.vb` | 运算符/函数 → `Vec*` 调用的映射与文本发射 |
| `PropertyProjection.vb` + `ObjectMemberTable.vb` | `@` 投影展开与成员表解析 |

工作流程：

1. **向量变量登记**：逐行扫描 `Dim` 声明，识别数值向量——`Dim x As Integer()`、`Dim x(10) As Integer`、`Dim x = {1, 2, 3}`（字面量推断）、函数的 `Double()` 参数、以及先前改写产生的向量结果，全部登记进向量符号表。只支持 `Short/Integer/Long/Single/Double` 五种元素类型；
2. **表达式解析**：对每一行用 Roslyn 解析出表达式语法树（只解析单行，不做全文语义模型）；
3. **类型推断**：对表达式中的标识符做**浅层**推断——查向量符号表、查字面量类型；遇到未知标识符、成员访问 `obj.Value`、下标访问 `x(0)` 就放弃整行改写；
4. **改写发射**：按运算符语义查 `SimdVocabulary` 词汇表，把表达式树映射为 `Vec*` 调用并按字符区间回写原行。类型提升规则由 `VectorType` 决定，例如 `/` 运算：`Single / Single` 得 `Single`，其余一律 `Double`（自动插入 `VecConvert`）；元素类型不一致时（`Short() + Integer()`）先插入 `VecConvert` 再运算，保证与"逐元素标量运算"完全等价。

`x + 5` 这一行最终被改写为 `VecAddScalar(Of Integer)(x, 5)`。生成代码只有在确实发生改写时才注入 `Imports Microsoft.VisualBasic.Math.SIMD.Vectorization`——词汇表刻意使用 `Vec*` 前缀并放在独立子命名空间，是为了避开 VB 对"两个已导入模块中的同名成员"报 `BC30562` 名称不明确的问题。

运行期后端是 sciBASIC 运行时的 `Vectorized` 模块，底层为 `System.Numerics.Vector`（SSE2/AVX/ARM SIMD）与 `SimdMath` 硬件内核。

### 3.5 @ 数组投影（PropertyProjection.vb + ObjectMemberTable.vb）

`list@x` 的展开需要一个关键信息：`x` 的类型。引擎用 Roslyn 解析**脚本内**（含被 `#include` 引入脚本）的 `Class/Structure` 定义块，构建「类型名 → 成员名 → 成员类型」的成员表（`ObjectMemberTable`），据此决定 `list@x` 的元素类型——只有带显式 `As` 类型子句的成员才会被登记。

展开规则：

| 写法 | 展开结果 |
|------|----------|
| `list@x` | `list.Select(Function(__vbs_o) __vbs_o.x).ToArray()` |
| `list@{x, y}` | `...New With {.x = ..., .y = ...}.ToArray()`（匿名类型数组） |
| `list@inner@x` | 链式逐级投影 |

展开同样极其保守：左侧元素类型无法确定、类型来自 dll 程序集、成员不存在、左侧不是标识符点号链……任何一项不确定都**不生成猜测性代码**，`@` 原样保留（脚本按原有方式报语法错误）。字符串字面量、注释、`1.5@` 这类 Decimal 类型字符位置也不会被误展开。

## 4. 阶段二：结构重构（ScriptRefactor + ScriptStructure）

预处理后的代码仍然是"顶层语句 + 顶层函数 + 类型定义块"混杂的形态，不能直接编译。结构重构分两步：

### 4.1 逐行块扫描（ScriptStructure.Scan）

`Scan` 用一个**块栈**逐行扫描代码：遇到 `Class/Structure/Interface/Enum/Module/Function/Sub/If/For/...` 等块起始关键字入栈，遇到对应的 `End XXX` 出栈，从而把代码切分为四类槽位：

- **头部语句**：`Imports` 与 `Option` 语句；
- **类型定义块**：完整的类型声明代码块（原样保留）；
- **顶层函数块**：无类型容器包裹的 `Function/Sub`；
- **顶层语句**：其余一切可执行语句，按出现顺序进入 `Slots` 列表。

扫描是文本级的，需要正确处理字符串字面量与注释中的伪关键字。

### 4.2 代码发射（BuildCode）

发射器把四类槽位组装进**固定容器结构**：

```vbnet
Option Strict Off
Option Explicit On
Option Infer On
Imports ...              ' 用户脚本头部 Imports + 自动注入的常用命名空间

<AssemblyAttributes()>   ' #package 等指令生成的特性

Namespace DynamicDll                          ' 固定顶层命名空间
    Module VBScriptHostMagics                 ' 魔法方法注入点
        Public Function ScriptDir() As String ...
    End Module

    Module Program
        Public Sub print(data As Object, ...) ' print 转发函数
        Public Function Main(args As CommandLine) As Integer
            ' 依赖无关的匿名函数最先落位 (slot = -1)
            Dim HelloWorld = Function() As String ... End Function

            ' 顶层语句，按源码顺序
            Dim A As Integer = args("--a")
            Call Console.WriteLine(new Test With {.A = A})

            ' 每条语句之后挂上"此时依赖已就绪"的匿名函数
            Dim Foo = Sub(a As Integer) ... End Sub

            Return 0
        End Function
    End Module

    Public Class Test                         ' 类型定义块并列于命名空间
        ...
    End Class
End Namespace
```

几个关键实现细节：

**顶层函数 → 匿名函数 + 落位求解**。VB 要求"先声明后使用"，匿名函数会捕获顶层变量与其它顶层函数，因此不能简单地把它们堆在 `Main` 开头。引擎对每个顶层函数求解其**依赖闭包**（捕获的变量 + 调用的其它函数），据此计算落位槽位（`ResolveFunctionSlots`）：不依赖任何东西的函数放在所有语句之前（slot = -1），其余按"其依赖的变量已经声明完毕"的原则插到对应的顶层语句之后。多个函数之间保持源码中的相对顺序。

**类型定义块不能放进 Module**。VB 不允许在 `Module` 内部再声明 `Module`（`BC30617`），而被 `#include` 引入的脚本常常贡献 `Module` 定义，因此类型块一律发射为命名空间的直接成员。

**魔法方法烘焙**。`ScriptDir()`、`Here()` 这些方法依赖脚本上下文（文件路径、元数据、搜索目录），引擎在预处理阶段直接把这些上下文以**常量字面量**的形式写进生成的 `VBScriptHostMagics` 模块源码——例如 `ScriptDir()` 的函数体就是 `Return "G:\path\to\script\dir"`。这比运行时传上下文对象简单得多，也让脚本的 `MakeProject` 转正产物可以脱离引擎独立运行。

**print 转发函数与名字遮蔽**。脚本里的 `print` 是宿主注入到 `Module Program` 的一个转发函数。为什么不能直接 `Imports Microsoft.VisualBasic.Printing`？因为在生成代码的导入作用域里，`Print` 这个名字已经被 VB 运行时的 `FileSystem.Print`（文件号重载）与本框架的 `CLITools` 同时导出，VB 对"两个已导入模块中的同名成员"直接报 `BC30561` 名称不明确——`print` 这个名字在脚本作用域里本来就是不可用的。解法是反过来利用 VB 的名字查找顺序："本类型自身的成员"优先于"已导入命名空间中的成员"，把 `print` 声明为脚本 `Module Program` 自己的成员，一次性把所有同名成员遮蔽掉，转发目标用全限定调用避免引入新的二义。

## 5. 阶段三：Roslyn 内存编译（DynamicDll.vb）

到这里，脚本已经变成一份**完整、合法的 VB.NET 源码**。`DynamicDll.CompileScript` 扩展方法接手编译：

### 5.1 引用收集与去重

`MetadataReference` 按如下优先级收集：

1. `#include` 引用的外部程序集（dll / nuget 资产 / 被引入脚本转发的依赖）；
2. 引擎宿主自身 assembly 与 `CommandLine` 所在的 sciBASIC 库；
3. VB 运行时；
4. 调用方传入的额外引用；
5. 当前 `AppDomain` 中所有已加载的 BCL 程序集。

关键细节是**按程序集简单名去重**：nuget 包的依赖树里极其常见"同名程序集的另一个副本"，若两个副本同时作为 `MetadataReference` 会产生重复类型定义的编译错误。去重策略是优先复用已经加载到当前 `AppDomain` 的版本。

### 5.2 编译与内存发射

```vbnet
Dim compilation As VisualBasicCompilation = VisualBasicCompilation.Create(
    assemblyName:=finalAsmName,      ' 显式参数 > #package 指令 > 脚本文件名
    syntaxTrees:=trees,
    references:=references,
    options:=options)                ' OutputKind.DynamicallyLinkedLibrary

Using ms As New MemoryStream(), pdb As New MemoryStream()
    Dim result As EmitResult = compilation.Emit(ms, pdb)

    If Not result.Success Then
        ' 汇总全部 Error 级诊断抛出
        Throw New InvalidOperationException("脚本代码编译失败!" & ...)
    End If

    Call ms.Seek(0, SeekOrigin.Begin)

    Dim ctx As New ScriptLoadContext("script-" & Guid.NewGuid().ToString("N"), script.Imports)
    Return New ScriptRuntime(ctx, ctx.LoadFromStream(ms))
End Using
```

IL 与 PDB 均发射到内存流，**全程不落盘**。编译失败时把 Roslyn 诊断汇总成可读的错误信息抛出——由于生成代码结构固定，报错行号可以直接对应到 `--verbose` 打印的生成代码上定位问题。

## 6. 阶段四：反射执行与卸载（ScriptRuntime.vb + ScriptLoadContext）

### 6.1 加载上下文的依赖解析

编译产物通过自定义的 `ScriptLoadContext`（继承 `AssemblyLoadContext`，`isCollectible:=True`）加载。其 `Load` 回调按如下顺序解析脚本**运行期**的依赖程序集：

1. 引擎宿主自身程序集——强制共享，保证宿主与脚本之间的类型同一性（否则反射传参必然类型分裂）；
2. Default ALC 中已加载的程序集——优先共享返回；
3. `#include` 文件的简单名称精确映射；
4. 在 `#include` 所在文件夹中按 `<名称>.dll` 探测，解决"依赖的依赖"这类传递依赖问题。

由于探测目录由全部 `#include` 程序集所在目录推导而来，nuget 包解压目录与其原生资产目录会被自动覆盖——脚本使用 nuget 包中的类型时，运行期不需要任何额外配置。

### 6.2 反射调用与卸载

`ScriptRuntime.Run` 做两件事：先用反射构造脚本可见的 `CommandLine` 参数对象（同样通过反射调用 `CommandLine.BuildFromArguments`，因为该类型在共享的 sciBASIC 库中，宿主与脚本看到的是同一个类型），然后反射调用 `DynamicDll.Program.Main(args As CommandLine)`：

```vbnet
Public Function Run(dynamicAsm As Assembly, args As Object) As Integer
    Dim targetType As Type = dynamicAsm.GetType($"{NameOf(DynamicDll)}.Program")
    Dim methodInfo As MethodInfo = targetType.GetMethod("Main", BindingFlags.Public Or BindingFlags.Static)
    Dim result As Object = methodInfo.Invoke(Nothing, New Object() {args})
    Return CInt(result)
End Function
```

脚本虚拟 `Main` 的返回值即进程退出码。`ScriptRuntime` 实现 `IDisposable`，`Dispose` 时调用 `ScriptLoadContext.Unload()` 卸载整个加载上下文——脚本 assembly 与其依赖均可被 GC 回收。这对把 VBS 作为类库嵌入宿主程序的场景至关重要：反复执行脚本不会造成程序集泄漏。

## 7. 两条发射路径：运行期与工程期

值得一提的是，代码发射有**两条路径**共享同一套预处理与结构扫描：

- **运行期**（`ScriptRefactor`）：顶层函数重写为匿名函数，组装 `Namespace+Module+Main`，内存编译执行；
- **工程期**（`ProjectCodeBuilder`，供 `make-project` 使用）：顶层函数还原为模块级 `Private Function/Sub`、被捕获的顶层变量提升为模块级字段、魔法方法物化为独立的 `VBScriptHostMagics.vb` 源文件、`#include` 转写为 `<Reference HintPath>` / `<PackageReference>` / 独立源码文件。

`PreprocessText` 是纯文本变换，因此两条路径完全共用——这保证了"脚本运行的行为"与"转正后工程的行为"一致。

## 8. 设计哲学

回顾整个实现，有几个贯穿始终的设计决策：

1. **文本级前端 + Roslyn 后端**。引擎不做完整编译器前端（没有 SemanticModel、没有数据流分析），只做文本级解析与改写，然后把"生成合法源码"这个最难且最容易出错的工作交给 Roslyn。好处是宿主程序极小、语义与 VB.NET 高度一致；
2. **保守改写，宁缺毋滥**。所有改写器（向量化、`@` 投影、默认参数）都遵循同一条铁律：任何一次推断不确定就放弃改写，保持原样。而这些写法在扩展引入之前本来就无法编译，因此不存在行为回归——最坏情况只是"新语法没生效"，绝不会"生成错误的代码"；
3. **编译期报错优于运行期静默出错**。无法覆盖的调用点让参数退化为必填，让 Roslyn 报"未提供参数"，而不是静默传入 `Nothing`；
4. **报告与可观测性**。每个改写器都输出报告（改写点数量、生成的桥接函数名、被跳过的行），配合 `--verbose` 打印生成代码，任何"为什么没生效"都能快速定位；
5. **可回收设计**。可回收 `AssemblyLoadContext` + `IDisposable`，脚本执行完即可整体卸载，适合长驻宿主程序。

## 小结

VBS 的本质是一个 **"源码到源码"的重构器 + Roslyn 内存编译器 + 可回收反射执行器** 的三明治结构：

- 语法扩展（`let`、元组、默认参数、向量化、`@`）在预处理阶段以文本改写的方式"脱糖"为标准 VB.NET；
- 顶层代码通过块扫描与落位求解装进固定的 `Namespace+Module+Main` 容器；
- Roslyn 负责真正的编译，IL 全程驻留内存；
- 自定义 `AssemblyLoadContext` 负责运行期依赖解析与执行后的完整卸载。

整套设计让 VB.NET 获得了一种"脚本语言"的使用体验，同时完整保留了静态编译语言的性能与 .NET 生态的全部能力。如果你对其中某个模块感兴趣，欢迎阅读 `vs_solutions/VBS/src/` 下的源码——每一处保守策略的背后注释里都写清了"为什么"。