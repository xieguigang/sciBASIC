---
name: JpegGifCodec
overview: 在 Microsoft.VisualBasic.Core (net10.0，无第三方图像库) 中补齐 JPEG 与 GIF 编解码能力。JPEG 沿用 Png 的「Codec+Reader+Writer+Image」四件套纯 VB 模式；GIF 的 LZW 部分**优先复用并扩展现有 src/Serialization/LZWStream.vb**（现有通用 LZW 为固定 14-bit 码宽、与 GIF 规范不兼容，故在保留原实现前提下新增 GIF 兼容的 GifLzwEncoder/GifLzwDecoder，复用其 bit-buffer 基础设施）。并在 DriverLoad 接入按魔数分派的默认解码驱动、在 Image.vb 新增 JpegImage/GifImage 子类并使 Bitmap.Save 按 ImageFormats 路由。JPEG 解码支持基线+渐进式（编码产基线），GIF 编码支持动画多帧。
todos:
  - id: impl-jpeg
    content: 新增 Jpeg/ 四件套：JpegCodec、JpegReader(基线+渐进式)、JpegWriter(基线)、JpegImage 对象
    status: completed
  - id: impl-gif-lzw
    content: 扩展 Serialization/LZWStream.vb，保留通用 LZW 并新增 GifLzwEncoder/GifLzwDecoder（GIF 语义、复用位缓冲）
    status: completed
  - id: impl-gif
    content: 新增 Gif/ 四件套：GifCodec(调用GifLzw+median-cut)、GifReader(多帧)、GifWriter(动画多帧)、GifImage 对象
    status: completed
    dependencies:
      - impl-gif-lzw
  - id: wire-loader
    content: 在 DriverLoad.vb 接入 DefaultImageLoader 魔数分派，打通 Register 与 Image.FromStream 回退
    status: completed
    dependencies:
      - impl-jpeg
      - impl-gif
  - id: route-save
    content: 在 Image.vb 新增 JpegImage/GifImage 子类，并让 Bitmap.Save 按 ImageFormats 路由到 Jpeg/Gif/Png 编码器
    status: completed
    dependencies:
      - impl-jpeg
      - impl-gif
  - id: verify-build
    content: 构建 Core.vbproj 并运行 JPEG/GIF 编码解码往返自检，确保 0 警告 0 错误
    status: completed
    dependencies:
      - wire-loader
      - route-save
---

## 用户需求概述

在 `Microsoft.VisualBasic.Core`（net10.0，无第三方图像库依赖）核心运行时中，补齐 JPEG 与 GIF 两种图像文件格式的编解码能力。现有 `Drawing/Bitmap/Png/` 已有一套纯 VB、无依赖的 PNG 编解码器（`PngCodec` 底层工具 + `PngReader` 解码 + `PngWriter` 编码 + `PngImage` 原始光栅模型），本次需以完全一致的方式新增 JPEG、GIF 两套编解码器。

## 核心功能

