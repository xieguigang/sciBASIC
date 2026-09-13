# VBS 脚本教程

本文件夹是 **VBS 脚本引擎**（`vs_solutions\VBS\VBS.vbproj`）的示例脚本集合。本文说明如何运行这些 demo，并以此讲解如何编写可被该引擎执行的 `.vb` 脚本。

> 引擎的实现原理与整体设计见 [`vs_solutions\VBS\README.md`](../../vs_solutions/VBS/README.md)，本文在其基础上补充了**实际编写时会遇到的规则、约束与排错方法**。

---

## 目录

- [1. 快速开始](#1-快速开始)
- [2. 引擎是怎么工作的](#2-引擎是怎么工作的)
- [3. 脚本语法](#3-脚本语法)
  - [3.1 顶层代码语句](#31-顶层代码语句)
  - [3.2 获取命令行参数](#32-获取命令行参数)
  - [3.3 引用外部程序集（#include）](#33-引用外部程序集include)
  - [3.4 Imports 与自动导入](#34-imports-与自动导入)
  - [3.5 类型定义块](#35-类型定义块)
  - [3.6 顶层函数与落位规则](#36-顶层函数与落位规则)
  - [3.7 元组分解](#37-元组分解)
- [4. 编写规则与注意事项](#4-编写规则与注意事项)
- [5. 示例脚本导览](#5-示例脚本导览)
- [6. 排错指南](#6-排错指南)

---

## 1. 快速开始

### 1.1 构建引擎

```bat
cd G:\GCModeller\src\runtime\sciBASIC#
dotnet build vs_solutions/VBS/VBS.vbproj -c Release
```

构建产物输出到 `.nuget\net10.0\`，入口程序为 **`vbs.exe`**。

### 1.2 运行脚本

```bat
cd .nuget\net10.0
vbs.exe G:\GCModeller\src\runtime\sciBASIC#\tutorials\VBS\tuple.vb
```

命令行格式为：

```bat
vbs.exe <脚本文件路径> [--参数名=参数值 ...] [--verbose]
```

- 第一个参数必须是脚本文件路径；
- 其后的参数会原样传给脚本（脚本内用 `?"--参数名"` 读取，见 [3.2](#32-获取命令行参数)）；
- `--verbose` 是引擎保留参数：打印**预处理之后生成的完整 VB.NET 代码**，写脚本时排错必用；
- 进程退出码就是脚本 `Main` 的返回值（引擎自动补 `Return 0`）。

### 1.3 在 Visual Studio 中调试

`vs_solutions\VBS\My Project\launchSettings.json` 里已经配置好了启动参数，把 `commandLineArgs` 改成你要跑的脚本路径，直接 F5 即可。

### 1.4 试跑全部示例

```bat
cd .nuget\net10.0
vbs.exe <repo>\tutorials\VBS\tuple.vb
vbs.exe <repo>\tutorials\VBS\kmeans.vb
vbs.exe <repo>\tutorials\VBS\hola_layout.vb
vbs.exe <repo>\tutorials\VBS\cuda.vb
vbs.exe <repo>\tutorials\VBS\mnist_umap.vb
vbs.exe <repo>\tutorials\VBS\word2vector.vb
vbs.exe <repo>\tutorials\VBS\tfidf.vb
vbs.exe <repo>\tutorials\VBS\linear_regression.vb
vbs.exe <repo>\tutorials\VBS\hierarchical_clustering.vb
```

> 上面这些脚本都会把结果图/结果表写到 `Z:\`；其中 `kmeans.vb` / `hierarchical_clustering.vb` / `mnist_umap.vb` / `word2vector.vb` / `tfidf.vb` 还需要读取外部数据文件（`linear_regression.vb` 使用的是脚本内部生成的合成数据）。若相应数据文件或盘符不存在，请按需修改脚本里的路径。

---

## 2. 引擎是怎么工作的

VBS 分三步把一个 `.vb` 文件跑起来：

```
脚本源码
  -> [1] 预处理: 解析 #include、展开 ?"--a" 与元组分解、扫描代码块结构
  -> [2] 组装: 生成固定容器 Namespace DynamicDll / Module Program / Main
  -> [3] Roslyn 内存编译 + 反射调用 Main，全程不落盘
```

固定容器长这样（这三点在引擎里是常量，不要与脚本中的符号重名）：

| 固定项 | 值 |
| --- | --- |
| 命名空间 | `DynamicDll` |
| 模块 | `Program` |
| 入口函数 | `Public Function Main(args As CommandLine) As Integer` |

### 一个完整的 before / after

脚本源码：

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

经引擎预处理后（`--verbose` 的实际输出）：

```vbnet
Option Strict Off
Option Explicit On
Option Infer On

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic
Imports System.Linq
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Xml.Linq
Namespace DynamicDll
    Module Program

        Public Function Main(args As CommandLine) As Integer
            Dim HelloWorld = Function() As String
                Return "hello world!"
            End Function

            Dim A As Integer = args("--a")
            Dim B As Boolean = args("--flag")
            Call console.writeLine(new Test With {.A = A})
            If B Then
              Call Console.writeLine(HelloWorld())
            End If

            Return 0
        End Function

        Public Class Test
            Public A As Integer

            Public Function ToString() As String
               ' 从外部dll模块 "abc.dll" 文件中导入
               Return ABC.Internal.Seed(A)
            End Function
        End Class
    End Module
End Namespace
```

可以读出三条关键变换：

1. **顶层语句**被搬进 `Main` 的方法体；
2. **顶层函数**被重写成匿名函数（`Dim HelloWorld = Function() ...`）；
3. **类型定义块**原样保留，作为 `Module Program` 的嵌套类型。

---

## 3. 脚本语法

脚本语法与正常 VB.NET 基本一致，差别集中在下面几点。**引擎把你的代码整体塞进一个 `Main` 函数里**，因此凡是"不能写在函数体里"的东西，脚本里也写不了。

### 3.1 顶层代码语句

正规的 VB.NET 要求语句、函数、表达式必须位于某个类型容器（Class / Module / Structure）之内；而 VBS 脚本允许它们**直接写在最顶层**：

```vbnet
' 顶层语句
Dim A As Integer = 123

' 顶层控制流块
For i As Integer = 1 To 10
    Call Console.WriteLine(i)
Next

' 顶层 Using 块（kmeans.vb 就是这么画图的）
Using plt As New ScatterPlot(800, 600, PlotTheme.Nature())
    plt.Title = "demo"
    plt.SavePng("Z:/demo.png", 300)
End Using
```

支持的顶层块结构：`Class` / `Structure` / `Interface` / `Enum`、`Function` / `Sub`、多行 lambda 赋值、`If...End If`、`Select Case`、`Try`、`Using`、`SyncLock`、`With`、`Do...Loop`、`While`、`For...Next`。

### 3.2 获取命令行参数

```vbnet
' 运行: vbs ./run.vb --a=123 --flag
Dim A As Integer = ?"--a"
Dim B As Boolean = ?"--flag"
```

`?"--a"` 在预处理阶段会被替换成 `args("--a")`，也就是引擎生成的 `Main(args As CommandLine)` 中的那个 `args`。因此：

- 参数名要和命令行里写的完全一致（含 `--` 前缀）；
- **顶层变量不能命名为 `args`**，否则会与 `Main` 的参数重名（报 `BC30734`）。

### 3.3 引用外部程序集（#include）

```vbnet
#include "Microsoft.VisualBasic.Drawing.dll"
#include "D:\libs\MyLib.dll"
```

- 路径可以是**绝对路径**，也可以是**相对路径**；
- 相对路径按下面顺序依次探测，第一个命中的生效：
  1. 脚本文件所在目录
  2. `App.HOME`（即 `vbs.exe` 所在目录）
  3. `App.HOME/libs`
  4. `App.HOME` 的上一级目录下的 `libs`
- 引擎会把命中的路径统一解析为绝对路径，再交给 Roslyn 作为编译引用；
- 被引用的程序集不存在时，`#include` 那一行只是**解析不到**（不会立刻报错），真正使用到其中类型时才会编译失败。

> 本仓库里 `.nuget\net10.0\` 是 `vbs.exe` 的所在目录，示例脚本 `#include` 的那些 `Microsoft.VisualBasic.*.dll` 都放在那里。

### 3.4 Imports 与自动导入

脚本顶层的 `Imports` 语句会被原样保留，并在其后追加引擎的固定导入：

```
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic
Imports System.Linq
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Xml.Linq
```

**这张表里没有的命名空间，需要自己在脚本里写 `imports`**。常用但需要显式声明的有：

```vbnet
imports System.Reflection       ' cuda.vb 里反射 MethodInfo 用到了
imports System.Text             ' StringBuilder
imports System.Text.RegularExpressions
imports System.IO
```

脚本里写 `imports` 时大小写随意，引擎按 `Imports`/`Option` 前缀识别（不区分大小写）。

### 3.5 类型定义块

`Class` / `Structure` / `Interface` / `Enum` 会**原样**成为 `Module Program` 的嵌套类型，可以正常实例化、也可以反射：

```vbnet
Public Class PearsonMetrics
    Public Shared Function RowSum(x As Single(), cols As Integer, i As Integer) As Single
        Dim sum As Single = 0.0F

        For k As Integer = 0 To cols - 1
            sum += x(i * cols + k)
        Next

        Return sum
    End Function
End Class

' 顶层语句里可以直接反射它
Dim m As MethodInfo = GetType(PearsonMetrics).GetMethod("RowSum")
```

这一点很有用：脚本里的 `Public Shared` 方法是**真正的静态方法**，能拿到 `MethodInfo`；而顶层函数只能拿到匿名函数（见下节）。`cuda.vb` 正是靠类型定义块 + 反射，把脚本里的 VB.NET 函数交给 IL→CUDA 流水线去反编译的。

### 3.6 顶层函数与落位规则

顶层 `Function` / `Sub` 会被重写为 `Main` 里的**局部匿名函数**：

```vbnet
' 脚本里写
public function rndPos() as FDGVector2
    return New FDGVector2(rnd.NextDouble() * 1000.0, rnd.NextDouble() * 1000.0)
End function

' 生成的代码
Dim rndPos = Function() as FDGVector2
    return New FDGVector2(rnd.NextDouble() * 1000.0, rnd.NextDouble() * 1000.0)
End function
```

既然是局部变量 + 闭包，就受 VB 的"先声明后使用"约束。引擎会**按依赖关系自动决定它们放在 `Main` 的哪个位置**：

| 函数情况 | 落位 |
| --- | --- |
| 不引用任何顶层变量、也不调用别的顶层函数 | 放在 `Main` 最前面（可以写在脚本任意位置，包括最后） |
| 引用了顶层变量 `x` | 放在声明 `x` 的那条语句**之后** |
| 调用了另一个顶层函数 `f` | 放在 `f` **之后** |

所以下面这样写是合法的（`hola_layout.vb` 就是这么写的）：

```vbnet
Dim g As New NetworkGraph
Dim rnd As New Random(12345)

' rndPos 捕获 rnd -> 自动排到 Dim rnd 之后
public function rndPos() as FDGVector2
    return New FDGVector2(rnd.NextDouble() * 1000.0, rnd.NextDouble() * 1000.0)
End function

' addNode 捕获 g、调用 rndPos -> 自动排到 rndPos 之后
public sub addNode(label As String)
    Call g.AddNode(New inode With {.label = label})
End Sub
```

**注意**：如果函数捕获的变量在脚本里声明得比调用点还晚，VB 语义下无解，仍会报 `BC32000`。把变量声明放在前面即可。

### 3.7 元组分解

引擎额外支持一行声明多个变量的元组分解语法（由 `TupleDestructuring` 预处理展开）：

```vbnet
Dim (a, b as double) = (1,2)

console.writeLine(a)
console.writeLine(b)
console.writeLine(a+b)
```

会被展开为临时元组变量 + 逐项赋值（每个 `Dim (...)` 使用互不冲突的临时变量名），所以同一个脚本里可以写多个元组声明。参见 `tuple.vb`。

---

## 4. 编写规则与注意事项

| # | 规则 | 说明 |
| --- | --- | --- |
| 1 | 顶层变量不要叫 `args` | 与生成的 `Main(args As CommandLine)` 参数重名，报 `BC30734` |
| 2 | 顶层函数**参数列表里不要出现带括号的类型** | 例如 `Public Function Sum(x As Single()) As Single` 不会被重写成 lambda，会原样留在 `Main` 里导致编译失败。**需要数组参数时，请把函数放进 `Public Class` 里写成 `Shared` 方法**（见 3.5） |
| 3 | 顶层函数要写在它捕获的顶层变量之后 | 见 3.6；违反时报 `BC32000` |
| 4 | 控制流块必须正确闭合 | `For...Next`、`Do...Loop`、`If...End If` 等；未闭合时引擎会把残留代码兜底输出，但生成的代码位置可能不符合预期 |
| 5 | 需要的命名空间自己 `imports` | 引擎只自动导入 [3.4](#34-imports-与自动导入) 列出的那几个 |
| 6 | 只支持能写进函数体的语法 | 例如不能在顶层再写 `Namespace`、`Module`、属性/字段声明等 |
| 7 | 行尾注释用 `'` | 引擎用 `'` 识别注释；注意字符串字面量里若含 `'` 会影响块结构判定 |
| 8 | 不需要写 `Return 0` | 引擎会在 `Main` 末尾自动补 `Return 0`；想返回其它退出码就自己写 `Return <n>` |

---

## 5. 示例脚本导览

按"由浅入深"的顺序阅读效果最好：

| 脚本 | 演示内容 | 涉及语法 |
| --- | --- | --- |
| `tuple.vb` | 最小可运行示例：元组分解后打印 | [3.7 元组分解](#37-元组分解) |
| `linear_regression.vb` | 合成数据 → 线性/多项式回归 → 预测与残差写回标签列 → 散点+拟合线存盘 | `NumericTable` 表入口、`label:` 标签列约定、顶层 `Using` 块 |
| `kmeans.vb` | 读 CSV → kmeans 聚类 → PCA → 散点图存盘 | `#include` 多个程序集、顶层语句、顶层 `Using` 块 |
| `hierarchical_clustering.vb` | 读 CSV → 距离矩阵 → 层次聚类（`hca` / `hcut`）→ 与真实物种交叉表 → PCA 散点图存盘 | 距离矩阵契约、`label:` 标签列约定、顶层 `Using` 块 |
| `hola_layout.vb` | HOLA 正交布局，45 节点 / 61 边网络 | **顶层 `Function` / `Sub` 捕获顶层变量**（[3.6](#36-顶层函数与落位规则) 的标准范例） |
| `tfidf.vb` | TF-IDF 文档距离矩阵 + 热力图 | 顶层 `For` 循环、交错数组、绘图 |
| `mnist_umap.vb` | MNIST 手写数字 → UMAP 降维可视化 | 大数据集加载、长耗时计算 |
| `word2vector.vb` | 文本 → word2vec → UMAP → kmeans 聚类 | 完整数据处理流水线 |
| `cuda.vb` | **IL → AST → CUDA**：把脚本里的 VB.NET 数学函数反编译成 `.cu`，在 GPU 上算皮尔逊相关矩阵与欧氏距离矩阵 | **类型定义块 + 反射**（[3.5](#35-类型定义块)）、`imports System.Reflection` |

其中 `cuda.vb` 是最能体现"脚本也能做重活"的一个：目标函数写在脚本的 `Public Class PearsonMetrics` 里（纯 VB.NET，零 CUDA 概念），脚本运行时把它们反编译成表达式树、发射成 `.cu` 内核源码，注册进 `KernelSources` 后用 NVRTC 即时编译并在 GPU 上执行，最后与 CPU 参考实现比对误差。它同时演示了"为什么目标函数要写进 `Class` 而不是写在顶层"——顶层函数会变成匿名函数，拿不到 `Shared` 的 `MethodInfo`。

---

## 6. 排错指南

### 6.1 先看生成的代码

任何"脚本行为不符合预期"的问题，**第一步都是加 `--verbose`**：

```bat
vbs.exe G:\...\hola_layout.vb --verbose
```

它会打印预处理后交给 Roslyn 的完整源码。对照 [第 2 节](#2-引擎是怎么工作的) 的容器结构，就能判断是"我的写法被引擎理解错了"还是"代码本身有错"。

### 6.2 编译失败

编译失败时引擎抛出 `InvalidOperationException: 脚本代码编译失败!`，并附上 Roslyn 的诊断（含生成代码的行号）。常见错误：

| 报错 | 原因 | 处理 |
| --- | --- | --- |
| `BC32000: Local variable 'x' cannot be referred to before it is declared` | 顶层函数捕获了声明在它之后的顶层变量 | 把变量声明移到函数之前（[3.6](#36-顶层函数与落位规则)） |
| `BC30734: 'args' is already declared as a parameter` | 顶层变量命名为 `args` | 改名 |
| `BC30027 / BC30289: 'End Function' expected / Statement cannot appear within a method body` | 顶层函数没被重写成匿名函数（通常是参数列表里出现了数组类型） | 把函数移进 `Public Class` 写成 `Shared` 方法（[规则 2](#4-编写规则与注意事项)） |
| `BC30451: 'Xxx' is not declared` | 缺少 `imports`，或 `#include` 的程序集没找到 | 补 `imports`；确认程序集在 [3.3](#33-引用外部程序集include) 的探测路径里 |
| `BC30035 / 语法错误` | 顶层出现了不能写进函数体的语法 | 见 [规则 6](#4-编写规则与注意事项) |

### 6.3 代码莫名"消失"

如果脚本跑起来什么都没发生、退出码却是 0，先 `--verbose` 看生成代码里你的语句在不在。通常是某个块没有正确闭合（例如少了 `Next` / `End Function`），导致后续代码被当成了那个块的内容。

---

## 附：一个最小模板

```vbnet
#include "Microsoft.VisualBasic.Drawing.dll"

imports microsoft.visualbasic.drawing

' 读取命令行参数: vbs demo.vb --n=10
Dim n As Integer = ?"--n"

' 顶层函数（不捕获任何顶层变量，可以写在任何位置）
Public Function Describe(count As Integer) As String
    Return $"count = {count}"
End Function

Call Console.WriteLine(Describe(n))

For i As Integer = 1 To n
    Call Console.WriteLine(i)
Next
```
