Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Dynamics

    ''' <summary>
    ''' The adaptive step size Dormand-Prince 5(4) solver for the <see cref="ODEs"/> dynamics system.
    ''' (用于求解 ODE 方程组的自适应步长 Dormand-Prince 5(4) 求解器)
    ''' </summary>
    ''' <remarks>
    ''' 求解器使用 7 个阶段的 Dormand-Prince 5(4) 嵌入式方法对，通过「5 阶解与 4 阶解之差」
    ''' 来估计局部截断误差并自动调整步长；由于具有 FSAL 性质，每一个积分步只需要 6 次函数
    ''' 求值。自适应的变步长结果会通过三次 Hermite 插值重采样到等间距的输出网格之上，因此
    ''' 返回的结果与 <see cref="ODEs.Solve"/> 完全同构。
    ''' </remarks>
    Public Class RungeKutta45

        ReadOnly ODEs As ODEs
        ReadOnly buffers As SolverBuffers

        Dim xArray As Double()
        Dim yColumns As Double()()

        Dim stepCount As Integer
        Dim evaluationCount As Integer

        Sub New(system As ODEs)
            ODEs = system
            buffers = New SolverBuffers(system.Size)
        End Sub

        ''' <summary>
        ''' The sampling time points of the last integration result, it contains ``n + 1``
        ''' points which includes both boundaries of the integration range.
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property XValues As Double()
            Get
                Return xArray
            End Get
        End Property

        ''' <summary>
        ''' The integration result columns of each state variable of the last integration result.
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property YValues As Double()()
            Get
                Return yColumns
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
            Call ODEs.ODEs(t, y, buffers.vk(stage))

            evaluationCount += 1
        End Sub

        ''' <summary>
        ''' Solve the ODEs system with the adaptive step size control.
        ''' </summary>
        ''' <param name="y0">The y initial value.(初值y(n))</param>
        ''' <param name="n">The number of the points on the output grid minus one.</param>
        ''' <param name="a">The lower bound of the integration range.</param>
        ''' <param name="b">The upper bound of the integration range.</param>
        ''' <param name="atol">The absolute error tolerance of each state variable.</param>
        ''' <param name="rtol">The relative error tolerance of each state variable.</param>
        ''' <param name="initialStep">The initial step size, zero means auto select.</param>
        ''' <param name="maxSteps">The maximal number of the integration steps.</param>
        ''' <returns></returns>
        Public Function Solve(y0 As Double(), n As Integer, a As Double, b As Double,
                              Optional atol As Double = 1.0E-9,
                              Optional rtol As Double = 1.0E-7,
                              Optional initialStep As Double = 0.0,
                              Optional maxSteps As Integer = 10000000) As RungeKutta45

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
            Dim tmp As Double() = buffers.tmp
            Dim y5 As Double() = buffers.ynext
            Dim k As Double()() = buffers.k
            Dim columns As Double()() = New Double(size - 1)() {}

            For i As Integer = 0 To columns.Length - 1
                columns(i) = New Double(n) {}
            Next

            xArray = ODESequence.CreatePoints(a, b, n)
            yColumns = columns
            stepCount = 0
            evaluationCount = 0

            For i As Integer = 0 To size - 1
                y(i) = y0(i)
                columns(i)(0) = y0(i)
            Next

            Dim span As Double = b - a
            Dim grid As Double() = xArray
            Dim gridIndex As Integer = 1
            Dim t As Double = a
            Dim h As Double = If(initialStep > 0.0, std.Min(initialStep, span), span / 100.0)
            Dim minStep As Double = 1.0E-14 * std.Max(1.0, std.Abs(b))
            Dim tNew As Double, err As Double, factor As Double

            ' 初始位置的导数(FSAL：这一步的结果会成为下一个积分步的 k1)
            Call Evaluate(t, buffers.vy, 0)

            While t < b
                If t + h > b Then
                    h = b - t
                End If
                If t + h <= t Then
                    Throw New InvalidOperationException(
                        $"Step size underflow at t = {t}: the ODEs system may be stiff, please try the implicit methods or reduce the integration range.")
                End If

                ODEs.CurrentStep = std.Max(0, gridIndex - 1)

                ' --- 第2阶段 ---
                Call SolverKernels.Combine(tmp, y, h * Dp5Coefficients.a2(0), k(0))
                Call Evaluate(t + Dp5Coefficients.c(1) * h, buffers.vtmp, 1)
                ' --- 第3阶段 ---
                Call SolverKernels.Combine2(tmp, y, h,
                                            Dp5Coefficients.a3(0), k(0),
                                            Dp5Coefficients.a3(1), k(1))
                Call Evaluate(t + Dp5Coefficients.c(2) * h, buffers.vtmp, 2)
                ' --- 第4阶段 ---
                Call SolverKernels.Combine3(tmp, y, h,
                                            Dp5Coefficients.a4(0), k(0),
                                            Dp5Coefficients.a4(1), k(1),
                                            Dp5Coefficients.a4(2), k(2))
                Call Evaluate(t + Dp5Coefficients.c(3) * h, buffers.vtmp, 3)
                ' --- 第5阶段 ---
                Call SolverKernels.Combine4(tmp, y, h,
                                            Dp5Coefficients.a5(0), k(0),
                                            Dp5Coefficients.a5(1), k(1),
                                            Dp5Coefficients.a5(2), k(2),
                                            Dp5Coefficients.a5(3), k(3))
                Call Evaluate(t + Dp5Coefficients.c(4) * h, buffers.vtmp, 4)
                ' --- 第6阶段 ---
                Call SolverKernels.Combine5(tmp, y, h,
                                            Dp5Coefficients.a6(0), k(0),
                                            Dp5Coefficients.a6(1), k(1),
                                            Dp5Coefficients.a6(2), k(2),
                                            Dp5Coefficients.a6(3), k(3),
                                            Dp5Coefficients.a6(4), k(4))
                Call Evaluate(t + Dp5Coefficients.c(5) * h, buffers.vtmp, 5)

                ' --- 5阶解 = 第7阶段的输入 ---
                Call SolverKernels.Combine5(y5, y, h,
                                            Dp5Coefficients.a7(0), k(0),
                                            Dp5Coefficients.a7(2), k(2),
                                            Dp5Coefficients.a7(3), k(3),
                                            Dp5Coefficients.a7(4), k(4),
                                            Dp5Coefficients.a7(5), k(5))
                Call Evaluate(t + h, buffers.vynext, 6)

                err = SolverKernels.ErrorNorm(y, y5, k, h, atol, rtol)
                factor = SolverKernels.StepFactor(err)

                If err <= 1.0 Then
                    ' 步长被接受：用三次 Hermite 插值填充这一步之内的输出网格点
                    tNew = t + h

                    While gridIndex <= n AndAlso grid(gridIndex) <= tNew
                        Dim theta As Double = (grid(gridIndex) - t) / h

                        For i As Integer = 0 To size - 1
                            columns(i)(gridIndex) = SolverKernels.Hermite(y(i), k(0)(i), y5(i), k(6)(i), h, theta)
                        Next

                        gridIndex += 1
                    End While

                    ' FSAL：k7 就是下一步的 k1
                    Call buffers.SetState(y5)
                    Call System.Array.Copy(k(6), k(0), size)

                    t = If(tNew >= b, b, tNew)
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

            ' 浮点误差可能会导致最后几个网格点没有被填充
            While gridIndex <= n
                For i As Integer = 0 To size - 1
                    columns(i)(gridIndex) = y(i)
                Next

                gridIndex += 1
            End While

            Return Me
        End Function
    End Class
End Namespace
