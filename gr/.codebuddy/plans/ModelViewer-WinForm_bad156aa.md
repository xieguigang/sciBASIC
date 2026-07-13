---
name: ModelViewer-WinForm
overview: 在 gr 目录下新建一个 VB.NET WinForm 项目 ModelViewer，引用 Landscape 与 Microsoft.VisualBasic.Imaging，加载多格式三维模型/PLY点云并用 Drawing3D 渲染，支持左键旋转、滚轮缩放、右键平移、表面/三角网格切换，以及 PLY 点云热图渲染。
todos:
  - id: create-winform-project
    content: 创建 ModelViewer 工程文件并配置对 Landscape 与 Imaging 的项目引用
    status: completed
  - id: implement-scene-renderer
    content: 实现 SceneRenderer：加载模型/点云、居中、自动视距与三种渲染模式
    status: completed
    dependencies:
      - create-winform-project
  - id: build-mainform-ui
    content: 实现 MainForm 界面：工具栏、双缓冲画布、模式切换与点云配色控件、状态栏
    status: completed
    dependencies:
      - create-winform-project
  - id: wire-mouse-interaction
    content: 接入左键旋转、滚轮缩放、右键平移交互并触发重绘
    status: completed
    dependencies:
      - implement-scene-renderer
      - build-mainform-ui
  - id: build-and-verify
    content: 构建项目并验证多格式模型、三角网格与点云热图渲染
    status: completed
    dependencies:
      - wire-mouse-interaction
---

## 用户需求概述

基于现有 `gr` 目录下的两个类库，使用 VB.NET 新建一个 WinForm 桌面程序，用于查看三维模型文件与 PLY 点云数据。

## 核心功能

- 引用 `Landscape` 项目，通过 `ModelLoader.LoadModel` 加载多种格式（STL/glTF/GLB/OBJ/DAE/3DS/3MF）的三维模型，统一转换为 `Drawing3D.Surface` 集合后由 `Microsoft.VisualBasic.Imaging.Drawing3D` 的 `Camera` 进行三维渲染。
- 引用 `Landscape.Ply.PlyReader` 读取 PLY 点云（`PointCloud`），并使用 `Drawing2D.Colors.Designer.GetColors(term, n, alpha)` 获取的调色板按热图（intensity/z）着色渲染点云。
- 鼠标左键拖拽旋转模型（修改摄像机 AngleX/AngleY）。
- 鼠标滚轮调整摄像机视距（修改 ViewDistance 实现缩放）。
- 鼠标右键拖拽平移画布（修改 Camera.Offset）。
- 提供切换选项：在「表面渲染」与「三角形网格（线框）」两种模式之间切换查看模型。
- 程序需同时支持三维网格模型渲染与 PLY 点云渲染两类数据。

## 视觉与交互效果

桌面窗口左侧/顶部为工具栏（打开文件、渲染模式切换、点云配色方案下拉），中央为 GDI+ 画布实时渲染三维模型或点云，底部状态栏显示当前文件、渲染模式、摄像机角度与视距；交互流畅、双缓冲无闪烁。

## 技术栈与依据

- 语言/框架：VB.NET，Windows Forms，目标框架 `net10.0-windows`（沿用 `imaging.NET5.vbproj` 的 `-windows` TFM，从而由 .NET SDK 自动定义 `WINDOWS` 符号，启用 `Camera.Draw(g As Graphics, ...)` 与 `PainterAlgorithm.SurfacePainter` 的 GDI+ 重载，并启用 WinForms）。
- 引用方式（沿用 `Landscape.vbproj` 现有做法，不修改现有库）：
- `..\Landscape\Landscape.vbproj`（AssemblyName=`Microsoft.VisualBasic.Imaging.Landscape`，RootNamespace=`Microsoft.VisualBasic.Imaging.Landscape`）
- `..\Microsoft.VisualBasic.Imaging\imaging.NET5.vbproj`（其依赖 Math/Core/html 等由 ProjectReference 自动传递）
- 渲染核心命名空间：`Microsoft.VisualBasic.Imaging.Drawing3D`（Camera/Surface/Point3D）、`Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Designer`、加载命名空间 `Microsoft.VisualBasic.Imaging.Landscape.Data` 与 `Microsoft.VisualBasic.Imaging.Landscape.Ply`。

## 实现方案

整体策略：新建独立的 `ModelViewer` WinForm 项目，封装一个 `SceneRenderer` 承载 `Camera` 与已加载的模型/点云状态，负责居中、自动适配视距与三种渲染模式；`MainForm` 负责界面、文件打开、模式/配色控件与鼠标交互，交互仅修改 `Camera` 的 `AngleX/AngleY`、`ViewDistance`、`Offset` 后触发 `Invalidate` 重绘。

关键决策与理由：

