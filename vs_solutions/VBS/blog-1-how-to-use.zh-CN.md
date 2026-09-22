# 用 vbs 直接运行 VB.NET 脚本：VBS 脚本引擎使用指南（How to Use）

> 本文是 VBS 脚本引擎系列的第一篇，讲解**如何使用**这个引擎来运行 VB.NET 脚本。
> 关于引擎内部是如何实现的，请参见系列第二篇：《VBS 脚本引擎是如何运行一个 VB.NET 脚本的（How to Implement）》。
> 文中全部示例均来自仓库 `tutorials/VBS/scripts/` 文件夹，可以直接运行。

## 1. VBS 是什么？

`vbs` 是一个用 VB.NET 编写的 **VB.NET 脚本引擎宿主程序**（源码位于 `vs_solutions/VBS/VBS.vbproj`）。它让你可以直接运行一个 `.vb` 脚本文件，而不需要：

- 先在 Visual Studio 里创建工程；
- 手动管理 `.vbproj`、引用、NuGet 依赖；
- 编译成 exe 再运行。

你只需要写一个 `.vb` 文件，然后：

```bash
vbs ./run.vb
```

引擎会在运行时对脚本源代码做文本解析与结构重构，再基于 Roslyn 在内存中编译为 assembly 并通过反射执行，全程不落盘。

除了标准 VB.NET 语法之外，脚本引擎还提供了一系列语法扩展，让脚本写起来更"脚本化"：

- **顶层语句**：代码可以直接写在文件顶层，不需要包在 `Sub Main` 里；
- **命令行参数语法**：`?"--a"` 一行取参数；
- **`#include` 指令**：引用 dll、其它脚本、甚至 nuget 包；
- **`let` 动态类型**：类似 Python 的动态变量；
- **元组分解**：`Dim (a, b) = GetTuple()`；
- **默认参数表达式**：可选参数的默认值可以是任意表达式；
- **向量化计算**：数值数组直接写 `x * y + 1`，引擎自动展开为 SIMD 逐元素运算；
- **`@` 数组投影**：`list@x` 把对象数组的成员投影成新数组（Perl 风格）；
- **`print` 数据打印**：按 GNU R 的格式打印向量、表格。

## 2. 构建与运行

### 2.1 构建

```bash
cd G:\GCModeller\src\runtime\sciBASIC#
dotnet build vs_solutions/VBS/VBS.vbproj -c Release
```

构建产物输出到 `.nuget\net10.0\`，入口程序为 `vbs.exe`。

### 2.2 运行第一个脚本

```bash
cd .nuget\net10.0
vbs.exe G:\GCModeller\src\runtime\sciBASIC#\tutorials\VBS\scripts\tuple\tuple.vb
```

命令行格式为：

```bash
vbs.exe <脚本文件路径> [--参数名=参数值 ...] [--verbose] [--no-vectorize]
```

| 选项 | 说明 |
|------|------|
| `--verbose` | 在控制台打印重构后生成的完整可编译代码，并以 Debug 模式编译。排查语法问题时非常好用 |
| `--no-vectorize` | 关闭数值向量的自动向量化改写 |
| `make-project <script.vb>` | 子命令：把脚本就地转换为正式的 vbproj 工程 |

## 3. 一个最小可运行的脚本

先看一个最小的 hello world（顶层语句）：

```vbnet
' hello.vb —— 不需要 Module、不需要 Sub Main
Call Console.WriteLine("hello world!")
```

直接运行：

```bash
vbs hello.vb
```

这就是 VBS 与"正式 VB.NET 程序"最大的区别：**代码语句可以直接写在文件顶层**。在标准的 VB.NET 中，可执行语句必须放入类型容器（例如 `Module` 里的 `Sub Main`）；而在 VBS 里，引擎会在解析阶段自动把这些顶层语句放进化虚拟的 `Main` 方法中。

## 4. 获取命令行参数

脚本可以直接通过 `?"--名称"` 语法获取命令行参数：

```vbnet
' vbs ./run.vb --a=123 --flag
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"

