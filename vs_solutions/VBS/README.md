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

# 关闭数值向量的自动向量化改写
vbs ./run.vb --no-vectorize

# 就地转换为正式的 vbproj 工程(并执行 dotnet build 验证)
vbs make-project ./run.vb
```

脚本宿主的入口逻辑(`Program.vb`)：

```vbnet
' 子命令分发: make-project 之外的第一个词元一律视为脚本文件路径
If String.Equals(args(0), "make-project", StringComparison.OrdinalIgnoreCase) Then
    Return MakeProject.Run(args)
End If

Dim cmdl As CommandLine = CommandLine.BuildFromArguments(args, NoSubCommand:=False)
Dim scriptFile As String = args(0)
Dim verbose As Boolean = cmdl("--verbose")
Dim vbs As ScriptParseResult = VBScript.ParseScript(scriptFile, verbose:=verbose)

Using script As ScriptRuntime = vbs.CompileScript(debug:=verbose)
    Return script.Run(args)   ' 返回值即脚本的进程退出码
End Using
```

## 实现原理

脚本引擎会通过脚本文件中的元数据解析，动态编译生成 assembly，具体过程为：引擎会首先从脚本中解析出``#include``元数据，得到引用的程序集、被引入的其它脚本以及 nuget 程序包。然后通过正则表达式提取出类型定义（class，structure，interface，enum，module）代码块，处理顶层函数为匿名函数，最后将得到的代码生成下面的固定命名空间以及类型的完整代码。

整个执行流水线分为四个阶段：

1. **解析**(`VBScript.ParseScript`)：读取源码，解析 `#include` 引用的外部依赖（dll / 其它脚本 / nuget 包，含递归展开与 nuget 传递依赖解析），解析 `#package/#author/#title/#version` 程序集元数据指令，并进行文本预处理（移除 `#include` 行、展开命令行参数语法、展开 `let` 动态类型声明、展开元组分解语法、展开默认参数表达式、展开 `@` 数组投影、数值向量化改写）；
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

`CompileScript` 会按下面的优先级收集 `MetadataReference`：

1. `#include` 所引用的外部程序集（包含本地 dll、nuget 包解析出的资产、以及被引入脚本转发的依赖；文件不存在时直接抛出 `FileNotFoundException`）；
2. 引擎宿主自身 assembly 与 `CommandLine` 所在的 sciBASIC 库（提供 `CommandLine` 类型定义）；
3. VB 运行时（支持脚本使用 `Microsoft.VisualBasic` 内置函数）；
4. 调用方传入的额外引用程序集；
5. 当前 `AppDomain` 中所有已加载的 BCL 程序集。

引用**按程序集简单名去重**：当某个 `#include`(nuget 包之中的资产尤其常见)指向的是同名程序集的另一个副本时，
会优先复用已经加载到当前 `AppDomain` 的版本，避免同一程序集的两个副本同时作为 `MetadataReference`
而产生重复类型定义的编译错误。

### 运行时依赖解析与卸载

编译产物通过自定义的 `ScriptLoadContext`（`isCollectible:=True`）加载，其 `Load` 回调按如下顺序解析脚本运行期的依赖程序集：

1. 引擎宿主自身程序集——强制共享，保证宿主与脚本之间的类型同一性（否则反射传参必然类型分裂）；
2. Default ALC 中已加载的程序集——优先共享返回（`CommandLine` 所在的 sciBASIC 库即属此类）；
3. `#include` 文件的简单名称精确映射；
4. 在 `#include` 所在文件夹中按 `<名称>.dll` 探测，解决"依赖的依赖"这类传递依赖问题。

由于探测目录是由全部 `#include` 程序集的所在目录推导出来的，nuget 包解压目录
(`~/.nuget/packages/<id>/<version>/lib/<tfm>/`)与其原生资产目录会被自动覆盖。

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

#### 2. 引用外部依赖(#include)

`#include` 预编译指令支持三类引用目标，语法统一为 `#include "<target>"`：

```vbnet
#include "/path/to/assembly.dll"      ' 1. 外部 CLR 程序集
#include "./lib/Helper.vb"            ' 2. 其它 VB 脚本
#include "Newtonsoft.Json@13.0.3"     ' 3. nuget 程序包
```

##### 2.1 引用外部 DLL 程序集

所导入的外部dll程序集的路径可以为绝对路径，或者相对于脚本文件的相对路径。在脚本引擎中会将这里引用的dll路径统一解析为绝对路径。

对于相对路径，引擎会依次在下列目录中探测：

1. 声明该指令的脚本文件所在的文件夹；
2. 引擎程序目录 `App.HOME`；
3. `App.HOME/libs`；
4. `App.HOME` 上级目录下的 `libs/`。

##### 2.2 引用其它的 VB 脚本

`#include` 指令的目标以 `.vb` 结尾时，目标脚本的代码会被**直接复制到主脚本的顶层命名空间**之中：
被引入脚本的 `Imports` 会提升到生成代码的头部，其类型定义(`Class`/`Structure`/`Interface`/`Enum`/`Module`)与主脚本的类型定义并列。
被引入脚本可以再次 `#include` 其它脚本(引擎会递归展开并去重，出现循环引用时报错)，其引用的 dll / nuget 包也会一并并入本次编译。

由于代码是"原地复制"而不是"编译为独立模块"，被引入的脚本存在下列限制(违反时引擎会给出指明文件与原因的错误)：

| 限制 | 原因 |
|------|------|
| 不允许调用魔法方法(`ScriptDir`/`Here`/`Package`/...) | 魔法方法只描述**主脚本**自身的上下文 |
| 不允许顶层可执行语句 | 顶层语句只能进入主脚本的 `Main` |
| 不允许顶层 `Function`/`Sub` | 顶层函数只能重写为主脚本 `Main` 之中的匿名函数 |
| 不允许 `#package`/`#author`/`#title`/`#version` 元数据指令 | 元数据只描述主脚本编译出的 assembly |

也就是：被引入的脚本只应该包含 `Imports` 与类型定义，等价于一个普通的 `.vb` 源码模块：

```vbnet
' ./lib/Helper.vb
Imports System.Text

Public Class LibraryHelper
    Public Shared Function Greet(name As String) As String
        Return $"hello, {name}!"
    End Function
End Class
```

##### 2.3 引用 nuget 程序包

