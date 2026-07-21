---
name: MESH-script-data-command
overview: 为数学表达式可视化项目的脚本引擎（MathScriptEngine）新增 data("file.csv"/"file.arff") 命令，通过可插拔委托在 MESH 工程中调用 DataFrame 模块的 read_csv/read_arff 读取数值列，并将列名（非法字符统一替换为下划线）清洗后注入为可用自变量/绘图向量变量。
todos:
  - id: engine-data-cmd
    content: 在 MathScriptEngine 增加 DataLoader 委托与 data() 命令解析分支
    status: completed
  - id: mesh-loader
    content: 在 ScriptEditorForm 注入 DataLoader 并实现 DataFrame 读取与列名清洗
    status: completed
    dependencies:
      - engine-data-cmd
  - id: help-doc
    content: 在 ScriptHelpForm 补充 data() 用法说明
    status: completed
    dependencies:
      - mesh-loader
  - id: verify-sample
    content: 用示例 CSV/ARFF 脚本验证 data() 加载与绘图
    status: completed
    dependencies:
      - mesh-loader
---

## 用户需求

为数学表达式可视化项目（MESH）的脚本引擎新增“读取外部数据框文件”的能力，使用类似 `data("data_frame.csv")` 的脚本命令，将文件中的数值列加载为环境中可直接引用的向量变量，从而可作为自变量或绘图数据使用。

## 产品概述

在脚本输入框（ScriptEditorForm）中，除原有的 `axis(...)` 内置指令外，新增 `data("文件名")` 命令。运行脚本时，引擎通过该命令按文件扩展名分流，调用 DataFrame 模块的 `read_csv` / `read_arff` 读取数据框，把所有数值类型的列以“清洗后的列名”为变量名注入环境。之后用户即可像使用 `x = axis(...)` 生成的自变量一样，用这些列名参与表达式求值或作为 `line` / `scatter` / `surface` 绘图指令的数据来源。

## 核心功能

- `data("xxx.csv" | "xxx.arff")` 脚本命令：按扩展名自动选择 `read_csv` 或 `read_arff` 加载数据框。
- 仅加载数值类型列（Double/Single/Integer/Long/Short/Byte 等），非数值列（字符串、日期、布尔等）忽略。
- 列名清洗：列名中空格等非法符号统一替换为下划线，以数字开头时前置下划线，并与已加载列名去重（冲突追加 `_2`、`_3`…）。
- 加载后的列名作为向量变量参与后续表达式与绘图指令（如 `plot(data.x, data.y)` 形态简化为 `scatter(col1, col2)`）。
- 路径解析：相对路径基于当前工作目录解析；文件缺失或读取失败时给出带行号的明确错误提示。

## 技术栈

- 语言/平台：VB.NET（.NET 10 Windows Forms），沿用现有工程。
- 脚本引擎：`Microsoft.VisualBasic.Math.Scripting.MathScriptEngine`（位于共享库 Mathematica\Math，被 MESH 引用）。
- 数据框模块：`Microsoft.VisualBasic.Data.DataFrame.DataFrame`（MESH 的 vbproj 已引用该工程，read_csv/read_arff 为 Shared 函数）。
- 设计原则：委托解耦——引擎只负责 `data()` 命令的解析与“注入环境”，实际文件读取由 MESH 工程通过 `DataLoader` 委托注入，引擎不新增对 DataFrame 的项目依赖，保持向后兼容（DataLoader 默认 Nothing 时无任何行为）。

## 实现方案

### 总体策略

在共享引擎 `MathScriptEngine` 中增加公共属性 `DataLoader As Func(Of String, Dictionary(Of String, Double()))` 与 `data(...)` 命令解析分支；在 MESH 工程的 `ScriptEditorForm` 中将 `DataLoader` 指向本地的 DataFrame 读取实现。运行 `RunScript` 时，逐行扫描；遇到 `data("path")` 语句即调用 `DataLoader` 取得“清洗列名→数值向量”字典，写入引擎内部的 `vars` 字典（引擎按 `TypeOf vars(name) Is Double()` 自动识别为向量变量），供后续表达式与绘图指令引用。

### 关键技术决策与权衡

- **为何用委托而非直接在引擎里引用 DataFrame**：`Microsoft.VisualBasic.Math` 是被大量项目共用的基础数学库，直接引用 DataFrame 会引入重量级数据依赖甚至潜在循环引用风险。委托方式让引擎保持对数据框零耦合，DataFrame 读取代码留在已引用该模块的 MESH 工程中。
- **为何在 ProcessLine 中识别 data()**：现有流程为“函数定义→绘图指令→赋值”，`data(...)` 是无左值的独立命令，应在赋值分支之前拦截，匹配则注入变量并 `Return`，不匹配则交还原有流程（向后兼容）。
- **数值列取值**：`FeatureVector` 的 `CType(col, Double())` 窄化转换仅对 Double 列有效（非 Double 数值列返回 Nothing），因此统一使用 `fv.vector.Cast(Of Double).ToArray()`，对所有数值类型列安全。
- **错误一致性**：参考现有 `axis`/`surface` 的处理，把失败转为 `result.Success=False; result.ErrorMessage="第 N 行: ..."`，与主流程错误展示完全一致。

