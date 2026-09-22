# GDI+ 绘图封装与 System.Drawing 互操作（Windows）

## 引言

`sciBASIC#` 的图形引擎是**驱动式**的：同一套绘图指令（`IGraphics`）可以渲染到位图、SVG 或 PostScript。要在 Windows 上产生**位图**输出，就需要一个 GDI+ 后端——这正是本包的角色。

它做三件事：

1. 把 GDI+ 的绘制表面包装成统一的画布（`GDICanvas` / `Graphics2D`）；
2. 在 `sciBASIC#` 图形模型与 `System.Drawing` 类型之间做双向转换；
3. 补齐 GDI+ 不提供的输出格式（GIF、TIFF）与字体度量能力。

## 设计目标

- **驱动可替换**：按 `IGraphics` 写的渲染代码不感知 GDI+；
- **互操作可逆**：`sciBASIC#` 模型 ↔ `System.Drawing` 类型可双向转换；
- **补齐缺口**：GDI+ 缺少的 GIF / TIFF 编码与字体度量在这里实现。

## 核心特性

- **画布**：`Graphics` 命名空间包装 GDI+ 绘制表面（`GDICanvas`、`Graphics2D`、`Wmf`），并提供 CSS 互操作桥（`CssInterop`）；
- **编码器**：`FileEncoder` 命名空间提供 GIF 与 TIFF 写出器（`GifEncoder`、`TiffWriter`）与图像格式枚举；
- **互操作**：`Interop` 命名空间承载 P/Invoke 绑定与托管图像（`GDIPlusInterop`、`GDIPlusImage`、`DrawingInterop`、`BitmapBuffer`），在 `sciBASIC#` 图形模型与 `System.Drawing` 类型之间封送；
- **图像与滤镜**：`Imager` 与 `Filters` 命名空间（`Effects`、`ImageExtensions`、`Utils`）在同一条流水线中应用位图效果；
- **文本**：`Text` 命名空间提供字体度量（`FontMetrics`）、图形文本绘制与文本布局模型。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Drawing`（根） | `Imager` 与扩展方法 |
| `....Drawing.Graphics` | GDI+ 画布包装与 CSS 互操作 |
| `....Drawing.FileEncoder` | GIF / TIFF 编码与图像格式枚举 |
| `....Drawing.Interop` | GDI+ 互操作与托管图像类型 |
| `....Drawing.Filters` | 位图效果与图像扩展 |
| `....Drawing.Imaging.BitmapImage` | 位图效果与像素工具 |
| `....Drawing.Drawing2D.Text` | 字体度量与文本绘制 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Drawing.Graphics

Using canvas As New GDICanvas(width:=800, height:=600, dpi:=96)
    Call canvas.DrawLine(Pens.Black, 0, 0, 800, 600)
    Call canvas.Save("output.png")
End Using
```

## 实现要点与平台约束

- **目标框架为 .NET Framework**：本包依赖 `System.Drawing`（Windows GDI+），因此**仅限 Windows**；跨平台场景请使用位图 / SVG / PostScript 驱动。
- **为什么需要 `BitmapBuffer`**：GDI+ 的位图访问有跨进程与生命周期约束，把像素数据先取到托管缓冲再处理，可以避免长时间锁定 GDI+ 对象。
- **字体度量的必要性**：排版（尤其自动换行）需要精确的字符宽度，`FontMetrics` 把 GDI+ 的度量结果封装为可复用的查询接口。

## 包信息

- Assembly：`Microsoft.VisualBasic.Drawing.Windows`
- TargetFramework：`net4.8`
- Tags：`scibasic;gdi-plus;system-drawing;graphics-interop;windows-forms;gif-encoder;tiff;font-metrics`
- 许可：GPL-3.0-or-later