```vbnet
#include "Newtonsoft.Json"               ' 自动取最新稳定版
#include "Newtonsoft.Json@13.0.3"        ' 精确版本
#include "Microsoft.Extensions.Logging@[8.0,9.0)"   ' 版本范围
```

- **依赖自动解析**：引擎读取包内 `.nuspec` 声明的依赖组，按"最接近 `net10.0`"的规则选择依赖组，并以广度优先方式递归展开全部传递依赖；
- **版本冲突消解**：与 NuGet 的规则一致 —— 同一个包被多处要求时取**满足全部约束的最低版本**，从而避免把不同大版本的组件混用在一起(约束之间无法同时满足时退化为"满足最多约束的最低版本")；
- **本地缓存**：包被解压到 NuGet 全局包目录 `~/.nuget/packages/<id>/<version>/`(可用环境变量 `NUGET_PACKAGES` 覆盖)，目录布局与 NuGet 自身完全一致，因此**可以直接复用** Visual Studio / `dotnet restore` 已经安装过的包；缓存命中时不再联网，二次运行零网络开销；
- **资产选择**：只收集 `lib/<tfm>` 与当前平台 `runtimes/<rid>/lib/<tfm>` 下的托管程序集；与引擎已经加载过的同名程序集冲突时会自动复用已加载的版本(避免同一程序集的两个副本造成重复类型定义)；
- 解析出的全部程序集自动加入编译引用与运行期依赖探测，脚本内可直接使用包中的类型；`Includes()` 魔法方法会一并列出这些程序集的绝对路径；
- 失败原因(包不存在、版本不存在、网络不可达)会连同 URL 一起报告出来。

> 实现位于 `dev/VisualStudio` 的 `VBProj.NuGet` 命名空间(`NuGetClient` / `NuGetResolver` / `NuGetVersion` / `NuGetFramework`)，`VBS.vbproj` 通过既有的 ProjectReference 复用，引擎不依赖 NuGet 官方客户端库。

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

#### 7. 程序集元数据指令(#package / #author / #title / #version)

脚本头部可以使用下列预处理指令声明动态编译 assembly 的元数据：

```vbnet
#package "MyScript.Package"     ' 设置动态编译 assembly 的名称
#author  "xieguigang"           ' assembly 级别 AssemblyCompanyAttribute
#title   "My First Script"      ' assembly 级别 AssemblyTitleAttribute
#version "1.2.3.4"              ' assembly 级别 AssemblyVersionAttribute
```

- `#package` 作为 Roslyn 编译时的 assembly name，其取值优先级为：`CompileScript` 的显式 `asmName` 参数 > `#package` 指令 > 脚本文件名；
- `#author` / `#title` / `#version` 会分别生成 assembly 级别的 `AssemblyCompanyAttribute` / `AssemblyTitleAttribute` / `AssemblyVersionAttribute`；
- 指令值可以使用双引号包裹，也可以直接写裸标记（例如 `#version 1.2.3.4`）；
- 若需要在运行时读回这些值，可以使用魔法方法 `Package()` / `Author()` / `Title()` / `Version()` / `Meta("key")`。

#### 8. let 动态类型声明

采用 `Dim` 声明时由 Roslyn 自动进行类型推断，得到的是一个强类型变量；而采用 `let` 声明时，引擎会在预处理阶段将其重写为 `Object` 类型的 `Dim` 声明：

```vbnet
Dim strong = "hello"     ' Roslyn 类型推断 => String(强类型)
let dynamic = "hello"    ' 预处理为 Dim dynamic As Object = "hello"(动态类型)
```

`let` 声明的变量被强制声明为 `Object`，因此可以在运行时重新绑定任意类型并动态访问其成员：

```vbnet
let value = 123
value = "now a string"
value = New Person With {.Name = "asuka", .Age = 18}

Call Console.WriteLine(value.Name)      ' 动态成员访问
```

引擎会自动区分 LINQ 查询表达式之中的 `Let` 子句，下面的 `Let` 不会被改写：

```vbnet
Dim numbers = {1, 2, 3, 4, 5, 6}
Dim squares = From n In numbers
              Let sq = n * n
              Where sq > 9
              Select sq
```

#### 9. 魔法方法

引擎会在预处理阶段把与脚本上下文相关的信息（脚本文件路径、头部指令元数据、`#include` 依赖与搜索目录）以常量/字面量的形式烘焙进生成代码，并注入到 `DynamicDll.VBScriptHostMagics` 模块之中。脚本无需任何 import 即可直接调用：

| 分组 | 方法 | 说明 |
|------|------|------|
| 脚本上下文 | `ScriptDir()` | 脚本文件所在的文件夹 |
| | `ScriptFile()` | 脚本文件的绝对路径 |
| | `ScriptName(Optional withExtension As Boolean = True)` | 脚本文件名 |
| | `Here(relpath)` | 将相对路径解析为相对于脚本所在文件夹的绝对路径 |
| | `ScriptText()` | 读取脚本自身的源代码文本 |
| | `ScriptLines()` | 逐行读取脚本自身的源代码 |
| | `Self()` | 脚本自身编译得到的 `Assembly` |
| 元数据反射 | `Package()` / `Author()` / `Title()` / `Version()` | 读回对应的 `#` 指令值 |
| | `Meta(key)` | 按名称(大小写不敏感)读取指令值，未知名称返回 `Nothing` |
| 依赖与定位 | `Includes()` | `#include` 引用的全部程序集绝对路径数组(本地 dll + nuget 资产 + 脚本转发依赖) |
| | `Locate(name)` | 按 `#include` 相同的搜索顺序定位文件(脚本目录、引擎目录、nuget 包解压目录)，返回绝对路径或 `Nothing` |

```vbnet
Call Console.WriteLine($"脚本位于: {ScriptDir()}")
Call Console.WriteLine($"配置文件: {Here("config.json")}")

For Each dll In Includes()
    Call Console.WriteLine(dll)
Next
```

#### 10. 向量化计算

脚本中声明为**数值数组**的变量在参与数学运算时，引擎会在重构阶段自动把标量写法展开为等价的
逐元素(SIMD)调用，脚本作者不需要手写 `For` 循环：

