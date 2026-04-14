# Sundials CVODE 常微分方程求解器 - VB.NET完整实现

## 项目概述

本项目是一个完全使用VB.NET语言实现的Sundials CVODE常微分方程求解器，**不依赖任何第三方数值计算库**，仅基于.NET框架中的基础数学函数库实现。

## 实现的核心功能

### 1. 向量操作类 (NVector.vb)
- 向量创建、复制、索引访问
- 向量算术运算：加法、减法、点乘、标量乘法
- 范数计算：L1范数、L2范数、无穷范数、加权RMS范数
- 原地操作：避免不必要的内存分配

### 2. 稠密矩阵类 (DenseMatrix.vb)
- 矩阵创建、复制、索引访问
- 矩阵运算：加法、减法、乘法、转置
- 矩阵-向量乘法
- 范数计算：Frobenius范数、1-范数、无穷范数

### 3. 线性求解器 (LinearSolver.vb)
- **DenseLinearSolver**: 稠密矩阵LU分解（带部分主元选择）
- **BandLinearSolver**: 带状矩阵LU分解（针对带状矩阵优化）
- 前代/回代求解
- 条件数估计

### 4. CVODE核心求解器 (CVODE.vb)
- **Adams-Bashforth-Moulton方法**（1-12阶）
  - 适用于非刚性问题
  - 预测-校正格式
- **BDF方法**（1-5阶）
  - 适用于刚性问题
  - Newton迭代求解
- 自适应步长控制
- 自适应阶数控制
- 误差估计和控制
- Jacobian矩阵计算（解析或有限差分）

### 5. 高级功能扩展 (CVODEAdvanced.vb)
- 根查找功能
- 事件检测
- 流式API构建器
- 求解结果导出

## 文件结构

```
CVODE_Solver/
├── NVector.vb              # 向量操作类 (~570行)
├── DenseMatrix.vb          # 稠密矩阵类 (~470行)
├── LinearSolver.vb         # 线性求解器 (~520行)
├── CVODE.vb                # CVODE核心求解器 (~1075行)
├── CVODEAdvanced.vb        # 高级功能扩展 (~460行)
├── CVODEExample.vb         # 使用示例 (~380行)
├── Sundials.CVODE.vbproj   # 项目文件
└── README.md               # 使用文档
```

## 使用方法

### 基本用法

```vb
Imports Sundials.CVODE

' 1. 定义右端函数
Dim rhsFunc As RHSFunction = Sub(t As Double, y As NVector, ydot As NVector)
                                 ydot(0) = y(1)          ' y1' = y2
                                 ydot(1) = -y(0)         ' y2' = -y1
                             End Sub

' 2. 创建求解器
Dim options As New CVODEOptions() With {
    .RelativeTolerance = 1.0E-6,
    .AbsoluteTolerance = 1.0E-8
}

Using solver As New CVODESolver(CVODEMethod.Adams, rhsFunc, 2, options)
    ' 3. 初始化
    Dim y0 As New NVector(New Double() {1.0, 0.0})
    solver.Initialize(0.0, y0)
    
    ' 4. 积分
    Dim y As New NVector(2)
    solver.Integrate(1.0, y)
    
    Console.WriteLine($"y(1) = ({y(0)}, {y(1)})")
End Using
```

### 使用构建器API

```vb
Dim solver As CVODESolver = New CVODEBuilder() _
    .UseMethod(CVODEMethod.BDF) _
    .WithRHS(rhsFunc, 3) _
    .WithJacobian(jacFunc) _
    .WithRelativeTolerance(1.0E-6) _
    .WithAbsoluteTolerance(1.0E-8) _
    .Build()
```

### 提供Jacobian函数（提高刚性问题的求解效率）

```vb
Dim jacFunc As JacobianFunction = Sub(t As Double, y As NVector, fy As NVector, J As DenseMatrix)
                                      J(0, 0) = 0.0
                                      J(0, 1) = 1.0
                                      J(1, 0) = -1.0
                                      J(1, 1) = 0.0
                                  End Sub

solver.SetJacobianFunction(jacFunc)
```

## 示例问题

### 示例1：简谐振子
- 方程：y'' + y = 0
- 方法：Adams
- 解析解：y = cos(t)

### 示例2：Lorenz系统
- 混沌系统
- 方法：BDF
- 参数：σ=10, ρ=28, β=8/3

### 示例3：Robertson问题
- 经典刚性ODE测试问题
- 方法：BDF
- 时间跨度：0 到 4×10^10

### 示例4：Van der Pol振子
- 中等刚性问题
- 方法：BDF
- 参数：μ = 1000

## 算法详解

### Adams方法

Adams-Bashforth-Moulton预测-校正方法：

**预测步（Adams-Bashforth）**：
```
y_{n+1}^{(p)} = y_n + h * Σ_{i=0}^{k-1} β_i * f_{n-i}
```

**校正步（Adams-Moulton）**：
```
y_{n+1} = y_n + h * Σ_{i=0}^{k} γ_i * f_{n+1-i}
```

### BDF方法

后向微分公式：
```
Σ_{i=0}^{k} α_i * y_{n+1-i} = h * β * f(t_{n+1}, y_{n+1})
```

需要Newton迭代求解隐式方程。

### 步长控制

基于局部截断误差估计：
```
err ≈ C * h^{k+1} * y^{(k+1)}
```

新步长：
```
h_new = h * safety * (tol/err)^{1/(k+1)}
```

## 性能特点

1. **纯VB.NET实现**：无需任何第三方依赖
2. **面向对象设计**：清晰的类层次结构
3. **内存高效**：大量使用原地操作
4. **自适应算法**：自动调整步长和阶数
5. **完整错误处理**：详细的异常信息

## 编译和运行

```bash
# 使用dotnet CLI
dotnet build Sundials.CVODE.vbproj
dotnet run --project Sundials.CVODE.vbproj

# 或使用Visual Studio
# 打开Sundials.CVODE.vbproj，按F5运行
```

## 许可证

MIT License

## 参考资料

1. SUNDIALS官方文档：https://sundials.readthedocs.io/
2. Hindmarsh, A. C., et al. "SUNDIALS: Suite of nonlinear and differential/algebraic equation solvers." ACM TOMS 31.3 (2005): 363-396.
3. Brown, P. N., G. D. Byrne, and A. C. Hindmarsh. "VODE: A variable-coefficient ODE solver." SIAM J. Sci. Stat. Comput. 10.5 (1989): 1038-1051.
