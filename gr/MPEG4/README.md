# MPEG4：sciBASIC# 的 H.264 视频编码与 MP4 封装库

## 概述

`MPEG4` 是 `sciBASIC#` 多媒体栈中的纯托管视频编码组件：把 RGBA 位图帧编码为 **H.264（MPEG-4 AVC）** 码流，并封装为 **MP4（ISO-BMFF）** 容器文件。全程不依赖任何原生编解码库或外部可执行程序，只依赖 .NET 运行时本身。

## 主要能力

- **H.264 视频编码管线**
  - I 帧为主的 Main Profile 编码：帧内预测（4x4/16x16 多模式）、整数变换与量化、重建环路；
  - CABAC 熵编码（含完整的上下文模型与码表，见 `H264Cabac.vb` / `H264CabacTables.vb`）；
  - 整数运动估计与 P 帧/参考帧支持（`H264MotionEstimation.vb`），B 帧预测缓冲已预留（当前由 `bFrameEnabled` 开关关闭）；
  - 码流语法按规范逐位写入：RBSP 尾比特与 emulation prevention（`BitStreamWriter.vb`）、SPS/PPS（`H264SpsPps.vb`）、Slice Header（`H264SliceHeader.vb`）。
- **MP4 ISO-BMFF 封装**
  - `Mp4BoxWriter` 按 ISO-BMFF 规范组装 `ftyp/moov/mvhd/trak/avcC/stbl` 等 Box 结构；
  - 支持 faststart（`moov` 前置）与 AVCC 格式 NAL 打包，可直接被播放器与浏览器加载。
- **编码器配置**：分辨率、帧率、QP、GOP 等参数集中在 `MP4Settings`，输出经 `MP4Stream` 写入磁盘。

## 典型用法

```vb
Using encoder As New MP4Encoder("demo.mp4", width, height, fps)
    For Each frame As Bitmap In frames
        encoder.WriteFrame(frame)
    Next
End Using
```

## 适用场景

- 科学计算 / 数据可视化的动画帧批量导出为标准 MP4 视频；
- 无需安装 FFmpeg 或其他原生依赖的纯 .NET 视频输出管线。

## 相关项目

- `gr/avi`：AVI（RIFF）容器编码器，提供 MJPEG 与未压缩 DIB 两种更简单的备选输出格式。