```vbnet
Dim x = {1, 2, 3, 4, 5}
Dim y = {2, 3, 4, 5, 1}

Dim sum = x + 5                 ' => VecAddScalar(Of Integer)(x, 5)
Dim z = (x * y + 6) / (x + y)   ' 整棵表达式树一次性展开
```

`--verbose` 打印出来的重构结果：

```vbnet
Dim sum = VecAddScalar(Of Integer)(x, 5)
Dim z = VecDivide(
            VecConvert(Of Integer, Double)(VecAddScalar(Of Integer)(VecMultiply(Of Integer)(x, y), 6)),
            VecConvert(Of Integer, Double)(VecAdd(Of Integer)(x, y)))
```

后端是 `Microsoft.VisualBasic.Runtime` 之中的
`Microsoft.VisualBasic.Math.SIMD.Vectorization.Vectorized` 模块，底层为
`System.Numerics.Vector`(SSE2/AVX/ARM Advanced SIMD)与 `SimdMath` 的硬件内联内核
(`SimdEngine` / `SimdMath` / `SimdReduce`)。

##### 10.1 数值向量的识别规则

| 写法 | 判定 |
|------|------|
| `Dim x As Integer()` / `Dim x As Double()` | 数值向量(一维数组) |
| `Dim x(10) As Integer` | 数值向量 |
| `Dim x = {1, 2, 3}` | 由字面量推断，本例得 `Integer()` |
| `Dim x = {1, 2.5}` | 取公共数值类型，本例得 `Double()` |
| `Dim x = New Double(4) {}` / `New Integer() {...}` | 数值向量 |
| `Function F(x As Double(), y As Integer)` 的参数 | 数值向量 |
| 改写产生的向量结果(如 `Dim z = x / y` 之后的 `z`) | 继续登记，后续语句可继续向量化 |

只支持 `Short` / `Integer` / `Long` / `Single` / `Double` 五种元素类型；
`Byte`/`SByte`/`UShort`/`UInteger`/`ULong`/`Decimal`、多维数组、交错数组与 `List(Of T)` 一律**不**参与改写。

##### 10.2 运算符与结果类型(严格遵循 VB 逐元素语义)

| 运算符 | 结果元素类型 | 示例 |
|--------|--------------|------|
| `+ - * Mod` | 两侧的公共数值类型(`Double > Single > Long > Integer > Short`) | `Integer() + 5` 得 `Integer()` |
| `/` | `Single / Single` 得 `Single`，其余一律 `Double` | `Integer() / Integer()` 得 `Double()` |
| `^` | 恒为 `Double` | `Integer() ^ 2` 得 `Double()` |
| `\` | 整型(`Integer`/`Long`) | `Integer() \ 2` 得 `Integer()` |
| 一元 `-` | 与操作数同类型 | `-x` |
| 标量在左 | 交换律运算按等价形式改写，非交换律保持语义 | `100 - x`、`100 / x`、`x Mod 3`、`2 ^ x` |

元素类型不一致时改写器会自动插入逐元素转换：`Short() + Integer()` 会先
`VecConvert(Of Short, Integer)(...)` 再运算，因此结果与「把同一个表达式逐元素地用标量 VB 语法算一遍」
完全等价。

> 唯一的已知轻微偏差：VB 之中 `Short \ Short` 的结果是 `Short`，而向量化版本统一提升为 `Integer`。
> 该偏差只影响元素类型的静态声明，不改变数值结果。

##### 10.3 逐元素数学函数

| 函数 | 展开形态 |
|------|----------|
| `Math.Abs(v)` | `VecAbs(Of T)(v)` |
| `Math.Sqrt/Exp/Log(v)` | `VecSqrt` / `VecExp` / `VecLog`(整型向量先提升为 `Double()`) |
| `Math.Log(v, base)` | `VecLog(<Double()>, base)` |
| `Math.Sign(v)` | `VecSign`(Double/Single) 或 `VecMap`(整型) |
| `Math.Floor/Ceiling/Truncate(v)` | `VecFloor` / `VecCeiling` / `VecTruncate`(仅 Double/Single) |
| `Math.Sin/Cos/Tan/Asin/Acos/Atan/Round/Log10(v)` | `VecMap(Of K, Double)(v, Function(__v As K) Math.Sin(__v))` |
| `Math.Pow(a, b)` | 与 `a ^ b` 相同 |

函数名可以写成 `Math.Sqrt(v)`，也可以(`Imports System.Math` 之后)写成裸名 `Sqrt(v)`；
若脚本自身定义了同名函数，裸名写法**不会**被改写。

##### 10.4 聚合归约

| 写法 | 展开形态 | 结果类型 |
|------|----------|----------|
| `x.Sum()` | `VecSum(x)` | 与元素类型相同 |
| `x.Average()` / `x.Mean()` | `VecMean(x)` | 整型输入得 `Double`；`Single` 得 `Single` |
| `x.Min()` / `x.Max()` | `VecMin(x)` / `VecMax(x)` | 与元素类型相同 |
| `x.Product()` | `VecProduct(x)` | 与元素类型相同 |
| `x.Count()` | `VecCount(Of T)(x)` | `Integer` |

归约走 `SimdReduce` 的**顺序**内核而不是 `SimdParallel` 的分块并行，以保证浮点结果可复现。

##### 10.5 关闭向量化

```bash
# 单次关闭(按次)
vbs ./run.vb --no-vectorize
vbs make-project ./run.vb --no-vectorize
```

```vbnet
#no-vectorize      ' 脚本级关闭: 写在脚本头部
#vectorize         ' 也可以重新打开(等价于 #vectorize on)
```

关闭之后脚本保持原有代码形态：数组运算必须手写循环，`x(i)` 这类下标访问也不会被误解为向量运算。
`--verbose` 模式下会明确打印 `----- vectorization: disabled -----`。

##### 10.6 保守策略与已知局限

改写只依据脚本**自身**的声明做浅层类型推断：不解析 `#include` 进来的程序集、不建立 Roslyn
`SemanticModel`、不做数据流分析。任何一次推断不确定就**放弃改写该表达式**
(于是保持原有行为 —— 而这类写法在本功能引入之前本来就无法编译，因此不存在行为回归)：

