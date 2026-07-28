---
name: move_presets_to_script_editor
overview: 将 MainForm 的预设数学表达式下拉框（cboPreset/12 个预设）移除，并将其按 SampleScript() 语法转换为预设绘图脚本，迁移到 ScriptEditorForm 的 cboPreset 中供用户选择（选择后仅载入编辑框，不自动运行）。
todos:
  - id: remove-mainform-preset
    content: 移除 MainForm 中 cboPreset/lblPreset 及预设曲面区域相关代码
    status: completed
  - id: add-script-presets
    content: 在 ScriptEditorForm 新增 BuildPresetScripts 按 SampleScript 语法生成 12 条预设脚本
    status: completed
  - id: wire-presets
    content: 将预设填入 cboPreset 并新增 SelectedIndexChanged 仅载入 TextBox1
    status: completed
    dependencies:
      - add-script-presets
  - id: verify-build
    content: 编译验证无残留引用并清理主界面工具栏空间
    status: completed
    dependencies:
      - remove-mainform-preset
      - wire-presets
---

## 产品概述

将三维数学表达式绘图器主界面（MainForm）中的「预设表达式」功能迁移至脚本编辑器窗口（ScriptEditorForm），并把原有的数学算式转换为与 SampleScript() 语法一致的可选绘图脚本，统一绘图脚本的入口体验，简化主界面顶部工具栏。

## 核心功能

- 完全移除主界面顶部的「预设」下拉框及其相关代码（字段、初始化、PresetInfo 类、BuildPresets()、PresetSelected 事件），释放主界面工具栏空间。
- 在脚本编辑器窗口已有的空白 cboPreset 下拉框中注入 12 条预设绘图脚本，按 `x = axis(min, max, n=80)`、`y = axis(min, max, n=80)`、`z(x, y) = <算式>`、`surface(x, y, z)` 的语法生成。
- 用户在脚本窗口选择预设后，仅将对应脚本文本载入多行编辑框（TextBox1），供用户查看/编辑后手动运行，不自动执行渲染。

## 技术栈

- 语言：VB.NET（.NET Framework WinForms）
- 窗体框架：System.Windows.Forms（SurfaceCanvas / PlotScene 三维渲染由现有项目提供）
- 脚本引擎：Microsoft.VisualBasic.Math.Scripting.MathScriptEngine（已在 ScriptEditorForm 中使用）

## 实现方案

采用「代码迁移 + 语法转换」策略：将 MainForm 中预设的表达式与取值范围数据平移至 ScriptEditorForm，并在载入时按 SampleScript() 的 DSL 语法拼装成脚本文本，避免在主界面维护两套表达逻辑。

关键决策：

1. **预设数据迁移而非复制**：将 PresetInfo 的 12 条预设（Name/Expression/XMin/XMax/YMin/YMax）整体搬入 ScriptEditorForm，主界面不再保留任何预设逻辑，消除重复维护。
2. **脚本模板固定 n=80**：沿用 SampleScript() 中 `axis(..., n=80)` 的采样数，保证渲染粒度一致且简单可控（YAGNI，不额外暴露分辨率选项）。
3. **选择即载入编辑框、不自动运行**：契合用户确认行为，避免误渲染，复用现有「运行」按钮与 ScriptExecuted 回传链路，零新增渲染耦合。
4. **cboPreset 设为 DropDownList**：预设为只读选项，禁止用户手输，避免非法值。

性能与可靠性：预设脚本在窗体构造时一次性生成为 `List(Of (Name, Script))`，仅 12 条，内存/初始化开销可忽略；选择时仅为字符串赋值，无额外计算；保持 SampleScript() 示例按钮（ToolStripButton2）不变，二者共存互不冲突。

## 实现注意

- 移除 MainForm 预设代码时，确认 OnDraw/ApplyMode/ModeChanged/UpdateStatus 不引用 cboPreset、lblPreset、BuildPresets、PresetInfo；本任务不在主界面新增任何替代控件。
- ScriptEditorForm 的 cboPreset 已声明 `Friend WithEvents`，可直接新增 `Handles cboPreset.SelectedIndexChanged`，无需额外 AddHandler。
- 初始 SelectedIndex 保持 -1（不预载任何脚本），保持窗体打开时的空白/可编辑态。
- 生成的脚本需与现有 MathScriptEngine 解析器兼容（axis/surface/z 函数定义顺序与 SampleScript 一致）。

## 架构设计

数据流转（修改后）：
用户选择预设 → cboPreset.SelectedIndexChanged → 写入 TextBox1.Text（仅载入）
用户点击「运行」→ MathScriptEngine.RunScript → RaiseEvent ScriptExecuted → MainForm.OnScriptExecuted 渲染
主界面与预设逻辑解耦，预设仅存在于脚本编辑窗口。

## 目录结构

Expression3DPlotter/
├── MainForm.vb            # [MODIFY] 删除 cboPreset/lblPreset 字段声明、InitializeComponent 中创建代码（原行196-199）、以及 "#Region 预设曲面"（PresetInfo 类、BuildPresets()、PresetSelected）。移除后确保无残留引用。
└── ScriptEditorForm.vb   # [MODIFY] 新增 BuildPresetScripts() 生成 12 条预设脚本；在 Sub New/InitializeComponent 中填充 cboPreset（DropDownList 风格）；新增 SelectedIndexChanged 处理，将选中脚本写入 TextBox1（不运行）。

## 关键代码结构

新增数据结构（ScriptEditorForm.vb 内，用于关联显示名与脚本文本）：

```
' 预设脚本项：显示名称 + 完整绘图脚本
Private Function BuildPresetScripts() As List(Of (Name As String, Script As String))
```

生成脚本模板（固定 n=80，遵循 SampleScript 语法）：

```
' # <Name>
' x = axis(<XMin>, <XMax>, n=80)
' y = axis(<YMin>, <YMax>, n=80)
' z(x, y) = <Expression>
' surface(x, y, z)
```