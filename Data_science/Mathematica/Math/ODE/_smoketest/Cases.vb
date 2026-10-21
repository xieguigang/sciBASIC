Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' The analytic solution test cases of the ODE solver package.
''' </summary>
Module Cases

#Region "Models"

    ''' <summary>
    ''' ``dy/dt = r * y * (1 - y / K)``, the analytic solution is
    ''' ``y(t) = K / (1 + (K - y0) / y0 * exp(-r * t))``
    ''' </summary>
    Public Class LogisticModel : Inherits ODEs

        Dim y As var

        Public r As Double = 1.0
        Public K As Double = 1.0

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            Dim yi As Double = y.Value

            dy(y) = r * yi * (1.0 - yi / K)
        End Sub

        Protected Overrides Function y0() As var()
            y.Value = 0.5
            Return {y}
        End Function

        Public ReadOnly Property Analytic(t As Double) As Double
            Get
                Return K / (1.0 + (K - 0.5) / 0.5 * std.Exp(-r * t))
            End Get
        End Property

        Public Function EnableFastPath() As LogisticModel
            Me.FastDerivative = Sub(t, yv, dy)
                                      dy(0) = r * yv(0) * (1.0 - yv(0) / K)
                                  End Sub

            Return Me
        End Function
    End Class

    ''' <summary>
    ''' ``x' = v``, ``v' = -omega^2 * x``, the energy ``0.5 * (v^2 + omega^2 * x^2)``
    ''' should be conserved.
    ''' </summary>
    Public Class HarmonicModel : Inherits ODEs

        Dim x As var
        Dim v As var

        Public omega As Double = 1.0

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            Dim xi As Double = x.Value
            Dim vi As Double = v.Value

            dy(x) = vi
            dy(v) = -omega * omega * xi
        End Sub

        Protected Overrides Function y0() As var()
            x.Value = 1.0
            v.Value = 0.0
            Return {x, v}
        End Function
    End Class

    ''' <summary>
    ''' The Lorenz attractor.
    ''' </summary>
    Public Class LorenzModel : Inherits ODEs

        Dim x As var
        Dim y As var
        Dim z As var

        Public sigma As Double = 10.0
        Public rho As Double = 28.0
        Public beta As Double = 8.0 / 3.0

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            Dim xv As Double = x.Value
            Dim yv As Double = y.Value
            Dim zv As Double = z.Value

            dy(x) = sigma * (yv - xv)
            dy(y) = xv * (rho - zv) - yv
            dy(z) = xv * yv - beta * zv
        End Sub

        Protected Overrides Function y0() As var()
            x.Value = 1.0
            y.Value = 1.5
            z.Value = 1.0
            Return {x, y, z}
        End Function

        Public Function EnableFastPath() As LorenzModel
            Me.FastDerivative = Sub(t, yv, dy)
                                      dy(0) = sigma * (yv(1) - yv(0))
                                      dy(1) = yv(0) * (rho - yv(2)) - yv(1)
                                      dy(2) = yv(0) * yv(1) - beta * yv(2)
                                  End Sub

            Return Me
        End Function
    End Class

    ''' <summary>
    ''' A linear chain system which is built from the <see cref="GenericODEs"/> model.
    ''' </summary>
    ''' <param name="size"></param>
    ''' <param name="fast"></param>
    ''' <returns></returns>
    Public Function CreateChain(size As Integer, fast As Boolean) As GenericODEs
        Dim symbols As var() = New var(size - 1) {}

        For i As Integer = 0 To size - 1
            symbols(i) = New var($"y{i}", 1.0)
        Next

        If fast Then
            Return New GenericODEs(symbols, Sub(t, yv, dy)
                                                For i As Integer = 0 To yv.Length - 1
                                                    Dim prev As Double = If(i = 0, 0.0, yv(i - 1))

                                                    dy(i) = -0.1 * yv(i) + 0.01 * prev
                                                Next
                                            End Sub)
        Else
            Dim derivative As [Function] = Sub(t As Double, ByRef dy As Vector)
                                               For i As Integer = 0 To symbols.Length - 1
                                                   Dim prev As Double = If(i = 0, 0.0, symbols(i - 1).Value)
                                                   Dim current As var = symbols(i)

                                                   dy(current) = -0.1 * current.Value + 0.01 * prev
                                               Next
                                           End Sub

            Return New GenericODEs(symbols, derivative)
        End If
    End Function

    ''' <summary>
    ''' A model which reads the reference data of the current integration step.
    ''' </summary>
    Public Class RefDrivenModel : Inherits RefODEs

        Dim u As var

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector, Y As ValueVector)
            dy(u) = CDbl(Y("y"))
        End Sub

        Protected Overrides Function y0() As var()
            u.Value = 0.0
            Return {u}
        End Function
    End Class

    ''' <summary>
    ''' A model without any state variable.
    ''' </summary>
    Public Class EmptyModel : Inherits ODEs

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        End Sub

        Protected Overrides Function y0() As var()
            Return New var() {}
        End Function
    End Class