Call Console.WriteLine($"a = {A}, flag = {B}")
```

运行：

```bash
vbs run.vb --a=123 --flag
```

`?"--a"` 本质上是对引擎创建的虚拟 `Main(args As CommandLine)` 方法中 `args` 变量的引用，预处理阶段会被改写为 `args("--a")`。`CommandLine` 会根据声明类型自动完成类型转换（数值、布尔开关等），例如上面的 `--flag` 开关会被解析为 `Boolean`。

## 5. 顶层函数

顶层 `Function` / `Sub` 定义没有类型容器，引擎会自动把它们重写为 `Main` 内部的局部匿名函数：

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

不需要关心声明顺序——引擎会根据每个匿名函数所捕获的顶层变量、以及所调用的其它顶层函数，自动求解其在 `Main` 中的插入位置，因此上面的代码可以按任意顺序书写。

## 6. 引用外部依赖（#include）

`#include` 预编译指令支持三类引用目标，语法统一为 `#include "<target>"`。

### 6.1 引用外部 DLL 程序集

```vbnet
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "./libs/mylib.dll"    ' 相对路径也可以
```

路径可以是绝对路径或相对路径。相对路径会依次在下列目录中探测：

1. 脚本文件所在的文件夹；
2. 引擎程序目录 `App.HOME`；
3. `App.HOME/libs`；
4. `App.HOME` 上级目录下的 `libs/`。

一个完整的例子——`tutorials/VBS/scripts/linear_regression/linear_regression.vb`：

```vbnet
#include "Microsoft.VisualBasic.Data.Bootstrapping.Fittings.dll"
#include "Microsoft.VisualBasic.Data.Framework.dll"
#include "Microsoft.VisualBasic.Data.DataPlot.dll"
#include "Microsoft.VisualBasic.Drawing.dll"

imports Microsoft.VisualBasic.Data
imports microsoft.visualbasic.data.plots
imports microsoft.visualbasic.drawing

' 1. 构造带噪声的线性数据集 y = 2x + 3 + noise
dim n = 100
dim y(n - 1) as double

for i = 0 to n - 1
    dim x = i * 0.1
    dim noise = ((i mod 7) - 3) * 0.1
    y(i) = 2.0 * x + 3.0 + noise
next

' 2. 建模、预测、绘图、导出 csv
dim model = table.LinearFit(y := "y")
call SkiaDriver.Register()

Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Plot(DataSerials(xs, ys, class_id).tolist())
    plt.SavePng(here("linear-regression.png"), 300)
End Using
```

运行之后会直接产出回归拟合图 `linear-regression.png` 与结果表 `linear-regression.csv`。可以看到，脚本可以做"读取数据 → 机器学习 → 可视化 → 导出文件"的完整数据科学流水线。

### 6.2 引用其它 VB 脚本

`#include` 的目标以 `.vb` 结尾时，目标脚本的代码会被直接复制到主脚本的顶层命名空间之中：

```vbnet
' ./lib/Helper.vb
Public Class LibraryHelper
    Public Shared Function Greet(name As String) As String
        Return $"hello, {name}!"
    End Function
End Class
```

```vbnet
' 主脚本 main.vb
#include "./lib/Helper.vb"

Call Console.WriteLine(LibraryHelper.Greet("vbs"))
```

规则要点：

- 被引入脚本可以再次 `#include` 其它脚本（引擎会递归展开并去重，循环引用会报错）；
- 被引入脚本只应该包含 `Imports` 与类型定义——不允许顶层可执行语句、顶层函数、魔法方法与 `#package` 等元数据指令，违反时引擎会指明文件与原因。

### 6.3 引用 nuget 程序包

```vbnet
#include "Newtonsoft.Json"               ' 自动取最新稳定版
#include "Newtonsoft.Json@13.0.3"        ' 精确版本
#include "Microsoft.Extensions.Logging@[8.0,9.0)"   ' 版本范围
```

nuget 引用的能力包括：

- **传递依赖自动解析**：按 nuspec 依赖组广度优先递归展开；
- **版本冲突消解**：同一包被多处要求时取满足全部约束的最低版本；
- **本地缓存**：包解压到 `~/.nuget/packages/`，与 NuGet 官方布局一致，可以直接复用 Visual Studio / `dotnet restore` 已经装好的包，缓存命中时零网络开销。

示例见 `tutorials/VBS/scripts/../test/test_include_nuget.vb`。

