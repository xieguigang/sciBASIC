---
name: 表达式可视化脚本引擎与2D/3D绘图改造
overview: 在 Mathematica\Math\Math\Scripting 中新增支持向量变量与绘图指令的脚本引擎（仅产出数据与指令，不依赖绘图）；在 Expression3DPlotter 中新增独立非模态脚本窗口，解析脚本后按 scatter/line/surface/axis/函数定义 产出向量数据与 PlotCommand；主窗口根据指令自动在 2D(PictureBox+DataPlot) 与 3D(SurfaceCanvas) 画布间切换，并为 3D 增加盒子网格面与带刻度坐标轴开关。
todos:
  - id: enhance-scripting-engine
    content: 在 Scripting 模块新增 PlotCommand 与 MathScriptEngine，支持向量、axis、函数与绘图指令
    status: completed
  - id: add-2d-bridge
    content: 为 DataPlot 增加 ToBitmap 并新建 Expression3DPlotter 二维渲染桥接 DataPlotView
    status: completed
  - id: build-script-editor
    content: 新建 ScriptEditorForm 非模态脚本窗口，运行脚本并回传 ScriptResult
    status: completed
    dependencies:
      - enhance-scripting-engine
  - id: extend-plotscene
    content: 扩展 PlotScene：新增向量入口 SetScatter/SetLine/SetSurface 与盒子网格/刻度轴开关
    status: completed
    dependencies:
      - enhance-scripting-engine
  - id: wire-mainform
    content: 改造 MainForm：新增 2D PictureBox、脚本模式按钮、PlotDispatcher 与两个 3D 开关
    status: completed
    dependencies:
      - add-2d-bridge
      - build-script-editor
      - extend-plotscene
  - id: build-verify
    content: 编写示例脚本并编译 Expression3DPlotter，验证 2D/3D 渲染与开关
    status: completed
    dependencies:
      - wire-mainform
---

## 用户需求概述

将现有的 `Visualization\Expression3DPlotter` WinForm 程序改造为“可通过脚本化编程生成二维/三维函数图像的数学表达式可视化程序”，并提供可复用的数学脚本引擎。

## 核心功能

- **数学脚本引擎增强**：改造 `Mathematica\Math\Math` 的 Scripting 模块，使其具备较完整的脚本化编程能力（变量、向量、函数定义、内置指令），可作为通用数学运算脚本引擎，且不依赖任何绘图组件。
- **二维绘图部件整合**：复用 `Visualization\DataPlot` 中的 `ScatterPlot`/`LinePlot` 作为二维函数作图部件，渲染结果可在主窗口显示。
- **脚本化输入窗口**：在 Expression3DPlotter 中新增一个独立的非模态“脚本输入框”窗口（多行文本 + 运行按钮），基于改造后的脚本引擎执行脚本，将运算结果（向量数据与绘图指令）回传主窗口并绘制。
- **脚本绘图指令**：支持 `scatter(x,y,[z])`（二维/三维散点）、`line(x,y,[z])`（二维/三维曲线）、`surface(x,y,z)`（三维曲面）、`x = axis(min,max,[step=d,n=1000])`（生成自变量向量，按步长或分辨率）、`y(x) = ...`（单自变量函数）、`f(x,y,z) = ...`（多自变量函数）。
- **3D 显示开关**：三维绘图时提供开关控制是否显示基于数据坐标轴上下限产生的“三维盒子六个面的网格”，以及是否显示“带刻度尺的三维坐标轴”。
- **画布自动切换**：主窗口同时保留 3D 画布（SurfaceCanvas）与 2D 图片框（PictureBox），根据脚本产出的指令（2D/3D）自动切换显示；原文本框表达式模式保留可用。

## 技术栈选择

- 语言/框架：VB.NET，.NET 10 Windows Forms（与现有 `Expression3DPlotter.vbproj` 一致）。
- 数学内核：复用现有 `Microsoft.VisualBasic.Math.Scripting`（`ExpressionEngine` 标量编译链），不改动其表达式编译器。
- 三维渲染：复用 `Microsoft.VisualBasic.Imaging.Drawing3D` 的 `Camera`/`Surface` 与现有 `PlotScene`。
- 二维渲染：引用 `Visualization\DataPlot`（`PlotEngine`/`ScatterPlot`/`LinePlot`），渲染到内存 `IGraphics` 位图。
- 解耦约束：脚本引擎仅产出“向量变量 + 绘图指令数据”，不引用 `Visualization`，保持可独立复用于其它程序。