#End Region

    ''' <summary>
    ''' Create the test model of ``y' = -y``, ``y(0) = 1``
    ''' </summary>
    ''' <returns></returns>
    Public Function DecayODE() As ODE
        Return New ODE With {
            .ID = "y'=-y",
            .y0 = 1.0,
            .df = Function(t, y) -y
        }
    End Function

#Region "Scalar solvers"

    Public Sub RunScalarCases()
        Console.WriteLine()
        Console.WriteLine("--- scalar ODE solvers ---")

        Dim decay As ODE = DecayODE()
        Dim expected As Double = std.Exp(-5.0)

        ' ---- 输出网格：n + 1 个采样点，并且包含积分区间的两个端点 ----
        Dim rk4 As ODEOutput = decay.RK4(100, 0.0, 5.0)
        Dim grid As Double() = rk4.X.ToArray

        Call Check("grid length is n + 1", grid.Length = 101, $"length={grid.Length}")
        Call Check("grid starts at the lower bound", grid(0) = 0.0, $"grid(0)={grid(0)}")
        Call Check("grid ends exactly at the upper bound", grid(grid.Length - 1) = 5.0, $"last={grid(grid.Length - 1)}")
        Call CheckClose("grid step", grid(1) - grid(0), 0.05, 1.0E-12)
        Call Check("solution length matches the grid", rk4.Y.vector.Length = grid.Length)
        Call CheckClose("rk4 initial value", rk4.Y.vector(0), 1.0, 1.0E-15)

        ' ---- 精度 ----
        Call CheckClose("rk4 exp(-t) at t=5", rk4.Y.vector.LastValue(), expected, 1.0E-8)

        Dim euler As ODEOutput = decay.Eluer(1000, 0.0, 5.0)
        Call CheckClose("euler exp(-t) at t=5", euler.Y.vector.LastValue(), expected, 1.0E-3)

        Dim rk2 As ODEOutput = decay.RK2(100, 0.0, 5.0)
        Call CheckClose("rk2 exp(-t) at t=5", rk2.Y.vector.LastValue(), expected, 1.0E-4)

        ' ---- 欧拉法必须使用 y 而不是 t 来更新 ----
        Dim unitSlope As New ODE With {
            .ID = "y'=1",
            .y0 = 0.0,
            .df = Function(t, y) 1.0
        }
        Dim line As ODEOutput = unitSlope.Eluer(100, 0.0, 5.0)

        Call CheckClose("euler y'=1 lands exactly on y=b", line.Y.vector.LastValue(), 5.0, 1.0E-12)

        ' ---- Gill 方法 ----
        Dim gill As ODEOutput = decay.Gill(100, 0.0, 5.0)

        Call Check("gill grid length", gill.Y.vector.Length = 101, $"length={gill.Y.vector.Length}")
        Call CheckClose("gill exp(-t) at t=5", gill.Y.vector.LastValue(), expected, 1.0E-8)
        Call CheckClose("gill agrees with rk4", gill.Y.vector.LastValue(), rk4.Y.vector.LastValue(), 1.0E-7)

        ' ---- RK4 的收敛阶应该为 4 ----
        Dim err100 As Double = std.Abs(decay.RK4(100, 0.0, 5.0).Y.vector.LastValue() - expected)
        Dim err200 As Double = std.Abs(decay.RK4(200, 0.0, 5.0).Y.vector.LastValue() - expected)
        Dim err400 As Double = std.Abs(decay.RK4(400, 0.0, 5.0).Y.vector.LastValue() - expected)
        Dim ratio1 As Double = err100 / err200
        Dim ratio2 As Double = err200 / err400

        Call Check("rk4 converges as h^4 (100 -> 200)", ratio1 > 12.0 AndAlso ratio1 < 22.0, $"ratio={ratio1:G4}")
        Call Check("rk4 converges as h^4 (200 -> 400)", ratio2 > 12.0 AndAlso ratio2 < 22.0, $"ratio={ratio2:G4}")

        ' ---- 积分区间必须覆盖到上界，否则 CDF 的结果会偏小 ----
        Dim integrand As Func(Of Double, Double) = Function(t) t
        Dim area As Double = integrand.CDF(New DoubleRange(0.0, 1.0), resolution:=100)

        Call CheckClose("cdf integral of t over [0,1]", area, 0.5, 1.0E-9)

        ' ---- ODE 模型的默认属性 ----
        Call CheckClose("ODE default property evaluates df", decay(0.0, 2.0), -2.0, 1.0E-15)

        ' ---- 错误处理 ----
        Call CheckThrows(Of InvalidOperationException)("rk4 requires the derivative function",
            Sub()
                Dim broken As New ODE With {.y0 = 1.0}

                Call broken.RK4(10, 0.0, 1.0)
            End Sub)

        Call CheckThrows(Of ArgumentOutOfRangeException)("rk4 requires positive steps",
            Sub()
                Call decay.RK4(0, 0.0, 1.0)
            End Sub)
    End Sub

