---
name: 实现Canvas画面快照复制与保存
overview: 在 MainForm.vb 中为已声明的 CopySnapshotImage、SaveSnapshotImageAsFile 两个 ToolStripButton 添加事件处理，将三维画布 SurfaceCanvas 当前显示的画面捕获为 Bitmap，分别复制到剪贴板与保存为 PNG 文件。
todos:
  - id: add-snapshot-helper
    content: 在 MainForm.vb 新增 GetCanvasSnapshot 辅助函数，用 DrawToBitmap 捕获三维画布
    status: completed
  - id: impl-copy-button
    content: 实现 CopySnapshotImage_Click，调用 Clipboard.SetImage 并状态反馈
    status: completed
    dependencies:
      - add-snapshot-helper
  - id: impl-save-button
    content: 实现 SaveSnapshotImageAsFile_Click，弹出 SaveFileDialog 保存 PNG 并反馈
    status: completed
    dependencies:
      - add-snapshot-helper
---

## 用户需求

在三维数学表达式绘图器主窗体（MainForm.vb）中实现已添加的两个工具栏按钮功能：CopySnapshotImage 与 SaveSnapshotImageAsFile。

## 产品概述

为三维绘图器增加画面导出能力：可将当前三维画布（SurfaceCanvas）实际显示的内容复制为图片或保存为图片文件，便于分享与存档。

## 核心功能

- **复制到剪贴板**：点击 CopySnapshotImage 按钮，将当前三维画布显示的画面捕获并放入系统剪贴板。
- **保存为图片文件**：点击 SaveSnapshotImageAsFile 按钮，弹出文件保存对话框，将当前三维画布画面以 PNG 格式保存为文件。
- **状态反馈**：操作成功后通过状态栏提示结果；无画布内容或捕获失败时给出相应提示。

## 范围约束

- 截图范围仅限三维画布 canvas（SurfaceCanvas），不捕获二维图（pic2D）。
- 保存文件仅支持 PNG 格式（默认扩展名 png）。

## 技术栈

- 语言/框架：VB.NET（WinForms），复用现有 MainForm / SurfaceCanvas / PlotScene 结构。
- 关键 API：`Control.DrawToBitmap`（捕获画布实际像素）、`Clipboard.SetImage`、`SaveFileDialog`、`System.Drawing.Imaging.ImageFormat`。

## 实现方案

### 总体策略

采用 `Canvas.DrawToBitmap` 直接捕获三维画布当前在屏幕上绘制的像素，而非用 `PlotScene.Draw` 程序化重绘。理由：用户明确要求“当前显示的画面”，`DrawToBitmap` 调用控件的 `OnPaint` 路径（SurfaceCanvas.vb 第 123-128 行），可 1:1 还原当前视角、视距、背景色及全部叠加层（坐标轴/盒子/网格/散点），且无需重复构造绘制上下文，最贴合需求且代码最简。

### 关键决策

1. **新增私有辅助函数 `GetCanvasSnapshot()`**：统一生成快照 Bitmap，供复制与保存两处复用（DRY）。当 `canvas` 为 Nothing、尺寸非法（Width/Height < 1）或 `Scene` 为 Nothing 时返回 Nothing，调用方据此提示。
2. **复制按钮**：取快照后 `Clipboard.SetImage(bmp)`。`Clipboard.SetImage` 内部会拷贝图像数据，调用后使用 `Using` 释放 Bitmap 安全无副作用。
3. **保存按钮**：使用 `SaveFileDialog`，`Filter` 仅设 PNG，`DefaultExt="png"`、`FileName="snapshot.png"`；按用户确认路径用 `bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png)` 保存（全命名空间避免与 `Microsoft.VisualBasic.Imaging` 歧义）。
4. **事件绑定**：沿用项目既有约定（参考 ToolStripButton2_Click，第 528 行）使用 `Handles CopySnapshotImage.Click` / `Handles SaveSnapshotImageAsFile.Click` 声明式绑定。

### 性能与可靠性

- `DrawToBitmap` 仅执行一次与画布同尺寸的位图绘制，开销等同于一次 `OnPaint`，无额外热点；位图在 `Using` 块内创建/释放，无内存泄漏。
- 捕获前做空值与尺寸校验，避免对未初始化/隐藏画布操作导致异常。
- 复用 `UpdateStatus(msg)` 反馈结果，与现有状态栏逻辑保持一致。

## 实现注意事项

- 命名空间歧义：`Imaging` 同时存在 `Microsoft.VisualBasic.Imaging`（文件顶部已 Imports）与 `System.Drawing.Imaging`。`ImageFormat` 必须写全 `System.Drawing.Imaging.ImageFormat`，不可简写。
- `Clipboard`、`SaveFileDialog`、`DialogResult` 来自 `System.Windows.Forms`，主窗体继承 `Form` 已隐式可见，可直接使用。
- 不改动 SurfaceCanvas / PlotScene，仅在 MainForm.vb 内新增代码，控制改动范围（blast radius）。

## 架构设计

无新增架构层级。仅在 MainForm 中增加快照辅助函数与两个事件处理子过程，依赖既有 `canvas`（SurfaceCanvas）与 `PlotScene.Draw` 渲染链路，保持现有分层（UI→SurfaceCanvas→PlotScene）不变。

## 目录结构

```
MESH/
└── MainForm.vb   # [MODIFY] 在 MainForm 类中新增 GetCanvasSnapshot() 辅助函数、
                  #          CopySnapshotImage_Click 与 SaveSnapshotImageAsFile_Click
                  #          两个事件处理子过程。复用现有 canvas 字段与 UpdateStatus 方法，
                  #          不改变其它逻辑。
```