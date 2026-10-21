---
name: ode-solver-review-and-optimize
overview: 审查并修复 ODE 求解器工程（odes-netcore5.vbproj）中的算法实现错误，把 Dynamics 下的 RK4 求解器重构为零分配的缓冲区内核，并新增 Dormand-Prince RK45 自适应步长求解器；同时新建 _smoketest 自检 + 基准工程做解析解对拍与加速比验证。
todos:
  - id: fix-scalar-solvers
    content: 修正标量求解器：Allocate 输出 n+1 点、Euler 公式、Gill 系数与边界、ODE/ODEOutput 健壮性
    status: completed
  - id: rebuild-rk4-core
    content: 新增 SolverCore 缓冲内核并重构 RungeKutta4：修正权重、x/y 对齐、初值记录与步数
    status: completed
    dependencies:
      - fix-scalar-solvers
  - id: optimize-dynamics-solve
    content: 优化 ODEs.Solve：去 ImputeNA、缓冲直写、预分配数组输出、Parameters 反射缓存、空 var 守卫
    status: in_progress
    dependencies:
      - rebuild-rk4-core
  - id: fast-api-and-extensions
    content: Use [subagent:code-explorer] to 核对 Vector 内核与下游调用点，新增数组式快速构造与 SolveFast 并接入 Extensions
    status: completed
    dependencies:
      - optimize-dynamics-solve
  - id: fix-aux-solvers
    content: 修正 RefODEs 时间索引、ValueVector 查找缓存、SolverIterator.Tick 结束判定、ODEsOut 长度守卫
    status: completed
    dependencies:
      - optimize-dynamics-solve
  - id: add-rk45
    content: 新增标量与方程组 Dormand-Prince RK45 自适应求解器（FSAL、PI 步长控制、稠密输出重采样）
    status: completed
    dependencies:
      - rebuild-rk4-core
  - id: smoketest-benchmark
    content: 新建 _smoketest 工程：解析解对拍、收敛阶、对齐断言、RK45 对拍与性能基准
    status: completed
    dependencies:
      - fast-api-and-extensions
      - fix-aux-solvers
      - add-rk45
  - id: verify-and-docs
    content: Use [skill:lsp-code-analysis] to 核对引用影响面，跑 Debug/Release 构建与下游编译，更新 README 并汇总审查报告
    status: completed
    dependencies:
      - smoketest-benchmark
---

## 用户需求

审查并优化 `ODE/odes-netcore5.vbproj`（常微分方程求解系统）的算法实现，找出实现错误并提升计算性能。

## 产品概述

该工程提供两类求解能力：一是标量单方程求解（Euler / RK2 / RK4 / Gill，输入 `df(x,y)` 委托，输出 `ODEOutput` 的 X/Y 序列）；二是方程组建模求解（继承 `ODEs` 用 `var` 字段声明状态量、`Double` 字段声明参数，输出 `ODEsOut` 的 x 序列与各变量序列，供 Lorenz 仿真、Monte Carlo/GA 参数拟合、CDF 积分等下游使用）。

## 核心功能

- **算法正确性修正（用户已确认全部修正）**：
- RK4 权重缺因子 2（当前只有 2 阶精度）修正为标准四阶；
- Euler 更新误用时间 t 代替 y 修正；
- Gill 参数顺序颠倒、循环越界、k3/k4 系数与终值权重错误修正；
- 方程组求解器结果 x 与 y 相差一步、初值未写入、多积分一步、积分终点不等于 b 全部修正。
- **性能优化（最高档）**：
- 保持现有公开 API 的零分配内核重构（每步从十余次 Vector/数组分配与 4 次 LINQ 填充降为 0）；
- 新增数组式直通求解入口，绕过 `var` 包装与反射开销；
- 新增 Dormand-Prince RK45(5(4)) 自适应步长求解器（误差控制 + FSAL + 稠密输出），标量与方程组两条路径都提供。
- **健壮性**：无 `var` 字段时不再抛异常；小步数下 `dx`/`Resolution` 不再越界；`SolverIterator.Tick` 结束后不再静默空转；`RefODEs` 不再依赖“每步恰好 4 次函数调用”的隐式约定。
- **验证交付**：新增可运行的 `_smoketest` 自检 + 基准工程（解析解对拍、收敛阶验证、x/y 对齐断言、RK45 与 RK4 对拍、新旧实现耗时与分配对比），构建 0 警告 0 错误，下游工程编译通过。

## 视觉与输出效果