- **JPEG 解码**：支持基线顺序（baseline sequential）与渐进式（progressive）两种 8-bit JPEG，覆盖灰度（1 通道）与 YCbCr 彩色（3/4 通道，含可选透明），输出 `BitmapBuffer`。
- **JPEG 编码**：仅产出基线顺序 8-bit JPEG；含前向 DCT、量化（按质量因子缩放量化表）、Huffman 编码，默认采用 Annex-K 标准 Huffman 表（可选由像素统计生成）。
- **GIF 解码**：解析逻辑屏幕描述符、全局/局部颜色表、图形控制扩展、Netscape 循环扩展与注释，逐帧 LZW 解码，天然支持多帧与动画元数据（延迟、处置方式、透明色），输出首帧及全部帧的 `BitmapBuffer`。
- **GIF 编码**：支持静态单帧与动画多帧；跨帧 median-cut 颜色量化构建 256 色全局颜色表，逐帧 LZW 编码，写入图形控制扩展（延迟/透明/处置）与 Netscape 循环扩展。
- **LZW 复用约束**：GIF 所需的 LZW 优先复用现有 `src/Serialization/LZWStream.vb`；现有通用 LZW（固定 14-bit 码宽、与 GIF 规范不兼容）予以保留，仅在其文件内**追加** GIF 兼容的 `GifLzwEncoder`/`GifLzwDecoder`，复用其位缓冲与字典表基础设施。
- **对应图像对象**：在 `Image.vb` 的抽象 `Image` 类（@143-151）下新增 `JpegImage`、`GifImage` 两个子类（同 `Bitmap` 模式包装 `BitmapBuffer`）；`GifImage` 额外保存多帧、各帧延迟/处置/透明以及循环次数。
- **默认图像解码驱动**：在 `DriverLoad.vb` 的 `Register(loader As Func(Of Stream, Image))`（@100-102）接点上接入按文件魔数自动分派的默认解码器，使 `Image.FromStream` / `FromFile` 开箱即支持 JPEG/GIF/PNG/BMP。
- **编码可达性**：让现有 `Bitmap.Save(stream, format)` 在 `format` 为 Jpeg/Gif/Png 时分别路由到新编码器，Bmp 维持现状，使编码器经由现有 API 可用。

## 像素约定

编解码器内部原始缓冲采用 RGB(A) 行主序（与 `PngImage` 对齐），在生成/消费 `BitmapBuffer`（BGR/BGRA）时做通道重排。

## 技术栈

- 语言/运行时：VB.NET，目标框架 net10.0（与 `Core.vbproj` 一致）。
- 依赖：**零第三方库**，全部纯 VB 手写（与现有 PNG 编解码器风格一致，保证跨平台）。
- 复用：现有 `BitmapBuffer`、`BitmapReader`(BMP)、`PngReader`/`PngWriter`/`PngImage` 流水线、`ImageFormats`/`ImageFormat` 枚举与类、`DriverLoad` 模块，以及现有 `Serialization/LZWStream.vb` 的位缓冲与字典表基础设施。

## 实现策略

沿用 `Png/` 的「Codec 共享模块 + Reader 解码 + Writer 编码 + Image 原始模型」四件套结构，分别落到 `Jpeg/`、`Gif/` 两个新子目录；GIF 的 LZW 部分扩展现有 `LZWStream.vb`。

### JPEG（纯 VB 从零，命名空间 Imaging.BitmapImage.Jpeg）

- **JpegCodec.vb（共享模块）**：标记常量（SOI=FFD8/EOI=FFD9/DQT=FFDB/SOF0=FFC0/SOF2=FFC2/DHT=FFC4/DRI=FFDD/SOS=FFDA/RSTn=FFD0–FFD7/APP0=FFE0/COM=FFFE）、标准亮度/色度量化表、之字扫描表、Huffman 表构建与解码、浮点 IDCT（朴素 8x8 或 AAN）、YCbCr→RGB、位读取器、渐进式 Ss/Se/Ah/Al 与 DC/AC 精炼累加、重启标记（RSTn/DRI）处理。
- **JpegReader.vb（解码）**：解析文件头标记，收集 DQT/DHT/SOF/SOS；按 8x8 块 Huffman 解码系数→反量化→IDCT→色彩转换；基线逐扫描完成，渐进式按 spectral selection / successive approximation 多次扫描精炼后合成。输出 8-bit RGB(A)/灰度 `BitmapBuffer`。
- **JpegWriter.vb（编码）**：前向 DCT→量化→Huffman 编码，按质量因子缩放量化步长，产出合法基线 JPEG 字节流；默认 Annex-K 标准 Huffman 表，可选统计生成。
- **JpegImage.vb**：`Inherits Image`，包装 `BitmapBuffer`（灰度 1 通道、彩色 3/4 通道）+ 质量/分量元信息；实现 `Size/Save/ConvertToBitmapStream/GetMemoryBitmap`。

### GIF（命名空间 Imaging.BitmapImage.Gif）

