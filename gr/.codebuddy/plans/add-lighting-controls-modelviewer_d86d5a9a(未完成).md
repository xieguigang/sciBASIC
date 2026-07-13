---
name: add-lighting-controls-modelviewer
overview: 在 ModelViewer 的 WinForm 中新增「光照参数」调节面板，解决三维模型渲染发白、亮度过高的问题。通过在右侧停靠面板提供环境光强度、光照亮度（缩放 LightColor 以削弱发白）、灯光颜色、光源方向（仰角/方位）等控件，实时绑定到 renderer.Camera 的 AmbientStrength / LightColor / LightDirection，并支持重置。纯 WinForm 代码改动（MainForm.vb），无需改动 Imaging/Landscape 库。
todos:
  - id: add-light-fields
    content: 在 MainForm 新增 baseLightColor/lightIntensity 字段与 LightDirFromAngles、ApplyLightColor 辅助函数
    status: pending
  - id: build-light-panel
    content: 在 BuildUI 新增右侧 DockStyle.Right 的「光照参数」面板及滑块/颜色按钮/重置按钮
    status: pending
    dependencies:
      - add-light-fields
  - id: wire-light-events
    content: 绑定光照控件事件，实时更新 Camera 的 AmbientStrength/LightColor/LightDirection 并刷新画布
    status: pending
    dependencies:
      - build-light-panel
  - id: reset-and-status
    content: 新增 ResetLighting 接入重置视角，并在状态栏显示光照参数
    status: pending
    dependencies:
      - wire-light-events
---

## 用户需求

ModelViewer 中三维模型渲染颜色发白、整体过亮，需要在 WinForm 界面上提供调节光照参数的控件，让用户实时调整以消除发白、降低亮度。

## 产品概述

在现有 ModelViewer 主窗体中新增一个右侧停靠的「光照参数」调节面板，提供环境光强度、光照亮度、灯光颜色、光源仰角/方位等滑块与按钮，改动即时反映到 3D 渲染。

## 核心功能

- 环境光强度滑块（AmbientStrength，0–1）控制暗部与对比度
- 光照亮度滑块（按比例缩放 LightColor 的 RGB，默认约 0.65）作为消除发白的核心控件
- 灯光颜色选择按钮（ColorDialog，默认白色），与亮度滑块共同决定 LightColor
- 光源仰角/方位滑块（换算为单位向量写入 LightDirection）
- 重置光照按钮（恢复默认仰角/方位、环境光、亮度、白色灯光），并纳入「重置视角」
- 状态栏同步显示光照相关参数

## 技术栈

- 语言/框架：VB.NET + Windows Forms（.NET 10，`net10.0-windows`），与现有 ModelViewer 项目一致
- 复用：`Microsoft.VisualBasic.Imaging.Drawing3D.Camera` 已暴露 `AmbientStrength As Double`、`LightColor As Color`、`LightDirection As Point3D`，无需改动 Imaging/Landscape 库

## 实现思路

仅修改 `ModelViewer\MainForm.vb`，在现有 `Form` 上新增一个 `Dock = DockStyle.Right` 的「光照参数」面板（`GroupBox` + `TrackBar`/`Button`/`Label`）。各控件直接读写 `renderer.Camera` 的对应属性并调用 `canvas.Invalidate()` 触发重绘。

关键修复杠杆（基于 `Light.vb` 的 `ComputeLighting`）：最终色 = `baseColor*(1-factor) + lightColor*factor`，被照面 `factor` 趋近 1 时会混入 `LightColor`。将 `LightColor` 由纯白降为按比例缩放的灰色，即可让模型本色透出、消除发白。因此「光照亮度」滑块是核心控件，它通过 `LightColor = baseLightColor.Scale(intensity)` 实现（`intensity` 默认 ~0.65）。

光源方向必须为单位向量（非单位会使 `diffuse = 法线·lightDirection` 超过 1 再次过亮），故用球面角换算并 `Normalize()`。默认光源方向沿用 `Camera` 构造的 `(2,-1,3).Normalize()`，对应仰角/方位在 UI 初始化时反推或直接以中性角度（如仰角 35°、方位 45°）作为默认并写入。

## 实现要点

- 纯 WinForm 改动，不触碰渲染库；所有改动集中在 `MainForm.vb`，保持现有 `RenderPanel`/`SceneRenderer` 不变
- `TrackBar` 的 `Scroll` 事件实时更新，避免卡顿（仅属性赋值 + `Invalidate`，无重计算开销）
- 光照亮度与灯光颜色拆成两个状态：`baseLightColor As Color`（用户选的基础色，默认 White）与 `lightIntensity As Double`（0–1），`Camera.LightColor = Color.FromArgb(base.R*intensity, ...)`；任一变化都重算
- `ResetView()` 同时调用新增的 `ResetLighting()`，保证重置视角时光照回到易读默认值
- 滑块取值范围：环境光 0–100→/100；亮度 0–100→/100（默认 65）；仰角 -90–90；方位 0–360

## 架构设计

现有结构无需变动，仅扩展 `MainForm`：
`MainForm` → 持有 `renderer: SceneRenderer` 与 `canvas: RenderPanel`；新增 `lightPanel` 及其子控件 → 事件处理程序 → 直接改 `renderer.Camera.AmbientStrength / LightColor / LightDirection` → `canvas.Invalidate()` → `SceneRenderer.Draw` → `Camera.Draw` → `ComputeLighting` 用新参数着色。

## 目录结构

```
ModelViewer/
└── MainForm.vb   # [MODIFY] 唯一改动文件。新增光照私有字段(baseLightColor, lightIntensity)、
                  #   LightDirFromAngles 辅助函数；在 BuildUI 中新增右侧 DockStyle.Right 的
                  #   「光照参数」GroupBox 及 TrackBar/Button；绑定各控件 Scroll/Click 事件实时
                  #   更新 Camera 光照；新增 ResetLighting 并接入 ResetView 与 UpdateStatus。
```

## 关键代码结构

```
' 球面角 -> 指向光源的单位向量（z 朝上）
Private Function LightDirFromAngles(elevDeg As Integer, azimDeg As Integer) As Point3D
    Dim el = elevDeg * Math.PI / 180, az = azimDeg * Math.PI / 180
    Return New Point3D(Math.Cos(el) * Math.Cos(az),
                       Math.Cos(el) * Math.Sin(az),
                       Math.Sin(el)).Normalize()
End Function

' 由基础色与亮度比例得到实际 LightColor
Private Sub ApplyLightColor()
    Dim r = CInt(baseLightColor.R * lightIntensity)
    Dim g = CInt(baseLightColor.G * lightIntensity)
    Dim b = CInt(baseLightColor.B * lightIntensity)
    renderer.Camera.LightColor = Color.FromArgb(255, r, g, b)
End Sub
```