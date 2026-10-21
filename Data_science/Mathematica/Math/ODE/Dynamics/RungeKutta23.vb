Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Dynamics

    ''' <summary>
    ''' The adaptive step size Bogacki-Shampine 3(2) solver for the <see cref="ODEs"/>
    ''' dynamics system.(自适应步长的 Bogacki-Shampine 3(2) 求解器)
    ''' </summary>
    ''' <remarks>
    ''' 3(2) 嵌入式方法对：使用 3 阶解向前推进，用 2 阶解估计局部截断误差。它具有 FSAL
    ''' 性质（第 4 个阶段的导数就是下一步的第 1 个阶段），因此每一个积分步只需要 3 次新的
    ''' 函数求值 —— 相比 RK45 的 6 次求值减少一半，适合对精度要求不高、或者需要大量重复
    ''' 仿真的场景。
    ''' 
    ''' 变步长的结果通过三次 Hermite 插值重采样到与 <see cref="ODEs.Solve"/> 完全相同的
    ''' ``n + 1`` 个等距采样点之上。
    ''' </remarks>
    Public Class RungeKutta23

        ' Butcher 表
        Private Const C2 As Double = 0.5
        Private Const C3 As Double = 0.75
        Private Const A21 As Double = 0.5
        Private Const A32 As Double = 0.75
        Private Const A41 As Double = 2.0 / 9.0
        Private Const A42 As Double = 1.0 / 3.0
        Private Const A43 As Double = 4.0 / 9.0
        Private Const ErrorExponent As Double = 1.0 / 3.0

        ReadOnly model As ODEs
        ReadOnly buffers As SolverBuffers

        Dim grid As GridWriter
        Dim stepCount As Integer
        Dim evaluationCount As Integer

        ''' <summary>
        ''' Create the adaptive RK23 solver of the given ODEs system.
        ''' </summary>
        ''' <param name="system"></param>
        Sub New(system As ODEs)
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
        ''' The number of the integration steps which are actually performed by the adaptive
        ''' step size controller.(自适应步长控制器实际执行的积分步数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumberOfSteps As Integer
            Get
                Return stepCount
            End Get
        End Property

        ''' <summary>
        ''' The number of the right-hand side function evaluations.(右端函数的求值次数)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NumberOfEvaluations As Integer
            Get
                Return evaluationCount
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the right-hand side function of the given stage.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="stage"></param>
        Private Sub Evaluate(t As Double, y As Vector, stage As Integer)
            Call model.Derivatives(t, y, buffers.vk(stage))

            evaluationCount += 1
        End Sub

        ''' <summary>
        ''' Solve the ODEs system with the adaptive step size control.
        ''' </summary>
        ''' <param name="y0">The y initial value.(初值)</param>
        ''' <param name="n">The number of the points on the output grid minus one.</param>
        ''' <param name="a">The lower bound of the integration range.</param>
        ''' <param name="b">The upper bound of the integration range.</param>
        ''' <param name="atol">The absolute error tolerance of each state variable.</param>
        ''' <param name="rtol">The relative error tolerance of each state variable.</param>
        ''' <param name="initialStep">The initial step size, zero means auto select.</param>
        ''' <param name="maxSteps">The maximal number of the integration steps.</param>
        ''' <returns></returns>
        Public Function Solve(y0 As Double(), n As Integer, a As Double, b As Double,
                              Optional atol As Double = 1.0E-6,
                              Optional rtol As Double = 1.0E-4,
                              Optional initialStep As Double = 0.0,
                              Optional maxSteps As Integer = 10000000) As RungeKutta23

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

            Dim size As Integer = buffers.Size
            Dim y As Double() = buffers.y
            Dim y3 As Double() = buffers.ynext
            Dim tmp As Double() = buffers.tmp
            Dim k As Double()() = buffers.k
            Dim span As Double = b - a
            Dim h As Double = If(initialStep > 0.0, std.Min(initialStep, span), span / 50.0)
            Dim minStep As Double = 1.0E-14 * std.Max(1.0, std.Abs(b))
            Dim t As Double = a
            Dim err As Double, factor As Double

            Me.grid = New GridWriter(a, b, n, size)
            Me.stepCount = 0
            Me.evaluationCount = 0

            Call buffers.SetState(y0)
            Call grid.WriteInitial(y)

            ' FSAL：t = a 处的导数作为 k1
            Call Evaluate(t, buffers.vy, 0)

            While t < b
                If t + h > b Then
                    h = b - t
                End If
                If t + h <= t Then
                    Throw New InvalidOperationException(
                        $"Step size underflow at t = {t}: the ODEs system may be stiff, please try the implicit methods or reduce the integration range.")
                End If

                model.CurrentStep = std.Max(0, grid.Cursor - 1)

                ' 第 2 阶段
                Call SolverKernels.Combine(tmp, y, h * A21, k(0))
                Call Evaluate(t + C2 * h, buffers.vtmp, 1)
                ' 第 3 阶段
                Call SolverKernels.Combine(tmp, y, h * A32, k(1))
                Call Evaluate(t + C3 * h, buffers.vtmp, 2)
                ' 3 阶解
                Call SolverKernels.Combine3(y3, y, h,
                                            A41, k(0),
                                            A42, k(1),
                                            A43, k(2))
                ' 第 4 阶段(FSAL)
                Call Evaluate(t + h, buffers.vynext, 3)

                ' 2 阶解与误差估计
                Dim sum As Double = 0.0

                For i As Integer = 0 To size - 1
                    Dim y2i As Double = y(i) + h * (
                        7.0 / 24.0 * k(0)(i) +
                        0.25 * k(1)(i) +
                        1.0 / 3.0 * k(2)(i) +
                        0.125 * k(3)(i))
                    Dim scale As Double = atol + rtol * std.Max(std.Abs(y(i)), std.Abs(y3(i)))
                    Dim e As Double = (y3(i) - y2i) / scale

                    sum += e * e
                Next

                err = std.Sqrt(sum / size)
                factor = StepFactor(err)

                If err <= 1.0 Then
                    ' 接受该步：用三次 Hermite 插值填充这一步内部的采样点
                    Call grid.WriteSpan(t, y, k(0), t + h, y3, k(3))

                    Call buffers.SetState(y3)
                    Call System.Array.Copy(k(3), k(0), size)

                    t = If(t + h >= b, b, t + h)
                End If

                stepCount += 1

                If stepCount > maxSteps Then
                    Throw New InvalidOperationException(
                        $"The maximal number of the integration steps ({maxSteps}) is exceeded at t = {t}!")
                End If

                h = h * factor

                If h < minStep AndAlso t < b Then
                    Throw New InvalidOperationException(
                        $"Step size underflow at t = {t}: the ODEs system may be stiff, please try the implicit methods or reduce the integration range.")
                End If
            End While

            Call grid.Flush(y)
            model.CurrentStep = grid.Cursor - 1
            Call model.SyncState(y)

            Return Me
        End Function

        ''' <summary>
        ''' The step size change factor of the 3(2) pair: ``0.9 * err^(-1/3)``, clamped into
        ''' ``[0.2, 5]``.
        ''' </summary>
        ''' <param name="err"></param>
        ''' <returns></returns>
        Private Shared Function StepFactor(err As Double) As Double
            Dim f As Double

            If Double.IsNaN(err) Then
                Return 0.2
            ElseIf err <= 0.0 Then
                Return 5.0
            End If

            f = 0.9 * std.Pow(err, -ErrorExponent)

            If f < 0.2 Then
                Return 0.2
            ElseIf f > 5.0 Then
                Return 5.0
            Else
                Return f
            End If
        End Function
    End Class
End Namespace