- 未知标识符、成员访问(`obj.Value`)、下标访问(`x(0)`)；
- 跨物理行的表达式续行(使用 `_` 或括号换行)；
- 物理行本身语法不完整(块首行 `If ... Then`、块尾行 `Next`/`End If` 等，这些行里没有可改写的表达式)；
- 同一名称在不同作用域被声明为不同的数值类型 —— 该名称会被永久排除，避免误改写。

`--verbose` 会打印改写点数量、识别到的向量变量，以及「引用了已知向量却未能改写」的行清单，便于排查。

被 `#include` 引入的脚本**不参与**向量化：它们只允许包含 `Imports` 与类型定义，且工程期会被写成
独立文件(看不到向量化所需的 `Imports`)，两条发射路径都统一跳过。

改写生成的是嵌套调用形式，每个中间结果会分配一个数组；运行时的
`SimdEngine.AddInPlace` / `SubtractInPlace` / `MultiplyInPlace` 已经具备「就地计算、表达式融合」
的扩展点，可作为后续优化方向。另外，标量操作数依赖生成代码的 `Option Strict Off` 做隐式数值转换
(运行期与工程期两条路径都固定为 `Option Strict Off`)。

#### 11. 向量化运算参考表

改写器发射的全部调用都指向运行时同一个模块
`Microsoft.VisualBasic.Math.SIMD.Vectorization.Vectorized`(统一使用 `Vec*` 前缀)：

| 分组 | 成员 |
|------|------|
| 向量 ⊕ 向量 | `VecAdd` / `VecSubtract` / `VecMultiply` / `VecDivide` / `VecIntegerDivide` / `VecModulo` / `VecPower` |
| 向量 ⊕ 标量 | `VecAddScalar` / `VecSubtractScalar` / `VecScalarSubtract` / `VecMultiplyScalar` / `VecDivideScalar` / `VecScalarDivide` / `VecIntegerDivideScalar` / `VecScalarIntegerDivide` / `VecModuloScalar` / `VecScalarModulo` / `VecPowerScalar` / `VecScalarPower` |
| 一元 | `VecNegate` / `VecAbs` / `VecSquare` / `VecSqrt` / `VecExp` / `VecLog` / `VecSign` / `VecFloor` / `VecCeiling` / `VecTruncate` / `VecReciprocal` |
| 映射与转换 | `VecMap` / `VecConvert` |
| 归约 | `VecSum` / `VecMean` / `VecMin` / `VecMax` / `VecProduct` / `VecCount` |

`VecSquare` / `VecReciprocal` 目前没有对应的改写来源(脚本里的 `v * v` / `1 / v` 会被发射为
`VecMultiply` / `VecScalarDivide`，以保持 VB 的逐元素类型语义)，它们作为词汇表的一部分保留，
脚本也可以直接调用。

#### 12. @ 数组投影运算符

对**对象数组**可以用 Perl 风格的 `@` 直接把成员投影成一个新数组：

```vbnet
Class CLRObjectType
    Property x As Double
    Property y As String
End Class

Dim list As CLRObjectType() = { ... }

Dim x = list@x                                  ' => Double()
Dim y = list@y                                  ' => String()

Dim z = list@x + {2, 3, 4, 5, 6, 7, 8, 9}
' => Dim z = VecAdd(list.Select(Function(__vbs_o) __vbs_o.x).ToArray(),
'                   VecConvert(Of Integer, Double)({2, 3, 4, 5, 6, 7, 8, 9}))
```

展开规则：

| 写法 | 展开结果 | 结果类型 |
|------|----------|----------|
| `list@x` | `list.Select(Function(__vbs_o) __vbs_o.x).ToArray()` | 成员类型的数组(`Double()`)，**可参与向量化** |
| `list@{x, y}` | `list.Select(Function(__vbs_o) New With {.x = __vbs_o.x, .y = __vbs_o.y}).ToArray()` | 匿名类型数组，**不参与向量化** |
| `list@{x}` | 同上(花括号形式一律给出匿名类型数组) | 匿名类型数组 |
| `list@inner@x` | 逐级投影：`...Select(...__vbs_o.inner).ToArray().Select(...__vbs_o.x).ToArray()` | 链式，逐级解析元素类型 |
| `list@x.Sum()` | `VecSum(list.Select(Function(__vbs_o) __vbs_o.x).ToArray())` | 投影结果上的成员调用照常处理 |

关键性质：

- **`@` 是语法糖，始终生效**：与 `let`、元组分解同级。`--no-vectorize` / `#no-vectorize`
  **只**关闭 SIMD 算术改写，`@` 照常展开(于是关闭之后数组聚合要写 LINQ 形式 `values.Sum()`)；
- **投影结果自动接入向量化**：`list@x` 得到的就是普通一维数组，因此第 10 节的全部能力
  (向量算术、标量广播、VB 类型提升、逐元素数学函数、聚合归约)都自动适用；
- **手写形式同样被识别**：脚本里直接写 `list.Select(Function(o) o.x).ToArray()` 与 `list@x` 等价，
  也会被向量化。

##### 12.1 成员类型从哪里来

引擎用 Roslyn 解析**主脚本**以及**被 `#include` 引入脚本**的 `Class`/`Structure` 定义块
(`Property x As Double`、`Public x As String`、`Dim x As Double`、带 `Get`/`Set` 的属性块都支持)，
得到「类型名 → 成员名 → 成员类型」的成员表，据此决定 `list@x` 的元素类型。
只有带**显式 `As` 类型子句**的成员才会被登记。

##### 12.2 保守策略(不展开的情形)

只有「左侧元素类型能从脚本声明解析出来 **且** 该类型确实声明了所请求的成员」才展开；
任何一项不确定都**不生成猜测性代码**，该 `@` 原样保留(脚本按原有方式报语法错误)。
判别粒度是逐个 `@`。典型的不展开情形：

| 情形 | 例子 |
|------|------|
| 左侧变量没有显式元素类型(函数返回类型不追踪) | `Dim l = GetList()` 之后的 `l@x` |
| 元素类型来自 `#include` 的 **dll 程序集**(只解析脚本内的类型定义) | `Dim p As Person()` 而 `Person` 定义在 dll 里 |
| 成员不存在，或成员没有类型子句 | `list@notExist` |
| 左侧不是标识符点号链 | `GetList()@x`、`list(0)@x` |
| 标量对象(请直接写 `obj.x`) | `obj@x` |
| 跨物理行的表达式续行 | `@` 落在被续行的部分 |