## 7. 程序集元数据指令

脚本头部可以声明动态编译 assembly 的元数据：

```vbnet
#package "MyScript.Package"     ' assembly 名称
#author  "xieguigang"           ' AssemblyCompany
#title   "My First Script"      ' AssemblyTitle
#version "1.2.3.4"              ' AssemblyVersion
```

在脚本中可以用魔法方法把这些值读回来（见下一节）。

## 8. 魔法方法

引擎会在预处理阶段把脚本上下文信息（脚本路径、元数据、依赖搜索目录）以常量形式烘焙进生成代码，脚本无需任何 import 即可调用：

| 分组 | 方法 | 说明 |
|------|------|------|
| 脚本上下文 | `ScriptDir()` | 脚本所在文件夹 |
| | `ScriptFile()` | 脚本绝对路径 |
| | `ScriptName()` | 脚本文件名 |
| | `Here(relpath)` | 相对路径 → 相对脚本目录的绝对路径 |
| | `ScriptText()` / `ScriptLines()` | 读取脚本自身源码 |
| | `Self()` | 脚本自身编译得到的 Assembly |
| 元数据 | `Package()` / `Author()` / `Title()` / `Version()` / `Meta(key)` | 读回 `#` 指令值 |
| 依赖定位 | `Includes()` | 全部 `#include` 程序集绝对路径 |
| | `Locate(name)` | 按 `#include` 相同的搜索顺序定位文件 |

最常用的是 `Here()`——脚本需要读写同目录下的数据文件时：

```vbnet
dim file = here("../../data/bezdekIris.csv")
dim table = NumericTableIO.ReadCsv(file, columns := {"D1","D2","D3","D4"})
```

这一行正是 `tutorials/VBS/scripts/kmeans/kmeans.vb` 的开头：读取经典 Iris 数据集，做 KMeans 聚类 + PCA 降维 + 散点图可视化。

## 9. let 动态类型

`Dim` 声明时由 Roslyn 自动类型推断，得到的是强类型变量；而 `let` 声明会被引擎预处理为 `Object` 类型，可以在运行时重新绑定任意类型并动态访问成员：

```vbnet
Dim strong = "hello"     ' 推断为 String，强类型
let dynamic = "hello"    ' 预处理为 Dim dynamic As Object = "hello"
```

完整示例（`tutorials/VBS/scripts/dynamic_type/dynamic_type.vb`）：

```vbnet
Dim a as integer
a = 123
console.writeline($"a := {a}")

let b = 456
console.writeline($"b := {b}")

' b 重新绑定为匿名类型，动态访问其字段
b = new with {.a = 123, .b = 456}

console.writeline($"b.a := {b.a}")
console.writeline($"b.b := {b.b}")

' 再次重新绑定为 Boolean
b = false

console.writeline($"check boolean := {b}")
```

运行输出：

```text
a := 123
b := 456
b.a := 123
b.b := 456
check boolean := False
```

一个变量先当数字、再当对象、最后当布尔值——这就是 `let` 带来的动态类型体验。引擎会自动区分 LINQ 查询表达式之中的 `Let` 子句，查询语句不会被误改写。

## 10. 元组分解

脚本支持元组分解语法，包括嵌套分解与 `For Each` 中的分解。完整示例（`tutorials/VBS/scripts/tuple/tuple.vb`）：

```vbnet
' 一行把元组分解为两个变量（可带类型标注）
Dim (a, b as double) = (1, 2)

console.writeline($"a := {a}")
console.writeline($"b := {b}")
console.writeline($"(a+b) := {a + b}")

' 元组数组字面量
dim tuples = {
    ("a", 123), ("b", 456), ("c", 789)
}

' For Each 迭代时逐项分解
for each (str as string, int) in tuples
    call console.writeline($"{str} => {int:F4}")
next
```

运行输出：

```text
demo test result of variable tuple deconstruct in vb:
a := 1
b := 2
(a+b) := 3
a => 123.0000
b => 456.0000
c => 789.0000
```

支持的写法还包括：跳位分解 `Dim (a, , c) = GetTuple()`、嵌套分解 `Dim (p, (q, r)) = GetNested()`、命名别名 `Dim (name:=v1, score:=v2) = GetTuple()`。

