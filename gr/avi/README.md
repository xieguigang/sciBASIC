# AVI 视频编码器：从 RGB 帧序列到可播放视频

## 引言

当我们把一组渲染出来的画面导出为视频时，通常面临两个选择：

- 使用带压缩的编解码器（H.264 等）——体积小，但要引入原生库与授权问题；
- 使用**无压缩容器**——体积大，但**零依赖、无损、易于调试与逐帧校验**。

本包选择后者：直接把 RGB 位图帧写进 **AVI（RIFF）** 容器。对科研可视化而言，这往往正是想要的——导出的动画要与渲染结果**逐像素一致**。

## 设计目标

- **零依赖**：不引入编解码器与原生库；
- **无损**：帧数据原样写入，便于校验与后续再处理；
- **简单可控**：帧率、尺寸、流元数据都由一个 `Settings` 集中描述。

## 核心特性

- **RIFF 头组装**：`AVIMainHeader`、`AVIStreamHeader` 与 `AVIStream` 负责构造 AVI 的主头与流头；
- **帧写入**：`FrameStream` 负责把位图帧追加到流中；
- **流程编排**：`Encoder` 依据 `Settings`（帧率、帧尺寸、编解码标签与流元数据）驱动整个写出过程；
- **字节缓冲**：`UInt8Array` 提供写帧时使用的字节缓冲。

## 命名空间与关键类型

全部类型位于根命名空间 `Microsoft.VisualBasic.Imaging.AVIMedia`：

| 类型 | 职责 |
|---|---|
| `AVIMainHeader` | RIFF 主头（文件级元信息与流数量） |
| `AVIStreamHeader` | 流头（帧率、帧尺寸、编解码标签） |
| `AVIStream` | 流对象（帧数据的组织与写出） |
| `FrameStream` | 帧写入：把位图帧追加到视频流 |
| `Encoder` | 编码器入口：组装头部并驱动帧写入 |
| `Settings` | 编码参数集合（帧率、尺寸、元数据） |
| `UInt8Array` | 写帧用的字节缓冲 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Imaging.AVIMedia

Dim settings As New Settings With {
    .FrameRate = 25,
    .Width = 1280,
    .Height = 720
}

Using encoder As New Encoder("animation.avi", settings)
    For Each frame As Bitmap In renderedFrames
        Call encoder.AddFrame(frame)
    Next

    Call encoder.Flush()
End Using
```

## 实现要点

- **RIFF 是「箱子套箱子」**：AVI 文件由 RIFF 块（chunk）嵌套组成，主头声明流的数量与索引位置，流头声明帧格式；顺序写错就会导致播放器无法识别。
- **为什么需要索引（idx1）**：AVI 靠索引块记录每一帧在文件中的偏移；播放器据此随机访问帧——这正是 `Encoder` 在收尾阶段必须完成的工作。
- **无压缩的取舍**：体积约为压缩方案的一个数量级，但换来「任何环境都能写出、任何播放器都能播放」，并且帧内容可被逐像素比对。

## 包信息

- Assembly：`Microsoft.VisualBasic.Imaging.AVIMedia`
- TargetFramework：`net10.0`
- Tags：`scibasic;avi;video-encoder;riff;animation;bitmap;frame-stream`
- 许可：GPL-3.0-or-later