`@` 出现在字符串字面量、行尾注释，以及 `1.5@` 这类 Decimal 类型字符位置时**都不会**被展开
(字符串插值 `$"..."` 内部目前也不支持)。

`--verbose` 会打印 `@` 投影次数与「已解析出成员表的类型名」，便于确认某个类型为什么没有生效：

```text
----- vectorization: 2 处 SIMD 改写, 4 处 @ 投影, 5 个向量变量 -----
    vectors: list, x, y, z, scaled
    object types: CLRObjectType
```

#### 13. 数据打印(print)

脚本里可以直接调用引擎注入的 `print(...)`，按 **GNU R 的格式**打印数据，用于调试：

```vbnet
Dim x = {1, 3, 24, 23, 4, 23}
Call print(x)
' [1]  1  3 24 23  4 23

Dim y As New List(Of Double) From {1, 3, 24, 23, 4, 23}
Call print(y)                  ' [1]  1  3 24 23  4 23(数组与 List 都可以)
```

方括号之中是**本行第一个元素的下标**(与 GNU R 一致，而不是元素个数)；超过宽度(缺省 80 字符)时
自动折行，续行带自己的起始下标；`width` 与 `digits` 都是具名可选参数：

```vbnet
Call print(Enumerable.Range(1, 40))
'  [1]  1  2  3  4 ... 25
' [26] 26 27 28 29 ... 40

Call print(Enumerable.Range(1, 40), width:=24)      ' 每行 6 个
Call print(New Double() {1 / 3, Math.PI}, digits:=3)   ' [1] 0.333  3.14
```

##### 13.1 元素类型的格式化规则

| 元素类型 | 输出 | 对齐 |
|----------|------|------|
| `Integer` / `Byte` / `Short` / `Long` / `UInteger` / `ULong` / `Decimal` | 整数文本 | 右 |
| `Single` / `Double` | 按有效数字位数截断(缺省 7 位，对应 R 的 `options(digits=7)`)；`NaN` / `Inf` / `-Inf` 用 R 的写法 | 右 |
| `Boolean` | `True` / `False` | 右 |
| `String` / `Char` | 带双引号的字面量 | 左 |
| `Enum` | `ToString` 之后按字符串向量打印(带双引号) | 左 |
| 其它 | `ToString` | 左 |

集合之中的 `Nothing` 元素打印为 `NA`，`Nothing` 对象打印为 `NULL`，空集合按 R 的写法打印为
`numeric(0)` / `character(0)` / `logical(0)`；元素本身也是集合时(例如 `Double()()` 或者
`List(Of List(Of Double))`)按 R 的 list 格式打印：

```vbnet
Call print({New Double() {1, 2}, New Double() {3, 4}})
' [[1]]
' [1] 1 2
'
' [[2]]
' [1] 3 4
```

##### 13.2 打印 NumericTable

`NumericTable`(相当于 R 之中的 data.frame)按**带边框的表格**打印：表头由特征列名与标签列名组成，
首列是行名(缺失时自动生成 `1..n`)，列名缺失或长度不匹配时按 R 的习惯生成 `V1..Vn` / `L1..Lk`
占位列名；行名一列左对齐，数值列右对齐。

```vbnet
Imports Microsoft.VisualBasic.Data      ' NumericTable 所在的命名空间

Dim tbl = NumericTable.FromRows(
    {"s1", "s2", "s3"},
    {New Double() {1, 4}, New Double() {2, 5}, New Double() {3, 6}},
    {"x", "y"})

Call tbl.SetLabel("cluster", {0, 0, 1})

Call print(tbl)
' +----+---+---+---------+
' |    | x | y | cluster |
' +----+---+---+---------+
' | s1 | 1 | 4 |       0 |
' +----+---+---+---------+
' | s2 | 2 | 5 |       0 |
' +----+---+---+---------+
' | s3 | 3 | 6 |       1 |
' +----+---+---+---------+
```

##### 13.3 运行期类型分派

脚本之中的变量常常是 `let` 声明的 `Object`(或者各不相同的集合类型)，因此 `print` 只有一个
`data As Object` 形参，由**运行期**按实际类型分派：**二维表 → 表格；集合 → R 向量；其它 → R 标量**。

```vbnet
Call print(42)                ' [1] 42
Call print(1.0 / 3)           ' [1] 0.3333333
Call print("done")            ' [1] "done"
Call print(True)              ' [1] True

let t = tbl
Call print(t)                 ' 与 print(tbl) 输出一致
```

需要拿到字符串而不是直接输出时，可以调用运行时的 `ToText`：

```vbnet
Dim text = Microsoft.VisualBasic.Printing.Print.ToText(New Double() {1, 2, 3})
' text = "[1] 1 2 3"
```

##### 13.4 实现说明

- 运行时实现是 sciBASIC 运行时的 `Microsoft.VisualBasic.Printing.Print` 模块(格式化内核为
  `Microsoft.VisualBasic.Printing.RFormat`)。以库的方式使用时，`Imports Microsoft.VisualBasic.Printing`
  之后调用 `Print(data)` / `ToText(data)` 即可；
- 脚本之中的 `print` 是**宿主注入的一个转发函数**(`ScriptRefactor.PrintForwarder`)，
  运行期与工程期(`make-project`)两条发射路径都会注入。
  之所以必须注入而不能简单地 `Imports Microsoft.VisualBasic.Printing`：在生成代码的导入作用域
  之中，`Print` 这个名字已经被 VB 运行时的 `Microsoft.VisualBasic.FileSystem`(文件号重载)与本框架的
  `Microsoft.VisualBasic.CommandLine.CLITools` 同时导出，而 VB 对"两个已导入模块之中的同名成员"
  会直接报 `BC30561`(名称不明确) —— 实测 `print(1)` / `print("a")` / `print(2.5)` 全都无法编译，
  也就是说 `print` 这个名字本来在脚本作用域之中就是不可用的；再增加一条 Imports 只会让二义变成三方二义；
- 解决办法是利用 VB 的名字查找顺序："本类型(Module)自身的成员"优先于"已导入命名空间之中的成员"。
  因此宿主把 `print` 声明为脚本 `Module Program` 自己的成员，一次性把上述同名成员全部遮蔽掉；
  转发目标固定为 `Microsoft.VisualBasic.Printing.Print.print(data As Object, ...)`(全限定调用，
  因此生成代码里不需要再增加 Imports)。

