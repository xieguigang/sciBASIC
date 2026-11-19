# Data_science/Mathematica/Math/CVODE_Solver/Sundials.CVODE.vbproj

- RootNamespace : Microsoft.VisualBasic.Math.Sundials.CVODE
- AssemblyName  : Microsoft.VisualBasic.Math.Sundials.CVODE
- TargetFramework: net10.0
- Source files  : 18
- Existing Title: Managed SUNDIALS CVODE Variable-Step ODE Solver with Adams and BDF Methods
- Existing Desc : A pure managed re-implementation of the CVODE ODE solver API for sciBASIC#, with no native P/Invoke: variable-step variable-order Adams and BDF integration, dense and banded LU linear solvers, event root finding and a fluent builder API.

CVODE is a solver for stiff and nonstiff ordinary differential equation (ODE) systems (initial value problem) given in explicit form y' = f(t,y). The methods used in CVODE are variable-order, variable-step multistep methods. For nonstiff problems, CVODE includes the Adams-Moulton formulas, with the order varying between 1 and 12. For stiff problems, CVODE includes the Backward Differentiation Formulas (BDFs) in so-called fixed-leading coefficient form, with order varying between 1 and 5. For either choice of formula, the resulting nonlinear system is solved (approximately) at each integration step. For this, CVODE offers the choice of either functional iteration, suitable only for nonstiff systems, and various versions of Newton iteration. In the cases of a direct linear solver (dense or banded), the Newton iteration is a Modified Newton iteration, in that the Jacobian is fixed (and usually out of date). When using a Krylov method as the linear solver, the iteration is an Inexact Newton iteration, using the current Jacobian (through matrix-free products), in which the linear residual is nonzero but controlled. The implicit nonlinear systems within implicit integrators are solved approximately at each integration step using a modified Newton method, an Inexact Newton method, or fixed-point solver (functional iteration). For the Newton-based methods and the serial or threaded NVECTOR modules in SUNDIALS, CVODE provides both direct (dense, band, or sparse) and preconditioned Krylov iterative (GMRES, BiCGStab, TFQMR) linear solvers. When used with one of the distributed parallel NVECTOR modules, including PETSc and hypre vectors, or a user-provided vector data structure, only the Krylov solvers are available, although a user may supply their own linear solver for any data structures if desired. For the serial vector structure, there is a banded preconditioner module called CVBANDPRE for use with the Krylov solvers, while for the distributed memory parallel structure there is a preconditioner module called CVBBDPRE which provides a band-block-diagonal preconditioner.
- Existing Tags : scibasic;ode-solver;cvode;adams;bdf;stiff-equations;root-finding

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.Math.Sundials.CVODE)

