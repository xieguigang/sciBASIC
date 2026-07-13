---
name: finish-parallel-simd-optimization
overview: 修复 ModelViewer 并行/SIMD 优化中遗留的编译错误并完成构建验证。核心是把 DrawPointCloud 中无法在 VB.NET 编译的 System.Numerics.Vector(Of Single) SIMD 投影块替换为并行标量投影（库侧 Camera.Rotate 的 Vector3/Matrix4x4 SIMD 已满足 SIMD 要求），再确保 dotnet build 零错误。
todos:
  - id: rewrite-drawpointcloud
    content: 重写 DrawPointCloud：删除 Vector(Of Single) SIMD 块与 px/py/pz 数组，改用 Parallel.For 标量投影写入 xyArr（与 Camera.Project 一致），并删除无用 Imports System.Numerics
    status: completed
  - id: build-verify
    content: 在 ModelViewer 目录运行 dotnet build，确认 0 错误（同时验证共享库 Camera.vb / PainterAlgorithm.vb 改动可编译）
    status: completed
    dependencies:
      - rewrite-drawpointcloud
  - id: runtime-check
    content: 加载一个已知模型（如 build_3DEngine 下的 .3mf）目测点云/表面渲染与优化前一致，确认库侧旋转数学等价无回归
    status: completed
    dependencies:
      - build-verify
---

修复 ModelViewer/SceneRenderer.vb 中 `DrawPointCloud` 方法的编译错误，完成三维渲染引擎并行 + SIMD 优化的收尾工作，使 `dotnet build` 通过（0 错误）。

问题根因：在 VB.NET 中 `Vector(Of Single)` 会被编译器解析为基元类型 `Single`，导致 `New System.Numerics.Vector(Of Single)(pz, j)` 与索引 `vx(k)` 均不可用（错误 BC30057 / BC30690 / BC30311）。该 SIMD 批处理块是本次优化中唯一未编译通过的部分。

用户已确认优化方向为「两者都做」（同时优化共享库 Microsoft.VisualBasic.Imaging 与应用层 ModelViewer）且采用「单精度 Vector3/Matrix4x4」策略；共享库侧的 SIMD 旋转（`Camera.Rotate` 批量重载，使用 `Vector3.Transform` + `Matrix4x4`）与并行化（`Camera.Draw`、`PainterAlgorithm.PainterBuffer`）均已编译通过。应用层点云投影无需再手写 `Vector(Of Single)` SIMD——库侧 `Camera.Rotate` 已提供 SIMD 加速，应用层只需用并行标量投影即可。

## 技术栈

- VB.NET / Windows Forms（.NET 10，`net10.0-windows`，AnyCPU/x64），与现有 ModelViewer 项目一致。
- 复用 `Microsoft.VisualBasic.Imaging.Drawing3D.Camera` 已暴露的 SIMD 批量 `Rotate(points As Point3D())`。

## 根因分析

- VB.NET 把 `Vector(Of Single)` 塌缩为关键字 `Single`，`New System.Numerics.Vector(Of Single)(pz, j)`、`vx(k)` 索引、`New Vector(Of Single)(vd) + vz` 等均无法编译。全限定 `System.Numerics.Vector(Of Single)` 仍失败，证明该类型在本项目中不可直接使用。

## 实现策略

- 删除 `DrawPointCloud` 内无法编译的 `While` + `Vector(Of Single)` 投影块及 `px/py/pz` 中间数组。
- 用 `System.Threading.Tasks.Parallel.For` 对每个点做标量透视投影，**投影公式与库 `Camera.Project` 严格一致**（`depth = viewDistance + Z`；`factor = depth <= 0 ? 0 : fov/depth`；`Xn = X*factor + width/2 + offset.X`），写入预分配 `xyArr(i)`。
- 颜色索引（含 PLY 自带颜色 / 热图 intensity）在同一 `Parallel.For` 内按索引写回 `cidxArr(i)`，无共享可变状态，线程安全。
- GDI+ `FillRectangle` 绘制保持串行（GDI+ 不可并行）。
- 旋转走库侧 SIMD（`Camera.Rotate(pts3)` 批量重载），满足「单精度 SIMD」要求。
- 移除已无用的 `Imports System.Numerics`（该文件不再直接引用 `Vector`）。

