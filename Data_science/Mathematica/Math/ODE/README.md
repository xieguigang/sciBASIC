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

| 方法 | 枚举值 | 阶数 | 类型 | 每步求值 | 适用 |
|---|---|---|---|---|---|
| Euler | `Euler` | 1 | 显式固定步长 | 1 | 教学 / 快速原型 |
| RK2（Heun） | `RK2` | 2 | 显式固定步长 | 2 | 一般用途 |
| Gill | `Gill` | 4 | 显式固定步长 | 4 | 长时程积分（舍入误差累积较小） |
| **RK4** | `RK4` | 4 | 显式固定步长 | 4 | **默认推荐** |
| ABM4（Adams-Bashforth-Moulton） | `ABM4` | 4 | 显式多步预测-校正 | 2（RK4 起步 4 步） | 光滑非刚性问题的批量仿真 |
| 后向 Euler（BDF1） | `BackwardEuler` | 1 | 隐式（L-稳定） | Newton 迭代 | 强刚性问题 |
| 梯形法（Crank-Nicolson） | `Trapezoidal` | 2 | 隐式（A-稳定） | Newton 迭代 | 一般刚性问题 |
| BDF2 | `BDF2` | 2 | 隐式 | Newton 迭代 | 刚性问题 |
| Rosenbrock-ROS2 | `RosenbrockROS2` | 2 | 线性隐式（L-稳定） | 每步 1 次 LU | 刚性问题（无 Newton 迭代） |
| **RK23**（Bogacki-Shampine 3(2)） | `RK23` | 3(2) | 自适应显式 | 3 | 粗略 / 低频仿真 |
| **RK45**（Dormand-Prince 5(4)） | `RK45` | 5(4) | 自适应显式 | 6 | 高精度通用 |
| Störmer-Verlet | `Verlet` | 2 | 辛几何 | 3 | 哈密顿系统（位置/动量可分裂） |
| Yoshida4 | `Yoshida4` | 4 | 辛几何 | 9 | 哈密顿系统（高精度能量守恒） |

### 单个标量方程（`ODE` 模型 + `df(x, y)` 委托）

| 方法 | 阶数 | 特点 | 适用 |
|---|---|---|---|
| Euler | 1 | 最简单，误差 O(h) | 教学 / 快速原型 |
| RK2 | 2 | 改进欧拉法（Heun） | 一般用途 |
| **RK4** | 4 | 精度 / 成本平衡最佳 | **默认推荐** |
| Gill | 4 | RK4 的变体，减少舍入误差累积 | 长时程积分 |
| RK45 | 5(4) | 自适应步长，输出重采样到等距网格 | 高精度 / 未知步长 |

> 注：本包提供的是**纯托管、无原生依赖**的通用求解器；变阶数、带误差控制的
> Adams / BDF（适合长时间刚性演化）仍然由 `Sundials.CVODE` 提供，两者可以按场景选择。

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

### 切换积分方法

```vbnet
' 统一入口：第 4 个参数选择算法
Dim euler = model.Solve(1000, 0.0, 5.0, ODESolverMethod.Euler)
Dim abm4 = model.Solve(1000, 0.0, 5.0, ODESolverMethod.ABM4)

' 便捷方法
Dim rk2 = model.SolveRK2(1000, 0.0, 5.0)
Dim gill = model.SolveGill(1000, 0.0, 5.0)
Dim rk23 = model.SolveRK23(1000, 0.0, 5.0, options:=New ODESolverOptions With {.Atol = 1.0E-6, .Rtol = 1.0E-4})
```

### 刚性（隐式）求解器与 Jacobian

隐式方法默认使用**中心差商**计算 Jacobian（零配置），也可以注入解析 Jacobian：

```vbnet
model.Jacobian = Sub(t, y, j)
                     j(0, 0) = rate
                 End Sub

Dim options As New ODESolverOptions With {
    .NewtonTolerance = 1.0E-10,
    .MaxNewtonIterations = 50,
    .RefactorEachIteration = False,     ' 修正 Newton：每步只分解一次迭代矩阵
    .MaxStepSubdivisions = 8            ' Newton 失败时对当前步做内部二分细分
}

Dim stiff1 = model.SolveBackwardEuler(200, 0.0, 1.0, options:=options)
Dim stiff2 = model.SolveTrapezoidal(200, 0.0, 1.0, options:=options)
Dim stiff3 = model.SolveBDF2(200, 0.0, 1.0, options:=options)
Dim stiff4 = model.SolveRosenbrockROS2(200, 0.0, 1.0, options:=options)
```