## 实现方案

整体采用“脚本引擎 → 绘图指令数据 → 主窗口调度器 → 2D/3D 渲染”的分层策略：

1. **引擎层（Scripting 模块）**：新增实例类 `MathScriptEngine`，变量表 `symbols As Dictionary(Of String, Object)`（值可为 `Double` 或 `Double()` 向量）；函数表 `userFuncs`（参数名 + 预编译 `Impl.Expression`）。向量计算复用现有标量 `ExpressionEngine` 以“逐元素循环 + 环境变量绑定”方式求值，无需改动表达式编译器，风险最低。
2. **指令模型（纯数据）**：新增 `PlotCommand`（类型 + X/Y/Z 向量 + 曲面 ZGrid + 配色/标签）与 `ScriptResult`（变量表 + 指令列表 + 错误信息）。
3. **脚本解析规则**：

- `name(args) = body` → 注册用户函数（预编译）。
- `x = axis(min,max,step=,n=)` → 直接生成向量变量。
- `y = <expr>`（表达式中引用向量符号）→ 对向量逐元素标量求值生成向量；标量内置函数（sin/cos 等）在向量参数上自动逐元素退化。
- `f(x,y,z)=...` + `surface(x,y,f)` → 由 x、y 两个一维向量构造笛卡尔网格，逐格调用 `f` 生成 `ZGrid` 二维数组（多自变量函数求值）。
- `scatter(...)`/`line(...)`/`surface(...)` → 解析括号内符号名，从变量表取向量，组装 `PlotCommand` 推入结果。

4. **2D 桥接**：`DataPlotView` 将 2D 类 `PlotCommand` 转为 `List(Of Series)`，调用 `ScatterPlot`/`LinePlot.Plot`，经 `PlotEngine` 新增的 `ToBitmap()` 导出 `System.Drawing.Bitmap` 供 `PictureBox` 显示（需给 `DataPlot` 的 `PlotEngine` 加一个只读的位图导出方法，属向后兼容的增量修改）。
5. **3D 增强**：`PlotScene` 新增接受“预计算向量”的入口 `SetScatter/SetLine/SetSurface`（与现有基于 `ExpressionEvaluator` 的入口并存），并记录数据包围盒；新增 `ShowBox`（盒子六面网格）、`ShowTicks`（带刻度尺三轴）两个开关，保留原 `ShowAxes`。
6. **主窗口调度**：`PlotDispatcher` 消费 `ScriptResult.Commands`：2D（scatter/line 无 Z 或 Z 全 0）走 `DataPlotView` 显示 `PictureBox`、隐藏 3D 画布；3D（含 Z 的散点/曲线、surface）走 `PlotScene` 并显示 3D 画布、隐藏 `PictureBox`。

## 实现注意

- **不改动**标量 `ExpressionEngine`/`ExpressionBuilder` 编译链；向量一律通过逐元素标量循环复用，降低回归风险。
- `PlotCommand`/`MathScriptEngine` 仅依赖 `Mathematica\Math`，不引用 `Visualization`，保证引擎可复用（需求1）。
- 性能：逐元素求值 O(N)，n=1000 级别完全可接受；surface 网格 NxM（如 50x50=2500）很快，若用户用 n=1000 双轴将达 1e6 次求值，属可接受上限，必要时提示分辨率。
- `Expression3DPlotter.vbproj` 需新增对 `..\..\DataPlot\DataPlot.vbproj` 的项目引用（DataPlot 目标 net10.0，可被 net10.0-windows 消费）。
- 保留现有表达式文本框模式，脚本模式作为新增入口，互不破坏。
- 异常捕获：脚本解析/求值错误写入 `ScriptResult.ErrorMessage`，由主窗口状态栏提示，不崩溃。

## 架构设计

```mermaid
flowchart TD
    A[ScriptEditorForm 非模态窗口] -->|RunScript(text)| B[MathScriptEngine]
    B -->|向量变量 + 内置指令| C[(symbols / userFuncs)]
    B -->|产出| D[ScriptResult: Variables + PlotCommand列表]
    D -->|事件回传| E[MainForm.PlotDispatcher]
    E -->|2D 指令| F[DataPlotView -> ScatterPlot/LinePlot -> Bitmap -> PictureBox]
    E -->|3D 指令| G[PlotScene.SetScatter/SetLine/SetSurface -> SurfaceCanvas]
    G -->|ShowBox / ShowTicks / ShowAxes| H[三维盒子网格 / 带刻度坐标轴]
```

