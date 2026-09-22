# 基于遗传编程的符号回归

## 引言

机器学习的多数模型是**黑箱**：给你预测值，但不告诉你「公式」是什么。而科研场景往往恰恰需要公式——因为它可以被写进论文、被物理解释、被继续推导。

**符号回归（symbolic regression）**正是为此：不是拟合参数，而是**搜索表达式本身**。本包用遗传编程（GP）实现这一搜索：

- **个体** = 一棵表达式树；
- **适应度** = 该表达式在数据上的误差（MSE / MAE / SSE / SAE）；
- **演化** = 交叉与变异重塑树的形状。

## 设计目标

- **结果为符号表达式**：输出可检查、可求导、可嵌入其它模型；
- **误差目标可选**：不同目标对离群点与噪声的敏感度不同；
- **表达式模型可插拔**：通过工厂构建初始个体，便于引入先验（如只允许使用特定算子）。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming`（根） | 遗传编程入口 |
| `....GeneticProgramming.evolution` | 演化引擎：种群、选择与世代推进 |
| `....GeneticProgramming.evolution.measure` | 适应度度量：MSE / MAE / SSE / SAE |
| `....GeneticProgramming.model` | 符号表达式模型 |
| `....GeneticProgramming.model.factory` | 初始表达式工厂 |
| `....GeneticProgramming.model.impl` | 表达式节点的具体实现 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming
Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution

' 1. 用工厂生成初始种群
Dim population = FactoryX.CreatePopulation(size:=500)

' 2. 指定误差目标并演化
Dim engine As New EvolutionEngine(measure:=Measure.MSE)

For i As Integer = 1 To generations
    Call engine.NextGeneration(population, x, y)
Next

' 3. 取出最优表达式
Dim best = engine.Best
Console.WriteLine(best.ToString())
```

## 实现要点

- **符号回归与参数拟合的分工**：参数拟合在**给定结构**下求最优参数（见 `DataFittings` 包）；符号回归搜索的是**结构本身**。两者常配合使用——GP 先找出候选形式，再用 LM 精确拟合其参数。
- **适应度函数的敏感度**：MSE 对大误差敏感（适合高斯噪声），MAE 对离群点稳健；选择哪个会直接影响搜索结果。
- **表达式膨胀（bloat）问题**：GP 倾向于演化出越来越大但精度提升有限的表达式；实现中通过限制树深度与选择压力来抑制。
- **可解释性的收益**：得到形如 `y = a·x1/(b + x2)` 的结果，比得到一个神经网络权重矩阵更容易被领域专家接受与验证。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming`
- TargetFramework：`net10.0`
- Tags：`scibasic;genetic-programming;symbolic-regression;expression-tree;evolutionary-algorithm;polynomial-model;fitness-function`
- 许可：GPL-3.0-or-later