## Public types
- Class CVODESolverEx (CVODEAdvanced.vb) - CVODE求解器扩展类 提供根查找等高级功能
- Class CVODEBuilder (CVODEBuilder.vb) - CVODE求解器构建器 提供流式API创建求解器
- Enum CVODEMethod (CVODEMethod.vb) - 求解方法枚举
- Class CVODEOptions (CVODEOptions.vb) - CVODE求解器配置选项
- Class CVODESolver (CVODESolver.vb) - CVODE常微分方程求解器（变阶变步长多步法）
- Enum CVODEStatus (CVODEStatus.vb) - 求解器返回状态
- Module CVODESolverExtensions (Extensions\CVODESolverExtensions.vb) - 把 CVODE 求解器的积分结果统一输出为 <see cref="ODESolution"/> 的扩展方法。 <see cref="ODESolution"/> 是 ODE 求解器框架的统一结果模型（原来 CVODE 专用的 ``ODESolution`` 已经删除），它同时提供 ``x`` / ``y`` / ``y0`` / ``params`` 的原始数据与 ``Count`` /
- Class CvodeSolverProvider (Extensions\ODEsSolverProvider.vb) - 这个类型只负责「桥接」： 1. 把 <see cref="ODEs.Derivatives(Double, Double(), Double())"/> 适配成 <see cref="RHSFunction"/>（直接使用 <see cref="Vector.Array"/> 的内部缓冲，零拷贝）；
- Module SolverExtensions (Extensions\SolverExtensions.vb) - ```vbnet Call SolverExtensions.Register() Dim result1 = model.Solve(1000, 0, 5, ODESolverMethod.CVODE_BDF)
- Module VectorExtensions (Extensions\VectorExtensions.vb)
- Class BandLinearSolver (LinearSolver\BandLinearSolver.vb) - 带状线性求解器 针对带状矩阵优化的LU分解
- Class DenseLinearSolver (LinearSolver\DenseLinearSolver.vb) - 稠密线性求解器 使用带部分主元选择的LU分解
- Enum LinearSolverResult (LinearSolver\LinearSolverResult.vb) - 线性求解器返回状态
- Enum LinearSolverType (LinearSolver\LinearSolverType.vb) - 线性求解器类型枚举
- Class NamespaceDoc (NamespaceDoc.vb) - The SUNDIALS CVODE ordinary differential equation solver integration: the native solver bindings together with the ODE integration API.
- Class RootFindingResult (RootFindingResult.vb) - 根查找结果

## Notable public members
- Public Sub New(method As CVODEMethod, rhsFunc As RHSFunction, n As Integer, Optional options As CVODEOptions = Nothing)
- Public Sub SetRootFunction(rootFunc As RootFunction, nRoots As Integer, Optional direction As Integer() = Nothing)
- Public Function IntegrateWithRootFinding(tOut As Double, yOut As Vector) As RootFindingResult
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
- Public Function Solve(t0 As Double, tEnd As Double, y0 As Vector, n As Integer,
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
- Public ReadOnly Property CurrentState As Vector
- Public ReadOnly Property CurrentStep As Double
- Public ReadOnly Property CurrentOrder As Integer
- Public ReadOnly Property TotalSteps As Long
- Public ReadOnly Property RHSFunctionEvaluations As Long
- Public ReadOnly Property NewtonIterations As Long
- Public ReadOnly Property LinearSolves As Long
- Public Function Initialize(t0 As Double, y0 As Vector) As CVODEStatus
- Public Function Integrate(tOut As Double, Optional yOut As Vector = Nothing) As CVODEStatus
- Public Sub SetJacobianFunction(jacFunc As JacobianFunction)
- Public Sub SetAbsoluteTolerance(atol As Double)
- Public Sub SetAbsoluteTolerance(atol As Vector)
- Public Sub SetRelativeTolerance(rtol As Double)
- Public Sub SetMaxStep(hMax As Double)
- Public Sub SetMinStep(hMin As Double)
- Protected Overridable Sub Dispose(disposing As Boolean)
- Public Sub Dispose() Implements IDisposable.Dispose
- Public Shared Function DefaultOptions() As CVODEOptions
- Public Shared Function StiffOptions() As CVODEOptions
- Public Shared Function NonStiffOptions() As CVODEOptions
- Public Shared Function HighPrecisionOptions() As CVODEOptions
- Public Function Solve(solver As CVODESolver, t0 As Double, tEnd As Double, y0 As Vector,
- Public Function Solve(method As CVODEMethod, rhs As RHSFunction, t0 As Double, tEnd As Double,
- Public ReadOnly Property Method As CVODEMethod
- Public ReadOnly Property Name As String Implements IODEsolverProvider.Name
- Public ReadOnly Property Description As String Implements IODEsolverProvider.Description
- Public Sub New(method As CVODEMethod, Optional name As String = Nothing, Optional description As String = Nothing)
- Public Function Solve(model As ODEs, y0 As Double(), n As Integer, a As Double, b As Double,
- Public Const AdamsName As String = "cvode.adams"
- Public Const BdfName As String = "cvode.bdf"
- ... and 28 more

## Imports
- Microsoft.VisualBasic.Math.Calculus
- Microsoft.VisualBasic.Math.Calculus.Dynamics
- Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
- Microsoft.VisualBasic.Math.LinearAlgebra
- Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
- std = System.Math
- System.Collections.Generic
- System.Runtime.CompilerServices

## File tree
- CVODEAdvanced.vb
- CVODEBuilder.vb
- CVODEMethod.vb
- CVODEOptions.vb
- CVODESolver.vb
- CVODEStatus.vb
- CVODEUtils.vb
- Extensions\CVODESolverExtensions.vb
- Extensions\ODEsSolverProvider.vb
- Extensions\SolverExtensions.vb
- Extensions\VectorExtensions.vb
- Func.vb
- LinearSolver\BandLinearSolver.vb
- LinearSolver\DenseLinearSolver.vb
- LinearSolver\LinearSolverResult.vb
- LinearSolver\LinearSolverType.vb
- NamespaceDoc.vb
- RootFindingResult.vb