Newton 迭代无法收敛的时候，当前积分步会用一阶的后向 Euler 做内部二分细分（最多
`MaxStepSubdivisions` 次）；仍然失败则抛出带有时间点信息的 `InvalidOperationException`，
不会把 NaN 写入结果。

### 辛几何求解器（哈密顿系统）

通用的 `f(t, y)` 无法自动识别位置/动量分裂，因此需要显式实现 `Kick` 与 `Drift`：

```vbnet
Public Class Harmonic : Inherits SymplecticODEs

    Dim q As var
    Dim p As var

    ' dq/dt = G(p)
    Protected Overrides Sub Drift(t As Double, q As Double(), p As Double(), dq As Double())
        dq(0) = p(0)
    End Sub

    ' dp/dt = F(q)
    Protected Overrides Sub Kick(t As Double, q As Double(), p As Double(), dp As Double())
        dp(0) = -q(0)
    End Sub

    Protected Overrides Function y0() As var()
        q.Value = 1.0
        p.Value = 0.0
        Return {q, p}
    End Function
End Class

Dim verlet = harmonic.SolveVerlet(2000, 0.0, 200.0)     ' 二阶，3 次求值/步
Dim yoshida4 = harmonic.SolveYoshida4(2000, 0.0, 200.0) ' 四阶，9 次求值/步
```

位置分量与动量分量默认取状态向量的前半/后半（要求维度为偶数）；也可以通过
`PositionIndices` / `MomentumIndices` 指定。`SymplecticODEs` 同时实现了通用的右端函数，
因此同样可以用 RK4 / RK45 对它积分（便于对照）。

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
- **RK23**：Bogacki-Shampine 3(2)（4 阶段 + FSAL，每步 3 次求值），步长因子
  `0.9 * err^(-1/3)`，同样通过 Hermite 插值重采样，适合粗略 / 低频仿真。
- **ABM4**：前 4 步使用 RK4 起步并保存导数历史，之后每个积分步只需 2 次函数求值
  （AB4 预测 + AM4 校正）；`n <= 4` 时自动退化为 RK4。
- **隐式方法**：Newton 迭代（默认修正 Newton，每步分解一次迭代矩阵 `W`）；
  后向 Euler 为 `I − hJ`，梯形法为 `I − h/2·J`，BDF2 为 `3/2·I − hJ`
  （BDF2 的第一步用后向 Euler 起步，否则会退化为 1 阶）。
- **Rosenbrock-ROS2**：`γ = 1 + 1/√2`、`a21 = 2 − √2`、`c21 = −3`、`b1 = b2 = 1/2`，
  满足相容性、二阶条件与 `R(∞) = 0`（L-稳定），每个积分步只需要一次 LU 分解。
- **辛几何**：`Verlet`（踢-漂移-踢）与 `Yoshida4`（三次 Verlet 组合，系数
  `w1 = 1/(2 − 2^(1/3))`、`w0 = −2^(1/3)/(2 − 2^(1/3))`）；它们保持离散辛结构，
  因此能量误差在长时程上有界而不是线性增长。
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

`_smoketest` 工程包含解析解对拍、收敛阶验证、结果对齐断言、刚性问题的稳定性对照、
Rosenbrock 稳定性函数、辛几何能量守恒、多步法起步、13 种方法的统一分发以及性能基准：

```shell
dotnet build _smoketest/smoketest.vbproj -c Release
dotnet _smoketest/bin/Release/net10.0/smoketest.dll
```

输出以 `PASS: 0 failure(s)` 结束并且退出码为 `0`（当前 200 条断言）。

基准示例（logistic，n = 20000，单变量，Release）：

| 方法 | 耗时(ms) | 分配(MB) | 函数求值 |
|---|---|---|---|
| euler | ~0.9 | 0.3 | 20000 |
| rk4 | ~2.4 | 0.3 | 80000 |
| abm4 | ~2.2 | 0.3 | 40012 |
| backward euler | ~7 | 0.3 | ~59900 |
| rosenbrock ros2 | ~6 | 0.3 | 40001 |
| rk23 / rk45 | ~0.3 | 0.3 | 166 / 85（自适应） |
| verlet / yoshida4 | ~1.5 / ~3.4 | 0.5 | 3 / 9 次每步 |

## 包信息

- Assembly：`Microsoft.VisualBasic.Math.Calculus`
- TargetFramework：`net10.0`
- Tags：`scibasic;ode;runge-kutta;euler-method;dynamical-system;numerical-integration;simulation`
- 许可：GPL-3.0-or-later