## 11. 向量化计算

这是 VBS 最有特色的功能：**声明为数值数组的变量在参与数学运算时，引擎自动把标量写法展开为等价的逐元素 SIMD 调用**，脚本作者完全不需要手写 `For` 循环：

```vbnet
Dim x = {1, 2, 3, 4, 5}
Dim y = {2, 3, 4, 5, 1}

Dim sum = x + 5                 ' => VecAddScalar(Of Integer)(x, 5)
Dim z = (x * y + 6) / (x + y)   ' 整棵表达式树一次性展开
```

用 `--verbose` 可以看到重构结果：

```vbnet
Dim sum = VecAddScalar(Of Integer)(x, 5)
Dim z = VecDivide(
            VecConvert(Of Integer, Double)(VecAddScalar(Of Integer)(VecMultiply(Of Integer)(x, y), 6)),
            VecConvert(Of Integer, Double)(VecAdd(Of Integer)(x, y)))
```

要点：

- 支持的元素类型：`Short` / `Integer` / `Long` / `Single` / `Double` 五种一维数值数组；
- 类型提升遵循 VB 逐元素语义，例如 `Integer() / Integer()` 得 `Double()`、`^` 恒为 `Double`；
- 数学函数同样向量化：`Math.Sqrt(v)` → `VecSqrt`、`Math.Abs(v)` → `VecAbs`；
- 聚合归约：`x.Sum()`、`x.Average()`、`x.Min()`、`x.Max()`、`x.Product()`；
- 后端基于 `System.Numerics.Vector`（SSE2/AVX/ARM SIMD）硬件加速；
- 不确定如何改写时引擎会保守放弃（保持原样），绝不会生成猜测性代码；
- 可以用命令行 `--no-vectorize` 或脚本头部 `#no-vectorize` 关闭。

## 12. @ 数组投影运算符

对**对象数组**可以用 Perl 风格的 `@` 把成员直接投影成新数组：

```vbnet
Class CLRObjectType
    Property x As Double
    Property y As String
End Class

Dim list As CLRObjectType() = { ... }

Dim x = list@x          ' => Double()
Dim y = list@y          ' => String()

' 投影结果自动接入向量化
Dim z = list@x + {2, 3, 4, 5, 6, 7, 8, 9}
```

`list@x` 等价于 `list.Select(Function(o) o.x).ToArray()`，但它是一等语法糖，而且投影得到的数组可以直接继续参与向量化运算。多成员投影 `list@{x, y}` 会生成匿名类型数组；`list@inner@x` 支持链式逐级投影。

## 13. print 数据打印

脚本里可以直接调用引擎注入的 `print(...)`，按 **GNU R 的格式**打印数据，调试时非常直观：

```vbnet
Dim x = {1, 3, 24, 23, 4, 23}
Call print(x)
' [1]  1  3 24 23  4 23

Call print(42)          ' [1] 42
Call print(1.0 / 3)     ' [1] 0.3333333
Call print("done")      ' [1] "done"
```

方括号里是本行第一个元素的下标（与 R 一致）；超过宽度（缺省 80 字符）自动折行；`width` 与 `digits` 是可选参数；`Nothing` 元素打印为 `NA`；`NumericTable`（相当于 R 的 data.frame）按带边框的表格打印。

## 14. 默认参数表达式

标准 VB.NET 只允许常数作为可选参数默认值，VBS 放宽为**任意表达式**：

```vbnet
Dim prefix = "[vbs] "

Function test(a As String, Optional b As Boolean = True,
              Optional c As testdata = If(b, New testdata(a), New testdata("default"))) As String
    Return c.text
End Function

Call print(test(a:="hello"))            ' => "hello"
Call print(test(a:="xxxxx", b:=False))  ' => "default"
```

默认值表达式可以引用更靠前的参数、模块级变量、`New` 构造与函数调用。

## 15. 转换为正式工程（make-project）

脚本调试完成之后，可以用 `make-project` 子命令把它**就地**转换为正式的 VB.NET 工程：

```bash
vbs make-project ./run.vb [--verbose] [--no-build] [--force]
```

产出（原始脚本保留为备份）：