无界面改动。运行 `_smoketest` 后控制台逐项打印断言结果与基准表格（用例、步数、耗时、分配字节、加速比），最后输出 `PASS: 0 failure(s)` 并以退出码 0 结束。

## 技术栈

- 语言/框架：VB.NET（`net10.0`），沿用现有 `odes-netcore5.vbproj`（RootNamespace `Microsoft.VisualBasic.Math.Calculus`，AssemblyName `Microsoft.VisualBasic.Math.ODEsSolver`，`GenerateDocumentationFile=True` 必须 0 警告）。
- 依赖（不新增）：`Core.vbproj`（`Vector(Of T)` / `Vector` / `SimdEngine` / `DoubleRange` / `Sequence`）、`Math.NET5.vbproj`。
- 验证：`dotnet build`（Debug/Release）+ 新建控制台 `_smoketest` 工程。

## 实施策略

### 1. 标量求解器（ODESolvers/）

- `Allocate` 改为输出 **n+1** 个点：X 直接构造 `Double(n){}` 并令 `X(i)=a+i*h`、`X(n)=b`（不再依赖 `Sequence(range,n)` 的 `Enumerate(n).Take(n)` 截断语义），Y 长度同为 n+1；`Sequence` 仅用于序列化兼容。
- `Eluer` 用 `y(i)=y(i-1)+h*f(x,y(i-1))`；`RK2`/`RK4` 保持 Heun / 经典四阶公式，但把热路径改为局部缓存 + `AggressiveInlining` 的私有 `Step` 函数，参数 `ByVal`。
- `Gill` 按标准系数重写：`k3=f(t+h/2, y+h[((√2-1)/2)k1+((2-√2)/2)k2])`、`k4=f(t+h, y+h[(-√2/2)k2+(1+√2/2)k3])`、`y+=h/6[k1+2(2-√2)k2+2(2+√2)k3+k4]`，循环 `1 To n-1`（配合 n+1 点输出）。
- 新增 `DormandPrince.vb`（`Partial Module ODESolver`）提供标量 `RK45(ode, t0, tEnd, atol, rtol, initialStep, maxSteps)`：DP5(4) 七阶段 + FSAL、`err=max|y5-y4|/(atol+rtol*max(|y|,|y5|))`、`h*=clamp(0.9*err^(-1/5),0.2,5)`，返回变步长结果（同时给出固定网格重采样重载供 CDF 使用）。
- `ODE.GetValue` 直连 `df` 属性并做 Nothing 检查；`ODEOutput.sum`/`GetPointsData` 改为基于数组的 O(1)/单次访问（`GetPointsData` 不再每次枚举都 `X.ToArray`）。

### 2. 方程组建模求解内核（Dynamics/）

- 新增 `SolverCore.vb`：统一持有 `y/k1..k7/ytmp/yerr/scratch` 的 `Double()` 预分配缓冲，提供融合内核（VB 内联循环，JIT 可自动向量化）：
- `Rk4Step(y, k1, k2, k3, k4, h)`：`y[i]=y[i]+h/6*(k1[i]+2k2[i]+2k3[i]+k4[i])`，全程 0 分配；
- `Dp5Step(...)`：RK45 阶段组合与误差范数，同样 0 分配。
避免与 `Vector` 运算符（每次返回新对象）混用；只在结果封箱时构造 `Vector`。
- `RungeKutta4`：修正权重为 `(k1+2k2+2k3+k4)*h/6`；`solverIteration` 改为「先记录 (a,y0)，再执行 n 步、共 n+1 点、`x(i)=a+i*h`、末点精确 b」，去掉多积分的第 n+1 次；保留 `Solve`/`GetResult`/`solverIteration` 签名（`SolverIterator` 依赖它），内部改用缓冲内核。
- `ODEs.ODEs(dx,y,ByRef k)`：删掉 `ImputeNA`（改为进入前一次性清零/直写），状态同步改为「缓存 `vars` 引用 + `y.Array` 缓冲直写 `var.Value`」，去掉 `y(x.Index)` 索引器虚调用；`func` 仍由用户重写，语义不变。
- `ODEs.Solve`：输出预分配 `Double(n+1)` 直写（`NamedCollection(Of Double).value` 本就是数组），删除 `List(Of Double)` 增长与二次 `ToArray`；`yinit` 用 `y0` 直接切片。
- `Parameters`/`GetVariables`/`GetParameters`：加 per-`Type` 的 `ConcurrentDictionary` 静态缓存，避免每次 Solve 两轮反射；构造时 `fields.Length=0` 用空数组。
- `RefODEs`：由计数器改为按时间步索引（`RefValues` 指针由求解器显式推进或按 `dx/h` 定位），与阶段数解耦；`ValueVector` 增加名称→序列的缓存解析，去掉每阶段 `InputHandler.ToString`。
- `SolverIterator.Tick`：检查 `MoveNext` 返回值并暴露 `Finished`；`ODEsOut.dx`/`Resolution` 对 `x.Length<3` 做守卫。