- **扩展 `Serialization/LZWStream.vb`**：保留原有通用 `LZWStream`/`LzwEncoder`/`LzwDecoder`（固定 14-bit、Mark Nelson 风格）不变；在基类新增 Protected 可变码宽读写辅助（基于现有 `bitBuffer`/`bitCounter`），并新增 `GifLzwEncoder`/`GifLzwDecoder`（均 `Inherits LZWStream`）：构造接收 `minCodeSize`，实现 GIF 语义——`clearCode = 1 << minCodeSize`、`eoiCode = clearCode + 1`、初始码宽 `minCodeSize+1`、字典满时码宽自增至 12 位、遇 `clearCode` 重置字典；操作对象为调色板索引字节流（`Byte()`），子块分帧由 `GifCodec` 包装层处理。复用基类 `bitBuffer`/`bitCounter`/`input` 与表数组字段。
- **GifCodec.vb（共享模块）**：签名 `GIF87a`/`GIF89a`、逻辑屏幕描述符、全局/局部颜色表、图形控制扩展、应用扩展（Netscape 循环）、调用 `GifLzwEncoder`/`GifLzwDecoder`、**中位切分 median-cut 颜色量化**（构建 256 色板）。
- **GifReader.vb（解码）**：LSD+GCT+图像描述符逐帧 LZW 解码+图形控制扩展+Netscape 循环+注释；调色板→RGB，处理透明与处置方式（disposal）；产出首帧与全部帧 `BitmapBuffer` 及每帧元信息。
- **GifWriter.vb（编码）**：跨帧 median-cut 构建全局颜色表，逐帧 LZW 编码，写图形控制扩展（延迟/透明/处置），动画时写 Netscape 循环扩展；支持静态与动画多帧。
- **GifImage.vb**：`Inherits Image`，保存 `Frames As List(Of BitmapBuffer)`、各帧 Delay/Disposal/TransparentIndex、`LoopCount`；`Size` 返回逻辑屏幕尺寸，`GetMemoryBitmap` 返回首帧。

### 默认解码驱动接入（DriverLoad.vb）

新增模块级 `DefaultImageLoader(s As Stream) As Image`：读取前若干字节魔数分派——`FF D8 FF`→`JpegImage`、`GIF87a/GIF89a`→`GifImage`、PNG 签名→现有 Png 流水线产出 `Bitmap`、`"BM"`→现有 `BitmapReader`→`Bitmap`。将 `Register(loader)` 接点与默认驱动打通：`Image.FromStream` 在 `CheckRasterImageLoader` 为假时回退到 `DefaultImageLoader`（或首次使用时把 `DefaultImageLoader` 注入 `loadImage`），确保 `FromFile`/`FromStream` 开箱支持 jpg/gif，且外部已注册 loader 仍优先。

### 编码可达性（Image.vb / Bitmap）

让 `Bitmap.Save(s, format As ImageFormats)` 按 `format` 路由：Jpeg→`JpegWriter`、Gif→`GifWriter`、Png→既有 `PngWriter`、Bmp→现状（MemoryBmp）。在 `Image.vb` 的 `NET8_0_OR_GREATER` 块内新增 `JpegImage`、`GifImage` 两个 `Image` 子类。

## 实现要点（防回归）

- 复用现有 `BitmapBuffer` 的 BGR/BGRA 行主序约定，新编解码器内部统一 RGB(A)，仅在与 `BitmapBuffer` 互转时重排通道，避免颜色错位。
- 扩展 `LZWStream.vb` 时**仅追加** GIF 兼容类，不改动既有通用 `LZWStream`/`LzwEncoder`/`LzwDecoder` 行为，保证向后兼容（已确认无外部调用方）。
- JPEG 为有损格式，自检仅校验尺寸一致与像素近似（容差内）；GIF 静态帧应逐字节一致，动画多帧校验循环次数/各帧延迟元数据。
- `DriverLoad` 仅新增默认分派逻辑，不改动既有 `Register` 外部契约；外部已注册 loader 时仍优先使用。
- 渐进式 JPEG 解码需在多次扫描间维护系数精炼状态，注意 Ah/Al 与 DC/AC 预测器的跨扫描连续性；GIF LZW 注意码宽从 9 位到 12 位的自增与清除码（ClearCode/EOI）。