### 性能与可靠性

- `data()` 在解析期执行一次，列向量一次性注入 `vars`，后续逐元素求值时通过 `BindScalars` 复用，无重复 IO 或重复解析（O(列数×行数) 一次加载，与现有 axis 向量同等的求值开销）。
- 委托调用用 Try 包裹，统一把文件不存在/解析异常转成带行号的 `ScriptResult` 错误，不抛到 UI 线程。
- 列名清洗保证字典 key 唯一且为合法符号名（`[A-Za-z_][A-Za-z0-9_]*`），避免后续正则变量识别失败。

## 实现要点（防回归）

- 仅在 `ProcessLine` 注释/空行判断之后、函数定义分支之前插入 `data()` 识别；其余流程零改动。
- `DataLoader Is Nothing` 时若脚本使用了 `data()`，应给出“数据加载器未配置”的明确错误，避免静默无效果。
- `DataLoader` 委托返回 `Dictionary(Of String, Double())`，key 必须是已清洗的唯一合法名；引擎直接 `vars(k)=v` 注入即可（无需额外注册 numericVars，原有 `ResolveArg`/`BindScalars` 通过 `vars` 判定 `Double()` 类型）。
- 路径解析：仅对相对路径拼 `Environment.CurrentDirectory`；保留绝对路径原样。
- 清洗函数需覆盖“空名、全非法字符、数字开头、与已有键冲突”四类边界。

## 架构设计

```mermaid
flowchart TD
    A[ScriptEditorForm 用户脚本] --> B[new MathScriptEngine + 设置 DataLoader]
    B --> C[engine.RunScript 逐行 ProcessLine]
    C -->|匹配 data('path')| D[HandleDataCommand]
    D -->|DataLoader(path)| E[MESH: LoadDataFrameFile]
    E -->|扩展名分流| F[DataFrame.read_csv / read_arff]
    F --> G[遍历 featureSet 取数值列 Cast Double]
    G -->|清洗列名| H[Dictionary(列名, Double())]
    H --> D
    D -->|写入 vars| C
    C -->|后续行/绘图指令引用列名| I[ResolveArg/BindScalars 求值]
    I --> J[PlotCommand 渲染]
```

## 目录结构与改动文件

```
Data_science\Mathematica\Math\Math\Scripting\
└── MathScriptEngine.vb   # [MODIFY] 共享脚本引擎。新增公共属性 DataLoader(Func(Of String, Dictionary(Of String, Double())))；
                          #          新增私有函数 HandleDataCommand(line, result) As Boolean，正则识别 data("path")/data('path')，
                          #          调用 DataLoader 后将列向量写入 vars；在 ProcessLine 注释/空行判断之后、函数定义分支之前调用。
                          #          保持 DataLoader=Nothing 时原有行为不变（向后兼容）。

Data_science\Visualization\MESH\
├── ScriptEditorForm.vb   # [MODIFY] ToolStripButton1_Click 创建 engine 后设置 engine.DataLoader = AddressOf LoadDataFrameFile；
                          #          新增私有函数 LoadDataFrameFile(path) As Dictionary(Of String, Double())（按扩展名分流 read_csv/read_arff、
                          #          过滤数值列、Cast 为 Double()、调用 SanitizeColumnName）；新增辅助 IsNumericType(t) 与 SanitizeColumnName(name, keys)。
└── ScriptHelpForm.vb     # [MODIFY] 在语法说明中新增 data() 命令示例（data("demo.csv") 后 scatter(col1, col2)），提升可用性。
```

## 关键代码结构

```
' MathScriptEngine.vb（共享引擎，新增公共 API）
Public Property DataLoader As Func(Of String, Dictionary(Of String, Double()))

' MESH 工程注入与实现（ScriptEditorForm.vb）
Private Function LoadDataFrameFile(path As String) As Dictionary(Of String, Double())
'   解析相对/绝对路径 -> *.arff 走 DataFrame.read_arff，其余走 DataFrame.read_csv（.tsv 传 vbTab）
'   遍历 df.featureSet，IsNumericType(fv.type) 为 True 时 out(SanitizeColumnName(fv.name, out.Keys)) = fv.vector.Cast(Of Double).ToArray()
'   返回 Dictionary(Of String, Double())
```