# 常微分方程求解核心与动态模型

## 引言

「动态系统」在数学上就是一组常微分方程：

$$\frac{d\mathbf{y}}{dt} = f(t, \mathbf{y}; \mathbf{p})$$

要把模型变成可运行的仿真，需要三件事：

1. **模型描述**：变量、参数与初值如何组织；
2. **积分方法**：用哪套数值格式推进时间；
3. **结果载体**：每一步的变量取值如何保存与后续分析。

本包把这三件事分开处理，使同一个模型可以换求解器，同一个求解器可以服务多个模型。

## 可用的积分方法

| 方法 | 阶数 | 特点 | 适用 |
|---|---|---|---|
| Euler | 1 | 最简单，误差 O(h) | 教学 / 快速原型 |
| RK2 | 2 | 折中 | 一般用途 |
| **RK4** | 4 | 精度 / 成本平衡最佳 | **默认推荐** |
| Gill | 4 | RK4 的变体，减少舍入误差累积 | 长时程积分 |
| 梯形法 | 2 | 隐式，数值稳定性更好 | 轻微刚性问题 |

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math.Calculus`（根） | 积分器与求解入口 |
| `....Calculus.Dynamics` | 动态系统模型：变量、参数与初值 |
| `....Calculus.Dynamics.Data` | 动态系统定义的数据模型 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics

' 1. 声明模型：变量、参数、初值（模型即数据）
Dim model As New DynamicModel()
Call model.AddVariable("y1", initial:=1.0)
Call model.AddVariable("y2", initial:=0.0)
Call model.AddParameter("k", value:=0.5)

' 2. 用 RK4 积分
Dim result = ODESolver.RK4(model, tEnd:=10.0, steps:=1000)

' 3. 结果可直接导出为数据框 / CSV 做后续分析（见 ODESolver.Extensions）
Dim df = result.ToDataFrame()
```

## 实现要点

- **为什么 RK4 是默认选择**：Euler 需要极小步长才能有可接受精度（成本高），而更高阶方法（如 RK8）又需要更多函数求值；RK4 用 4 次求值换来 O(h⁴) 精度，是长期实践中的最佳平衡点。
- **模型即数据的价值**：把变量、参数与初值声明为数据，使模型可以来自配置文件或界面输入，而不必为每个模型写一个类。
- **显式 vs 隐式**：Euler / RK 族都是显式方法——直接用当前导数推下一步；当方程"刚"（各分量时间尺度差异巨大）时显式方法需要极小的步长才能稳定，此时应改用梯形法或 `Sundials.CVODE` 这类带变步长自适应的方法。
- **与 `Sundials.CVODE` 的分工**：本包提供轻量、无依赖的通用积分；CVODE 提供变步长变阶数的 Adams / BDF 求解，专门处理刚性问题。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Calculus`
- TargetFramework：`net10.0`
- Tags：`scibasic;ode;runge-kutta;euler-method;dynamical-system;numerical-integration;simulation`
- 许可：GPL-3.0-or-later