### 3. 数组式直通快速 API（扩展性）

- `GenericODEs` 增加构造：`Sub New(vars As var(), df As Func(Of Double, Double(), Double()))`（y 只读、返回写入外部缓冲的导数），`ODEs` 增加 `SolveFast(n, a, b)`：不做 `var` 回写、不反射、不装箱，直接走上缓冲内核。
- `Extensions.Solve(IEnumerable(Of NonlinearVar), dt)` 与 `CDF` 内部改用快速入口（Lorenz/Lorenz 3 变量 24000 步是主要受益者）。

### 4. 性能与复杂度

- 单次 RK4 步：原实现约 14 次 `Vector` 分配 + 4 次 LINQ（每次枚举器+数组+`Vector`）+ N 次属性虚调用；新实现为 O(N) 单次遍历、0 次托管分配（结果封箱除外）。
- 输出：O(n·N) 一次性预分配，无 `List` 扩容与复制；预期 2~6 倍吞吐提升（小 N 场景由分配主导，收益更大）。
- 自适应 RK45：每步约 6~7 次函数求值，按容差自动降步，刚性/长时程场景显著减少总步数；FSAL 复用 k7→k1 再省 1 次。
- 复制/同步热点避免 N+1 次遍历：状态回写与内核合并为一次循环。

### 5. 兼容性与风险控制

- 公开类型与成员签名全部保留（`ODEsOut`、`ODEOutput`、`Solve/GetResult/solverIteration`、`df` 委托），仅数值结果按修正后变化（用户已确认）；新增成员一律带 XML 注释。
- 不改 `Math`/`Core` 工程，不改 `Vector` 运算符语义，避免波及下游；不引入全局可变静态状态（`Parameters` 缓存按 `Type` 只读）。
- 线程安全：仍保持「一个实例一次求解」的既有约束，缓冲内核为实例私有，不新增跨实例共享。
- VB 注意事项：`Module` 成员不能 `Shared`；局部变量不得与类型/属性同名（`Index/Step/next/call/set` 为保留字）；`System.Math` 全限定避免被同名 Module 遮蔽；`Module`/`Class` 里避免与 `Array`/`File` 类型撞名。

### 6. 验证方案

- `_smoketest/Program.vb` 断言集：`y'=-y`→`e^-t`；`y'=t`→`t²/2`；简谐振子能量守恒；Lorenz 短时程与参考值；RK4 收敛阶（h 减半误差比≈16）；Euler/RK2/Gill/RK4 相互一致性；x/y 对齐与 `x.Last=b`；`ODEOutput.sum` 的 `∫f dt` 校验；RK45 与高分辨率 RK4 对拍（误差、步数、函数求值次数）；`RefODEs` 索引不再越界；`SolverIterator` 结束后 `Tick` 不抛异常。
- 基准：n=1000/10000/24000、N=3/10，输出耗时、`GC.GetTotalAllocatedBytes`、加速比；与 `git stash` 版实现旧版对拍（基准函数以 `SolveLegacy` 形式保留在 smoketest 中）。
- 构建验收：`dotnet build ode/odes-netcore5.vbproj -c Debug` 与 `-c Release` 均 0 警告 0 错误；`_smoketest` 退出码 0；`ODESolver.Extensions`、`Bootstrapping`、`Lorenz_system`、`Visualization/test5` 编译通过；README 更新为实际支持的方法表（并说明梯形法未实现或补充实现）。

## 目录结构（工程根：g:/pixelArtist/src/framework/Data_science/Mathematica/Math/ODE）

