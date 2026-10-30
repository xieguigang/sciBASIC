Namespace Dynamics

    ''' <summary>
    ''' The available integration algorithms of the <see cref="ODEs"/> dynamics system.
    ''' (常微分方程组可用的积分算法)
    ''' </summary>
    ''' <remarks>
    ''' | 方法 | 阶数 | 类型 | 适用 |
    ''' |---|---|---|---|
    ''' | <see cref="Euler"/> | 1 | 显式固定步长 | 教学 / 快速原型 |
    ''' | <see cref="RK2"/> | 2 | 显式固定步长 | 一般用途 |
    ''' | <see cref="Gill"/> | 4 | 显式固定步长 | 长时程积分(舍入误差累积较小) |
    ''' | <see cref="RK4"/> | 4 | 显式固定步长 | **默认推荐** |
    ''' | <see cref="ABM4"/> | 4 | 显式多步预测-校正 | 光滑非刚性问题的批量仿真 |
    ''' | <see cref="BackwardEuler"/> | 1 | 隐式(L-稳定) | 强刚性问题 |
    ''' | <see cref="Trapezoidal"/> | 2 | 隐式(A-稳定) | 一般刚性问题 |
    ''' | <see cref="BDF2"/> | 2 | 隐式 | 刚性问题 |
    ''' | <see cref="RosenbrockROS2"/> | 2 | 线性隐式(L-稳定) | 刚性问题(无需 Newton 迭代) |
    ''' | <see cref="RK23"/> | 3(2) | 自适应显式 | 粗略/低频仿真 |
    ''' | <see cref="RK45"/> | 5(4) | 自适应显式 | 高精度通用 |
    ''' | <see cref="Verlet"/> | 2 | 辛几何 | 哈密顿系统(位置/动量可分裂) |
    ''' | <see cref="Yoshida4"/> | 4 | 辛几何 | 哈密顿系统(高精度能量守恒) |
    ''' </remarks>
    Public Enum ODESolverMethod
        ''' <summary>
        ''' Forward Euler method, 1st order.(前向欧拉法)
        ''' </summary>
        Euler = 0
        ''' <summary>
        ''' The improved Euler (Heun) method, 2nd order.(改进欧拉法)
        ''' </summary>
        RK2
        ''' <summary>
        ''' The 4-stage 4th order Gill method, it has a smaller round-off accumulation than RK4.
        ''' (四阶 Gill 方法，舍入误差累积较小)
        ''' </summary>
        Gill
        ''' <summary>
        ''' The classical 4-stage 4th order Runge-Kutta method.(经典四阶龙格库塔法)
        ''' </summary>
        RK4
        ''' <summary>
        ''' The 4th order Adams-Bashforth-Moulton predictor-corrector method.(四阶阿当姆斯预测-校正法)
        ''' </summary>
        ABM4
        ''' <summary>
        ''' The implicit backward Euler method (BDF1), L-stable.(隐式后向欧拉法)
        ''' </summary>
        BackwardEuler
        ''' <summary>
        ''' The implicit trapezoidal rule (Crank-Nicolson), A-stable.(隐式梯形法)
        ''' </summary>
        Trapezoidal
        ''' <summary>
        ''' The 2nd order backward differentiation formula, it is suitable for the stiff problems.
        ''' (二阶后向微分公式)
        ''' </summary>
        BDF2
        ''' <summary>
        ''' The 2-stage 2nd order L-stable Rosenbrock method (linearly implicit).(二阶线性隐式方法)
        ''' </summary>
        RosenbrockROS2
        ''' <summary>
        ''' The Bogacki-Shampine 3(2) adaptive step size method.(自适应 RK23 方法)
        ''' </summary>
        RK23
        ''' <summary>
        ''' The Dormand-Prince 5(4) adaptive step size method.(自适应 RK45 方法)
        ''' </summary>
        RK45
        ''' <summary>
        ''' The 2nd order Stormer-Verlet symplectic method.(二阶辛几何算法)
        ''' </summary>
        Verlet
        ''' <summary>
        ''' The 4th order Yoshida composition of the Stormer-Verlet method.(四阶吉田分解)
        ''' </summary>
        Yoshida4
        ''' <summary>
        ''' The Adams (non-stiff) integrator of the managed ``Sundials.CVODE`` package, it is an
        ''' external solver which should be registered by ``SolverExtensions.Register()`` first.
        ''' (托管版 CVODE 的 Adams 方法，需要先调用 SolverExtensions.Register() 注册)
        ''' </summary>
        CVODE_Adams
        ''' <summary>
        ''' The BDF (stiff) integrator of the managed ``Sundials.CVODE`` package, it is an
        ''' external solver which should be registered by ``SolverExtensions.Register()`` first.
        ''' (托管版 CVODE 的 BDF 方法，需要先调用 SolverExtensions.Register() 注册)
        ''' </summary>
        CVODE_BDF
    End Enum

    ''' <summary>
    ''' The optional settings of the <see cref="ODEs"/> integrators.(方程组求解器的可选设置)
    ''' </summary>
    Public Class ODESolverOptions

        ''' <summary>
        ''' The absolute error tolerance of the adaptive solvers.(自适应求解器的绝对误差容限)
        ''' </summary>
        ''' <returns></returns>
        Public Property Atol As Double = 1.0E-9

        ''' <summary>
        ''' The relative error tolerance of the adaptive solvers.(自适应求解器的相对误差容限)
        ''' </summary>
        ''' <returns></returns>
        Public Property Rtol As Double = 1.0E-7

        ''' <summary>
        ''' The initial step size of the adaptive solvers, zero means auto select.
        ''' (自适应求解器的初始步长，零表示自动选择)
        ''' </summary>
        ''' <returns></returns>
        Public Property InitialStep As Double = 0.0

        ''' <summary>
        ''' The maximal number of the integration steps of the adaptive solvers.
        ''' (自适应求解器的最大积分步数)
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxSteps As Integer = 10000000

        ''' <summary>
        ''' The convergence tolerance of the Newton iterations of the implicit solvers.
        ''' (隐式求解器 Newton 迭代的收敛容限)
        ''' </summary>
        ''' <returns></returns>
        Public Property NewtonTolerance As Double = 1.0E-10

        ''' <summary>
        ''' The maximal number of the Newton iterations of each integration step.
        ''' (每一个积分步允许的最大 Newton 迭代次数)
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxNewtonIterations As Integer = 50

        ''' <summary>
        ''' Should the iteration matrix be refactorized at each Newton iteration? When this
        ''' option is disabled, the modified Newton method will be used.
        ''' (是否在每一次 Newton 迭代都重新分解迭代矩阵)
        ''' </summary>
        ''' <returns></returns>
        Public Property RefactorEachIteration As Boolean = False

        ''' <summary>
        ''' The maximal number of the internal subdivisions when a Newton iteration is failed
        ''' on the current step.(Newton 迭代失败的时候，当前积分步允许的最大内部细分次数)
        ''' </summary>
        ''' <returns></returns>
        Public Property MaxStepSubdivisions As Integer = 8

        ''' <summary>
        ''' The analytic Jacobian of the ODEs system, when this delegate is not presented the
        ''' central finite difference will be used.
        ''' (可选的解析 Jacobian，未提供的时候使用中心差商)
        ''' </summary>
        ''' <returns></returns>
        Public Property Jacobian As Action(Of Double, Double(), Double(,))

        ''' <summary>
        ''' The state index of the positions ``q`` of the symplectic integrators, when this
        ''' value is not presented the first half of the state vector will be used.
        ''' (辛几何积分器的位置量索引)
        ''' </summary>
        ''' <returns></returns>
        Public Property PositionIndices As Integer()

        ''' <summary>
        ''' The state index of the momenta ``p`` of the symplectic integrators, when this
        ''' value is not presented the second half of the state vector will be used.
        ''' (辛几何积分器的动量索引)
        ''' </summary>
        ''' <returns></returns>
        Public Property MomentumIndices As Integer()

        ''' <summary>
        ''' The solver specific options which is passed to the external (plugin) solver as-is,
        ''' for example the ``CVODEOptions`` instance of the ``Sundials.CVODE`` package.
        ''' (透传给外部求解器的专有配置对象)
        ''' </summary>
        ''' <remarks>
        ''' ```vbnet
        ''' Dim options As New ODESolverOptions With {
        '''     .Atol = 1.0E-10,
        '''     .ProviderOptions = New CVODEOptions With {.MaxOrder = 5}
        ''' }
        ''' ```
        ''' </remarks>
        ''' <returns></returns>
        Public Property ProviderOptions As Object

        ''' <summary>
        ''' Create a copy of the current options.
        ''' </summary>
        ''' <returns></returns>
        Public Function Clone() As ODESolverOptions
            Return New ODESolverOptions With {
                .Atol = Atol,
                .Rtol = Rtol,
                .InitialStep = InitialStep,
                .MaxSteps = MaxSteps,
                .NewtonTolerance = NewtonTolerance,
                .MaxNewtonIterations = MaxNewtonIterations,
                .RefactorEachIteration = RefactorEachIteration,
                .MaxStepSubdivisions = MaxStepSubdivisions,
                .Jacobian = Jacobian,
                .PositionIndices = PositionIndices,
                .MomentumIndices = MomentumIndices,
                .ProviderOptions = ProviderOptions
            }
        End Function
    End Class
End Namespace
