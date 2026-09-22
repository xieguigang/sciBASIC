# WAV 音频文件读写与声纹特征提取

## 引言

音频分析的完整链路是：

```text
.wav 文件 ─► 采样数据 ─► 帧化 ─► 频域特征（Mel 滤波器组）─► 归一化 ─► 特征向量
```

本包同时覆盖两端：

- **读**：解析 RIFF/WAVE 容器，拿到采样率、通道数、位深与原始采样缓冲；
- **取特征**：把采样数据转换为适合机器学习模型输入的声纹特征向量。

## 核心能力

### WAVE 容器解析与写出

- 直接访问 `FMT` 与 `data` 子块（sub-chunk）；
- 获取采样率、通道数、位深与 PCM 采样数据；
- 支持写出 WAVE 文件。

### 声纹特征提取

| 步骤 | 说明 |
|---|---|
| **Mel 滤波器组** | 把线性频谱映射到 Mel 刻度（更接近人耳感知） |
| **Delta 系数** | 描述特征的**变化率**，捕捉动态信息 |
| **CMVN** | 倒谱均值方差归一化，消除信道与录音条件差异 |
| **L2 归一化** | 把特征向量投影到单位球面，使距离度量更稳定 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Wave

' 1. 读取 WAV 文件
Using wav As WaveFile = WaveFile.Open("./audio.wav")
    Console.WriteLine($"rate={wav.SampleRate}, channels={wav.Channels}, bits={wav.BitsPerSample}")

    ' 2. 提取声纹特征（Mel 滤波器组 + delta + CMVN + L2）
    Dim features = wav.ExtractVoiceprint()
    Console.WriteLine(features.Dimensions)
End Using

' 3. 写出 WAVE 文件
Call WaveFile.Write("./copy.wav", samples, sampleRate:=16000, channels:=1)
```

## 实现要点

- **为什么用 Mel 刻度**：人耳对低频的分辨能力远高于高频；Mel 刻度正是对这一感知特性的近似。用 Mel 滤波器组可以把"人听起来相似"的信号映射到特征空间中相近的位置。
- **CMVN 解决什么问题**：同一句话用不同麦克风录制，得到的原始特征会有系统性的均值 / 方差偏移；CMVN 通过减去均值、除以标准差把这种偏移消除，显著提升跨设备泛化能力。
- **delta 系数的价值**：静态特征描述"当前是什么音"，delta 特征描述"音在怎么变"；两者结合才能刻画语音的动态过程。
- **L2 归一化的作用**：在高维特征上，向量的**模长**往往由录音音量决定，而非内容；归一化到单位球面后，余弦相似度只反映方向（内容）差异。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Wave`
- TargetFramework：`net10.0`
- Tags：`scibasic;wav;audio;voiceprint;mfcc;mel-filterbank;cmvn;riff`
- 许可：GPL-3.0-or-later