```
ODE/
├── odes-netcore5.vbproj            # [MODIFY] 增加 <Compile Remove="_smoketest\**" />，保持包元数据不变
├── README.md                       # [MODIFY] 方法表/精度说明与实现对齐，补充 RK45 与快速 API 用法、性能说明
├── ODESolvers/
│   ├── Solver.vb                   # [MODIFY] Allocate 改 n+1 点（X 手建、末点=b）；Eluer 公式修正；RK2/RK4 ByVal+内联热路径；新增 RK45 标量入口的共享步进辅助
│   ├── Gill.vb                     # [MODIFY] 按标准 Gill 系数重写 k3/k4/终值权重，修正 df(x,y) 参数顺序与循环边界 1..n-1
│   ├── DormandPrince.vb            # [NEW] 标量 Dormand-Prince 5(4)：FSAL、PI 步长控制、误差范数、变步长结果与固定网格重采样重载（带 XML 注释）
│   ├── ODE.vb                      # [MODIFY] GetValue 直连 df 并判空；ToString 安全；保留默认属性语义
│   └── ODEOutput.vb                # [MODIFY] sum/GetPointsData 改数组直取，避免每次枚举重建 X；保持 INamedValue/Xml 序列化属性不变
├── Dynamics/
│   ├── SolverCore.vb               # [NEW] 预分配 Double() 缓冲（y/k1..k7/err/scratch）+ 融合 RK4/DP5 内核 + 步长控制，全部 0 分配（Internal/Public 需带注释）
│   ├── RungeKutta4.vb             # [MODIFY] 修正权重；solverIteration 改为记录 y0 后执行 n 步、共 n+1 点、末点=b；内部改用 SolverCore 内核，保留公开签名
│   ├── RungeKutta45.vb             # [NEW] 方程组自适应求解：DP5(4)、atol/rtol、FSAL、最大/最小步长、稠密输出按等距网格重采样，返回 ODEsOut 兼容结构
│   ├── ODEs.vb                     # [MODIFY] 去 ImputeNA；状态同步走 Double() 缓冲直写 var.Value；Solve 预分配数组输出；空 var 字段守卫；Parameters 反射缓存；新增 SolveFast/SolveAdaptive
│   ├── GenericODEs.vb              # [MODIFY] 新增 Func(Of Double, Double(), Double()) 构造与数组式 func 适配
│   ├── RefODEs.vb                  # [MODIFY] 用时间索引替代 RK 计数器，避免阶段数耦合与越界
│   ├── SolverIterator.vb           # [MODIFY] Tick 检查 MoveNext，新增 Finished 属性，避免结束后静默空转
│   └── Data/
│       ├── ODEsOut.vb              # [MODIFY] dx/Resolution 长度守卫
│       └── ValueVector.vb          # [MODIFY] 名称→序列解析缓存，降低每阶段字典+字符串开销
├── Extensions.vb                   # [MODIFY] CDF/ECDF 走修正后的 RK4/RK45 并校验积分区间；Solve(NonlinearVar(), dt) 走快速入口
└── _smoketest/
    ├── smoketest.vbproj            # [NEW] 控制台工程（OutputType=Exe，引用 ..\odes-netcore5.vbproj），不参与库编译
    ├── Program.vb                  # [NEW] 断言框架（assert 计数 + PASS/FAIL 汇总 + 退出码），主入口
    ├── Cases.vb                    # [NEW] 解析解用例：指数/多项式/简谐振子/Lorenz，含旧实现 SolveLegacy 对拍基准
    └── Benchmark.vb                # [NEW] 耗时/分配/加速比基准，打印表格
```

## 关键接口（新增，供下游对接）

```
' 数组式直通入口：y 只读，返回值不参与（导数写入 dy）
Public Sub New(vars As var(), df As Func(Of Double, Double(), Double()))

' 自适应求解：返回与 Solve 兼容的 ODEsOut，按 n 点等距网格重采样
Public Function SolveAdaptive(n As Integer, a As Double, b As Double,
                              Optional atol As Double = 1.0E-9,
                              Optional rtol As Double = 1.0E-7,
                              Optional initialStep As Double = 0.0) As ODEsOut
```

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 在实现前后核对 `Vector`/`Vector(Of T)` 的原位内核与访问器（`Array`、`LinearSumInPlace`、`CopyFrom`、`SimdEngine`）签名，并盘点 `ODEsOut`/`ODEOutput`/`ODEs.Solve` 的全部下游调用点（ODESolver.Extensions、Bootstrapping、GAF_test2、Lorenz_system、Visualization/test5），确认签名兼容性。
- Expected outcome: 一份“可安全调用的 Math/Core API 清单 + 下游调用点清单”，确保重构不破坏编译，且不重复造已有的向量内核。

### Skill

- **lsp-code-analysis**
- Purpose: 修改完成后用语义导航核对被改成员的引用（`Eluer`/`RK2`/`RK4`/`Gill`/`Solve`/`solverIteration`/`GetResult`/`GetValue`/`RefValues`），确认没有遗漏的调用方与因签名/行为变化导致的隐式破坏。
- Expected outcome: 引用清单与影响面报告，作为最终验收证据（配合 `dotnet build` 0 警告 0 错误）。