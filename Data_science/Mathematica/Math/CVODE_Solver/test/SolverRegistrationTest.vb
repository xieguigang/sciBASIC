Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Sundials.CVODE
Imports std = System.Math

''' <summary>
''' CVODE 求解器注册到 ODE 求解器框架的集成自检。
''' 
''' ```shell
''' dotnet test.dll --register
''' ```
''' </summary>
Public Module SolverRegistrationTest

    Private ReadOnly failures As New List(Of String)
    Private assertions As Integer

#Region "Test models"

    ''' <summary>
    ''' ``y' = rate * y``，``y(0) = 1``，解析解 ``y = exp(rate * t)``
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
    ''' ``y' = rate * (y - cos t) - sin t``，``y(0) = 1``，解析解恒为 ``y = cos t``
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
    End Class

#End Region

    ''' <summary>
    ''' Run all of the registration self test cases, the return value is the process exit code.
    ''' </summary>
    ''' <returns></returns>
    Public Function Run() As Integer
        Console.WriteLine("=== Sundials.CVODE -> ODEs solver registration self test ===")

        Call CheckUnregistered()
        Call CheckRegister()

        Call CheckNonStiff()
        Call CheckStiff()
        Call CheckOptions()
        Call CheckStepLimit()
        Call CheckJacobianBridge()
        Call CheckUnifiedOutput()
        Call CheckUnregister()

        Console.WriteLine()
        Console.WriteLine($"assertions: {assertions}, failure(s): {failures.Count}")

        If failures.Count = 0 Then
            Console.WriteLine("PASS: 0 failure(s)")
            Return 0
        Else
            For Each message As String In failures
                Console.WriteLine("  failed: " & message)
            Next

            Console.WriteLine($"FAIL: {failures.Count} failure(s)")
            Return 1
        End If
    End Function