#### 14. 默认参数表达式

VB.NET 只允许把**常数或常数表达式**作为可选参数(`Optional`)的默认值。脚本引擎放宽了这个限制：
顶层 `Function`/`Sub` 的可选参数可以书写**任意可以产生值的表达式**，例如引用更靠前的参数、
模块级变量、`New` 构造与函数调用：

```vbnet
' vbs ./test/test_default_expression.vb
Class testdata
    Public text As String

    Sub New(text As String)
        Me.text = text
    End Sub
End Class

Dim prefix = "[vbs] "     ' 模块级变量也可以被默认值表达式引用

Function test(a As String, Optional b As Boolean = True,
              Optional c As testdata = If(b, New testdata(a), New testdata("default"))) As String
    Return c.text
End Function

Call print(test(a:="xxxxx", b:=False))   ' => "default"
Call print(test(a:="hello"))             ' => "hello"
```

引擎在**预处理阶段**把它改写成等价且可以被 Roslyn 编译的代码，脚本作者不需要做任何额外声明。

##### 14.1 改写规则

**① 声明**: 带默认值的参数一律去掉 `Optional` 与 `= 默认值`，变为必填参数：

```vbnet
Function test(a As String, Optional b As Boolean = True, Optional c As testdata = If(b, ...)) As String
' =>
Function test(a As String, b As Boolean, c As testdata) As String
```

> 之所以必须去掉 `Optional`：运行期发射路径会把顶层函数重写为 `Main` 之中的匿名函数
> (见第 4 节)，而 VB 不允许 lambda 参数声明为 `Optional`(`BC33010`)。
> 参数变成必填同时也保证了「绝不静默取到 `Nothing`」—— 任何**没有被改写到的**调用点
> 都会在编译期报「未提供参数」，而不是静默地拿到 `Nothing`。

**② `Function` 的调用点**: 按需生成一个**桥接函数**，调用点只把被调用名替换为桥接函数名、
实参表原样保留：

```vbnet
Call print(test(a:="xxxxx", b:=False))
' => Call print(test_defaults(__vbs_test_a:="xxxxx", __vbs_test_b:=False))

' 脚本末尾(紧随原函数之后)生成的桥接函数:
Function test_defaults(__vbs_test_a As String, __vbs_test_b As Boolean) As String
    Dim __vbs_test_c = If(__vbs_test_b, New testdata(__vbs_test_a), New testdata("default"))
    Return test(__vbs_test_a, __vbs_test_b, __vbs_test_c)
End Function
```

由于只做名字替换，`While test(a, b)`、`If test(...) Then`、`print(test(...))` 这类
**任意表达式位置**都能被改写，且不改变求值顺序与具名实参语义。桥接函数按「实际提供的参数子集」
去重，因此同一函数的不同调用形态各生成一个(`test_defaults`、`test_defaults2` ...)。

**③ `Sub` 的调用点**: VB 之中 Sub 调用只能是语句，因此直接**就地展开**为多行，
不引入桥接函数(也就不会给 Sub 的热点调用增加一次函数调用)：

```vbnet
Call emit(a:="inline")
' =>
Dim __vbs_emit_a_2 = "inline"
Dim __vbs_emit_c_3 = New testdata(__vbs_emit_a_2)
Call emit(__vbs_emit_a_2, __vbs_emit_c_3)
```

展开次序固定为「被提供的实参(书写顺序) → 被省略的参数(声明顺序) → 完整调用」，
与 VB 左到右求值以及「先算实参再算默认值」的语义一致。

**④ 语句容器展开**: 当 Sub 调用所在的结构容不下多行语句时，先把它展开为块：

```vbnet
Dim handler = Sub() Call emit(a:="from-lambda")
' =>
Dim handler = Sub()
                  Dim __vbs_emit_a_8 = "from-lambda"
                  Dim __vbs_emit_c_9 = New testdata(__vbs_emit_a_8)
                  Call emit(__vbs_emit_a_8, __vbs_emit_c_9)
              End Sub

If True Then Call emit(a:="from-ifthen")
' =>
If True Then
    Dim __vbs_emit_a_10 = "from-ifthen"
    ...
End If
```

##### 14.2 不变的部分

- **常量默认值**不参与改写：它们被原样搬到桥接函数里(`Dim __vbs_test_b = True`)，语义不变；
- **提供了全部参数**的调用点保持原样；
- 不含非常数默认值表达式的函数完全不受影响；
- 生成的桥接函数名(`<函数名>_defaults`)与临时变量名(`__vbs_<函数名>_<参数名>_<序号>`)
  都会对全文做符号冲突检查。

##### 14.3 保守策略与已知局限

改写器只做文本级分析(不建立 Roslyn 语义模型)，任何不确定都**放弃改写** —— 于是保持原有行为，
而这类写法在本功能引入之前本来也无法编译，因此不存在行为回归。放弃改写的条件：

| 情形 | 例子 |
|------|------|
| 含 `ParamArray` 或方法泛型参数表 | `Function f(Of T)(x As T, Optional y As T = ...)` |
| 参数类型子句带括号(既有 `ToLambdaSignature` 同样不支持) | `Optional x As List(Of Integer) = ...` |
| 参数表跨物理行 | 续行书写的签名 |
| 类型块内部存在同名成员(名字遮蔽) | `Class A : Function test(...) : End Class` |
| 同名顶层函数(重载) | 两个 `Function test(...)` |
| 默认值引用了自身或更靠后的参数 | `Optional b As Integer = c + 1` |

已知局限：

- 只处理**主脚本顶层**的 `Function`/`Sub`，类型定义块(`Class`/`Module`/`Structure`/`Interface`/`Enum`)
  内部的方法不处理；
- 默认值表达式本身不能跨物理行续行；
- Sub 的**无括号调用**(`test a, b`)、`Else`/`ElseIf` 分支内嵌的单行调用、冒号分隔的多语句行
  会被退化为「参数必填」而在编译期报错(不会静默出错)；
- 只含常量默认值的可选参数(即没有出现任何非常数默认值表达式的函数)不在本特性的处理范围内，
  它们在运行期路径之中仍会受 `BC33010` 限制 —— 这与本功能引入之前的行为一致；
