# Data_science/Mathematica/Math/CVODE_Solver/Sundials.CVODE.vbproj

- RootNamespace : Microsoft.VisualBasic.Math.Sundials.CVODE
- AssemblyName  : Microsoft.VisualBasic.Math.Sundials.CVODE
- TargetFramework: net10.0
- Source files  : 16
- Existing Title: Managed CVODE Variable-Order ODE Solver in Pure VB.NET
- Existing Desc : A pure managed VB.NET re-implementation of the CVODE solver API with no native P/Invoke, offering variable-step variable-order Adams and BDF integration plus dense and banded linear solvers. Part of sciBASIC#.
- Existing Tags : scibasic;ode-solver;cvode;adams-bdf;stiff-equations;numerics

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Math.Sundials.CVODE)

## Public types
- Class CVODESolverEx (CVODEAdvanced.vb) - CVODE求解器扩展类 提供根查找等高级功能
- Class CVODEBuilder (CVODEBuilder.vb) - CVODE求解器构建器 提供流式API创建求解器
- Enum CVODEMethod (CVODEMethod.vb) - 求解方法枚举
- Class CVODEOptions (CVODEOptions.vb) - CVODE求解器配置选项
- Class CVODESolver (CVODESolver.vb) - CVODE常微分方程求解器（变阶变步长多步法）
- Enum CVODEStatus (CVODEStatus.vb) - 求解器返回状态
- Class DenseMatrix (DenseMatrix.vb) - 稠密矩阵类，提供矩阵存储和基本操作 采用列优先存储方式，便于与BLAS/LAPACK风格的操作兼容
- Class BandLinearSolver (LinearSolver\BandLinearSolver.vb) - 带状线性求解器 针对带状矩阵优化的LU分解
- Class DenseLinearSolver (LinearSolver\DenseLinearSolver.vb) - 稠密线性求解器 使用带部分主元选择的LU分解
- Enum LinearSolverResult (LinearSolver\LinearSolverResult.vb) - 线性求解器返回状态
- Enum LinearSolverType (LinearSolver\LinearSolverType.vb) - 线性求解器类型枚举
- Class NVector (NVector.vb) - NVector类实现了SUNDIALS中的N_Vector抽象数据类型 提供向量的基本操作：创建、复制、算术运算、范数计算等
- Class ODESolution (ODESolution.vb) - 常微分方程求解结果
- Class RootFindingResult (RootFindingResult.vb) - 根查找结果

## Notable public members
- Public Sub New(method As CVODEMethod, rhsFunc As RHSFunction, n As Integer, Optional options As CVODEOptions = Nothing)
- Public Sub SetRootFunction(rootFunc As RootFunction, nRoots As Integer, Optional direction As Integer() = Nothing)
- Public Function IntegrateWithRootFinding(tOut As Double, yOut As NVector) As RootFindingResult
- Public Function UseMethod(method As CVODEMethod) As CVODEBuilder
- Public Function WithRHS(rhsFunc As RHSFunction, dimension As Integer) As CVODEBuilder
- Public Function WithJacobian(jacFunc As JacobianFunction) As CVODEBuilder
- Public Function WithRelativeTolerance(rtol As Double) As CVODEBuilder
- Public Function WithAbsoluteTolerance(atol As Double) As CVODEBuilder
- Public Function WithMaxOrder(maxOrder As Integer) As CVODEBuilder
- Public Function WithMaxSteps(maxSteps As Integer) As CVODEBuilder
- Public Function WithMaxStep(maxStep As Double) As CVODEBuilder
- Public Function WithInitialStep(h0 As Double) As CVODEBuilder
- Public Function Build() As CVODESolver
- Public Property RelativeTolerance As Double = 0.0001
- Public Property AbsoluteTolerance As Double = 0.00000001
- Public Property MaxOrder As Integer = 5
- Public Property MaxSteps As Integer = 10000
- Public Property MaxStep As Double = 0.0
- Public Property MinStep As Double = 0.0
- Public Property InitialStep As Double = 0.0
- Public Property MaxNewtonIterations As Integer = 4
- Public Property NewtonConvergenceFactor As Double = 0.1
- Public Property MaxGrowthFactor As Double = 10.0
- Public Property MinReductionFactor As Double = 0.2
- Public Property SafetyFactor As Double = 0.9
- Public Property UseUserJacobian As Boolean = False
- Public Property JacobianUpdateFrequency As Integer = 20
- Public Sub New(method As CVODEMethod, rhsFunc As RHSFunction, n As Integer, Optional options As CVODEOptions = Nothing)
- Public ReadOnly Property CurrentTime As Double
- Public ReadOnly Property CurrentState As NVector
- Public ReadOnly Property CurrentStep As Double
- Public ReadOnly Property CurrentOrder As Integer
- Public ReadOnly Property TotalSteps As Long
- Public ReadOnly Property RHSFunctionEvaluations As Long
- Public ReadOnly Property NewtonIterations As Long
- Public ReadOnly Property LinearSolves As Long
- Public Function Initialize(t0 As Double, y0 As NVector) As CVODEStatus
- Public Function Integrate(tOut As Double, Optional yOut As NVector = Nothing) As CVODEStatus
- Public Sub SetJacobianFunction(jacFunc As JacobianFunction)
- Public Sub SetAbsoluteTolerance(atol As Double)
- Public Sub SetAbsoluteTolerance(atol As NVector)
- Public Sub SetRelativeTolerance(rtol As Double)
- Public Sub SetMaxStep(hMax As Double)
- Public Sub SetMinStep(hMin As Double)
- Protected Overridable Sub Dispose(disposing As Boolean)
- Public Sub Dispose() Implements IDisposable.Dispose
- Public Shared Function DefaultOptions() As CVODEOptions
- Public Shared Function StiffOptions() As CVODEOptions
- Public Shared Function NonStiffOptions() As CVODEOptions
- Public Shared Function HighPrecisionOptions() As CVODEOptions
- Public Sub New(rows As Integer, cols As Integer)
- Public Sub New(data As Double(,))
- Public Sub New(other As DenseMatrix)
- Public ReadOnly Property Rows As Integer
- Public ReadOnly Property Columns As Integer
- Public ReadOnly Property IsSquare As Boolean
- Public ReadOnly Property Data As Double(,)
- Public Shared Function Identity(n As Integer) As DenseMatrix
- Public Shared Function Zeros(rows As Integer, cols As Integer) As DenseMatrix
- Public Shared Function Ones(rows As Integer, cols As Integer) As DenseMatrix
- ... and 87 more

## Imports
- std = System.Math

## File tree
- CVODEAdvanced.vb
- CVODEBuilder.vb
- CVODEMethod.vb
- CVODEOptions.vb
- CVODESolver.vb
- CVODEStatus.vb
- CVODEUtils.vb
- DenseMatrix.vb
- Func.vb
- LinearSolver\BandLinearSolver.vb
- LinearSolver\DenseLinearSolver.vb
- LinearSolver\LinearSolverResult.vb
- LinearSolver\LinearSolverType.vb
- NVector.vb
- ODESolution.vb
- RootFindingResult.vb