#End Region

#Region "Dynamics system"

    Public Sub RunDynamicsCases()
        Console.WriteLine()
        Console.WriteLine("--- ODEs dynamics system ---")

        ' ---- x 与 y 必须严格对齐，并且包含两端点 ----
        Dim logistic As New LogisticModel()
        Dim n As Integer = 1000
        Dim result As ODEsOut = logistic.Solve(n, 0.0, 5.0)
        Dim xValues As Double() = result.x
        Dim yValues As Double() = result.y("y").value

        Call Check("dynamics grid length", xValues.Length = n + 1, $"length={xValues.Length}")
        Call Check("dynamics series length", yValues.Length = n + 1, $"length={yValues.Length}")
        Call Check("dynamics grid starts at a", xValues(0) = 0.0, $"x(0)={xValues(0)}")
        Call Check("dynamics grid ends at b", xValues(xValues.Length - 1) = 5.0, $"last={xValues(xValues.Length - 1)}")
        Call CheckClose("dynamics series starts with y0", yValues(0), 0.5, 1.0E-15)
        Call CheckClose("dynamics step size", result.dx, 0.005, 1.0E-15)
        Call CheckClose("dynamics resolution", result.Resolution, n, 1.0E-9)

        ' ---- 精度：与解析解比较 ----
        Call CheckClose("logistic rk4 matches the analytic solution", yValues.LastValue(), logistic.Analytic(5.0), 1.0E-10)
        Call CheckClose("state variable is synchronized", logistic("y").Value, yValues.LastValue(), 1.0E-15)
        Call CheckClose("integrated y0 is recorded", result.y0("y"), 0.5, 1.0E-15)

        ' ---- 参数反射 ----
        Dim parameters = logistic.Parameters

        Call Check("double fields are detected as parameters",
                   parameters.ContainsKey("r") AndAlso parameters.ContainsKey("K"),
                   $"keys={String.Join(",", parameters.Keys)}")
        Call Check("state variable is excluded from parameters", Not parameters.ContainsKey("y"))
        Call CheckClose("parameter value", parameters("r"), 1.0, 1.0E-15)

        ' ---- 无 var 字段的模型 ----
        Dim empty As New EmptyModel()

        Call Check("empty model has no state variable", empty.GetY0().Length = 0)
        Call CheckThrows(Of ArgumentOutOfRangeException)("empty model cannot be integrated",
            Sub()
                Call empty.Solve(10, 0.0, 1.0)
            End Sub)

        ' ---- 谐振子的能量守恒 ----
        Dim harmonic As New HarmonicModel()
        Dim harmonicResult = harmonic.Solve(2000, 0.0, 20.0)
        Dim positions As Double() = harmonicResult.y("x").value
        Dim velocities As Double() = harmonicResult.y("v").value
        Dim maxDrift As Double = 0.0

        For i As Integer = 0 To positions.Length - 1
            Dim energy As Double = 0.5 * (positions(i) * positions(i) + velocities(i) * velocities(i))
            Dim drift As Double = std.Abs(energy - 0.5)

            If drift > maxDrift Then
                maxDrift = drift
            End If
        Next

        Call Check("harmonic oscillator energy conservation", maxDrift < 1.0E-9, $"max drift={maxDrift:G4}")
        Call CheckClose("harmonic oscillator cos(t) at t=20", positions.LastValue(), std.Cos(20.0), 1.0E-7)

        ' ---- Lorenz 系统的确定性与有限性 ----
        Dim lorenz As New LorenzModel()
        Dim lorenz1 = lorenz.Solve(2000, 0.0, 2.0)
        Dim lorenz2 = lorenz.Solve(2000, 0.0, 2.0)

        Call Check("lorenz is deterministic", lorenz1.y("x").value.MaxDiff(lorenz2.y("x").value) = 0.0)
        Call Check("lorenz result is finite", Not lorenz1.HaveNaN)
        Call Check("lorenz has three output series", lorenz1.y.Count = 3, $"count={lorenz1.y.Count}")

        ' ---- 数值表转换 ----
        Dim table = lorenz1.AsTable()

        Call Check("AsTable sample count", table.rowNames.Length = 2001, $"count={table.rowNames.Length}")
        Call Check("AsTable feature count", table.featureNames.Length = 3, $"count={table.featureNames.Length}")
        Call CheckClose("AsTable cell value", table.features(2000)(0), lorenz1.y("x").value(2000), 1.0E-15)

        ' ---- 数组式直通路径与 var 路径必须完全一致 ----
        Dim slowModel As New LogisticModel()
        Dim fastModel As New LogisticModel()

        Call fastModel.EnableFastPath()
        Dim slowResult = slowModel.Solve(500, 0.0, 5.0)
        Dim fastResult = fastModel.Solve(500, 0.0, 5.0)

        Call Check("fast derivative path matches the var path",
                   slowResult.y("y").value.MaxDiff(fastResult.y("y").value) < 1.0E-15,
                   $"max diff={slowResult.y("y").value.MaxDiff(fastResult.y("y").value):G4}")

        Dim chainSlow = CreateChain(3, fast:=False)
        Dim chainFast = CreateChain(3, fast:=True)
        Dim chainSlowResult = chainSlow.Solve(100, 0.0, 1.0)
        Dim chainFastResult = chainFast.Solve(100, 0.0, 1.0)

        Call Check("generic ODEs fast path matches",
                   chainSlowResult.y("y0").value.MaxDiff(chainFastResult.y("y0").value) < 1.0E-15)

        ' ---- RefODEs：参考数据的定位与积分步索引解耦 ----
        Dim reference = logistic.Solve(200, 0.0, 2.0)
        Dim driven As New RefDrivenModel With {
            .RefValues = New ValueVector With {
                .Y = reference.y
            }
        }
        Dim drivenResult = driven.Solve(200, 0.0, 2.0)
        Dim referenceValues As Double() = reference.y("y").value
        Dim integral As Double = 0.0

        For i As Integer = 0 To referenceValues.Length - 2
            integral += 0.5 * (referenceValues(i) + referenceValues(i + 1)) * reference.dx
        Next

        Call Check("RefODEs series length", drivenResult.y("u").value.Length = 201, $"length={drivenResult.y("u").value.Length}")
        Call CheckClose("RefODEs integrates the reference data", drivenResult.y("u").value.LastValue(), integral, 1.0E-2)
    End Sub

