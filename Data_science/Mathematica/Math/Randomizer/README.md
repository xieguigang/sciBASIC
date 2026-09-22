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

## 核心能力

- 均匀随机数生成（多种算法）；
- **正态偏差生成**（表驱动 RAND）——蒙特卡洛与随机算法的基础；
- 采样辅助方法（随机抽样、随机重排）；
- 集中式种子管理，保证实验可复现。

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