#Region "Cases"

    Private Sub CheckUnregistered()
        Console.WriteLine()
        Console.WriteLine("--- unregistered ---")

        Dim model As New DecayModel()

        Call CheckThrows("the unregistered name raises an exception",
            Sub()
                Call model.Solve(100, 0.0, 5.0, "cvode.bdf")
            End Sub)
        Call CheckThrows("the unregistered enumeration raises an exception",
            Sub()
                Call model.Solve(100, 0.0, 5.0, ODESolverMethod.CVODE_BDF)
            End Sub)

        Try
            Call model.Solve(100, 0.0, 5.0, "cvode.bdf")
        Catch ex As InvalidOperationException
            Call Check("the exception message contains the registration hint",
                       ex.Message.Contains("Register()"), ex.Message)
        End Try
    End Sub

    Private Sub CheckRegister()
        Console.WriteLine()
        Console.WriteLine("--- register ---")

        Call SolverExtensions.Register()
        Call SolverExtensions.Register()   ' 幂等：第二次调用不应该抛出异常

        Call Check("the cvode solvers are registered", SolverExtensions.IsRegistered())
        Call Check("the registration names are visible",
                   ODESolverRegistry.RegisteredNames.Contains(SolverExtensions.BdfName) AndAlso
                   ODESolverRegistry.RegisteredNames.Contains(SolverExtensions.AdamsName))
        Call Check("the enumeration values are registered",
                   ODESolverRegistry.IsRegistered(ODESolverMethod.CVODE_Adams) AndAlso
                   ODESolverRegistry.IsRegistered(ODESolverMethod.CVODE_BDF))
    End Sub

    Private Sub CheckNonStiff()
        Console.WriteLine()
        Console.WriteLine("--- non stiff problem: y' = -y ---")

        Dim model As New DecayModel()
        Dim exact As Double = std.Exp(-1.0)
        Dim bdf = model.Solve(20, 0.0, 1.0, ODESolverMethod.CVODE_BDF)

        ' 状态同步需要在其它求解调用之前检查（后续求解会覆盖模型的状态变量）
        Call Check("the output shape", bdf.x.Length = 21 AndAlso bdf.x(0) = 0.0 AndAlso bdf.x(20) = 1.0,
                   $"length={bdf.x.Length}")
        Call Check("the state variable is synchronized", model("y").Value = bdf.y("y").value(20))
        Call Check("the diagnostics info is available",
                   bdf.info IsNot Nothing AndAlso bdf.info.ContainsKey("steps"))

        Dim adams = model.Solve(20, 0.0, 1.0, SolverExtensions.AdamsName)
        Dim [default] = model.Solve(20, 0.0, 1.0, SolverExtensions.DefaultName)

        Call CheckClose("cvode bdf matches the analytic solution", bdf.y("y").value.LastValue(), exact, 1.0E-2)
        Call CheckClose("cvode adams matches the analytic solution", adams.y("y").value.LastValue(), exact, 1.0E-2)
        Call CheckClose("the default cvode solver matches the analytic solution", [default].y("y").value.LastValue(), exact, 1.0E-2)
        Call Check("the string and the enumeration entry are consistent",
                   bdf.y("y").value.MaxDiff(model.Solve(20, 0.0, 1.0, SolverExtensions.BdfName).y("y").value) < 1.0E-9)

        ' 与内建的高分辨率 RK4 对拍（只比较终点值：两者的输出网格并不相同）
        Dim reference = model.Solve(20000, 0.0, 1.0)
        Dim diff As Double = std.Abs(bdf.y("y").value.LastValue() - reference.y("y").value.LastValue())

        Call Check("cvode agrees with the high resolution rk4", diff < 1.0E-2, $"diff={diff:G4}")
    End Sub

    Private Sub CheckStiff()
        Console.WriteLine()
        Console.WriteLine("--- stiff problem: y' = -1e4(y - cos t) - sin t ---")

        Dim model As New StiffModel() With {.rate = -10000.0}
        Dim result = model.Solve(20, 0.0, 0.2, ODESolverMethod.CVODE_BDF)

        Call Check("cvode is stable on the stiff problem", Not result.HaveNaN)

        Dim max As Double = 0.0
        Dim values As Double() = result.y("y").value

        For i As Integer = 0 To values.Length - 1
            Dim err As Double = std.Abs(values(i) - std.Cos(result.x(i)))

            If err > max Then
                max = err
            End If
        Next

        Call CheckClose("cvode error on the stiff problem", max, 0.0, 1.0E-2)
    End Sub

    Private Sub CheckOptions()
        Console.WriteLine()
        Console.WriteLine("--- provider options ---")

        Dim model As New DecayModel()
        Dim exact As Double = std.Exp(-1.0)
        Dim loose = model.Solve(20, 0.0, 1.0, ODESolverMethod.CVODE_BDF,
                                options:=New ODESolverOptions With {
                                    .ProviderOptions = New CVODEOptions With {
                                        .RelativeTolerance = 1.0E-2,
                                        .AbsoluteTolerance = 1.0E-4
                                    }
                                })
        Dim tight = model.Solve(20, 0.0, 1.0, ODESolverMethod.CVODE_BDF,
                                options:=New ODESolverOptions With {
                                    .ProviderOptions = New CVODEOptions With {
                                        .RelativeTolerance = 1.0E-10,
                                        .AbsoluteTolerance = 1.0E-12
                                    }
                                })
        Dim looseError As Double = std.Abs(loose.y("y").value.LastValue() - exact)
        Dim tightError As Double = std.Abs(tight.y("y").value.LastValue() - exact)

        Call Check("the tighter tolerance is not worse", tightError <= looseError * 10 + 1.0E-9,
                   $"loose={looseError:G4} tight={tightError:G4}")
        Call Check("the tight tolerance result is accurate", tightError <= 1.0E-4,
                   $"tight={tightError:G4}")
    End Sub

    Private Sub CheckStepLimit()
        Console.WriteLine()
        Console.WriteLine("--- integration step limit ---")

        Dim model As New DecayModel()

        ' 显式指定一个很小的 MaxSteps：适配器会把它提升到 n（覆盖输出网格），
        ' 但是仍然不足以完成积分，故必须抛出带有时间点信息的异常
        Call CheckThrows("the too small max steps is reported as an exception",
            Sub()
                Call model.Solve(100, 0.0, 1.0, ODESolverMethod.CVODE_BDF,
                                 options:=New ODESolverOptions With {.MaxSteps = 50})
            End Sub)

        Try
            Call model.Solve(100, 0.0, 1.0, ODESolverMethod.CVODE_BDF,
                             options:=New ODESolverOptions With {.MaxSteps = 50})
        Catch ex As InvalidOperationException
            Call Check("the failure message contains the failure time", ex.Message.Contains("failed at t"), ex.Message)
        End Try
    End Sub

    Private Sub CheckJacobianBridge()
        Console.WriteLine()
        Console.WriteLine("--- analytic jacobian bridge ---")

        Dim numeric As New StiffModel() With {.rate = -10000.0}
        Dim analytic As New StiffModel() With {.rate = -10000.0}

        analytic.Jacobian = Sub(t, y, j)
                                j(0, 0) = analytic.rate
                            End Sub

        Dim withNumeric = numeric.Solve(20, 0.0, 0.2, ODESolverMethod.CVODE_BDF)
        Dim withAnalytic = analytic.Solve(20, 0.0, 0.2, ODESolverMethod.CVODE_BDF)

        Call Check("the analytic jacobian produces the same result",
                   withNumeric.y("y").value.MaxDiff(withAnalytic.y("y").value) < 1.0E-8,
                   $"max diff={withNumeric.y("y").value.MaxDiff(withAnalytic.y("y").value):G4}")
    End Sub

    Private Sub CheckUnifiedOutput()
        Console.WriteLine()
        Console.WriteLine("--- unified output (ODEsOut) ---")

        Dim rhs As RHSFunction = Sub(t, y, ydot)
                                     ydot(0) = -y(0)
                                 End Sub
        Dim result As ODEsOut = New CVODEBuilder() _
            .UseMethod(CVODEMethod.Adams) _
            .WithRHS(rhs, 1) _
            .WithRelativeTolerance(1.0E-6) _
            .Solve(0.0, 1.0, New Vector(New Double() {1.0}), 10)

        Call Check("the builder outputs an ODEsOut object", result IsNot Nothing)
        Call Check("the unified output provides the sample count", result.Count = 11, $"count={result.Count}")
        Call Check("the unified output provides the state vector", result(0)(0) = 1.0)
        Call Check("the unified output provides the component series",
                   std.Abs(result.GetComponent(0).LastValue() - std.Exp(-1.0)) < 5.0E-3)
        Call Check("the unified output provides the csv text",
                   result.ToCSV().StartsWith("Time,y0"),
                   result.ToCSV().Substring(0, std.Min(40, result.ToCSV().Length)))
        Call Check("the unified output provides the table view", result.AsTable().rowNames.Length = 11)
    End Sub

    Private Sub CheckUnregister()
        Console.WriteLine()
        Console.WriteLine("--- unregister ---")

        Call SolverExtensions.Unregister()

        Call Check("the cvode solvers are unregistered", Not SolverExtensions.IsRegistered())
        Call CheckThrows("the solver can not be used after the unregister",
            Sub()
                Call New DecayModel().Solve(10, 0.0, 1.0, SolverExtensions.BdfName)
            End Sub)
    End Sub

