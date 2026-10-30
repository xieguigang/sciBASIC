Imports Microsoft.VisualBasic.Math.Calculus.Dynamics

''' <summary>
''' 把托管版的 CVODE 求解器注册到 ODE 求解器框架的注册表之中，之后就可以像内建的
''' RK4 / RK45 那样调用它：
''' 
''' ```vbnet
''' Call SolverExtensions.Register()
''' 
''' Dim result1 = model.Solve(1000, 0, 5, ODESolverMethod.CVODE_BDF)
''' Dim result2 = model.Solve(1000, 0, 5, "cvode.adams")
''' ```
''' </summary>
''' <remarks>
''' 注册是**显式**的：只有在调用过 <see cref="Register"/> 之后，``ODESolverMethod.CVODE_Adams``、
''' ``ODESolverMethod.CVODE_BDF`` 以及 ``"cvode"`` / ``"cvode.adams"`` / ``"cvode.bdf"`` 这些名称
''' 才会被 ODEs 的求解分发入口识别；未注册时调用这些方法会抛出带有注册指引的
''' ``InvalidOperationException``。
''' </remarks>
Public Module SolverExtensions

    ''' <summary>
    ''' The registration name of the CVODE Adams (non-stiff) integrator.
    ''' </summary>
    Public Const AdamsName As String = "cvode.adams"

    ''' <summary>
    ''' The registration name of the CVODE BDF (stiff) integrator.
    ''' </summary>
    Public Const BdfName As String = "cvode.bdf"

    ''' <summary>
    ''' The default registration name of the CVODE solver, it is equivalent to the BDF method
    ''' which is suitable for the stiff problems.
    ''' (默认的注册名，等价于适合刚性问题的 BDF 方法)
    ''' </summary>
    Public Const DefaultName As String = "cvode"

    ''' <summary>
    ''' Register the CVODE integrators into the ODEs solver framework, this method is safe to be
    ''' invoked multiple times.
    ''' (把 CVODE 求解器注册到 ODEs 求解框架，可以重复调用)
    ''' </summary>
    ''' <param name="replace">
    ''' Should the existed registrations be overridden? When this value is ``False`` the
    ''' registration is skipped if the solver is already registered.
    ''' </param>
    Public Sub Register(Optional replace As Boolean = False)
        Call Ensure(AdamsName, New CvodeSolverProvider(CVODEMethod.Adams, AdamsName), replace)
        Call Ensure(BdfName, New CvodeSolverProvider(CVODEMethod.BDF, BdfName), replace)
        Call Ensure(DefaultName, New CvodeSolverProvider(CVODEMethod.BDF, DefaultName), replace)

        ' 枚举入口：与字符串入口共用同一套实现
        Call Ensure(ODESolverMethod.CVODE_Adams, New CvodeSolverProvider(CVODEMethod.Adams, AdamsName), replace)
        Call Ensure(ODESolverMethod.CVODE_BDF, New CvodeSolverProvider(CVODEMethod.BDF, BdfName), replace)
    End Sub

    ''' <summary>
    ''' Remove all of the CVODE registrations from the ODEs solver framework, this method is
    ''' useful for the unit test and the hot switching scenario.
    ''' (从 ODEs 求解框架中移除 CVODE 的注册)
    ''' </summary>
    Public Sub Unregister()
        Call ODESolverRegistry.Unregister(AdamsName)
        Call ODESolverRegistry.Unregister(BdfName)
        Call ODESolverRegistry.Unregister(DefaultName)
        Call ODESolverRegistry.Unregister(ODESolverMethod.CVODE_Adams)
        Call ODESolverRegistry.Unregister(ODESolverMethod.CVODE_BDF)
    End Sub

    ''' <summary>
    ''' Is the CVODE integrators are registered already?
    ''' </summary>
    ''' <returns></returns>
    Public Function IsRegistered() As Boolean
        Return ODESolverRegistry.IsRegistered(AdamsName) AndAlso
            ODESolverRegistry.IsRegistered(BdfName) AndAlso
            ODESolverRegistry.IsRegistered(ODESolverMethod.CVODE_Adams) AndAlso
            ODESolverRegistry.IsRegistered(ODESolverMethod.CVODE_BDF)
    End Function

    Private Sub Ensure(name As String, provider As IODEsolverProvider, replace As Boolean)
        If Not replace AndAlso ODESolverRegistry.IsRegistered(name) Then
            Return
        End If

        Call ODESolverRegistry.Register(name, provider, replace)
    End Sub

    Private Sub Ensure(method As ODESolverMethod, provider As IODEsolverProvider, replace As Boolean)
        If Not replace AndAlso ODESolverRegistry.IsRegistered(method) Then
            Return
        End If

        Call ODESolverRegistry.Register(method, provider, replace)
    End Sub
End Module