## 架构设计

```mermaid
flowchart TD
  A[Image.FromFile/FromStream] --> B{DriverLoad.CheckRasterImageLoader?}
  B -- 外部已注册 --> C[loadImage 委托]
  B -- 否(默认) --> D[DefaultImageLoader 魔数分派]
  D -->|FFD8FF| E[JpegImage + JpegReader]
  D -->|GIF87a/89a| F[GifImage + GifReader]
  D -->|PNG签名| G[PngReader → Bitmap]
  D -->|BM| H[BitmapReader → Bitmap]
  E --> I[BitmapBuffer]
  F --> J[多帧 BitmapBuffer]
  I --> K[Bitmap.Save(format)]
  K -->|Jpeg| L[JpegWriter]
  K -->|Gif| M[GifWriter]
  K -->|Png| N[PngWriter]
  K -->|Bmp| O[MemoryBmp]
  F -->|LZW| P[GifLzwEncoder/Decoder 扩展自 LZWStream]
  G -->|LZW| P
```

## 目录结构

```
src/
├── Serialization/LZWStream.vb          # [MODIFY] 保留通用 LZW；追加 GifLzwEncoder/GifLzwDecoder（GIF 兼容：minCodeSize/ClearCode/EOI/可变码宽），复用 bitBuffer/bitCounter/表数组
└── Drawing/Bitmap/
    ├── Jpeg/                          # [NEW] JPEG 编解码子模块（命名空间 Imaging.BitmapImage.Jpeg）
    │   ├── JpegCodec.vb              # [NEW] 共享底层：标记/量化表/之字扫描/Huffman/IDCT/YCbCr→RGB/位读取器/渐进式精炼/重启标记
    │   ├── JpegImage.vb             # [NEW] Inherits Image 的图像对象，包装 BitmapBuffer + 质量/分量元信息
    │   ├── JpegReader.vb            # [NEW] 解码模块：支持基线 + 渐进式，输出 BitmapBuffer
    │   └── JpegWriter.vb            # [NEW] 编码模块：前向DCT+量化+Huffman，产出基线 JPEG
    ├── Gif/                          # [NEW] GIF 编解码子模块（命名空间 Imaging.BitmapImage.Gif）
    │   ├── GifCodec.vb              # [NEW] 共享底层：签名/LSD/颜色表/图形控制/Netscape循环/调用GifLzw/median-cut量化
    │   ├── GifImage.vb             # [NEW] Inherits Image 的图像对象，保存多帧/延迟/处置/循环
    │   ├── GifReader.vb            # [NEW] 解码模块：逐帧 LZW 解码 + 扩展解析，输出多帧 BitmapBuffer
    │   └── GifWriter.vb            # [NEW] 编码模块：多帧动画 LZW 编码 + 扩展写入
    ├── Png/PngWriter.vb             # [REFERENCE] 复用既有 PNG 编码路由
    ├── BitmapReader.vb              # [REFERENCE] BMP 解码回退
    ├── netcore8.0/Image.vb          # [MODIFY] 在 NET8_0_OR_GREATER 内新增 JpegImage、GifImage 子类；Bitmap.Save 按 format 路由到新编码器
    └── GDI+/Driver/DriverLoad.vb    # [MODIFY] 新增 DefaultImageLoader 魔数分派，打通 Register(loader) 与默认驱动；FromStream 无外部 loader 时回退默认
```

## 验证

- 复用现有 `_smoketest` 风格或临时控制台做 编码→解码 往返：JPEG 校验尺寸一致与像素近似；GIF 静态帧逐字节一致；GIF 动画校验多帧数/循环次数/各帧延迟。
- 确认 `LZWStream` 中通用 LZW 与新增 GIF LZW 共存互不干扰（无外部调用方，行为不变）。
- `dotnet build Core.vbproj -c Debug/Release` 确保 0 警告 0 错误。