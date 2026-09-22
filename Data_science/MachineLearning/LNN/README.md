# 面向时间序列的液态神经网络（含 ODE 求解器）

## 引言

普通 RNN 的状态更新是**离散递归**：

$$h_{t+1} = f(h_t, x_t)$$

它隐含一个假设：时间是均匀采样的步。但真实世界的时间序列常常**采样不均匀**（传感器偶尔丢包、临床指标按需检测）。

**液态神经网络（Liquid Neural Network）**换了一个视角：让隐藏状态遵循一个**可学习的常微分方程**

$$\frac{dh}{dt} = f(h, x, t; \theta)$$

这样时间以连续量出现，采样间隔直接进入积分过程，因此天然适配不规则采样。

## 核心能力

| 能力 | 命名空间 / 类型 | 说明 |
|---|---|---|
| **ODE 积分** | `ODESolver` | Euler、Heun、RK4 与自适应 RK45 |
| **连续时间单元** | `LiquidCell`、`LiquidLayer` | 液态神经网络的构造单元 |
| **非线性** | `ActivationFunctions` | 激活函数集合 |
| **模型与训练** | `LiquidNeuralNetwork`、`LNNTrainer` | 组装模型并用 SGD / Adam 训练 |
| **时间序列预处理** | `TimeSeriesUtils` | 窗口切分、缩放与数据集划分 |

## 为什么有四种积分器

| 积分器 | 阶数 | 特点 |
|---|---|---|
| **Euler** | 1 | 最快，误差 O(h) |
| **Heun** | 2 | 预测-校正，精度提升明显 |
| **RK4** | 4 | 精度 / 成本最佳平衡 |
| **RK45（自适应）** | 4/5 | 自动调整步长，兼顾精度与速度 |

选择依据是**状态变化的剧烈程度**：变化平缓时 Euler 足够，出现快速瞬态时应改用自适应步长。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork

' 1. 定义液态单元与层
Dim cell As New LiquidCell(stateSize:=32, odeSolver:=ODESolver.RK4)
Dim net As New LiquidNeuralNetwork()
Call net.AddLayer(New LiquidLayer(cell))
Call net.AddLayer(New DenseLayer(units:=1))

' 2. 时间序列预处理（窗口切分 + 缩放）
Dim windows = TimeSeriesUtils.Window(raw, windowSize:=50, stride:=5)
Dim scaled = TimeSeriesUtils.Scale(windows)

' 3. 训练（SGD / Adam）
Dim trainer As New LNNTrainer(optimizer:=Optimizer.Adam, learningRate:=0.002)
Call trainer.Train(net, scaled, epochs:=100)
```

## 实现要点

- **连续时间带来的好处**：采样间隔 Δt 直接作为积分步长参与计算，因此"同样两个观测之间隔了 1 秒还是 1 小时"对模型是**有意义**的信息——这是离散 RNN 无法表达的。
- **可学习动态**：ODE 的右端函数 `f` 由网络参数化，因此模型学到的不是"状态转移表"，而是"状态的演化规律"；这使模型具有更好的外推性质。
- **积分器的成本权衡**：RK4 每步需要 4 次 `f` 求值（4 倍于 Euler），因此训练时间显著更长；自适应 RK45 在状态平缓时自动放大步长，通常在长序列上更划算。
- **与 `ODESolver.Extensions` 的联系**：求解器与本包共享 ODE 求解的核心实现，因此积分结果同样可以导出为数据框做分析。

## 包信息

- Assembly：`Microsoft.VisualBasic.DeepLearning.LiquidNeuralNetwork`
- TargetFramework：`net10.0`
- Tags：`scibasic;liquid-neural-network;ode;runge-kutta;continuous-time;time-series;recurrent-network`
- 许可：GPL-3.0-or-later