```
run.vb               ' 原始脚本（备份）
run.vbproj           ' 生成的 SDK 风格工程
src/
 ├── Program.vb            ' 顶层语句/顶层函数/类型定义重构后的代码
 ├── VBScriptHostMagics.vb ' 魔法方法物化之后的普通源码
 └── Helper.vb             ' 每个 #include 引入的脚本各占一个文件
```

转换规则举例：顶层语句进入 `Main`；顶层函数还原为模块级 `Private Function`；被捕获的顶层变量提升为字段；`#include "pkg@ver"` 转为 `<PackageReference>`；`#package` 等指令映射为工程属性。生成之后默认执行 `dotnet build` 验证。

也就是说：**用脚本快速原型 → 验证成功后一键转正为工程**，这是 VBS 设计上非常顺手的开发工作流。

## 16. 示例脚本导览

`tutorials/VBS/scripts/` 文件夹中的每一个子文件夹都是一个完整可运行的 demo，多数带有 `stdout.txt`（预期输出）与产出的图片：

| Demo | 内容 | 展示的特性 |
|------|------|------------|
| `tuple` | 元组分解基础 | 元组语法 |
| `dynamic_type` | 动态变量重绑定 | `let` 动态类型 |
| `linear_regression` | 线性回归 + 拟合图导出 | `#include` dll、`here()`、数据科学流水线 |
| `kmeans` | Iris 数据集聚类 + PCA + 散点图 | `NumericTable`、`here()` 读取数据 |
| `word2vec` | 词向量训练 + UMAP + KMeans | NLP 与机器学习全流程 |
| `hola_layout` | 网络图 HOLA 正交布局 | 复杂类型定义、顶层函数、图形渲染 |
| `mnist_cnn` / `spiking_nn` / `tf-idf` / `hierarchical_clustering` / `mnist_umap` | 深度学习与统计学习示例 | 大型 `#include` 依赖管理 |

以 `kmeans` 为例，完整运行方式：

```bash
cd .nuget\net10.0
vbs.exe G:\GCModeller\src\runtime\sciBASIC#\tutorials\VBS\scripts\kmeans\kmeans.vb
```

运行后即可在脚本目录看到聚类结果的 PCA 散点图 `bezdekIris-pca-groups.png` 与分组结果 `bezdekIris-kmeans.csv`。其余 demo 的运行方式完全相同。

## 17. 以库的方式嵌入使用

`VBS.vbproj` 除了提供 `vbs` 命令行宿主，还可以作为类库被其他 .NET 程序引用，内嵌脚本执行能力：

```vbnet
Imports VBScriptHost.Script

' 1. 解析脚本
Dim vbs As ScriptParseResult = VBScript.ParseScript("./run.vb")

' 2. 编译为内存 assembly
Using runtime As ScriptRuntime = vbs.CompileScript(
    asmName:="MyScript",
    extraRefs:={"./extra.dll"})

    ' 3. 执行，返回值为脚本退出码
    Dim exitCode As Integer = runtime.Run({"--a", "123"})
End Using   ' Dispose 后动态加载的 assembly 会被卸载
```

## 18. 排错建议

- **编译错误看不清？** 加 `--verbose` 运行，会打印重构后生成的完整可编译代码——你看到的报错行号对应的就是这份生成代码；
- **`#include` 找不到文件？** 检查相对路径是相对脚本文件所在目录解析的；也可以用 `Locate(name)` 魔法方法验证搜索顺序；
- **向量化没有生效？** `--verbose` 会打印改写点数量、识别到的向量变量与「引用了已知向量却未能改写」的行清单；
- **需要与传统 VB.NET 语义对齐？** 用 `--no-vectorize` 关闭 SIMD 改写（`@` 投影仍然生效）。

## 小结

VBS 把 VB.NET 变成了一个可以像 Python / R 一样"打开就写"的脚本语言，同时又完整保留了 .NET 生态的一切能力：nuget 包、外部 dll、强类型、LINQ、异步……再加上向量化、动态类型、元组分解这些语法糖，非常适合数据分析、科学计算与快速原型场景。写完的脚本调试成功后，还能一键 `make-project` 转正为正式工程。

下一篇博客我们将深入引擎内部，看看它是如何把一个"不像 VB.NET 的 VB.NET 脚本"变成可以在内存中编译执行的 assembly 的。