- 顶层函数的参数名若与顶层变量同名，运行期路径会报 `BC36641`(VB 不允许 lambda 参数遮蔽外层变量)，
  这也是既有行为；桥接函数内部一律使用 `__vbs_` 前缀的混淆名字，因此不会**新增**这类冲突。

`--verbose` 会打印改写点数量、生成的桥接函数名、就地展开的 Sub 名与被退化为必填的参数：

```text
----- default parameters: 11 处调用点改写, 4 个桥接函数, 1 个 Sub 就地展开 -----
    inline subs: emit
    bridges: test_defaults2, test_defaults3, test_defaults4, greet_defaults
```

## 转换为正式的 vbproj 工程(make-project)

脚本调试完成之后，可以用 `make-project` 子命令把它**就地**转换为一个正式的 VB.NET 工程：

```bash
vbs make-project ./run.vb [--verbose] [--no-build] [--force] [--runtime <dll>]
```

| 选项 | 说明 |
|------|------|
| `--verbose` | 输出解析细节(解析出的程序集、nuget 包等)与异常堆栈 |
| `--no-build` | 只生成工程文件，不执行构建验证 |
| `--force` | 覆盖已经存在的工程(会先清空 `src/` 目录) |
| `--runtime <dll>` | 指定 sciBASIC 运行时程序集(`Microsoft.VisualBasic.Runtime.dll`)，默认为引擎自身使用的副本 |
| `--no-vectorize` | 关闭数值向量的自动向量化改写(`@` 投影仍然生效) |

### 产出

全部产出位于脚本所在目录，**原始脚本文件保持不动**(但不再参与编译)：

```
run.vb               ' 原始脚本(保留为备份)
run.vbproj           ' 生成的 SDK 风格工程
src/
 ├── Program.vb            ' 由脚本顶层语句/顶层函数/类型定义重构成的代码
 ├── VBScriptHostMagics.vb ' 魔法方法物化之后的普通源码文件
 └── Helper.vb             ' 每一个 #include 引入的脚本各占一个文件
```

工程只编译 `src/` 下由本命令生成的源码(`EnableDefaultCompileItems=false`)，因此目录之中的其它 `.vb` 文件不会被误编译。
生成之后默认执行 `dotnet build` 验证；本机没有 `dotnet` CLI 时会降级为警告而不中断。

### 代码如何被"转正"

| 脚本写法 | 转换之后的正式代码 |
|----------|--------------------|
| 顶层可执行语句 | `Public Function Main(argv As String()) As Integer` 之中的语句 |
| `?"--a"` 参数语法 | 模块级字段 `Private args As CommandLine`(在 `Main` 内由 `CommandLine.BuildFromArguments` 赋值)，语句展开为 `args("--a")` |
| 顶层 `Function`/`Sub` | 模块内真正的 `Private Function`/`Private Sub`(不再是匿名函数，因此不再需要落位求解) |
| 被顶层函数捕获的顶层变量 | 提升为模块级字段(否则函数无法访问)；脚本没有顶层函数时不做任何提升，全部保持为 `Main` 的局部变量 |
| `let` 声明 | 保持为 `Private x = ...` 的动态类型(Object)语义 |
| 元组分解语法 | 展开为多条独立的 `Dim` 语句 |
| 默认参数表达式 | 声明去掉 `Optional`；`Function` 生成模块级 `Private Function` 桥接函数，`Sub` 的调用点就地展开为多行临时变量 |
| 魔法方法 | 物化为 `src/VBScriptHostMagics.vb` 之中的普通方法，脱离脚本引擎也可以直接调用 |
| 类型定义 | 位于工程命名空间 `DynamicDll` 之下的顶层类型 |
| `#include "xxx.dll"` | `<Reference Include="xxx"><HintPath>...</HintPath></Reference>`(指向引擎自身程序集的引用会被自动跳过) |
| `#include "pkg@version"` | `<PackageReference Include="pkg" Version="version" />`(只输出根包，传递依赖交给 NuGet restore) |
| `#include "./lib/Helper.vb"` | 独立的 `Compile` 源码文件 `src/Helper.vb` |

### 元数据指令到工程属性的映射

| 指令 | 工程属性 |
|------|----------|
| `#package "X"` | `<AssemblyName>X</AssemblyName>` |
| `#title "X"` | `<AssemblyTitle>X</AssemblyTitle>` 与 `<Product>X</Product>` |
| `#author "X"` | `<Authors>X</Authors>` 与 `<Company>X</Company>` |
| `#version "X"` | `<Version>X</Version>` 与 `<AssemblyVersion>X</AssemblyVersion>` |

工程文件的其余设置：`OutputType=Exe`、`TargetFramework=net10.0`、`LangVersion=16`、
`Option Strict Off` / `Option Explicit On` / `Option Infer On`(与脚本引擎的运行期语义保持一致)，
并且**显式置空 `<RootNamespace>`** —— 否则 SDK 会以工程名作为根命名空间前缀，导致脚本之中以 `DynamicDll.` 限定的类型引用失效。

