Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports std = System.Math

' The adaptive step size Dormand-Prince 5(4) integrator for a single scalar ordinary
' differential equation.(标量常微分方程的自适应步长 Dormand-Prince 5(4) 积分器)
Partial Module ODESolver

    ''' <summary>
    ''' Dormand-Prince 5(4) 自适应步长积分（标量常微分方程）
    ''' </summary>
    ''' <remarks>
    ''' 与 <see cref="ODESolver.Eluer"/>、<see cref="ODESolver.RK2"/>、<see cref="ODESolver.RK4"/>
    ''' 这些固定步长方法不同，本方法通过「5 阶解与 4 阶解之差」估计局部截断误差，并自动调整
    ''' 步长，因此可以在解变化剧烈的区间自动加密步长、在解平缓的区间放大步长。由于 Dormand-Prince
    ''' 方法对具有 FSAL 性质，每一个积分步只需要 6 次函数求值。
    ''' 
    ''' 返回值仍然是等间距的输出网格 ``[a, b]`` 上的 ``n + 1`` 个采样点，变步长的结果会通过
    ''' 三次 Hermite 插值重采样到该网格之上，因此可以直接替换固定步长的 RK4 调用。
    ''' </remarks>
    ''' <param name="ode">The ODE model.</param>
    ''' <param name="n">输出网格的间隔数，结果将会含有 ``n + 1`` 个采样点</param>
    ''' <param name="a">积分下界</param>
    ''' <param name="b">积分上界</param>
    ''' <param name="atol">绝对误差容限</param>
    ''' <param name="rtol">相对误差容限</param>
    ''' <param name="initialStep">初始步长，小于等于零的时候自动选择</param>
    ''' <param name="maxSteps">最大积分步数</param>
    ''' <param name="numberOfSteps">实际执行的积分步数</param>
    ''' <param name="numberOfEvaluations">右端函数的求值次数</param>
    ''' <returns></returns>
    <Extension>
    Public Function RK45(ode As ODE, n As Integer, a As Double, b As Double,
                         Optional atol As Double = 1.0E-9,
                         Optional rtol As Double = 1.0E-7,
                         Optional initialStep As Double = 0.0,
                         Optional maxSteps As Integer = 10000000,
                         Optional ByRef numberOfSteps As Integer = 0,
                         Optional ByRef numberOfEvaluations As Integer = 0) As ODEOutput

        If n < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(n), "The number of the integration steps must be positive!")
        End If
        If b <= a Then
            Throw New ArgumentOutOfRangeException(NameOf(b), "The upper bound of the integration range must be greater than the lower bound!")
        End If

        Dim f As df = GetDerivative(ode)
        Dim out As ODEOutput = ode.Allocate(n, a, b)
        Dim grid As Double() = out.X.ToArray
        Dim yValues As Double() = out.Y.vector

        Dim k As Double() = New Double(6) {}   ' k1 .. k7
        Dim t As Double = a
        Dim y As Double = ode.y0
        Dim span As Double = b - a
        Dim h As Double = If(initialStep > 0.0, std.Min(initialStep, span), span / 100.0)
        Dim minStep As Double = 1.0E-14 * std.Max(1.0, std.Abs(b))
        Dim gridIndex As Integer = 1
        Dim y5 As Double, err As Double, factor As Double, tNew As Double, theta As Double

        numberOfSteps = 0
        numberOfEvaluations = 0

        ' FSAL：t = a 处的导数作为 k1
        k(0) = f(t, y)
        numberOfEvaluations = 1

        While t < b
            If t + h > b Then
                h = b - t
            End If
            If t + h <= t Then
                Throw New InvalidOperationException(
                    $"Step size underflow at t = {t}: the ODE is may be stiff, please try the implicit methods or reduce the integration range.")
            End If

            ' 第2 ~ 6 阶段
            k(1) = f(t + Dp5Coefficients.c(1) * h, y + h * Dp5Coefficients.a2(0) * k(0))
            k(2) = f(t + Dp5Coefficients.c(2) * h,
                     y + h * (Dp5Coefficients.a3(0) * k(0) + Dp5Coefficients.a3(1) * k(1)))
            k(3) = f(t + Dp5Coefficients.c(3) * h,
                     y + h * (Dp5Coefficients.a4(0) * k(0) + Dp5Coefficients.a4(1) * k(1) + Dp5Coefficients.a4(2) * k(2)))
            k(4) = f(t + Dp5Coefficients.c(4) * h,
                     y + h * (Dp5Coefficients.a5(0) * k(0) + Dp5Coefficients.a5(1) * k(1) +
                              Dp5Coefficients.a5(2) * k(2) + Dp5Coefficients.a5(3) * k(3)))
            k(5) = f(t + Dp5Coefficients.c(5) * h,
                     y + h * (Dp5Coefficients.a6(0) * k(0) + Dp5Coefficients.a6(1) * k(1) +
                              Dp5Coefficients.a6(2) * k(2) + Dp5Coefficients.a6(3) * k(3) +
                              Dp5Coefficients.a6(4) * k(4)))

            ' 5阶解
            y5 = y + h * (Dp5Coefficients.a7(0) * k(0) +
                          Dp5Coefficients.a7(2) * k(2) +
                          Dp5Coefficients.a7(3) * k(3) +
                          Dp5Coefficients.a7(4) * k(4) +
                          Dp5Coefficients.a7(5) * k(5))
            ' 第7阶段：由于 FSAL 性质，这个导数同时也是下一步的 k1
            k(6) = f(t + h, y5)
            numberOfEvaluations += 6

            ' 4阶解用于估计误差
            Dim y4 As Double = y + h * (Dp5Coefficients.b4(0) * k(0) +
                                        Dp5Coefficients.b4(2) * k(2) +
                                        Dp5Coefficients.b4(3) * k(3) +
                                        Dp5Coefficients.b4(4) * k(4) +
                                        Dp5Coefficients.b4(5) * k(5) +
                                        Dp5Coefficients.b4(6) * k(6))
            Dim scale As Double = atol + rtol * std.Max(std.Abs(y), std.Abs(y5))

            err = std.Abs(y5 - y4) / scale
            factor = SolverKernels.StepFactor(err)

            If err <= 1.0 Then
                tNew = t + h

                While gridIndex <= n AndAlso grid(gridIndex) <= tNew
                    theta = (grid(gridIndex) - t) / h
                    yValues(gridIndex) = SolverKernels.Hermite(y, k(0), y5, k(6), h, theta)
                    gridIndex += 1
                End While

                y = y5
                k(0) = k(6)
                t = If(tNew >= b, b, tNew)
            End If

            numberOfSteps += 1

            If numberOfSteps > maxSteps Then
                Throw New InvalidOperationException(
                    $"The maximal number of the integration steps ({maxSteps}) is exceeded at t = {t}!")
            End If

            h = h * factor

            If h < minStep AndAlso t < b Then
                Throw New InvalidOperationException(
                    $"Step size underflow at t = {t}: the ODE is may be stiff, please try the implicit methods or reduce the integration range.")
            End If
        End While

        ' 浮点误差可能会导致最后几个网格点没有被填充
        While gridIndex <= n
            yValues(gridIndex) = y
            gridIndex += 1
        End While

        Return out
    End Function
End Module
