# 信号处理：FFT、小波、滤波与峰值检测

## 引言

一维信号处理虽然方法众多，但目标不外乎四类：

1. **换域**：从时域变到频域 / 时频域，看清成分（FFT、小波）；
2. **去噪**：抑制不需要的成分（滤波、Kalman）；
3. **找特征**：定位关键点（峰值检测）；
4. **对齐**：把两条或多条信号对齐（DTW、COW）。

本包为这四类目标各自提供实现，并且都建立在同一套信号数据结构之上。

## 核心能力

### 换域

| 命名空间 | 方法 |
|---|---|
| `...SignalProcessing.FFT` | 快速傅里叶变换与频谱分析 |
| `...SignalProcessing.WaveletTransform` | 小波变换（时频局部化，适合非平稳信号） |

### 去噪与估计

| 命名空间 | 方法 |
|---|---|
| `...SignalProcessing.Filters` | 数字滤波，含 Savitzky-Golay 平滑（保形保峰） |
| `...SignalProcessing.KalmanFilter` | Kalman 状态估计 |
| `...SignalProcessing.HungarianAlgorithm` | 最优分配（用于信号匹配） |

### 特征与对齐

| 命名空间 | 方法 |
|---|---|
| `...SignalProcessing.PeakFinding` | 峰值检测 |
| `...SignalProcessing.NDtw`（+ `Preprocessing`） | 动态时间规整（DTW）及其预处理 |
| `...SignalProcessing.COW` | 相关优化规整（色谱类信号对齐） |
| （根） | 基线校正、重采样与归一化 |
| `...SignalProcessing.Sampler.EmGaussian` | EM 高斯混合采样 |
| `...SignalProcessing.Source`（+ `Arithmetic` / `Generators`） | 信号源：算术运算与波形 / 噪声发生器 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.SignalProcessing

' 1. 谱分析
Dim spectrum = FFT.Forward(signal)

' 2. 保形平滑（Savitzky-Golay 不会把峰抹平）
Dim smoothed = Filters.SavitzkyGolay(signal, window:=11, order:=3)

' 3. 峰值检测
Dim peaks = PeakFinding.Detect(smoothed, minHeight:=0.1)

' 4. 两条信号对齐（含基线校正预处理）
Dim aligned = NDtw.Align(Preprocessing.BaselineCorrect(a), Preprocessing.BaselineCorrect(b))

' 5. 色谱式对齐
Dim warped = COW.Align(reference, sample, segments:=20)

' 6. 生成测试信号
Dim test = Source.Generators.Sine(frequency:=5, sampleRate:=100, length:=1000)
```

## 实现要点

- **FFT vs 小波的选择**：FFT 给出**全局**频率成分，但丢失"何时出现"的信息；小波在时间与频率上同时局部化，适合瞬态与突变（如心电的 R 波）。
- **Savitzky-Golay 为什么适合光谱**：它在滑动窗口内做**多项式最小二乘拟合**，因此能平滑噪声同时保留峰高与峰宽——普通移动平均会把峰抹低、抹宽。
- **DTW 与 COW 的分工**：DTW 通过允许时间轴非线性伸缩来对齐形状相似的信号（适合语音、心电）；COW 通过分段线性伸缩对齐**峰位置**（适合色谱、光谱），更符合仪器漂移的物理模型。
- **基线校正为何要在对齐之前**：基线偏移会严重干扰相似度度量，导致对齐结果偏离；先校正基线再对齐是标准顺序。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.SignalProcessing`
- TargetFramework：`net10.0`
- Tags：`scibasic;signal-processing;fft;wavelet;peak-detection;filtering;savitzky-golay;kalman-filter;dtw;cow-alignment`
- 许可：GPL-3.0-or-later
