Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Dynamics

    ''' <summary>
    ''' The residual forms of the Newton based implicit methods.(基于 Newton 迭代的隐式方法的残差形式)
    ''' </summary>
    Public Enum ImplicitForm
        ''' <summary>
        ''' ``y_{n+1} − y_n − h·f(t_{n+1}, y_{n+1}) = 0``
        ''' </summary>
        BackwardEuler
        ''' <summary>
        ''' ``y_{n+1} − y_n − h/2·(f(t_n, y_n) + f(t_{n+1}, y_{n+1})) = 0``
        ''' </summary>
        Trapezoidal
        ''' <summary>
        ''' ``3/2·y_{n+1} − 2·y_n + 1/2·y_{n−1} − h·f(t_{n+1}, y_{n+1}) = 0``
        ''' </summary>
        BDF2
    End Enum

    ''' <summary>
    ''' The base class of the implicit (stiff) integrators of the <see cref="ODEs"/> dynamics system.
    ''' (方程组隐式(刚性)积分器的基类)
    ''' </summary>
    ''' <remarks>
    ''' 基类负责：
    ''' 
    ''' 1. 输出网格与 ``n + 1`` 个采样点；
    ''' 2. ``y_n`` / ``y_{n−1}`` / ``f(t_n, y_n)`` 的历史维护（BDF2 与梯形法需要）；
    ''' 3. 修正 Newton 迭代（每个积分步分解一次迭代矩阵 ``W``，默认不重复分解）；
    ''' 4. Newton 失败时对当前步做内部二分细分，细分使用一阶的隐式 Euler 以保证收敛；
    ''' 5. 发散检查与最终状态同步。
    ''' 
    ''' Jacobian 默认由中心差商给出，若模型提供了 <see cref="ODEs.Jacobian"/>（或者在
    ''' <see cref="ODESolverOptions.Jacobian"/> 之中指定），则直接使用解析 Jacobian。
    ''' </remarks>
    Public MustInherit Class ImplicitSolverBase

        Friend ReadOnly model As ODEs
        Friend ReadOnly buffers As SolverBuffers
        Friend ReadOnly work As ImplicitWorkspace
        Friend ReadOnly options As ODESolverOptions
        Friend ReadOnly jacobian As JacobianEvaluator
        Friend grid As GridWriter

        Private evaluations As Integer
        Private jacobianEvaluations As Integer
        Private newtonFailures As Integer
        Private stepIndex As Integer

        ''' <summary>
        ''' Create the implicit solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="options"></param>
        Protected Sub New(system As ODEs, Optional options As ODESolverOptions = Nothing)
            If system Is Nothing Then
                Throw New ArgumentNullException(NameOf(system))
            End If

            Me.model = system
            Me.options = If(options, New ODESolverOptions())
            Me.buffers = New SolverBuffers(system.Size)
            Me.work = buffers.EnsureImplicit()
            Me.jacobian = New JacobianEvaluator(system, Me.options.Jacobian)
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
        ''' The number of the Jacobian evaluations of the last integration.
        ''' (最近一次积分的 Jacobian 求值次数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumberOfJacobianEvaluations As Integer
            Get
                Return jacobianEvaluations
            End Get
        End Property

        ''' <summary>
        ''' The number of the Newton iterations which is failed to converge.
        ''' (Newton 迭代未收敛的次数，包含触发步长细分的次数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumberOfNewtonFailures As Integer
            Get
                Return newtonFailures
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the right-hand side function of the given state.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="k"></param>
        Protected Sub Evaluate(t As Double, y As Vector, k As Vector)
            Call model.Derivatives(t, y, k)

            evaluations += 1
        End Sub

        ''' <summary>
        ''' Evaluate the Jacobian matrix at the given state.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="j"></param>
        Protected Sub EvaluateJacobian(t As Double, y As Double(), j As Double(,))
            Call jacobian.Evaluate(t, y, j)

            jacobianEvaluations += 1
        End Sub

        ''' <summary>
        ''' Solve the ODEs system by the current implicit method.
        ''' </summary>
        ''' <param name="y0">The y initial value.(初值)</param>
        ''' <param name="n">The number of the integration steps.(积分步数)</param>
        ''' <param name="a">The lower bound of the integration range.(积分下界)</param>
        ''' <param name="b">The upper bound of the integration range.(积分上界)</param>
        ''' <returns></returns>
        Public Function Solve(y0 As Double(), n As Integer, a As Double, b As Double) As ImplicitSolverBase
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
            Me.jacobianEvaluations = 0
            Me.newtonFailures = 0

            Call SolverKernels.Copy(buffers.y, y0)
            Call SolverKernels.Copy(work.yStage, y0)
            Call SolverKernels.Copy(work.yPrev, y0)
            Call grid.WriteInitial(buffers.y)

            ' f(t_0, y_0)
            Call Evaluate(a, buffers.vy, work.vfPrev)

            Dim h As Double = (b - a) / n
            Dim tPrev As Double, tNext As Double

            For iStep As Integer = 1 To n
                tPrev = a + h * (iStep - 1)
                tNext = a + h * iStep
                model.CurrentStep = iStep - 1
                Me.stepIndex = iStep

                If Not StepCore(tPrev, tNext, h) Then
                    ' Newton 迭代失败：对当前步做内部二分细分
                    Call Subdivide(tPrev, tNext, h)
                End If

                ' 历史推进：y_{n-1} ← y_n，y_n ← y_{n+1}
                Call SolverKernels.Copy(work.yPrev, work.yStage)
                Call SolverKernels.Copy(work.yStage, buffers.y)

                ' f(t_{n+1}, y_{n+1})
                Call Evaluate(tNext, buffers.vy, work.vfPrev)

                Call SolverKernels.EnsureFinite(buffers.y, tNext, Me.GetType().Name)
                Call grid.WriteNext(buffers.y)
            Next

            model.CurrentStep = n

            ' 积分结束之后，把最终状态同步到各个状态变量对象之上
            Call model.SyncState(buffers.y)

            Return Me
        End Function

        ''' <summary>
        ''' Perform one implicit step from ``tPrev`` to ``tNext``, the result should be written
        ''' into <see cref="SolverBuffers.y"/> and ``True`` should be returned when the
        ''' nonlinear solver is converged.
        ''' (执行一个隐式积分步，收敛的时候把结果写入状态缓冲并返回 True)
        ''' </summary>
        ''' <param name="tPrev"></param>
        ''' <param name="tNext"></param>
        ''' <param name="h"></param>
        Protected MustOverride Function StepCore(tPrev As Double, tNext As Double, h As Double) As Boolean

        ''' <summary>
        ''' The index of the current integration step, it starts from 1.
        ''' (当前积分步的序号，从 1 开始)
        ''' </summary>
        ''' <returns></returns>
        Protected ReadOnly Property CurrentStepIndex As Integer
            Get
                Return stepIndex
            End Get
        End Property

        ''' <summary>
        ''' The Newton solve of one implicit step for the given residual form.
        ''' (对指定的残差形式执行一次 Newton 求解)
        ''' </summary>
        ''' <param name="form"></param>
        ''' <param name="t">The time at the end of the step.</param>
        ''' <param name="h"></param>
        ''' <param name="yStart">The state at the begin of the step.</param>
        ''' <param name="yOut">The output state buffer.</param>
        ''' <returns></returns>
        Protected Function NewtonStep(form As ImplicitForm, t As Double, h As Double,
                                      yStart As Double(), yOut As Double()) As Boolean

            Dim iterate As Double() = buffers.ynext
            Dim g As Double() = work.rhs
            Dim delta As Double() = work.delta
            Dim matrixBuffer As Double(,) = work.matrix
            Dim tolerance As Double = options.NewtonTolerance
            Dim factorized As Boolean = False
            Dim converged As Boolean = False

            ' 显式 Euler 预测
            Call SolverKernels.Copy(iterate, yStart)
            Call SolverKernels.AddScaled(iterate, h, work.fPrev)

            For iteration As Integer = 1 To std.Max(1, options.MaxNewtonIterations)
                ' 计算残差 G(y)，同时把当前的导数写入 work.fCurrent
                Call Evaluate(t, buffers.vynext, work.vfCurrent)
                Call Residual(form, iterate, yStart, h, g)

                If (Not factorized) OrElse options.RefactorEachIteration Then
                    Call EvaluateJacobian(t, iterate, work.jacobian)
                    Call SolverKernels.BuildIterationMatrix(matrixBuffer, work.jacobian,
                                                          DiagonalOf(form), h * JacobianFactorOf(form))

                    If Not work.lu.Factor(matrixBuffer) Then
                        newtonFailures += 1
                        Return False
                    End If

                    factorized = True
                End If

                ' W Δ = G，则 y ← y − Δ
                Call work.lu.Solve(g, delta)
                Call SolverKernels.AddScaled(iterate, -1.0, delta)

                If SolverKernels.MaxAbs(delta) <= tolerance * (1.0 + SolverKernels.MaxAbs(iterate)) Then
                    converged = True
                    Exit For
                End If

                ' 修正 Newton：定期重新计算 Jacobian，避免迭代停滞
                If iteration Mod 8 = 0 Then
                    factorized = False
                End If
            Next

            If Not converged Then
                newtonFailures += 1
                Return False
            End If

            Call SolverKernels.Copy(yOut, iterate)

            Return True
        End Function

        ''' <summary>
        ''' The residual of the given implicit form:
        ''' 
        ''' - 后向 Euler：``G = y − y_n − h·f(t, y)``
        ''' - 梯形法：``G = y − y_n − h/2·(f_n + f(t, y))``
        ''' - BDF2：``G = 3/2·y − 2·y_n + 1/2·y_{n−1} − h·f(t, y)``
        ''' </summary>
        Private Sub Residual(form As ImplicitForm, y As Double(), yStart As Double(), h As Double, g As Double())
            Dim f As Double() = work.fCurrent

            Select Case form
                Case ImplicitForm.BackwardEuler
                    For i As Integer = 0 To g.Length - 1
                        g(i) = y(i) - yStart(i) - h * f(i)
                    Next
                Case ImplicitForm.Trapezoidal
                    Dim fPrev As Double() = work.fPrev

                    For i As Integer = 0 To g.Length - 1
                        g(i) = y(i) - yStart(i) - 0.5 * h * (fPrev(i) + f(i))
                    Next
                Case ImplicitForm.BDF2
                    Dim yPrev As Double() = work.yPrev

                    For i As Integer = 0 To g.Length - 1
                        g(i) = 1.5 * y(i) - 2.0 * yStart(i) + 0.5 * yPrev(i) - h * f(i)
                    Next
                Case Else
                    Throw New NotSupportedException($"Unsupported implicit form: {form}")
            End Select
        End Sub

        ''' <summary>
        ''' The diagonal coefficient of the iteration matrix ``W = diagonal·I − h·factor·J``.
        ''' </summary>
        Private Shared Function DiagonalOf(form As ImplicitForm) As Double
            If form = ImplicitForm.BDF2 Then
                Return 1.5
            Else
                Return 1.0
            End If
        End Function

        ''' <summary>
        ''' The Jacobian coefficient of the iteration matrix ``W = diagonal·I − h·factor·J``.
        ''' </summary>
        Private Shared Function JacobianFactorOf(form As ImplicitForm) As Double
            If form = ImplicitForm.Trapezoidal Then
                Return 0.5
            Else
                Return 1.0
            End If
        End Function

        ''' <summary>
        ''' Subdivide the failed step internally by the implicit Euler method, the output grid
        ''' is not affected because the end time of the step is unchanged.
        ''' (用隐式 Euler 对失败的积分步做内部二分细分，输出网格保持不变)
        ''' </summary>
        ''' <param name="tPrev"></param>
        ''' <param name="tNext"></param>
        ''' <param name="h"></param>
        Private Sub Subdivide(tPrev As Double, tNext As Double, h As Double)
            Dim maxSubdivisions As Integer = std.Max(2, options.MaxStepSubdivisions)
            Dim subdivisions As Integer = 2
            Dim run As Double() = work.yPrevPrev

            While subdivisions <= maxSubdivisions
                Dim hSub As Double = h / subdivisions
                Dim ok As Boolean = True

                Call SolverKernels.Copy(run, work.yStage)

                For k As Integer = 1 To subdivisions
                    Dim tSub As Double = tPrev + hSub * (k - 1)

                    ' 子步起点的导数，用于 Newton 迭代的预测
                    Call Evaluate(tSub, work.vyPrevPrev, work.vfPrev)

                    If Not NewtonStep(ImplicitForm.BackwardEuler, tSub + hSub, hSub, run, run) Then
                        ok = False
                        Exit For
                    End If
                Next

                If ok Then
                    Call SolverKernels.Copy(buffers.y, run)
                    Return
                End If

                subdivisions *= 2
            End While

            Throw New InvalidOperationException(
                $"{Me.GetType().Name}: the Newton iteration is failed at t = {tNext}, the ODEs system may be too stiff for the current step size!")
        End Sub
    End Class

    ''' <summary>
    ''' The implicit backward Euler method (BDF1), 1st order and L-stable.(隐式后向欧拉法)
    ''' </summary>
    ''' <remarks>
    ''' 每一个积分步求解 ``y_{n+1} − y_n − h·f(t_{n+1}, y_{n+1}) = 0``，迭代矩阵为 ``I − h·J``。
    ''' 它是一阶方法，但是具有最好的稳定性（L-稳定），适合强刚性问题。
    ''' </remarks>
    Public Class BackwardEulerSolver : Inherits ImplicitSolverBase

        ''' <summary>
        ''' Create the backward Euler solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="options"></param>
        Sub New(system As ODEs, Optional options As ODESolverOptions = Nothing)
            MyBase.New(system, options)
        End Sub

        Protected Overrides Function StepCore(tPrev As Double, tNext As Double, h As Double) As Boolean
            Return NewtonStep(ImplicitForm.BackwardEuler, tNext, h, work.yStage, buffers.y)
        End Function
    End Class

    ''' <summary>
    ''' The implicit trapezoidal rule (Crank-Nicolson), 2nd order and A-stable.(隐式梯形法)
    ''' </summary>
    ''' <remarks>
    ''' 每一个积分步求解 ``y_{n+1} − y_n − h/2·(f_n + f(t_{n+1}, y_{n+1})) = 0``，
    ''' 迭代矩阵为 ``I − h/2·J``。它没有数值阻尼，因此对刚性问题可能会出现振荡。
    ''' </remarks>
    Public Class TrapezoidalSolver : Inherits ImplicitSolverBase

        ''' <summary>
        ''' Create the trapezoidal solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="options"></param>
        Sub New(system As ODEs, Optional options As ODESolverOptions = Nothing)
            MyBase.New(system, options)
        End Sub

        Protected Overrides Function StepCore(tPrev As Double, tNext As Double, h As Double) As Boolean
            Return NewtonStep(ImplicitForm.Trapezoidal, tNext, h, work.yStage, buffers.y)
        End Function
    End Class

    ''' <summary>
    ''' The 2nd order backward differentiation formula (BDF2).(二阶后向微分公式)
    ''' </summary>
    ''' <remarks>
    ''' 每一个积分步求解 ``3/2·y_{n+1} − 2·y_n + 1/2·y_{n−1} − h·f(t_{n+1}, y_{n+1}) = 0``，
    ''' 迭代矩阵为 ``3/2·I − h·J``。第一个积分步取 ``y_{-1} = y_0``，等价于用后向 Euler 起步
    ''' （一阶精度），之后的每一个积分步都是二阶精度。
    ''' </remarks>
    Public Class Bdf2Solver : Inherits ImplicitSolverBase

        ''' <summary>
        ''' Create the BDF2 solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="options"></param>
        Sub New(system As ODEs, Optional options As ODESolverOptions = Nothing)
            MyBase.New(system, options)
        End Sub

        Protected Overrides Function StepCore(tPrev As Double, tNext As Double, h As Double) As Boolean
            ' 第一个积分步没有 y_{n-1}，必须使用后向 Euler 起步：
            ' 如果直接取 y_{-1} = y_0 代入 BDF2 的公式，那么第一步实际上只是走了一个 2h/3
            ' 的隐式 Euler 步，会引入 O(h) 的系统性误差，使整个方法的收敛阶退化为一阶。
            If CurrentStepIndex = 1 Then
                Return NewtonStep(ImplicitForm.BackwardEuler, tNext, h, work.yStage, buffers.y)
            Else
                Return NewtonStep(ImplicitForm.BDF2, tNext, h, work.yStage, buffers.y)
            End If
        End Function
    End Class

    ''' <summary>
    ''' The 2-stage 2nd order L-stable Rosenbrock method (ROS2), it is a linearly implicit
    ''' method which requires only one LU decomposition per step and no Newton iteration.
    ''' (二阶线性隐式 Rosenbrock 方法：每个积分步只需要一次 LU 分解，无需 Newton 迭代)
    ''' </summary>
    ''' <remarks>
    ''' 系数取 ``γ = 1 + 1/√2``、``a21 = 2 − √2``、``c21 = −3``、``b1 = b2 = 1/2``，可以验证：
    ''' 
    ''' ```
    ''' (1) 相容性：b1 + b2 = 1
    ''' (2) 二阶条件：b2·(a21 + c21) = 1/2 − γ
    ''' (3) L-稳定性：R(∞) = 1 − 1/γ + (1/2 − γ)/γ² = 0
    ''' ```
    ''' 
    ''' 因此该方法的稳定性函数在 ``z → −∞`` 的时候趋于零，非常适合刚性问题的求解。
    ''' </remarks>
    Public Class RosenbrockRos2Solver : Inherits ImplicitSolverBase

        ''' <summary>
        ''' ``γ = 1 + 1/√2``
        ''' </summary>
        Public Const Gamma As Double = 1.7071067811865476
        ''' <summary>
        ''' ``a21 = 2 − √2 = 1/γ``
        ''' </summary>
        Public Const A21 As Double = 0.5857864376269049
        ''' <summary>
        ''' ``c21 = −3``
        ''' </summary>
        Public Const C21 As Double = -3.0
        ''' <summary>
        ''' ``b1 = b2 = 1/2``
        ''' </summary>
        Public Const B1 As Double = 0.5
        ''' <summary>
        ''' ``b1 = b2 = 1/2``
        ''' </summary>
        Public Const B2 As Double = 0.5

        ''' <summary>
        ''' Create the ROS2 solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="options"></param>
        Sub New(system As ODEs, Optional options As ODESolverOptions = Nothing)
            MyBase.New(system, options)
        End Sub

        Protected Overrides Function StepCore(tPrev As Double, tNext As Double, h As Double) As Boolean
            Dim y As Double() = buffers.y
            Dim yStart As Double() = work.yStage
            Dim k1 As Double() = buffers.k(0)
            Dim k2 As Double() = buffers.k(1)
            Dim f1 As Double() = buffers.k(2)
            Dim y1 As Double() = work.yPrevPrev
            Dim matrixBuffer As Double(,) = work.matrix
            Dim rhs As Double() = work.rhs
            Dim product As Double() = work.delta

            ' J(t_n, y_n) 与迭代矩阵 M = I − γhJ
            Call EvaluateJacobian(tPrev, yStart, work.jacobian)
            Call SolverKernels.BuildIterationMatrix(matrixBuffer, work.jacobian, 1.0, Gamma * h)

            If Not work.lu.Factor(matrixBuffer) Then
                Return False
            End If

            ' k1 = M⁻¹ · h·f_n
            Dim fPrev As Double() = work.fPrev

            For i As Integer = 0 To rhs.Length - 1
                rhs(i) = h * fPrev(i)
            Next

            Call work.lu.Solve(rhs, k1)

            ' y1 = y_n + a21·k1
            Call SolverKernels.Copy(y1, yStart)
            Call SolverKernels.AddScaled(y1, A21, k1)

            ' f1 = f(t_{n+1}, y1)
            Call Evaluate(tNext, work.vyPrevPrev, buffers.vk(2))

            ' k2 = M⁻¹ · [h·f1 + h·J·(c21·k1)]
            Call SolverKernels.MatVec(product, work.jacobian, k1)

            For i As Integer = 0 To rhs.Length - 1
                rhs(i) = h * (f1(i) + C21 * product(i))
            Next

            Call work.lu.Solve(rhs, k2)

            ' y_{n+1} = y_n + b1·k1 + b2·k2
            Call SolverKernels.Copy(y, yStart)
            Call SolverKernels.AddScaled(y, B1, k1)
            Call SolverKernels.AddScaled(y, B2, k2)

            Return True
        End Function
    End Class
End Namespace
