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

### 方程组（`ODEs` 模型 + `var` 状态变量）

| 方法 | 阶数 | 特点 | 适用 |
|---|---|---|---|
| **RK4** | 4 | 精度 / 成本平衡最佳，零分配内核 | **默认推荐** |
| **RK45**（Dormand-Prince 5(4)） | 5(4) | 自适应步长 + 误差控制 + FSAL | 解变化剧烈 / 未知合适步长 |

### 单个标量方程（`ODE` 模型 + `df(x, y)` 委托）

| 方法 | 阶数 | 特点 | 适用 |
|---|---|---|---|
| Euler | 1 | 最简单，误差 O(h) | 教学 / 快速原型 |
| RK2 | 2 | 改进欧拉法（Heun） | 一般用途 |
| **RK4** | 4 | 精度 / 成本平衡最佳 | **默认推荐** |
| Gill | 4 | RK4 的变体，减少舍入误差累积 | 长时程积分 |
| RK45 | 5(4) | 自适应步长，输出重采样到等距网格 | 高精度 / 未知步长 |

> 注：早期文档提到的「梯形法（隐式）」在当前代码中**并未实现**，隐式/刚性方程请使用
> `Sundials.CVODE` 这类带变步长变阶数（BDF）的求解器。

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Math.Calculus`（根） | 积分器与求解入口 |
| `....Calculus.Dynamics` | 动态系统模型：变量、参数与初值 |
| `....Calculus.Dynamics.Data` | 动态系统定义的数据模型 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics

Public Class LogisticModel : Inherits ODEs

    Dim y As var

    Public r As Double = 1.0
    Public K As Double = 1.0

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        Dim yi As Double = y.Value

        dy(y) = r * yi * (1.0 - yi / K)
    End Sub

    Protected Overrides Function y0() As var()
        y.Value = 0.5
        Return {y}
    End Function
End Class

' 固定步长 RK4：结果序列含有 n + 1 个采样点，包含积分区间的两个端点
Dim model As New LogisticModel()
Dim result = model.Solve(n:=1000, a:=0.0, b:=5.0)

' 自适应步长 RK45：同样返回等距网格上的 n + 1 个采样点
Dim adaptive = model.SolveAdaptive(n:=1000, a:=0.0, b:=5.0, atol:=1.0E-9, rtol:=1.0E-7)

' 结果可直接导出为数据框 / CSV / 数值表
Dim df = result.DataFrame()
Dim tbl = result.AsTable()
```

### 性能敏感场景：数组式直通导数

`ODEs.func` 的每个阶段都需要把状态向量同步到 `var` 对象上，再通过 `dy(x)` 写回导数。
在蒙特卡洛参数拟合、遗传算法这类需要成千上万次求解的场景下，可以设置
`FastDerivative` 跳过这层包装：

```vbnet
model.FastDerivative = Sub(t, y, dy)
                           dy(0) = r * y(0) * (1.0 - y(0) / K)
                       End Sub
```

或者使用 `GenericODEs` 的数组式构造函数：

```vbnet
Dim system As New GenericODEs(
    vars,
    Sub(t, y, dy)
        For i As Integer = 0 To y.Length - 1
            dy(i) = -0.1 * y(i)
        Next
    End Sub)
```

注意：使用直通入口的时候，积分过程中不会更新各个 `var` 变量的数值（积分结束时仍会同步
最终状态）。基准测试见下一节。

## 实现要点

- **采样网格**：所有求解器都返回 `n + 1` 个等距采样点，`x(0) = a`、`x(n) = b`，
  末点精确等于积分上界（早期版本只返回 `[a, b)` 上的 n 个点并且结果序列与时间序列错开一步，
  已修正）。
- **零分配内核**：RK4/RK45 使用预分配的 `Double()` 工作缓冲（`SolverBuffers`）与融合
  算术内核，每一个积分步都不产生托管堆分配，输出列也是预分配后直写。相比通过 `Vector`
  运算符逐个阶段拼接临时向量的旧实现，实测吞吐提升 4~13 倍、分配量下降 1~2 个数量级
  （变量越多，直通路径的收益越明显）。
- **RK45**：Dormand-Prince 5(4) 嵌入式方法对，用 5 阶解与 4 阶解之差估计局部截断误差
  （`atol + rtol * max(|y|, |y5|)`），按 `0.9 * err^(-1/5)`（限幅 `[0.2, 5]`）调整步长；
  具有 FSAL 性质，每个积分步只需 6 次函数求值。变步长结果通过三次 Hermite 插值重采样到
  等距输出网格。
- **模型即数据的价值**：把变量、参数与初值声明为数据，使模型可以来自配置文件或界面输入，
  而不必为每个模型写一个类。
- **显式 vs 隐式**：Euler / RK 族都是显式方法——直接用当前导数推下一步；当方程"刚"
  （各分量时间尺度差异巨大）时显式方法需要极小的步长才能稳定，此时应改用隐式方法或
  `Sundials.CVODE`。
- **与 `Sundials.CVODE` 的分工**：本包提供轻量、无依赖的通用积分；CVODE 提供变步长变阶数的
  Adams / BDF 求解，专门处理刚性问题。
- **重复求解**：`GenericODEs` 在构造时快照初值，每次 `Solve` 都从同一初值出发；而
  `ODEs` 子类的 `y0()` 实现负责设置每次求解的初值。

## 自检与基准

`_smoketest` 工程包含解析解对拍、收敛阶验证、结果对齐断言、RK45 与 RK4 对拍以及性能基准：

```shell
dotnet build _smoketest/smoketest.vbproj -c Release
dotnet _smoketest/bin/Release/net10.0/smoketest.dll
```

输出以 `PASS: 0 failure(s)` 结束并且退出码为 `0`。

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Calculus`
- TargetFramework：`net10.0`
- Tags：`scibasic;ode;runge-kutta;euler-method;dynamical-system;numerical-integration;simulation`
- 许可：GPL-3.0-or-later
