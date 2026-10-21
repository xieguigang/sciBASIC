Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Emit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ``y`` 声明的类型为<see cref="var"/>类型的域;
''' ``parameter`` 声明的类型为<see cref="Double"/>类型的域
''' </summary>
Public MustInherit Class Model : Inherits ODEs

    Sub New()
        Call MyBase.New()
    End Sub

    Protected Sub New(vars As var())
        Call MyBase.New(vars)
    End Sub

    Protected Overrides Function y0() As var()
        Return {}
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="estimates">新的方程参数，这里面需要同时包括了参数和y初始值</param>
    ''' <param name="n%"></param>
    ''' <param name="a%"></param>
    ''' <param name="b%"></param>
    ''' <returns></returns>
    ''' <remarks>线程不安全的</remarks>
    Public Function RunTest(estimates As Dictionary(Of String, Double), n%, a%, b%) As ODEsOut
        Dim model As Type = Me.GetType()
        'Dim parms$() = ODEs.GetParameters(model).ToArray
        'Dim vars$() = ODEs.GetVariables(model).ToArray

        'For Each var$ In vars
        '    Me(var).value = estimates(var)
        'Next

        'For Each parm$ In parms
        '    Dim [set] As Action(Of Object, Double) =
        '        Delegates.FieldSet(Of Double)(model, parm)
        '    Call [set](Me, estimates(parm))
        'Next

        'Return Me.Solve(n, a, b, incept:=True)
        Return RunTest(model, estimates, estimates, n, a, b) ' y0也被包含在estimates之中，所以传递两个
    End Function

    ''' <summary>
    ''' 这个函数是为并行化而设计的，线程安全的
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="estimates">新的方程参数，这里面需要同时包括了参数和y初始值</param>
    ''' <param name="n%"></param>
    ''' <param name="a%"></param>
    ''' <param name="b%"></param>
    ''' <returns></returns>
    Public Shared Function RunTest(model As Type,
                                   y0 As Dictionary(Of String, Double),
                                   estimates As Dictionary(Of String, Double),
                                   n%, a%, b%,
                                   Optional ref As ODEsOut = Nothing) As ODEsOut

        Dim parms$() = ODEs.GetParameters(model).ToArray
        Dim vars$() = ODEs.GetVariables(model).ToArray
        Dim x As Model = TryCast(Activator.CreateInstance(model), Model)
        Dim var$ = Nothing
        Dim parm$ = Nothing

        Try
            For Each var$ In vars    ' 设置初始值
                x(var).value = y0(var)
            Next
        Catch ex As Exception
            Dim msg$ = $"Model required a parameter which is named ``{var}``, but '{var}' is not exists in list: {y0.KeysJson}"
            ex = New Exception(msg, ex)
            Throw ex
        End Try

        Try
            For Each parm$ In parms  ' 设置参数值
                Dim [set] As Action(Of Object, Double) =
                    Delegates.FieldSet(Of Double)(model, parm)
                Call [set](x, estimates(parm))
            Next
        Catch ex As Exception
            Dim msg$ = $"Model required a parameter which is named ``{parm}``, but '{parm}' is not exists in list: {estimates.KeysJson}"
            ex = New Exception(msg, ex)
            Throw ex
        End Try

        If Not ref Is Nothing Then
            Dim partTokens As Integer = n / ref.x.Length

        End If

        Return x.Solve(n, a, b, incept:=True)
    End Function

    ''' <summary>
    ''' 这个函数是为并行化而设计的，线程安全的
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="estimates">新的方程参数，这里面需要同时包括了参数和y初始值</param>
    ''' <param name="n%"></param>
    ''' <param name="a%"></param>
    ''' <param name="b%"></param>
    ''' <returns></returns>
    Public Shared Function RunTest(model As Type,
                                   y0 As Dictionary(Of String, Double),
                                   estimates As var(),
                                   n%, a%, b%,
                                   Optional ref As ODEsOut = Nothing) As ODEsOut

        Dim args As Dictionary(Of String, Double) =
            estimates _
            .ToDictionary(Function(x) x.Name,
                          Function(x) x.value)
        Return RunTest(model, y0, args, n, a, b, ref)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="estimates"></param>
    ''' <param name="n%"></param>
    ''' <param name="a%"></param>
    ''' <param name="b%"></param>
    ''' <param name="modify">修改部分数据</param>
    ''' <returns></returns>
    Public Function RunTest(estimates As Dictionary(Of String, Double)(),
                            n%, a%, b%,
                            Optional modify As Dictionary(Of String, Double) = Nothing) As ODEsOut

        Dim parms As New Dictionary(Of String, Double)

        For Each var$ In estimates(Scan0%).Keys
            Dim dist = estimates.Select(Function(x) x(var$)).Distributes
            Dim most As DoubleTagged(Of Integer) =
                LinqAPI.DefaultFirst(Of DoubleTagged(Of Integer)) <= From x As DoubleTagged(Of Integer)
                                                                     In dist.Values
                                                                     Select x
                                                                     Order By x.value Descending
            parms(var$) = most.Tag
        Next

        If Not modify.IsNullOrEmpty Then
            For Each var$ In modify.Keys
                parms(var$) = modify(var$)
            Next
        End If

        Return RunTest(parms, n, a, b)
    End Function
End Class