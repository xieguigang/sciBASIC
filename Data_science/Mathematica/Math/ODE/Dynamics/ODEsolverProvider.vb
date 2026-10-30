Imports System.Collections.Concurrent
Imports System.Linq
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data

Namespace Dynamics

    ''' <summary>
    ''' The contract of the external (plugin) ODEs solvers: an implementation lives in another
    ''' assembly (for example the managed ``Sundials.CVODE`` package) and registers itself into
    ''' the <see cref="ODESolverRegistry"/>, so that it can be invoked from
    ''' <see cref="ODEs.Solve(Integer, Double, Double, ODESolverMethod, Boolean, ODESolverOptions)"/>
    ''' just like the built-in integrators.
    ''' (外部(插件)求解器的契约：实现在其他程序集之中，通过注册表注入到 ODEs 的分发体系)
    ''' </summary>
    ''' <remarks>
    ''' 插件只需要实现「给定时间区间与输出网格，返回积分结果」这一件事，而且：
    ''' 
    ''' 1. 结果统一使用 <see cref="ODEsOut"/> 输出（``x`` 为 ``n + 1`` 个采样点，列顺序与状态变量序号一致）；
    ''' 2. 结果形状、数值有限性以及状态变量同步都由 ODE 框架统一校验与处理，插件不需要
    '''    （也无法）访问 ``Size`` / ``SyncState`` 这些内部成员。
    ''' </remarks>
    Public Interface IODEsolverProvider

        ''' <summary>
        ''' The registration name of the solver, for example ``cvode.bdf``.
        ''' (求解器的注册名)
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Name As String

        ''' <summary>
        ''' The description which is used in the log and the exception message.
        ''' (描述信息，用于日志与异常信息)
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Description As String

        ''' <summary>
        ''' Solve the ODEs system over the integration range ``[a, b]``.
        ''' </summary>
        ''' <param name="model">The ODEs system which provides the right-hand side function.</param>
        ''' <param name="y0">The initial state of the integration.</param>
        ''' <param name="n">The number of the integration steps, the result should contain ``n + 1`` sampling points.</param>
        ''' <param name="a">The lower bound of the integration range.</param>
        ''' <param name="b">The upper bound of the integration range.</param>
        ''' <param name="options">The solver settings, the ``ProviderOptions`` field carries the solver specific options.</param>
        ''' <returns>
        ''' The integration result: ``x`` contains ``n + 1`` sampling points which include both
        ''' boundaries, and the result columns are ordered by the state variable index.
        ''' </returns>
        Function Solve(model As ODEs, y0 As Double(), n As Integer, a As Double, b As Double,
                       options As ODESolverOptions) As ODEsOut
    End Interface

    ''' <summary>
    ''' The thread safe registry of the external ODEs solvers, it maps both the registration name
    ''' (string) and the <see cref="ODESolverMethod"/> enumeration value to the provider.
    ''' (外部求解器的注册表：字符串名称与枚举值双向映射到求解器实现)
    ''' </summary>
    ''' <remarks>
    ''' 使用方式（以托管版 CVODE 为例）：
    ''' 
    ''' ```vbnet
    ''' Call Microsoft.VisualBasic.Math.Sundials.CVODE.SolverExtensions.Register()
    ''' 
    ''' Dim r1 = model.Solve(1000, 0, 5, ODESolverMethod.CVODE_BDF)
    ''' Dim r2 = model.Solve(1000, 0, 5, "cvode.adams")
    ''' ```
    ''' </remarks>
    Public NotInheritable Class ODESolverRegistry

        Private Shared ReadOnly providers As New ConcurrentDictionary(Of String, IODEsolverProvider)(StringComparer.OrdinalIgnoreCase)
        Private Shared ReadOnly methods As New ConcurrentDictionary(Of ODESolverMethod, IODEsolverProvider)()

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Register a solver with the given name.
        ''' </summary>
        ''' <param name="name">The registration name, e.g. ``cvode.bdf``.</param>
        ''' <param name="provider">The solver implementation.</param>
        ''' <param name="replace">Should the existed registration be overridden?</param>
        Public Shared Sub Register(name As String, provider As IODEsolverProvider, Optional replace As Boolean = False)
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentNullException(NameOf(name), "The registration name of the ODEs solver can not be empty!")
            End If
            If provider Is Nothing Then
                Throw New ArgumentNullException(NameOf(provider))
            End If

            Dim key As String = name.Trim

            If replace Then
                providers(key) = provider
            ElseIf Not providers.TryAdd(key, provider) Then
                Throw New InvalidOperationException(
                    $"The ODEs solver '{key}' is already registered, please use ``replace:=True`` to override it!")
            End If
        End Sub

        ''' <summary>
        ''' Register a solver with the given <see cref="ODESolverMethod"/> enumeration value, the
        ''' enumeration name will be registered as the string name at the same time.
        ''' </summary>
        ''' <param name="method"></param>
        ''' <param name="provider"></param>
        ''' <param name="replace"></param>
        Public Shared Sub Register(method As ODESolverMethod, provider As IODEsolverProvider, Optional replace As Boolean = False)
            If provider Is Nothing Then
                Throw New ArgumentNullException(NameOf(provider))
            End If

            Dim key As String = method.ToString()

            If Not replace Then
                If methods.ContainsKey(method) OrElse providers.ContainsKey(key) Then
                    Throw New InvalidOperationException(
                        $"The ODEs solver '{key}' is already registered, please use ``replace:=True`` to override it!")
                End If
            End If

            methods(method) = provider
            providers(key) = provider
        End Sub

        ''' <summary>
        ''' Try to get the solver which is registered with the given name.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="provider"></param>
        ''' <returns></returns>
        Public Shared Function TryGet(name As String, ByRef provider As IODEsolverProvider) As Boolean
            provider = Nothing

            If String.IsNullOrWhiteSpace(name) Then
                Return False
            End If

            Return providers.TryGetValue(name.Trim, provider)
        End Function

        ''' <summary>
        ''' Try to get the solver which is registered with the given enumeration value.
        ''' </summary>
        ''' <param name="method"></param>
        ''' <param name="provider"></param>
        ''' <returns></returns>
        Public Shared Function TryGet(method As ODESolverMethod, ByRef provider As IODEsolverProvider) As Boolean
            Return methods.TryGetValue(method, provider)
        End Function

        ''' <summary>
        ''' Is the solver which has the given name is registered?
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function IsRegistered(name As String) As Boolean
            Return Not String.IsNullOrWhiteSpace(name) AndAlso providers.ContainsKey(name.Trim)
        End Function

        ''' <summary>
        ''' Is the given enumeration value is registered?
        ''' </summary>
        ''' <param name="method"></param>
        ''' <returns></returns>
        Public Shared Function IsRegistered(method As ODESolverMethod) As Boolean
            Return methods.ContainsKey(method)
        End Function

        ''' <summary>
        ''' The snapshot of the registered solver names, it is used by the error message and the
        ''' user interface which lists all of the available solvers.
        ''' (已注册的求解器名称快照)
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RegisteredNames As String()
            Get
                Return providers.Keys _
                    .OrderBy(Function(name) name, StringComparer.OrdinalIgnoreCase) _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' Remove the registration of the given name.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function Unregister(name As String) As Boolean
            Dim removed As Boolean = False
            Dim key As String = If(name, "").Trim
            Dim discarded As IODEsolverProvider = Nothing

            If key <> "" Then
                removed = providers.TryRemove(key, discarded)

                ' 同步移除枚举表中的映射
                For Each method As ODESolverMethod In [Enum].GetValues(Of ODESolverMethod)()
                    If String.Equals(method.ToString(), key, StringComparison.OrdinalIgnoreCase) Then
                        If methods.TryRemove(method, discarded) Then
                            removed = True
                        End If
                    End If
                Next
            End If

            Return removed
        End Function

        ''' <summary>
        ''' Remove the registration of the given enumeration value.
        ''' </summary>
        ''' <param name="method"></param>
        ''' <returns></returns>
        Public Shared Function Unregister(method As ODESolverMethod) As Boolean
            Dim discarded As IODEsolverProvider = Nothing
            Dim removed As Boolean = methods.TryRemove(method, discarded)

            If providers.TryRemove(method.ToString(), discarded) Then
                removed = True
            End If

            Return removed
        End Function

        ''' <summary>
        ''' Remove all of the registrations, this method is designed for the unit test only.
        ''' (清空注册表，仅供测试使用)
        ''' </summary>
        Public Shared Sub Clear()
            Call providers.Clear()
            Call methods.Clear()
        End Sub
    End Class
End Namespace
