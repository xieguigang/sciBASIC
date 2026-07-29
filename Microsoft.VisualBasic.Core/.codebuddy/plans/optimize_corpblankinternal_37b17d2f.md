---
name: optimize_corpblankinternal
overview: 重写 ImageTools.vb 中的 CorpBlankInternal 函数，消除纯色背景减裁时的过度减裁（误裁内容）问题，改用 BitmapBuffer 单次扫描计算精确内容包围盒。
todos:
  - id: rewrite-corpblank
    content: 重写 CorpBlankInternal 为单次包围盒扫描，消除过度减裁
    status: completed
  - id: preserve-margin-edge
    content: 保留 margin 填充逻辑并处理整图背景边界情况
    status: completed
    dependencies:
      - rewrite-corpblank
  - id: verify-build
    content: 编译验证 ImageTools 模块无错误
    status: completed
    dependencies:
      - rewrite-corpblank
      - preserve-margin-edge
---

## 用户需求

优化 `src\Drawing\GDI+\ImageTools.vb` 中的私有函数 `CorpBlankInternal`，该函数针对纯色背景图像，基于指定背景色（`blankColor`，默认白色）进行减裁，将多余背景裁到仅保留 `margin` 大小边距。

## 产品概述

对纯色背景位图做"内容自动包围盒裁剪"：找到所有非背景像素的最小外接矩形，将图片裁到该矩形，再在四周补 `margin` 像素的背景色。要求在保证不裁掉任何真实内容（非背景像素）的前提下完成减裁。

## 核心特性

- 基于 `BitmapBuffer.GetPixel` 进行快速内存像素读取，判断像素是否等于背景色（复用 `GDIColors.Equals` 含容差与透明色处理）。
- 计算精确的内容包围盒（`minX/minY/maxX/maxY`），裁剪到包围盒以彻底消除"过度减裁"（误裁内容）问题。
- 保留 `margin` 参数语义：裁剪后再在四周补 `margin` 像素背景色。
- 处理边界情况：整图均为背景色时返回原图，不再退化为极小尺寸裁剪。

## 技术栈

- 语言/框架：VB.NET，目标框架涵盖 .NET Framework 4.8 与 .NET (net6/netcore8)。
- 现有依赖（同模块可复用，无需新增）：`BitmapBuffer`（内存位图缓冲，`FromBitmap`/`GetPixel`/`Width`/`Height`）、`GDIColors.Equals`、`ImageCrop` 扩展、`DriverLoad.CreateDefaultRasterGraphics` / `IGraphics` / `GdiRasterGraphics`。

## 实现方案

### 总体策略

将现有 4 趟单向扫描 + 重复裁剪（在循环内对循环变量做不对称 `top-=1`/`left-=1`/`bottom+=1`/`right+=1` 调整）重写为**单次全图扫描计算精确内容包围盒**，再一次性精确裁剪，最后复用原 `margin` 填充逻辑。

### 关键技术决策与权衡

1. **单次包围盒扫描替代 4 趟重复裁剪**：原实现每趟扫描后都对图像重新裁剪并重新构建 `BitmapBuffer`，且 `top/left` 有 `>0` 守卫而 `bottom/right` 无条件 `+1`，上下/左右边界处理不对称，在已裁切区域上再次计算会使最终裁剪矩形错位，可能切入内容。改用"扫描全部像素记录 `min/max`"后，裁剪矩形严格等于内容外接矩形，内容像素必然被包含，从数学上杜绝过度减裁。
2. **性能**：时间复杂度由约 4×W×H（且含 4 次重裁与重读缓冲）降为 1×W×H 单次扫描；`GetPixel` 为内联按行主序读取，缓存友好。空间复杂度 O(1)（仅保存 4 个边界标量）。
3. **复用现有基础设施**：仍通过 `BufferInternal` 获取 `BitmapBuffer`（其内部已对 `FromBitmap` 异常做 trace 包装），最终裁剪仍调用 `res.ImageCrop(cropArea)`（已在 NET48/NET8 双框架下封装，行为一致），避免引入新的平台分支。
4. **边界/容差一致性**：背景判定继续使用 `GDIColors.Equals(p, blankColor)`（默认容差 3，已正确处理透明色 alpha），与原有语义完全一致。

### 关键算法（伪代码）

```
bmp = BufferInternal(res, trace)
minX = res.Width : minY = res.Height : maxX = -1 : maxY = -1
For y = 0 To bmp.Height - 1
  For x = 0 To bmp.Width - 1
    If Not GDIColors.Equals(bmp.GetPixel(x, y), blankColor) Then
      minX = Min(minX, x) : maxX = Max(maxX, x)
      minY = Min(minY, y) : maxY = Max(maxY, y)
    End If
  Next
Next
If maxX < 0 Then Return res   ' 整图皆背景色，返回原图
cropRect = New Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1)
res = res.ImageCrop(cropRect)
' 下方保留原 margin 填充逻辑（不变）
```

## 实现要点（防止回退）

- 保持 `CorpBlankInternal` 签名 `Private Function CorpBlankInternal(res As Bitmap, margin%, blankColor As Color, isTransparent As Boolean, trace$) As Image` 不变，公开重载 `CorpBlank` 行为不受影响，向后兼容。
- 仅修改函数体（170–293 行），不改动 `BufferInternal`、`ImageCrop`、公开 `CorpBlank` 等其它成员。
- 复用 `GDIColors.Equals` 默认容差 3，不自行比较 ARGB，避免破坏透明色与抗锯齿容差语义。
- 裁剪矩形宽高用 `max - min + 1`，确保包含边界内容像素；调用 `ImageCrop` 时传递精确矩形，不额外偏移，消除原 `±1` 不对称调整。
- 原 `margin > 0` 时的填充逻辑（`CreateDefaultRasterGraphics` + `gfx.Clear` + `gfx.DrawImage`）原样保留。
- 整图背景场景返回原图（而非原实现的近似 1×1 退化裁剪），避免输出空/极小图。

## 架构与目录

本任务为单文件内函数级逻辑优化，不新增模块、类型或公共 API，符合现有 `Imaging` 命名空间结构。

## 目录结构

```
src/Drawing/GDI+/ImageTools.vb   # [MODIFY] 重写 CorpBlankInternal（170-293 行）函数体：
                                 #   1) 通过 BufferInternal 取得 BitmapBuffer；
                                 #   2) 单次双重循环扫描全像素，用 GDIColors.Equals 判定非背景，
                                 #      记录 minX/minY/maxX/maxY 精确包围盒；
                                 #   3) maxX<0（整图背景）时直接返回原图；
                                 #   4) 用 res.ImageCrop(Rectangle(minX,minY,maxX-minX+1,maxY-minY+1)) 精确裁剪；
                                 #   5) 保留并复用原 margin>0 的四周填充逻辑（不变）。
```