> 注意：生成的工程通过 `<Reference>` + `<HintPath>` 指向 `Microsoft.VisualBasic.Runtime.dll`(sciBASIC 运行时，提供 `CommandLine` 类型)。如果需要在其它机器上构建，请用 `--runtime` 指定该程序集，或者自行改写为对应的 `<PackageReference>`。

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
| `Program.vb` | `vbs` 命令行入口：子命令分发(`make-project` / 脚本执行)，串联 解析 -> 编译 -> 执行 流程 |
| `src/DynamicDll.vb` | Roslyn 内存编译（引用收集与按程序集名去重、编译、IL 发射与加载）与反射调用 |
| `src/MakeProject.vb` | `make-project` 子命令：编排工程转换、写出 `<name>.vbproj` 与 `src/*.vb`、调用 `dotnet build` 验证 |
| `src/VBScript/VBScript.vb` | 脚本解析入口：串联 `#include` 解析 -> 元数据解析 -> 代码重构 |
| `src/VBScript/IncludeDirective.vb` | `#include` 指令模型与解析器：dll / 脚本 / nuget 三类目标的判别、脚本引用的递归展开与合法性校验 |
| `src/VBScript/ScriptStructure.vb` | 脚本静态结构模型与逐行块扫描器(头部语句/类型定义块/顶层函数块/顶层语句槽位) |
| `src/VBScript/ScriptRefactor.vb` | 文本预处理与**运行期**代码发射器：顶层函数重写为匿名函数、按依赖求解落位、组装固定容器结构；并注入 `print` 调试打印转发函数(遮蔽 VB 运行时的同名成员，见"脚本语法-13") |
| `src/VBScript/ProjectCodeBuilder.vb` | **工程期**代码发射器：标准 `Main` 入口、顶层函数还原为模块级 `Private Function/Sub`、捕获变量提升为字段 |
| `src/VBScript/ScriptMetadata.vb` | 程序集元数据指令(`#package/#author/#title/#version`)解析与 assembly 特性生成 |
| `src/VBScript/LetStatement.vb` | `let` 动态类型声明展开，并区分 LINQ 查询之中的 `Let` 子句 |
| `src/VBScript/Magics.vb` | 脚本上下文魔法方法源码生成器(运行期注入 / 工程期物化为源码文件) |
| `src/VBScript/TupleDestructuring.vb` | 元组分解语法展开 |
| `src/VBScript/Syntax/DefaultParameterExpression.vb` | **默认参数表达式预处理阶段入口**：顶层声明收集、Function 桥接路径与 Sub 就地展开路径的调用点编排、声明改写、覆盖判定与改写报告 |
| `src/VBScript/Syntax/DefaultParameterSignature.vb` | 顶层函数签名与参数的文本级解析模型(常量/非常数默认值分类)，桥接函数源码发射，参数名到混淆名/临时变量名的词元级替换 |
| `src/VBScript/Syntax/Vectorization/Vectorization.vb` | **向量化预处理阶段入口**：`#no-vectorize` / `#vectorize` 指令处理、逐行驱动、改写报告 |
| `src/VBScript/Syntax/Vectorization/VectorType.vb` | 数值类型模型与 VB 逐元素类型提升规则(`Promote` / `DivideKind` / `PowerKind` / `IntegerDivideKind` / `ModuloKind`) |
| `src/VBScript/Syntax/Vectorization/ObjectMemberTable.vb` | 用 Roslyn 解析脚本类型定义块，得到「类型名 → 成员名 → 成员类型」的成员表，供 `@` 投影解析元素类型 |
| `src/VBScript/Syntax/Vectorization/PropertyProjection.vb` | `@` 数组投影运算符的文本级展开(掩码字符串/注释、单成员/多成员/链式、类型不可解析时不展开) |
| `src/VBScript/Syntax/Vectorization/VectorExpressionRewriter.vb` | 向量化改写核心：基于 Roslyn 的语句解析与浅层类型推断、识别数组字面量与投影形式、按字符区间回写原行 |
| `src/VBScript/Syntax/Vectorization/SimdVocabulary.vb` | 运算符/数学函数/聚合归约 → `Vec*` 调用的映射与文本发射(含 `VecConvert` 的插入位置决策) |
| `src/VBScript/ScriptParseResult.vb` | 解析结果数据对象(程序集元数据、引入的脚本与 nuget 包、解析后的程序集列表、向量化开关) |
| `src/VBScript/ScriptRuntime.vb` | 脚本运行时：封装动态 assembly 的执行与卸载(`IDisposable`) |

nuget 客户端位于 `dev/VisualStudio` 项目的 `VBProject/NuGet/` 目录(命名空间 `VBProj.NuGet`)，
作为该库的可复用能力由 `VBS.vbproj` 通过 ProjectReference 引用：

| 文件 | 职责 |
|------|------|
| `NuGetVersion.vb` | 语义化版本号与版本范围(`[1.2.3]` / `[1.0,2.0)` / `1.2.3` …)的解析、比较与排序 |
| `NuGetFramework.vb` | 目标框架 moniker(TFM)的兼容等级、最优匹配与当前运行平台标识 |
| `NuGetClient.vb` | nuget.org flat-container 客户端：版本索引、nupkg 流式下载、全局包目录缓存复用与原子写入 |
| `NuGetResolver.vb` | nuspec 依赖组解析、传递依赖广度优先展开、版本冲突消解与资产(lib/runtimes)选择 |
| `NuGetPackage.vb` | 解析结果模型(包/依赖/资产)与 nuget 相关异常类型 |

向量化的运行期后端位于 sciBASIC 运行时(`Microsoft.VisualBasic.Runtime`)之中：

| 文件 | 职责 |
|------|------|
| `Microsoft.VisualBasic.Core/src/Math/SIMD/Vectorized.vb` | 向量化运算词汇表(`Namespace Math.SIMD.Vectorization` + `Public Module Vectorized`，全 `Vec*` 成员)：把「数值类型 × 运算形态」一次补全，内部转调既有 SIMD 内核(`SimdEngine` / `SimdMath` / `SimdReduce`)，框架/硬件无内核的运算(整除 `\`、取余 `Mod`、乘积归约、逐元素映射)退化为标量循环 |

> 为什么改写器不复用 `SimdExtensions` 的 `Simd*` 成员：VB 对「两个已导入模块之中的同名成员」
> 会直接报 `BC30562`(名称不明确)，即使签名不同、即使调用处写了显式泛型实参也无法化解。
> 因此词汇表统一使用 `Vec*` 前缀，并被刻意放在 `Math.SIMD.Vectorization` 子命名空间 ——
> 生成代码只需要 `Imports Microsoft.VisualBasic.Math.SIMD.Vectorization` 一条导入语句，
> 既看不到 `SimdExtensions`，也不会把 `Math.SIMD` 之中那些泛化命名的历史门面类
> (`Add`/`Subtract`/`Multiply`/`Divide`/`Modulo`/`Exponent`)带进脚本作用域。

数据打印(`print`)的运行期实现同样位于 sciBASIC 运行时之中：

| 文件 | 职责 |
|------|------|
| `Microsoft.VisualBasic.Core/src/Printing/Print.vb` | `Printing.Print` 公开 API：集合的 R 向量打印、`NumericTable` 的表格打印、按运行时类型分派的 `print(data As Object, ...)` 与 `ToText(...)` |
| `Microsoft.VisualBasic.Core/src/Printing/RFormat.vb` | R 风格格式化内核：元素/数值格式化、折行与行首下标对齐、嵌套集合的 list 格式、`ConsoleTableBuilder` 表格渲染 |