## 实现要点

- 投影公式必须与 `Camera.Project` 完全一致（`depth <= 0` 时 `factor = 0`），否则点云投影会与 `DrawMesh`/`ToScreen` 的屏幕坐标产生偏差。
- `Parallel.For` 仅做只读投影 + 按索引写回预分配数组，闭包捕获的 `rotated/cloud/Camera/intensityMin/intensityMax` 均为只读引用，安全。
- 不改动渲染库对外行为，仅库内部旋转/并行化实现变化；`SceneRenderer` 公共接口（属性、Draw、Load*）保持不变，回归风险低。

## 架构设计

仅修改 `ModelViewer/SceneRenderer.vb` 的 `DrawPointCloud` 方法实现，其余结构（Draw 分发、DrawGround、DrawModelAsPointCloud、库侧 SIMD/并行）不变。

## 目录结构

```
ModelViewer/
└── SceneRenderer.vb   # [MODIFY] 重写 DrawPointCloud 投影块：
                       #   - 删除 Vector(Of Single) 的 While 循环与 px/py/pz 数组
                       #   - 改用 Parallel.For 标量投影写入 xyArr（与 Camera.Project 一致）
                       #   - 颜色索引并行计算，GDI+ 串行绘制
                       #   - 删除 Imports System.Numerics
```

## 关键代码结构

```
Private Sub DrawPointCloud(g As Graphics)
    Dim palette = GetColorTable()
    Dim colorCount = palette.Length
    Dim sz = Math.Max(1, PointSize)
    Dim half = sz / 2.0F

    Dim cnt = cloud.Length
    Dim xyArr(cnt - 1) As PointF
    Dim cidxArr(cnt - 1) As Integer

    ' 旋转由库侧 SIMD 完成（Camera.Rotate 批量重载，Vector3/Matrix4x4）
    Dim pts3 = cloud.Select(Function(c) New Point3D(c.x, c.y, c.z)).ToArray()
    Dim rotated = Camera.Rotate(pts3).ToArray()

    Dim vd = Camera.ViewDistance, fov = Camera.FieldOfView
    Dim ox = Camera.Offset.X, oy = Camera.Offset.Y

    ' 并行标量投影（与 Camera.Project 公式一致）
    System.Threading.Tasks.Parallel.For(0, cnt, Sub(i)
        Dim p = rotated(i)
        Dim depth = vd + p.Z
        Dim factor As Double = If(depth <= 0, 0, fov / depth)
        xyArr(i) = New PointF(
            CSng(p.X * factor + Camera.Screen.Width / 2.0F + ox),
            CSng(p.Y * factor + Camera.Screen.Height / 2.0F + oy))

        If UseEmbeddedColor AndAlso Not String.IsNullOrEmpty(cloud(i).color) Then
            cidxArr(i) = -1
        Else
            Dim v = If(cloud(i).intensity <> 0, cloud(i).intensity, cloud(i).z)
            Dim t = (v - intensityMin) / (intensityMax - intensityMin)
            If t < 0 Then t = 0 Else If t > 1 Then t = 1
            cidxArr(i) = CInt(t * (colorCount - 1))
        End If
    End Sub)

    ' 串行绘制（GDI+ 不可并行）
    For i = 0 To cnt - 1
        Dim brush As System.Drawing.Brush
        If cidxArr(i) = -1 Then
            brush = GetEmbeddedBrush(cloud(i).color)
        Else
            brush = colorBrushes(cidxArr(i))
        End If
        g.FillRectangle(brush, xyArr(i).X - half, xyArr(i).Y - half, sz, sz)
    Next
End Sub
```