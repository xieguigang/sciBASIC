Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Dynamics

    ''' <summary>
    ''' The base class of the explicit fixed step size integrators of the <see cref="ODEs"/>
    ''' dynamics system.(方程组显式固定步长积分器的基类)
    ''' </summary>
    ''' <remarks>
    ''' 所有的显式求解器共享同一条执行流程：
    ''' 
    ''' 1. 按 ``n + 1`` 个等距采样点创建输出网格；
    ''' 2. 逐步调用子类实现的 <see cref="StepCore"/>；
    ''' 3. 每一个积分步都检查 NaN/Inf，出现发散的时候抛出带有时间点信息的异常；
    ''' 4. 积分结束后把最终状态同步到各个 ``var`` 状态变量之上。
    ''' 
    ''' 求值与缓冲全部复用 <see cref="SolverBuffers"/> 的预分配内存，因此每一个积分步
    ''' 都不会产生托管堆分配。
    ''' </remarks>
    Public MustInherit Class ExplicitFixedStepSolver

        Friend ReadOnly model As ODEs
        Friend ReadOnly buffers As SolverBuffers
        Friend grid As GridWriter

        Private evaluations As Integer

        ''' <summary>
        ''' Create the fixed step size solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        Protected Sub New(system As ODEs)
            If system Is Nothing Then
                Throw New ArgumentNullException(NameOf(system))
            End If

            Me.model = system
            Me.buffers = New SolverBuffers(system.Size)
        End Sub

        ''' <summary>
        ''' The ``n + 1`` sampling time points of the last integration result.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property XValues As Double()
            Get
                Return If(grid Is Nothing, Nothing, grid.Points)
            End Get
        End Property

        ''' <summary>
        ''' The result column of each state variable of the last integration result.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property YValues As Double()()
            Get
                Return If(grid Is Nothing, Nothing, grid.Columns)
            End Get
        End Property

        ''' <summary>
        ''' The number of the right-hand side function evaluations of the last integration.
        ''' (最近一次积分的右端函数求值次数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumberOfEvaluations As Integer
            Get
                Return evaluations
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the right-hand side function of the given stage.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="k"></param>
        Protected Sub Evaluate(t As Double, y As Vector, k As Vector)
            Call model.Derivatives(t, y, k)

            evaluations += 1
        End Sub

        ''' <summary>
        ''' Increase the counter of the right-hand side evaluations, it is required by the
        ''' solvers which are not going through <see cref="Evaluate"/> (the symplectic solvers).
        ''' </summary>
        ''' <param name="count"></param>
        Protected Sub CountEvaluations(count As Integer)
            evaluations += count
        End Sub

        ''' <summary>
        ''' Solve the ODEs system by the current method.
        ''' </summary>
        ''' <param name="y0">The y initial value.(初值)</param>
        ''' <param name="n">The number of the integration steps.(积分步数)</param>
        ''' <param name="a">The lower bound of the integration range.(积分下界)</param>
        ''' <param name="b">The upper bound of the integration range.(积分上界)</param>
        ''' <returns></returns>
        Public Function Solve(y0 As Double(), n As Integer, a As Double, b As Double) As ExplicitFixedStepSolver
            If n < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(n), "The number of the integration steps must be positive!")
            End If
            If b <= a Then
                Throw New ArgumentOutOfRangeException(NameOf(b), "The upper bound of the integration range must be greater than the lower bound!")
            End If
            If y0 Is Nothing OrElse y0.Length <> buffers.Size Then
                Throw New ArgumentException(
                    $"The dimension of the given y0 initial value ({If(y0 Is Nothing, "null", y0.Length.ToString)}) is not matched to the ODEs system ({buffers.Size})!",
                    NameOf(y0))
            End If

            Me.grid = New GridWriter(a, b, n, buffers.Size)
            Me.evaluations = 0

            Call buffers.SetState(y0)
            Call grid.WriteInitial(buffers.y)

            Dim y As Double() = buffers.y
            Dim h As Double = (b - a) / n
            Dim t As Double

            For iStep As Integer = 1 To n
                t = a + h * (iStep - 1)
                model.CurrentStep = iStep - 1

                Call StepCore(t, h, iStep)
                Call CheckFinite(y, t + h)
            Next

            model.CurrentStep = n

            ' 积分结束之后，把最终状态同步到各个状态变量对象之上
            Call model.SyncState(y)

            Return Me
        End Function

        ''' <summary>
        ''' Perform one integration step: the implementation should update the state buffer
        ''' <see cref="SolverBuffers.y"/> and append the result into the output grid.
        ''' (执行一个积分步：更新状态缓冲并把结果写入输出网格)
        ''' </summary>
        ''' <param name="t">The time at the begin of the step.</param>
        ''' <param name="h">The step size.</param>
        ''' <param name="index">The index of the current step, it starts from 1.</param>
        Protected MustOverride Sub StepCore(t As Double, h As Double, index As Integer)

        ''' <summary>
        ''' Throw an informative exception when the integration is diverged.
        ''' </summary>
        ''' <param name="y"></param>
        ''' <param name="t"></param>
        Friend Sub CheckFinite(y As Double(), t As Double)
            For i As Integer = 0 To y.Length - 1
                If Double.IsNaN(y(i)) OrElse Double.IsInfinity(y(i)) Then
                    Throw New InvalidOperationException(
                        $"{Me.GetType().Name}: the integration is diverged at t = {t}, please reduce the number of the integration steps!")
                End If
            Next
        End Sub
    End Class

    ''' <summary>
    ''' The forward Euler method, 1st order.(前向欧拉法，一阶精度)
    ''' </summary>
    ''' <remarks>
    ''' ``y_{n+1} = y_n + h · f(t_n, y_n)``，每一个积分步只需要 1 次函数求值，但是它的
    ''' 数值稳定性区间很小，通常只用于教学演示或者对精度要求很低的快速原型。
    ''' </remarks>
    Public Class EulerSolver : Inherits ExplicitFixedStepSolver

        ''' <summary>
        ''' Create the forward Euler solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        Sub New(system As ODEs)
            MyBase.New(system)
        End Sub

        Protected Overrides Sub StepCore(t As Double, h As Double, index As Integer)
            Dim y As Double() = buffers.y
            Dim k1 As Double() = buffers.k(0)

            Call Evaluate(t, buffers.vy, buffers.vk(0))

            For i As Integer = 0 To y.Length - 1
                y(i) += h * k1(i)
            Next

            Call grid.WriteNext(y)
        End Sub
    End Class

    ''' <summary>
    ''' The improved Euler (Heun) method, 2nd order.(改进欧拉法，二阶精度)
    ''' </summary>
    ''' <remarks>
    ''' ``k1 = f(t_n, y_n)``、``k2 = f(t_n + h, y_n + h·k1)``、
    ''' ``y_{n+1} = y_n + h/2 · (k1 + k2)``，每一个积分步需要 2 次函数求值。
    ''' </remarks>
    Public Class RungeKutta2 : Inherits ExplicitFixedStepSolver

        ''' <summary>
        ''' Create the 2nd order Runge-Kutta solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        Sub New(system As ODEs)
            MyBase.New(system)
        End Sub

        Protected Overrides Sub StepCore(t As Double, h As Double, index As Integer)
            Dim y As Double() = buffers.y
            Dim k1 As Double() = buffers.k(0)
            Dim k2 As Double() = buffers.k(1)

            Call Evaluate(t, buffers.vy, buffers.vk(0))
            Call SolverKernels.Combine(buffers.tmp, y, h, k1)
            Call Evaluate(t + h, buffers.vtmp, buffers.vk(1))

            ' y = y + h/2 * (k1 + k2)，原位更新(读写同一个下标不会互相影响)
            Call SolverKernels.Combine2(y, y, 0.5 * h, 1.0, k1, 1.0, k2)

            Call grid.WriteNext(y)
        End Sub
    End Class

    ''' <summary>
    ''' The 4-stage 4th order Gill method.(四阶 Gill 方法)
    ''' </summary>
    ''' <remarks>
    ''' Gill 方法是经典四阶 Runge-Kutta 的一个变体，它的系数经过特殊选择以减少舍入误差
    ''' 的累积，适合长时程积分：
    ''' 
    ''' ```
    ''' k1 = f(t, y)
    ''' k2 = f(t + h/2, y + h/2·k1)
    ''' k3 = f(t + h/2, y + h[((√2−1)/2)·k1 + ((2−√2)/2)·k2])
    ''' k4 = f(t + h,   y + h[−(√2/2)·k2 + (1 + √2/2)·k3])
    ''' y_{n+1} = y + h/6·[k1 + 2(2−√2)·k2 + 2(2+√2)·k3 + k4]
    ''' ```
    ''' </remarks>
    Public Class GillSolver : Inherits ExplicitFixedStepSolver

        Private Shared ReadOnly Sqrt2 As Double = std.Sqrt(2.0)

        ''' <summary>
        ''' Create the 4th order Gill solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        Sub New(system As ODEs)
            MyBase.New(system)
        End Sub

        Protected Overrides Sub StepCore(t As Double, h As Double, index As Integer)
            Dim y As Double() = buffers.y
            Dim k1 As Double() = buffers.k(0)
            Dim k2 As Double() = buffers.k(1)
            Dim k3 As Double() = buffers.k(2)
            Dim k4 As Double() = buffers.k(3)

            Call Evaluate(t, buffers.vy, buffers.vk(0))

            Call SolverKernels.Combine(buffers.tmp, y, 0.5 * h, k1)
            Call Evaluate(t + 0.5 * h, buffers.vtmp, buffers.vk(1))

            Call SolverKernels.Combine2(buffers.tmp, y, h,
                                        0.5 * (Sqrt2 - 1.0), k1,
                                        0.5 * (2.0 - Sqrt2), k2)
            Call Evaluate(t + 0.5 * h, buffers.vtmp, buffers.vk(2))

            Call SolverKernels.Combine2(buffers.tmp, y, h,
                                        -0.5 * Sqrt2, k2,
                                        1.0 + 0.5 * Sqrt2, k3)
            Call Evaluate(t + h, buffers.vtmp, buffers.vk(3))

            Call SolverKernels.Combine4(y, y, h / 6.0,
                                        1.0, k1,
                                        2.0 - Sqrt2, k2,
                                        2.0 + Sqrt2, k3,
                                        1.0, k4)

            Call grid.WriteNext(y)
        End Sub
    End Class

    ''' <summary>
    ''' The 4th order Adams-Bashforth-Moulton predictor-corrector method.(四阶阿当姆斯预测-校正法)
    ''' </summary>
    ''' <remarks>
    ''' 多步法：前 3 步使用四阶 Runge-Kutta 起步并保存导数历史，之后每一个积分步只需要
    ''' 2 次函数求值：
    ''' 
    ''' ```
    ''' 预测(AB4)：y* = y_n + h/24·(55f_n − 59f_{n−1} + 37f_{n−2} − 9f_{n−3})
    ''' 校正(AM4)：y_{n+1} = y_n + h/24·(9f(t_{n+1}, y*) + 19f_n − 5f_{n−1} + f_{n−2})
    ''' ```
    ''' 
    ''' 与四阶 Runge-Kutta 相比，它在光滑问题上的吞吐量更高（求值次数减半），但是不稳定
    ''' 区间与精度都会被历史误差所影响，不适合刚性或者解变化剧烈的问题。
    ''' </remarks>
    Public Class AdamsBashforthMoulton4 : Inherits ExplicitFixedStepSolver

        Private history As Double()()
        Private historyCount As Integer

        ''' <summary>
        ''' Create the ABM4 solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        Sub New(system As ODEs)
            MyBase.New(system)
        End Sub

        ''' <summary>
        ''' The number of the Runge-Kutta startup steps of the multistep method.(起步步数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property StartupSteps As Integer
            Get
                Return 4
            End Get
        End Property

        Protected Overrides Sub StepCore(t As Double, h As Double, index As Integer)
            Dim y As Double() = buffers.y

            If history Is Nothing Then
                history = buffers.EnsureHistory(4)
                historyCount = 0
            End If

            If historyCount < 4 Then
                ' 使用 RK4 起步，并把 f(t + h, y_{n+1}) 压入历史
                Call StepRungeKutta4(t, h)
                Call Evaluate(t + h, buffers.vy, buffers.vk(0))

                Call ShiftHistory()
                Call SolverKernels.Copy(history(0), buffers.k(0))
            Else
                Dim f0 As Double() = history(0)     ' f_n
                Dim f1 As Double() = history(1)     ' f_{n-1}
                Dim f2 As Double() = history(2)     ' f_{n-2}
                Dim f3 As Double() = history(3)     ' f_{n-3}
                Dim predicted As Double() = buffers.k(1)
                Dim corrected As Double() = buffers.k(2)

                ' 预测
                Call SolverKernels.Combine4(buffers.tmp, y, h / 24.0,
                                            55.0, f0,
                                            -59.0, f1,
                                            37.0, f2,
                                            -9.0, f3)
                Call Evaluate(t + h, buffers.vtmp, buffers.vk(1))

                ' 校正
                Call SolverKernels.Combine4(corrected, y, h / 24.0,
                                            9.0, predicted,
                                            19.0, f0,
                                            -5.0, f1,
                                            1.0, f2)

                ' 校正之后的导数作为新的 f_{n+1}
                Call SolverKernels.Copy(y, corrected)
                Call Evaluate(t + h, buffers.vy, buffers.vk(0))

                Call ShiftHistory()
                Call SolverKernels.Copy(history(0), buffers.k(0))
            End If

            Call grid.WriteNext(y)
        End Sub

        ''' <summary>
        ''' Shift the derivative history: ``f_{n-3} ← f_{n-2} ← f_{n-1} ← f_n``.
        ''' </summary>
        Private Sub ShiftHistory()
            For i As Integer = 3 To 1 Step -1
                Call SolverKernels.Copy(history(i), history(i - 1))
            Next

            If historyCount < 4 Then
                historyCount += 1
            End If
        End Sub

        ''' <summary>
        ''' One classical 4th order Runge-Kutta step, it is used for the startup of the
        ''' multistep method.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="h"></param>
        Private Sub StepRungeKutta4(t As Double, h As Double)
            Dim y As Double() = buffers.y
            Dim k1 As Double() = buffers.k(0)
            Dim k2 As Double() = buffers.k(1)
            Dim k3 As Double() = buffers.k(2)
            Dim k4 As Double() = buffers.k(3)

            Call Evaluate(t, buffers.vy, buffers.vk(0))
            Call SolverKernels.Combine(buffers.tmp, y, 0.5 * h, k1)
            Call Evaluate(t + 0.5 * h, buffers.vtmp, buffers.vk(1))
            Call SolverKernels.Combine(buffers.tmp, y, 0.5 * h, k2)
            Call Evaluate(t + 0.5 * h, buffers.vtmp, buffers.vk(2))
            Call SolverKernels.Combine(buffers.tmp, y, h, k3)
            Call Evaluate(t + h, buffers.vtmp, buffers.vk(3))

            Call SolverKernels.Combine4(y, y, h / 6.0,
                                        1.0, k1,
                                        2.0, k2,
                                        2.0, k3,
                                        1.0, k4)
        End Sub
    End Class
End Namespace
