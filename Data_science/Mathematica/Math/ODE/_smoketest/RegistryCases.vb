Imports System.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports std = System.Math

''' <summary>
''' The test cases of the external solver registry and the unified output model.
''' (外部求解器注册表与统一输出模型的自检)
''' </summary>
Module RegistryCases

#Region "Test providers"

    ''' <summary>
    ''' 用显式 Euler 实现的测试求解器：它的结果需要与内建的 ``SolveEuler`` 完全一致
    ''' </summary>
    Public Class EulerProvider : Implements IODEsolverProvider

        Public ReadOnly Property Name As String Implements IODEsolverProvider.Name
        Public ReadOnly Property Description As String Implements IODEsolverProvider.Description

        Public Sub New(Optional name As String = "test.euler")
            Me.Name = name
            Me.Description = "The explicit Euler provider which is used by the self test."
        End Sub

        Public Function Solve(model As ODEs, y0 As Double(), n As Integer, a As Double, b As Double,
                              options As ODESolverOptions) As ODEsOut Implements IODEsolverProvider.Solve

            Dim grid As Double() = ODESequence.CreatePoints(a, b, n)
            Dim size As Integer = y0.Length
            Dim columns As Double()() = New Double(size - 1)() {}
            Dim y As Double() = CType(y0.Clone(), Double())
            Dim k As Double() = New Double(size - 1) {}
            Dim h As Double = (b - a) / n

            For i As Integer = 0 To size - 1
                columns(i) = New Double(n) {}
                columns(i)(0) = y0(i)
            Next

            For iStep As Integer = 1 To n
                Call model.Derivatives(a + h * (iStep - 1), y, k)

                For i As Integer = 0 To size - 1
                    y(i) += h * k(i)
                    columns(i)(iStep) = y(i)
                Next
            Next

            Dim names As String() = ODEs.GetVariables(model.GetType()).ToArray

            Return ODEsOut.FromColumns(grid, columns, names)
        End Function
    End Class

    ''' <summary>
    ''' 故意返回错误结果的求解器：用于验证框架侧的校验逻辑
    ''' </summary>
    Public Class BrokenProvider : Implements IODEsolverProvider

        Private ReadOnly kind As String

        Public ReadOnly Property Name As String Implements IODEsolverProvider.Name
        Public ReadOnly Property Description As String Implements IODEsolverProvider.Description

        ''' <param name="kind">columns / grid / nan / endpoint</param>
        Public Sub New(kind As String)
            Me.kind = kind
            Me.Name = $"broken.{kind}"
            Me.Description = $"The broken provider ({kind}) which is used by the self test."
        End Sub

        Public Function Solve(model As ODEs, y0 As Double(), n As Integer, a As Double, b As Double,
                              options As ODESolverOptions) As ODEsOut Implements IODEsolverProvider.Solve

            Dim size As Integer = y0.Length
            Dim grid As Double() = ODESequence.CreatePoints(a, b, n)
            Dim columns As Double()() = New Double(size - 1)() {}

            For i As Integer = 0 To size - 1
                columns(i) = New Double(n) {}
            Next

            Select Case kind
                Case "columns"
                    ' 少一列
                    Dim one As Double()() = New Double()() {columns(0)}

                    Return ODEsOut.FromColumns(grid, one)
                Case "grid"
                    ' 时间网格长度不对
                    Dim longer As Double() = ODESequence.CreatePoints(a, b + 1, n)
                    Dim longerColumns As Double()() = New Double()() {New Double(n) {}}

                    Return ODEsOut.FromColumns(longer, longerColumns)
                Case "nan"
                    For i As Integer = 0 To size - 1
                        For j As Integer = 0 To n
                            columns(i)(j) = Double.NaN
                        Next
                    Next
                Case "endpoint"
                    For j As Integer = 0 To n
                        grid(j) = grid(j) + 0.5
                    Next
            End Select

            Return ODEsOut.FromColumns(grid, columns)
        End Function
    End Class

    ''' <summary>
    ''' 返回固定值并且记录 ``ProviderOptions`` 的求解器
    ''' </summary>
    Public Class ProbeProvider : Implements IODEsolverProvider

        Public ReadOnly Property Name As String Implements IODEsolverProvider.Name
        Public ReadOnly Property Description As String Implements IODEsolverProvider.Description

        ''' <summary>
        ''' 最近一次调用所接收到的 <see cref="ODESolverOptions.ProviderOptions"/> 引用
        ''' </summary>
        Public Property LastProviderOptions As Object

        Public Sub New(Optional name As String = "test.probe")
            Me.Name = name
            Me.Description = "The probe provider which is used by the self test."
        End Sub

        Public Function Solve(model As ODEs, y0 As Double(), n As Integer, a As Double, b As Double,
                              options As ODESolverOptions) As ODEsOut Implements IODEsolverProvider.Solve

            LastProviderOptions = If(options, New ODESolverOptions()).ProviderOptions

            Dim grid As Double() = ODESequence.CreatePoints(a, b, n)
            Dim columns As Double()() = New Double(y0.Length - 1)() {}

            For i As Integer = 0 To y0.Length - 1
                columns(i) = New Double(n) {}

                For j As Integer = 0 To n
                    columns(i)(j) = 42.0
                Next
            Next

            Return ODEsOut.FromColumns(grid, columns)
        End Function
    End Class

#End Region

    Public Sub RunAll()
        Console.WriteLine()
        Console.WriteLine("--- unified output model (ODEsOut) ---")
        Call RunOutputModelCases()

        Console.WriteLine()
        Console.WriteLine("--- external solver registry ---")
        Call RunRegistryCases()

        ' 清理：避免影响其他用例
        Call ODESolverRegistry.Clear()
    End Sub

    Private Sub RunOutputModelCases()
        Dim times As Double() = {0.0, 1.0, 2.0}
        Dim c1 As Double() = {1.0, 2.0, 3.0}
        Dim c2 As Double() = {4.0, 5.0, 6.0}
        Dim result As ODEsOut = ODEsOut.FromColumns(times, {c1, c2})

        Call Check("from columns: the sample count", result.Count = 3, $"count={result.Count}")
        Call Check("from columns: the default name", result.columnNames.SequenceEqual({"y0", "y1"}))
        Call Check("from columns: the initial values", result.y0("y0") = 1.0 AndAlso result.y0("y1") = 4.0)
        Call Check("from columns: the state vector of a sample point",
                   result(1)(0) = 2.0 AndAlso result(1)(1) = 5.0)
        Call Check("from columns: the component series", result.GetComponent(1).MaxDiff(c2) = 0.0)

        ' 列顺序必须与 columnNames 一致：y10 的字典序会被排在 y2 之前，但是这里必须按序号取列
        Dim size As Integer = 12
        Dim names As String() = New String(size - 1) {}
        Dim columns As Double()() = New Double(size - 1)() {}

        For i As Integer = 0 To size - 1
            names(i) = $"y{i}"
            columns(i) = New Double() {CDbl(i), CDbl(i) * 2}
        Next

        Dim ordered As ODEsOut = ODEsOut.FromColumns(New Double() {0.0, 1.0}, columns, names)

        Call Check("column order is resolved by columnNames, not by the dictionary order",
                   ordered.GetComponent(2)(0) = 2.0 AndAlso ordered.GetComponent(10)(0) = 10.0,
                   $"c2={ordered.GetComponent(2)(0)} c10={ordered.GetComponent(10)(0)}")
        Call Check("the item indexer follows the column order", ordered(0)(2) = 2.0)

        ' ToCSV 与旧的 ODESolution.ToCSV 保持一致
        Dim csv As String = ODEsOut.FromColumns(New Double() {0.0, 1.0},
                                                {New Double() {1.0, 2.0}, New Double() {3.0, 4.0}}).ToCSV()
        Dim expected As String = "Time,y0,y1" & vbCrLf & "0,1,3" & vbCrLf & "1,2,4" & vbCrLf

        Call Check("to csv is compatible with the legacy ODESolution", csv = expected, csv.Replace(vbCrLf, "\n"))

        Call Check("the table view is available", result.AsTable().rowNames.Length = 3)
    End Sub

    Private Sub RunRegistryCases()
        Dim model As New Cases.LogisticModel()
        Dim euler As ODEsOut = model.SolveEuler(50, 0.0, 1.0)

        ' ---- 字符串入口 ----
        Call ODESolverRegistry.Register("test.euler", New EulerProvider("test.euler"))

        Dim registered As ODEsOut = model.Solve(50, 0.0, 1.0, "test.euler")

        Call Check("the registered provider produces the same result as the built-in euler",
                   registered.y("y").value.MaxDiff(euler.y("y").value) = 0.0,
                   $"max diff={registered.y("y").value.MaxDiff(euler.y("y").value):G4}")
        Call Check("the registered solver output shape", registered.x.Length = 51 AndAlso registered.x(50) = 1.0)
        Call Check("the state variable is synchronized", model("y").Value = registered.y("y").value(50))
        Call Check("is registered", ODESolverRegistry.IsRegistered("test.euler"))
        Call Check("registered names contains the provider",
                   ODESolverRegistry.RegisteredNames.Contains("test.euler"))

        ' ---- 枚举入口（使用 CVODE_Adams 这个枚举值，此处无需引用 CVODE） ----
        Call ODESolverRegistry.Register(ODESolverMethod.CVODE_Adams, New EulerProvider("test.enum"))

        Dim byMethod As ODEsOut = model.Solve(50, 0.0, 1.0, ODESolverMethod.CVODE_Adams)

        Call Check("the enumeration dispatch is resolved by the registry",
                   byMethod.y("y").value.MaxDiff(euler.y("y").value) = 0.0)

        ' ---- 未注册 ----
        Call CheckThrows(Of InvalidOperationException)("the unregistered name raises an exception",
            Sub()
                Call model.Solve(10, 0.0, 1.0, "no.such.solver")
            End Sub)
        Call CheckThrows(Of InvalidOperationException)("the unregistered enumeration raises an exception",
            Sub()
                Call model.Solve(10, 0.0, 1.0, ODESolverMethod.CVODE_BDF)
            End Sub)

        Try
            Call model.Solve(10, 0.0, 1.0, "no.such.solver")
        Catch ex As InvalidOperationException
            Call Check("the unregistered message contains the registration hint",
                       ex.Message.Contains("not registered") AndAlso ex.Message.Contains("Register()"),
                       ex.Message)
        End Try

        ' ---- 重复注册与覆盖 ----
        Call CheckThrows(Of InvalidOperationException)("the duplicated registration raises an exception",
            Sub()
                Call ODESolverRegistry.Register("test.euler", New EulerProvider("test.euler"))
            End Sub)

        Call ODESolverRegistry.Register("test.euler", New ProbeProvider("test.euler"), replace:=True)

        Dim replaced As ODEsOut = model.Solve(10, 0.0, 1.0, "test.euler")

        Call Check("the replace option overrides the registration", replaced.y("y").value(0) = 42.0)

        ' ---- ProviderOptions 透传 ----
        Dim probe As New ProbeProvider("test.probe")
        Dim providerOptions As New Object()
        Dim options As New ODESolverOptions With {.ProviderOptions = providerOptions}

        Call ODESolverRegistry.Register("test.probe", probe)
        Call model.Solve(10, 0.0, 1.0, "test.probe", options:=options)

        Call Check("the provider options are passed through", probe.LastProviderOptions Is providerOptions)

        ' ---- 内建方法的字符串名 ----
        Dim rk4ByName As ODEsOut = model.Solve(100, 0.0, 5.0, "RK4")
        Dim rk4 As ODEsOut = model.Solve(100, 0.0, 5.0)

        Call Check("the built-in solver can be invoked by its enumeration name",
                   rk4ByName.y("y").value.MaxDiff(rk4.y("y").value) = 0.0)

        ' ---- 结果校验（使用 2 个状态变量的模型，才能检测到列数不一致的情况） ----
        Dim multi As New Cases.HarmonicModel()

        For Each kind As String In New String() {"columns", "grid", "nan", "endpoint"}
            Dim provider As New BrokenProvider(kind)

            Call ODESolverRegistry.Register(provider.Name, provider)
            Call CheckThrows(Of InvalidOperationException)($"the broken provider ({kind}) is rejected",
                Sub()
                    Call multi.Solve(20, 0.0, 1.0, provider.Name)
                End Sub)
        Next

        ' ---- 注销 ----
        Call Check("unregister removes the string registration", ODESolverRegistry.Unregister("test.euler"))
        Call Check("the removed solver is not registered anymore", Not ODESolverRegistry.IsRegistered("test.euler"))
        Call CheckThrows(Of InvalidOperationException)("the solver can not be used after the unregister",
            Sub()
                Call model.Solve(10, 0.0, 1.0, "test.euler")
            End Sub)
        Call Check("unregister removes the enumeration registration",
                   ODESolverRegistry.Unregister(ODESolverMethod.CVODE_Adams))
        Call Check("the removed enumeration is not registered anymore",
                   Not ODESolverRegistry.IsRegistered(ODESolverMethod.CVODE_Adams))
    End Sub
End Module