## 目录结构与文件清单

```
Mathematica/Math/Math/Scripting/
├── PlotCommand.vb        # [NEW] 纯数据模型：Enum PlotKind{Scatter,Line,Surface}；
│                         #       Class PlotCommand(Kind, X,Y,Z() , ZGrid()() , Scheme, Label)；
│                         #       Class ScriptResult(Variables As Dictionary(Of String,Object),
│                         #       Commands As List(Of PlotCommand), ErrorMessage, Success)。无绘图依赖。
└── MathScriptEngine.vb  # [NEW] 实例类脚本引擎：symbols(Double|Double())、userFuncs、
                         #       axis() 内置、元素求值、函数定义与应用、scatter/line/surface
                         #       指令解析；RunScript(text) As ScriptResult。复用 ExpressionEngine。

Visualization/DataPlot/Engine/
└── PlotEngine.vb        # [MODIFY] 新增 Public Function ToBitmap() As Bitmap，
                         #           返回内部 IGraphics 对应的 System.Drawing.Bitmap（增量、向后兼容）。

Visualization/Expression3DPlotter/
├── MathScriptEngine 接入 # [MODIFY] ScriptEngine.vb 可选新增便捷入口 EvaluateScript 委托给 MathScriptEngine
│                         #          （或在 MainForm 直接 new MathScriptEngine，最小改动即可）。
├── ScriptEditorForm.vb  # [NEW] 非模态脚本窗口：多行 TextBox + 运行/示例/关闭按钮；
│                         #       运行后通过事件 ScriptExecuted(result As ScriptResult) 回传 MainForm。
├── DataPlotView.vb      # [NEW] 2D 桥接：Render(commands, w, h) As Bitmap；
│                         #       将 PlotCommand 转为 List(Of Series)，调用 ScatterPlot/LinePlot.Plot，
│                         #       经 PlotEngine.ToBitmap 返回位图。
├── PlotDispatcher.vb    # [NEW] 消费 ScriptResult.Commands，按 2D/3D 自动分流到 PictureBox 或 PlotScene。
├── PlotScene.vb         # [MODIFY] 新增 SetScatter/SetLine/SetSurface(向量重载)；
│                         #       记录bounds；新增 ShowBox、ShowTicks 属性；
│                         #       新增 DrawBox(六面网格) 与 DrawRuler(带数字刻度三轴)。
├── MainForm.vb          # [MODIFY] 新增 PictureBox(2D,默认隐藏)、ScriptEditorForm 实例与“脚本模式”按钮；
│                         #       新增“显示盒子网格面”“显示带刻度坐标轴”两个 CheckBox；
│                         #       接入 PlotDispatcher；保留原有表达式模式。
└── Expression3DPlotter.vbproj # [MODIFY] ItemGroup 新增 DataPlot.vbproj 项目引用。
```

## 关键代码结构

```
' Scripting/PlotCommand.vb
Namespace Microsoft.VisualBasic.Math.Scripting
    Public Enum PlotKind : Scatter : Line : Surface : End Enum

    Public Class PlotCommand
        Public Kind As PlotKind
        Public X As Double()
        Public Y As Double()
        Public Z As Double()            ' 三维散点/曲线可选
        Public ZGrid As Double()()      ' surface 用
        Public Scheme As String
        Public Label As String
    End Class

    Public Class ScriptResult
        Public Variables As New Dictionary(Of String, Object)
        Public Commands As New List(Of PlotCommand)
        Public ErrorMessage As String = ""
        Public Success As Boolean = True
    End Class
End Namespace

' Scripting/MathScriptEngine.vb（接口级）
Public Class MathScriptEngine
    Public Function RunScript(script As String) As ScriptResult
    Public Sub SetVariable(name As String, value As Object)   ' Double 或 Double()
    Public Sub AddFunction(name As String, params() As String, body As String)
End Class

' Expression3DPlotter/PlotScene.vb（新增成员）
Public Property ShowBox As Boolean = True     ' 三维盒子六面网格
Public Property ShowTicks As Boolean = False  ' 带刻度尺三轴
Public Sub SetScatter(x As Double(), y As Double(), z As Double())
Public Sub SetLine(x As Double(), y As Double(), z As Double())
Public Sub SetSurface(x As Double(), y As Double(), zGrid As Double()())
```