1. **居中 + 自动视距适配**：加载后将全部顶点减去质心居中，计算包围球半径 `R`，令 `ViewDistance = R * FieldOfView / (0.4 * Min(canvasW, canvasH))`（FOV 默认 256），保证任意尺度模型初始完整可见；避免 `Camera` 默认 `ViewDistance=0`、`Screen=Empty` 导致投影坍塌到中心。
2. **表面模式复用库能力**：直接调用 `camera.Draw(g, surfaces, drawPath:=False)`，内部完成旋转+投影+画家算法 z 排序+光照着色，稳定高效。
3. **网格（线框）模式自绘**：库内 `drawPath:=True` 为「填充+黑描边」混合效果，不满足「仅看三角形网格」。本方案对每面 `camera.Rotate(vertices)` 后 `camera.Project(...)`，取 `PointXY(camera.Screen)` 得 `PointF()`，再用 `g.DrawPolygon(meshPen, pts)` 仅描边不填充，得到清晰三角形网格。因每个 `Surface` 即一个面（多为三角形），可正确呈现三角网格。
4. **点云热图着色**：投影每个 `PointCloud` 点，将归一化 `intensity`（无 intensity 时回退到归一化 z）映射到 `Designer.GetColors(term, 256, alpha)` 返回的 `Color()` 索引取色，用 `g.FillRectangle`（比 `FillEllipse` 更快）绘制小方块点；PLY 自带 `color` 字符串可优先用于单点着色。
5. **双缓冲绘制**：画布控件（Panel）设置 `DoubleBuffered = True`，在 `Paint` 事件中完成绘制，杜绝闪烁；鼠标交互只改状态不重算几何，绘制时实时旋转/投影。

性能与可靠性：

- 表面模式：画家算法 z 排序为 O(n log n)，库已实现，无需额外优化。
- 点云模式：大数据量下逐点 GDI+ 绘制偏慢，采用 `FillRectangle`、点径 1–3px、关闭高质量插值以提升吞吐；超大点云可后续加 LOD/抽稀（本期预留接口，不做过度设计）。
- 资源：文件流由 `LoadModel`/`ReadFile` 内部释放；`Camera`/`Graphics` 无长生命周期资源，重绘无内存泄漏。

## 实现注意事项

- `Camera` 必须显式初始化：`Screen = 画布尺寸`、`ViewDistance = 自动适配值`、`FieldOfView = 256`、`Offset = PointF(0,0)`，否则投影失效。
- `MouseWheel` 需为画布订阅并处理 `Handled`，避免滚轮滚动容器；`ViewDistance` 需下限钳制（如 > 1）防止除零/反转。
- 旋转灵敏度取约 0.4°/像素；右键平移按像素直接累加到 `Offset`。
- 沿用现有命名与扩展方法：`Surface.CreateObject()` 已将 `Landscape.Data.Surface` 转为 `Drawing3D.Surface`（含 `brush`）；`PointXY(screen)` 为 `Projection.vb` 中的扩展方法。
- 不修改 `Landscape` 与 `Microsoft.VisualBasic.Imaging` 任何源码，纯新增项目。

## 架构设计

```mermaid
flowchart LR
    A[MainForm 工具栏/画布/状态栏] -->|打开文件| B(ModelLoader.LoadModel / PlyReader.ReadFile)
    B -->|SceneModel| C[SceneRenderer 居中+适配]
    B -->|PointCloud()| C
    C -->|Drawing3D.Surface 集合| D[Camera]
    C -->|Point3D+intensity| D
    D -->|角度/视距/偏移 交互修改| A
    D -->|Surface模式| E[camera.Draw 填充+光照]
    D -->|Mesh模式| F[自绘 DrawPolygon 线框]
    D -->|点云模式| G[GetColors 热图 + FillRectangle]
    E & F & G --> H[GDI+ 画布 Paint]
```

## 目录结构

```
g:\pixelArtist\src\framework\gr\ModelViewer\
├── ModelViewer.vbproj   # [NEW] SDK 风格 WinForm 工程。TargetFramework=net10.0-windows；OutputType=WinExe；UseWindowsForms=true；OptionStrict=Off；ProjectReference 引用 ..\Landscape\Landscape.vbproj 与 ..\Microsoft.VisualBasic.Imaging\imaging.NET5.vbproj
├── Program.vb           # [NEW] 程序入口。包含 <STAThread> Sub Main，调用 ApplicationConfiguration.Initialize 与 Application.Run(New MainForm)
├── MainForm.vb          # [NEW] 主窗体。顶部工具栏（打开文件按钮、渲染模式 RadioButton/ComboBox、点云配色 ComboBox）、中央双缓冲 Panel 画布、底部 StatusStrip 显示信息；实现 MouseDown/MouseMove/MouseUp/MouseWheel 交互并修改 Camera 状态后 Invalidate；Paint 事件调用 SceneRenderer.Draw
└── SceneRenderer.vb     # [NEW] 渲染核心。封装 Camera 与当前数据（Surface 集合或 PointCloud 列表）、枚举 RenderMode(Surface/Mesh/PointCloud)、LoadModel(path)/LoadPointCloud(path)、居中计算、自动视距适配；提供 Draw(g As Graphics, canvasSize) 按模式分派渲染；维护点云配色 term 与 alpha；公开相机参数供 MainForm 读写
```

## 关键代码结构（接口/类型摘要）

```
' SceneRenderer.vb 中的核心成员（实现级签名）
Public Enum RenderMode
    Surface
    Mesh
    PointCloud
End Enum

Public Class SceneRenderer
    Public Property Camera As Camera
    Public Property Mode As RenderMode
    Public Property ColorScheme As String   ' 传入 Designer.GetColors 的 term，如 "viridis"
    Public Property PointSize As Integer

    Public Sub LoadModel(filePath As String)   ' 内部调用 ModelLoader.LoadModel + Surface.CreateObject + 居中/适配
    Public Sub LoadPointCloud(filePath As String) ' 内部调用 PlyReader.ReadFile + 居中/适配
    Public Sub Draw(g As Graphics, canvas As Size)
End Class
```