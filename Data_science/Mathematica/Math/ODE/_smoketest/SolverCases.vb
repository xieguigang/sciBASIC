Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' The test cases of the newly added ODEs integrators: the convergence order, the stiff
''' problem, the Rosenbrock stability function, the symplectic energy conservation, the
''' multistep startup and the unified dispatch.
''' </summary>
Module SolverCases

#Region "Models"

    ''' <summary>
    ''' ``y' = rate * y``, ``y(0) = 1``，解析解为 ``y = exp(rate * t)``
    ''' </summary>
    Public Class DecayModel : Inherits ODEs

        Dim y As var

        Public rate As Double = -1.0

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(y) = rate * y.Value
        End Sub

        Protected Overrides Function y0() As var()
            y.Value = 1.0
            Return {y}
        End Function
    End Class

    ''' <summary>
    ''' ``y' = rate·(y − cos t) − sin t``，``y(0) = 1``，解析解恒为 ``y = cos t``。
    ''' ``rate`` 取很大的负值的时候就成为一个刚性(快瞬态)问题。
    ''' </summary>
    Public Class StiffModel : Inherits ODEs

        Dim y As var

        Public rate As Double = -10000.0

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            Dim yi As Double = y.Value

            dy(y) = rate * (yi - std.Cos(dx)) - std.Sin(dx)
        End Sub

        Protected Overrides Function y0() As var()
            y.Value = 1.0
            Return {y}
        End Function

        Public Shared ReadOnly Property Exact As Func(Of Double, Double)
            Get
                Return Function(t) std.Cos(t)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' 简谐振子：``dq/dt = p``、``dp/dt = -omega²·q``，能量 ``E = 0.5·(p² + omega²·q²)``
    ''' </summary>
    Public Class HarmonicSymplectic : Inherits SymplecticODEs

        Dim x As var
        Dim v As var

        Public omega As Double = 1.0

        Protected Overrides Sub Drift(t As Double, q As Double(), p As Double(), dq As Double())
            dq(0) = p(0)
        End Sub

        Protected Overrides Sub Kick(t As Double, q As Double(), p As Double(), dp As Double())
            dp(0) = -omega * omega * q(0)
        End Sub

        Protected Overrides Function y0() As var()
            x.Value = 1.0
            v.Value = 0.0
            Return {x, v}
        End Function
    End Class

#End Region

    Public Sub RunAll()
        Call RunOrderCases()
        Call RunStiffCases()
        Call RunRosenbrockStability()
        Call RunSymplecticCases()
        Call RunStartupCases()
        Call RunDispatchCases()
    End Sub

#Region "Convergence order"

    Private Sub ReportOrder(name As String, coarse As ODEsOut, fine As ODEsOut, exact As Double, order As Double)
        Dim errCoarse As Double = std.Abs(coarse.y("y").value.LastValue() - exact)
        Dim errFine As Double = std.Abs(fine.y("y").value.LastValue() - exact)
        Dim ratio As Double = errCoarse / errFine
        Dim expected As Double = std.Pow(2.0, order)

        Call Check($"{name}: the error is decreased when h is halved", errFine < errCoarse,
                   $"err(h)={errCoarse:G4} err(h/2)={errFine:G4}")
        Call Check($"{name}: the convergence order is about {order}",
                   ratio > expected * 0.6 AndAlso ratio < expected * 1.6,
                   $"ratio={ratio:G3} expected~{expected:G2} err(h)={errCoarse:G4} err(h/2)={errFine:G4}")
    End Sub

    Public Sub RunOrderCases()
        Console.WriteLine()
        Console.WriteLine("--- solver accuracy and convergence order ---")

        ' ---- 显式方法：y' = -y, [0, 5] ----
        Dim decay As New DecayModel()
        Dim exact As Double = std.Exp(-5.0)

        Call ReportOrder("euler", decay.SolveEuler(100, 0.0, 5.0), decay.SolveEuler(200, 0.0, 5.0), exact, 1.0)
        Call ReportOrder("rk2", decay.SolveRK2(100, 0.0, 5.0), decay.SolveRK2(200, 0.0, 5.0), exact, 2.0)
        Call ReportOrder("gill", decay.SolveGill(100, 0.0, 5.0), decay.SolveGill(200, 0.0, 5.0), exact, 4.0)
        Call ReportOrder("rk4", decay.Solve(100, 0.0, 5.0), decay.Solve(200, 0.0, 5.0), exact, 4.0)
        Call ReportOrder("abm4", decay.SolveABM4(100, 0.0, 5.0), decay.SolveABM4(200, 0.0, 5.0), exact, 4.0)

        ' ---- 隐式方法：y' = -y, [0, 1]（λ = -1 保证固定步长的稳定性） ----
        Dim exact1 As Double = std.Exp(-1.0)

        Call ReportOrder("backward euler",
                         decay.SolveBackwardEuler(50, 0.0, 1.0),
                         decay.SolveBackwardEuler(100, 0.0, 1.0), exact1, 1.0)
        Call ReportOrder("trapezoidal",
                         decay.SolveTrapezoidal(50, 0.0, 1.0),
                         decay.SolveTrapezoidal(100, 0.0, 1.0), exact1, 2.0)
        Call ReportOrder("bdf2",
                         decay.SolveBDF2(50, 0.0, 1.0),
                         decay.SolveBDF2(100, 0.0, 1.0), exact1, 2.0)
        Call ReportOrder("rosenbrock ros2",
                         decay.SolveRosenbrockROS2(50, 0.0, 1.0),
                         decay.SolveRosenbrockROS2(100, 0.0, 1.0), exact1, 2.0)

        ' ---- 自适应方法需要与解析解一致 ----
        Dim rk23 = decay.SolveRK23(50, 0.0, 5.0, options:=New ODESolverOptions With {.Atol = 1.0E-10, .Rtol = 1.0E-8})

        Call CheckClose("rk23 matches the analytic solution", rk23.y("y").value.LastValue(), exact, 1.0E-7)
        Call Check("rk23 series length", rk23.y("y").value.Length = 51, $"length={rk23.y("y").value.Length}")
    End Sub

#End Region

#Region "Stiff problems"

    Private Function MaxError(result As ODEsOut, exact As Func(Of Double, Double)) As Double
        Dim values As Double() = result.y("y").value
        Dim max As Double = 0.0

        For i As Integer = 0 To values.Length - 1
            Dim err As Double = std.Abs(values(i) - exact(result.x(i)))

            If err > max Then
                max = err
            End If
        Next

        Return max
    End Function

    Public Sub RunStiffCases()
        Console.WriteLine()
        Console.WriteLine("--- stiff problem: y' = -1e4(y - cos t) - sin t ---")

        Dim stiff As New StiffModel() With {.rate = -10000.0}
        Dim exact As Func(Of Double, Double) = StiffModel.Exact

        ' ---- 显式方法必须发散（作为对照） ----
        Dim rk4 = stiff.Solve(200, 0.0, 1.0)
        Dim rk4Diverged As Boolean = rk4.HaveNaN OrElse std.Abs(rk4.y("y").value.LastValue()) > 1000.0

        Call Check("rk4 (explicit) diverges on the stiff problem", rk4Diverged,
                   $"last={rk4.y("y").value.LastValue():G4}")

        Call CheckThrows(Of InvalidOperationException)("euler (explicit) throws on the stiff problem",
            Sub()
                Call stiff.SolveEuler(200, 0.0, 1.0)
            End Sub)

        ' ---- 隐式方法必须保持稳定并且保持精度 ----
        Dim backwardEuler = stiff.SolveBackwardEuler(200, 0.0, 1.0)
        Dim trapezoidal = stiff.SolveTrapezoidal(200, 0.0, 1.0)
        Dim bdf2 = stiff.SolveBDF2(200, 0.0, 1.0)
        Dim ros2 = stiff.SolveRosenbrockROS2(200, 0.0, 1.0)

        Call Check("backward euler is stable on the stiff problem", Not backwardEuler.HaveNaN)
        Call CheckClose("backward euler error on the stiff problem", MaxError(backwardEuler, exact), 0.0, 2.0E-2)
        Call CheckClose("trapezoidal error on the stiff problem", MaxError(trapezoidal, exact), 0.0, 5.0E-3)
        Call CheckClose("bdf2 error on the stiff problem", MaxError(bdf2, exact), 0.0, 5.0E-3)
        Call CheckClose("rosenbrock ros2 error on the stiff problem", MaxError(ros2, exact), 0.0, 5.0E-3)

        ' ---- 提供解析 Jacobian 的时候结果需要与差商版本一致 ----
        Dim analytic As New StiffModel With {.rate = -10000.0}
        analytic.Jacobian = Sub(t, y, j)
                                j(0, 0) = analytic.rate
                            End Sub

        Dim withAnalytic = analytic.SolveBDF2(200, 0.0, 1.0)

        Call Check("the analytic jacobian produces the same bdf2 result",
                   withAnalytic.y("y").value.MaxDiff(bdf2.y("y").value) < 1.0E-8,
                   $"max diff={withAnalytic.y("y").value.MaxDiff(bdf2.y("y").value):G4}")

        ' ---- 正常求解的时候不应该出现 Newton 失败 ----
        Dim directModel As New StiffModel With {.rate = -10000.0}
        Dim direct As New BackwardEulerSolver(directModel)

        Call direct.Solve(directModel.GetY0(), 200, 0.0, 1.0)

        Call Check("the newton iteration is converged on the stiff problem", direct.NumberOfNewtonFailures = 0,
                   $"failures={direct.NumberOfNewtonFailures}")
        Call Check("the jacobian is evaluated once per step", direct.NumberOfJacobianEvaluations = 200,
                   $"jacobian={direct.NumberOfJacobianEvaluations}")

        ' ---- 无法收敛的时候必须抛出带有时间点信息的异常，而不是静默返回 NaN ----
        Dim tight As New ODESolverOptions With {
            .MaxNewtonIterations = 1,
            .NewtonTolerance = 1.0E-14,
            .MaxStepSubdivisions = 4
        }

        Call CheckThrows(Of InvalidOperationException)("the diverged newton iteration raises an exception",
            Sub()
                Call stiff.SolveBackwardEuler(50, 0.0, 1.0, options:=tight)
            End Sub)
    End Sub

#End Region

#Region "Rosenbrock stability function"

    Public Sub RunRosenbrockStability()
        Console.WriteLine()
        Console.WriteLine("--- rosenbrock ros2 stability function ---")

        ' R(z) = y_1 / y_0，对于 z 很小的时候应该与 1 + z + z^2/2 一致（二阶）
        Dim smallZ As New DecayModel() With {.rate = -1.0}
        Dim hSmall As Double = 1.0E-4
        Dim smallStep = smallZ.SolveRosenbrockROS2(1, 0.0, hSmall)
        Dim z As Double = -hSmall
        Dim r As Double = smallStep.y("y").value(1) / smallStep.y("y").value(0)

        Call CheckClose("ros2 matches the 2nd order taylor expansion", r, 1.0 + z + 0.5 * z * z, 1.0E-9)

        ' L-稳定：z → -∞ 的时候 R(z) → 0
        Dim hugeZ As New DecayModel() With {.rate = -1.0E8}
        Dim hugeStep = hugeZ.SolveRosenbrockROS2(1, 0.0, 1.0)
        Dim rInf As Double = hugeStep.y("y").value(1) / hugeStep.y("y").value(0)

        Call CheckClose("ros2 is L-stable: R(-inf) = 0", rInf, 0.0, 1.0E-6)

        ' 系数条件自检
        Dim coefficients As Double = RosenbrockRos2Solver.B1 + RosenbrockRos2Solver.B2

        Call CheckClose("ros2 consistency: b1 + b2 = 1", coefficients, 1.0, 1.0E-12)
        Call CheckClose("ros2 order condition: b2·(a21 + c21) = 1/2 - γ",
                        RosenbrockRos2Solver.B2 * (RosenbrockRos2Solver.A21 + RosenbrockRos2Solver.C21),
                        0.5 - RosenbrockRos2Solver.Gamma, 1.0E-12)
    End Sub

#End Region

#Region "Symplectic integrators"

    Private Function EnergyDrift(result As ODEsOut) As (peak As Double, early As Double, late As Double)
        Dim positions As Double() = result.y("x").value
        Dim velocities As Double() = result.y("v").value
        Dim half As Integer = positions.Length \ 2
        Dim peak As Double = 0.0
        Dim early As Double = 0.0
        Dim late As Double = 0.0

        For i As Integer = 0 To positions.Length - 1
            Dim energy As Double = 0.5 * (positions(i) * positions(i) + velocities(i) * velocities(i))
            Dim drift As Double = std.Abs(energy - 0.5)

            If drift > peak Then
                peak = drift
            End If

            If i < half Then
                If drift > early Then
                    early = drift
                End If
            Else
                If drift > late Then
                    late = drift
                End If
            End If
        Next

        Return (peak, early, late)
    End Function

    Public Sub RunSymplecticCases()
        Console.WriteLine()
        Console.WriteLine("--- symplectic integrators ---")

        Dim harmonic As New HarmonicSymplectic()
        Const n As Integer = 2000
        Const range As Double = 200.0

        Console.WriteLine($"    debug: y0={harmonic.GetY0().Length} split={harmonic.SplitSize}")

        Dim verlet = harmonic.SolveVerlet(n, 0.0, range)
        Dim yoshida = harmonic.SolveYoshida4(n, 0.0, range)
        Dim rk4 = harmonic.Solve(n, 0.0, range)

        Dim verletDrift = EnergyDrift(verlet)
        Dim yoshidaDrift = EnergyDrift(yoshida)
        Dim rk4Drift = EnergyDrift(rk4)

        Call Check("verlet keeps the energy bounded", verletDrift.peak < 1.0E-2, $"peak={verletDrift.peak:G4}")
        Call Check("verlet has no secular energy drift",
                   verletDrift.late < verletDrift.early * 1.5 + 1.0E-12,
                   $"early={verletDrift.early:G4} late={verletDrift.late:G4}")
        Call Check("yoshida4 is more accurate than verlet",
                   yoshidaDrift.peak < verletDrift.peak,
                   $"yoshida={yoshidaDrift.peak:G4} verlet={verletDrift.peak:G4}")
        Call Check("yoshida4 keeps the energy bounded", yoshidaDrift.peak < 1.0E-3, $"peak={yoshidaDrift.peak:G4}")
        Call Check("the symplectic methods conserve the energy better than rk4",
                   verletDrift.peak < rk4Drift.peak,
                   $"verlet={verletDrift.peak:G4} rk4={rk4Drift.peak:G4}")

        ' 辛几何求解器只适用于 SymplecticODEs 模型
        Call CheckThrows(Of InvalidOperationException)("verlet rejects a non-symplectic model",
            Sub()
                Call New Cases.LogisticModel().SolveVerlet(10, 0.0, 1.0)
            End Sub)

        ' 模型本身仍然可以使用通用的显式求解器
        Call CheckClose("the symplectic model also works with rk4",
                        rk4.y("x").value.LastValue(), std.Cos(range), 1.0E-2)
    End Sub

#End Region

#Region "Multistep startup"

    Public Sub RunStartupCases()
        Console.WriteLine()
        Console.WriteLine("--- multistep startup ---")

        Dim logistic As New Cases.LogisticModel()

        ' 少于起步步数的时候应该与 RK4 完全一致
        Dim abm4Small = logistic.SolveABM4(4, 0.0, 1.0)
        Dim rk4Small = logistic.Solve(4, 0.0, 1.0)

        Call Check("abm4 falls back to rk4 during the startup steps",
                   abm4Small.y("y").value.MaxDiff(rk4Small.y("y").value) = 0.0)

        ' 起步之后与 RK4 应该高度一致（两者都是四阶）
        Dim abm4 = logistic.SolveABM4(1000, 0.0, 5.0)
        Dim rk4 = logistic.Solve(1000, 0.0, 5.0)

        Call Check("abm4 agrees with rk4",
                   abm4.y("y").value.MaxDiff(rk4.y("y").value) < 1.0E-7,
                   $"max diff={abm4.y("y").value.MaxDiff(rk4.y("y").value):G4}")

        ' BDF2：小步数与梯形法对拍
        Dim bdf2Small = logistic.SolveBDF2(5, 0.0, 0.1)
        Dim trapezoidalSmall = logistic.SolveTrapezoidal(5, 0.0, 0.1)

        Call Check("bdf2 works with a small number of steps",
                   Not bdf2Small.HaveNaN AndAlso bdf2Small.y("y").value.Length = 6)
        Call Check("bdf2 is close to the trapezoidal rule",
                   bdf2Small.y("y").value.MaxDiff(trapezoidalSmall.y("y").value) < 1.0E-3,
                   $"max diff={bdf2Small.y("y").value.MaxDiff(trapezoidalSmall.y("y").value):G4}")
    End Sub

#End Region

#Region "Unified dispatch"

    Public Sub RunDispatchCases()
        Console.WriteLine()
        Console.WriteLine("--- unified solver dispatch ---")

        Dim explicitMethods As ODESolverMethod() = {
            ODESolverMethod.Euler,
            ODESolverMethod.RK2,
            ODESolverMethod.Gill,
            ODESolverMethod.RK4,
            ODESolverMethod.ABM4,
            ODESolverMethod.BackwardEuler,
            ODESolverMethod.Trapezoidal,
            ODESolverMethod.BDF2,
            ODESolverMethod.RosenbrockROS2,
            ODESolverMethod.RK23,
            ODESolverMethod.RK45
        }
        Dim logistic As New Cases.LogisticModel()
        Dim options As New ODESolverOptions With {.MaxSteps = 1000000}

        For Each method As ODESolverMethod In explicitMethods
            Dim result = logistic.Solve(20, 0.0, 2.0, method, options:=options)
            Dim values As Double() = result.y("y").value

            Call Check($"{method}: the sample count is n + 1", result.x.Length = 21, $"length={result.x.Length}")
            Call Check($"{method}: the series length matches", values.Length = 21, $"length={values.Length}")
            Call Check($"{method}: the grid starts at the lower bound", result.x(0) = 0.0, $"x(0)={result.x(0)}")
            Call Check($"{method}: the grid ends at the upper bound", result.x(20) = 2.0, $"x(n)={result.x(20)}")
            Call Check($"{method}: the result is finite", Not result.HaveNaN)
        Next

        Dim symplecticMethods As ODESolverMethod() = {ODESolverMethod.Verlet, ODESolverMethod.Yoshida4}
        Dim harmonic As New HarmonicSymplectic()

        For Each method As ODESolverMethod In symplecticMethods
            Dim result = harmonic.Solve(20, 0.0, 2.0, method)

            Call Check($"{method}: the sample count is n + 1", result.x.Length = 21)
            Call Check($"{method}: the result is finite", Not result.HaveNaN)
        Next

        ' 旧的 Solve(n, a, b, incept:=...) 重载不能被新重载影响
        Dim withIncept = logistic.Solve(10, 0.0, 1.0, incept:=True)

        Call Check("the legacy solve overload is still available", withIncept.x.Length = 11)
    End Sub

#End Region
End Module