#End Region

#Region "Adaptive solver"

    Public Sub RunAdaptiveCases()
        Console.WriteLine()
        Console.WriteLine("--- adaptive step size solver (RK45) ---")

        Dim decay As ODE = DecayODE()
        Dim expected As Double = std.Exp(-5.0)
        Dim steps As Integer = 0
        Dim evaluations As Integer = 0

        Dim rk45 As ODEOutput = decay.RK45(100, 0.0, 5.0,
                                           atol:=1.0E-10, rtol:=1.0E-8,
                                           numberOfSteps:=steps,
                                           numberOfEvaluations:=evaluations)

        Call Check("rk45 grid length", rk45.Y.vector.Length = 101, $"length={rk45.Y.vector.Length}")
        Call CheckClose("rk45 exp(-t) at t=5", rk45.Y.vector.LastValue(), expected, 1.0E-8)
        Call Check("rk45 uses fewer steps than the output grid", steps < 100, $"steps={steps}")
        Call Check("rk45 evaluations is 6 * steps + 1", evaluations = 6 * steps + 1, $"evaluations={evaluations} steps={steps}")

        ' ---- 方程组路径 ----
        Dim logistic As New LogisticModel()
        Dim adaptive = logistic.SolveAdaptive(100, 0.0, 5.0, atol:=1.0E-10, rtol:=1.0E-8)

        Call CheckClose("solve adaptive logistic", adaptive.y("y").value.LastValue(), logistic.Analytic(5.0), 1.0E-7)
        Call Check("solve adaptive series length", adaptive.y("y").value.Length = 101)

        ' ---- 容差越紧，步数越多 ----
        Dim looseSteps As Integer = 0
        Dim tightSteps As Integer = 0

        Call decay.RK45(100, 0.0, 5.0, atol:=1.0E-3, rtol:=1.0E-3, numberOfSteps:=looseSteps)
        Call decay.RK45(100, 0.0, 5.0, atol:=1.0E-12, rtol:=1.0E-12, numberOfSteps:=tightSteps)

        Call Check("tighter tolerance requires more steps", looseSteps < tightSteps, $"loose={looseSteps} tight={tightSteps}")

        ' ---- Lorenz：自适应结果需要与高分辨率的固定步长结果一致 ----
        Dim lorenz As New LorenzModel()
        Dim refined = lorenz.Solve(20000, 0.0, 2.0)
        Dim adaptiveLorenz = lorenz.SolveAdaptive(1000, 0.0, 2.0, atol:=1.0E-10, rtol:=1.0E-8)

        For Each varName As String In New String() {"x", "y", "z"}
            Call CheckClose($"lorenz rk45 matches the refined rk4 [{varName}]",
                            adaptiveLorenz.y(varName).value.LastValue(),
                            refined.y(varName).value.LastValue(),
                            1.0E-6)
        Next

        ' ---- 快瞬态：y' = -50 * (y - cos t) - sin t，解析解为 y = cos t ----
        Dim transient As New ODE With {
            .ID = "fast transient",
            .y0 = 1.0,
            .df = Function(t, y) -50.0 * (y - std.Cos(t)) - std.Sin(t)
        }
        Dim transientResult = transient.RK45(200, 0.0, 3.0, atol:=1.0E-10, rtol:=1.0E-8)

        Call CheckClose("rk45 fast transient cos(t) at t=3", transientResult.Y.vector.LastValue(), std.Cos(3.0), 1.0E-6)

        ' ---- 错误处理 ----
        Call CheckThrows(Of InvalidOperationException)("rk45 respects maxSteps",
            Sub()
                Call decay.RK45(100, 0.0, 5.0, maxSteps:=3)
            End Sub)

        Call CheckThrows(Of ArgumentOutOfRangeException)("rk45 rejects an empty range",
            Sub()
                Call decay.RK45(100, 1.0, 1.0)
            End Sub)
    End Sub

