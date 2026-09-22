# Gibbs 采样与 Metropolis-Hastings 采样器

## 引言

当联合分布难以直接采样时，MCMC（马尔可夫链蒙特卡洛）提供了一条迂回路径：**构造一条马尔可夫链，使其平稳分布恰好是目标分布**，然后沿链采样即可。

本包提供两种最常用的 MCMC 方法，并分别服务于两类具体问题：

| 方法 | 在这里解决什么问题 |
|---|---|
| **Gibbs 采样** | 生物序列中的**重复模体发现** |
| **Metropolis-Hastings** | 列联表分析中的 **Markov 基采样**与卡方检验 |

## Gibbs 采样：为什么适合模体发现

模体（motif）发现的问题形式是：给定若干条序列，寻找一个在所有序列中都出现（且允许少量变异）的模式，但**每条序列中的出现位置未知**。

Gibbs 采样天然匹配这个结构：

1. 随机初始化每条序列中模体的位置；
2. **逐个位置重采样**：固定其它所有序列的位置，据此构建位置权重矩阵（PWM），再按该矩阵为当前序列重新采样一个位置；
3. 重复足够多轮后，采样结果收敛到高评分的位置组合。

**关键优势**：不需要枚举所有位置组合（那是指数级的），因为条件分布可以精确计算并直接采样。

## Metropolis-Hastings：Markov 基

列联表的独立性检验需要知道在「行和列边际固定」的条件下各种表格出现的概率；Markov 基给出了在这一约束空间中移动的合法步。本包实现了基于 Markov 基的 Metropolis-Hastings 采样，用于：

- 估计列联表的精确 p 值（当卡方近似不适用时）；
- 探索边际固定下的表格分布。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.GibbsSampling

' 1. 模体发现（Gibbs 采样）
Dim sampler As New MotifSampler(sequences, motifWidth:=10)

Call sampler.Run(iterations:=2000, burnIn:=500)

Console.WriteLine(sampler.ConsensusMotif)
Console.WriteLine(sampler.Score)

' 2. 列联表检验（Metropolis-Hastings over Markov basis）
Dim p = MarkovBasisSampler.PValue(table, steps:=100000)
```

## 实现要点

- **为什么要区分 burn-in**：马尔可夫链需要若干步才能从初始状态「走到」目标分布；这段时间的样本不代表目标分布，必须丢弃。丢弃的步数即 burn-in。
- **Gibbs 是 MH 的特例**：当条件分布可以精确采样时，Metropolis-Hastings 的接受率恒为 1，这正是 Gibbs 采样——所以它更快，但要求条件分布可采样。
- **收敛诊断的重要性**：模体发现对初始位置敏感，实践中会并行运行多条链并比较共识模体是否一致。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.GibbsSampling`
- TargetFramework：`net10.0`
- Tags：`scibasic;gibbs-sampling;metropolis-hastings;mcmc;motif-finding;markov-basis;contingency-table`
- 许可：GPL-3.0-or-later
