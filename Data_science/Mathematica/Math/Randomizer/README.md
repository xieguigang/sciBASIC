# 随机数生成器与可重放采样

## 引言

「随机数」在科研计算里有一个反直觉的要求：**必须可复现**。仿真结果、蒙特卡洛估计、随机抽样都应当能通过固定种子重放。同时，不同场景对随机源的要求也不同：

- **统计模拟**：要求周期长、分布均匀（Mersenne Twister）；
- **大规模循环**：要求速度快（xorshift）；
- **多线程**：要求线程安全；
- **生成正态偏差**：需要专门的变换方法。

本包把这些需求分别实现，并集中管理种子。

## 可用的生成器

| 生成器 | 特点 | 适用 |
|---|---|---|
| **Mersenne Twister** | 周期 2¹⁹⁹³⁷−1，等分布性质优良 | 统计模拟、蒙特卡洛 |
| **xorshift 风格** | 极快，状态小 | 吞吐敏感的循环 |
| **线程安全包装** | 并发安全访问 | 多线程采样 |
| **表驱动 RAND** | 经典实现，用于正态偏差 | 兼容性 / 教学 |
| **TrueRandom** | 混合真随机：OS 加密级 CSPRNG + 环境噪声注入 | 不可预测性、蒙特卡洛、随机抽样 |
| **EnvironmentEntropy** | 纯环境噪声熵收集器（PDH/进程/GC/计时） | 高熵种子、密钥盐 |

## 真随机数（环境噪声驱动）

前四类均为**伪随机**生成器（由种子决定、可复现）。当研究需要**不可预测**
的真随机源（如密钥盐、需抗回溯的蒙特卡洛、安全相关抽样）时，本包提供两套
基于操作系统环境噪声的真随机组件：

- **`TrueRandom`**（混合生成器，快、大规模）：以操作系统加密级 CSPRNG
  （`System.Security.Cryptography.RandomNumberGenerator`，由系统熵池驱动）做
  批量展开，并用 `EntropyMixer`（HMAC-SHA256 计数器模式）将其与环境噪声折叠，
  每 1024 字节刷新一次密钥（折叠新 CSPRNG 块 + 一次环境噪声采样），实现前向保密。
  实现 `IProvideRandomValues` 接口，并额外提供批量方法
  `GenerateBytes(n)` / `GenerateInt32s(n)` / `GenerateDoubles(n)`。
- **`EnvironmentEntropy`**（纯环境噪声熵收集器，慢、高熵）：只采样实时环境噪声
  —— 高精度计时抖动、系统时钟、进程 CPU/内存/线程/句柄统计、GC 计数与堆大小、
  线程池积压、GUID，以及 **Windows 上的 PDH 性能计数器**（处理器时间、上下文
  切换、磁盘队列、可用内存等；非 Windows 自动回退到跨平台统计）——经 SHA256
  压缩并演化产出。**不调用任何 CSPRNG**，适合少量高熵种子。

> 环境噪声采集（尤其 PDH `CollectQueryData`）为毫秒级，仅用于初始化与周期性
> 刷新；批量产出主路径为 HMAC-SHA256 计数器模式，吞吐可接受。

## 核心能力

- 均匀随机数生成（多种算法）；
- **正态偏差生成**（表驱动 RAND）——蒙特卡洛与随机算法的基础；
- 采样辅助方法（随机抽样、随机重排）；
- **真随机数生成**：`TrueRandom` / `EnvironmentEntropy` 基于 OS 环境噪声与 CSPRNG；
- 集中式种子管理，保证实验可复现（伪随机部分）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math

' 1. 固定种子，保证可复现
Dim rng As New MersenneTwister(seed:=20240919)

' 2. 均匀随机数
Dim u As Double = rng.NextDouble()

' 3. 正态偏差（用于蒙特卡洛）
Dim z As Double = rnd.NextNormal()

' 4. 从数据中随机抽样
Dim sample = rng.ReservoirSampling(population, k:=100)

' 5. 并行场景使用线程安全包装
Dim safeRng As New ThreadSafeRandom(seed:=42)

' 6. 真随机数（环境噪声 + CSPRNG，不可预测）
Dim trng As New TrueRandom()
Dim bytes = trng.GenerateBytes(1024)        ' 1024 字节真随机
Dim ints  = trng.GenerateInt32s(100)        ' 100 个全范围 Int32
Dim dbls  = trng.GenerateDoubles(50)        ' 50 个 [0,1) 双精度
Dim r = trng.Next(1, 7)                     ' [1,7) 整数（骰子）

' 7. 纯环境噪声高熵种子（不依赖 CSPRNG）
Dim ent As New EnvironmentEntropy()
Dim seed = ent.GetSeed()                    ' 32 字节（256-bit）高熵种子
Dim more = ent.GetBytes(64)                 ' 64 字节环境熵
```

## 实现要点

- **周期长度为什么重要**：伪随机序列必然循环。若周期短于模拟所需随机数总量，序列会重复，导致结果出现虚假规律。Mersenne Twister 的超长周期正是为大规模模拟设计的。
- **正态偏差的生成**：均匀分布不能直接转为正态分布，必须通过变换（如 Box-Muller、Marsaglia 极坐标法或表驱动近似）实现；本包提供表驱动 RAND 以兼顾速度与精度。
- **平行采样与可复现性的冲突**：多线程共享一个随机源会引入不确定的调用顺序，破坏可复现性；正确做法是为每个线程派生独立子种子。
- **抽样方法**：蓄水池抽样（reservoir sampling）可以在**不知道总体大小**的情况下做等概率抽样，适合流式数据。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math`（`Randomizer`）
- TargetFramework：`net10.0`
- Tags：`scibasic;random;mersenne-twister;xorshift;monte-carlo;pseudorandom;normal-deviates;sampling`
- 许可：GPL-3.0-or-later