#End Region

#Region "Robustness"

    Public Sub RunRobustnessCases()
        Console.WriteLine()
        Console.WriteLine("--- robustness ---")

        ' ---- ODEsOut 的长度守卫 ----
        Dim shortSeries As New ODEsOut With {
            .x = New Double() {1.0}
        }

        Call Check("dx guard for the short series", shortSeries.dx = 0.0)
        Call Check("resolution guard for the short series", shortSeries.Resolution = 0.0)

        ' ---- SolverIterator 在结束之后不再静默空转 ----
        Dim logistic As New LogisticModel()
        Dim solver As New RungeKutta4(logistic)
        Dim iterator As SolverIterator = New SolverIterator(solver).Config(logistic.GetY0(), 20, 0.0, 1.0)
        Dim triggerCount As Integer = 0

        Call iterator.Bind(Sub() triggerCount += 1)

        For i As Integer = 1 To 30
            Call iterator.Tick()
        Next

        Call Check("solver iterator is finished", iterator.Finished)
        Call Check("solver iterator stops triggering after the end", triggerCount = 21, $"count={triggerCount}")

        ' ---- 导数求值 API ----
        Dim derivatives As Double() = New Double(0) {}
        Call logistic.Derivatives(0.0, New Double() {0.5}, derivatives)

        Call CheckClose("derivatives api", derivatives(0), 0.25, 1.0E-15)
        Call CheckThrows(Of ArgumentException)("derivatives api validates the state dimension",
            Sub()
                Call logistic.Derivatives(0.0, New Double() {0.5, 1.0}, New Double() {0.0, 0.0})
            End Sub)

        ' ---- 参数名称与变量名称的反射缓存 ----
        Dim variableNames = ODEs.GetVariables(GetType(LorenzModel)).ToArray
        Dim parameterNames = ODEs.GetParameters(GetType(LogisticModel)).ToArray

        Call Check("reflection variable names", variableNames.Length = 3, $"count={variableNames.Length}")
        Call Check("reflection parameter names", parameterNames.Length = 2, $"count={parameterNames.Length}")
    End Sub

#End Region
End Module