#End Region

#Region "Assertions"

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = "")
        assertions += 1

        If Not condition Then
            failures.Add(name & " " & detail)
            Console.WriteLine($"  [fail] {name} {detail}")
        End If
    End Sub

    Private Sub CheckClose(name As String, actual As Double, expected As Double, tolerance As Double)
        Dim diff As Double = std.Abs(actual - expected)

        Call Check(name, diff <= tolerance,
                   $"expected={expected:G6} actual={actual:G6} diff={diff:G4} tolerance={tolerance:G4}")
    End Sub

    Private Sub CheckThrows(name As String, action As Action)
        assertions += 1

        Try
            Call action()

            failures.Add(name & ": no exception was thrown")
            Console.WriteLine($"  [fail] {name}: no exception was thrown")
        Catch ex As InvalidOperationException
            ' expected
        Catch ex As Exception
            failures.Add(name & ": " & ex.GetType().Name & ": " & ex.Message)
            Console.WriteLine($"  [fail] {name}: {ex.GetType().Name}")
        End Try
    End Sub

    <Runtime.CompilerServices.Extension>
    Public Function LastValue(v As Double()) As Double
        If v Is Nothing OrElse v.Length = 0 Then
            Return Double.NaN
        Else
            Return v(v.Length - 1)
        End If
    End Function

    <Runtime.CompilerServices.Extension>
    Public Function MaxDiff(a As Double(), b As Double()) As Double
        If a Is Nothing OrElse b Is Nothing Then
            Return Double.NaN
        End If
        If a.Length <> b.Length Then
            Return Double.PositiveInfinity
        End If

        Dim max As Double = 0.0

        For i As Integer = 0 To a.Length - 1
            Dim d As Double = std.Abs(a(i) - b(i))

            If d > max Then
                max = d
            End If
        Next

        Return max
    End Function

#End Region
End Module
