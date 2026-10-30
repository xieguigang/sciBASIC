#Region "Microsoft.VisualBasic::5deb090abc5b72a0d46c1467c9acc904, Data_science\Mathematica\Math\ODE\Dynamics\ODEs.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 240
    '    Code Lines: 128 (53.33%)
    ' Comment Lines: 77 (32.08%)
    '    - Xml Docs: 77.92%
    ' 
    '   Blank Lines: 35 (14.58%)
    '     File Size: 8.62 KB


    '     Class ODEs
    ' 
    '         Properties: Parameters
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateOutput, GetParameters, GetVariables, GetY0, Solve
    '                   TimePopulator
    ' 
    '         Sub: ODEs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Concurrent
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    ''' <summary>
    ''' Solving ODEs in R language, as example for test this class:
    ''' 
    ''' ```R
    ''' func &lt;- function(t, x, parms) {
    '''    with(as.list(c(parms, x)), {
    '''    
    '''       dP &lt;- a * P      - b * C * P
    '''       dC &lt;- b * P * C  - c * C
    '''      
    '''       list(c(dP, dC))
    '''    })
    ''' }
    '''
    ''' y0    &lt;- c(P = 2, C = 1)
    ''' parms &lt;- c(a = 0.1, b = 0.1, c = 0.1)
    ''' out   &lt;- ode(y = y0, times = 0:100, func, parms = parms)
    ''' 
    ''' head(out)
    ''' plot(out)
    ''' ```
    ''' </summary>
    Public MustInherit Class ODEs

        ReadOnly __vars As Dictionary(Of String, var)
        Protected Friend vars As var()

        Public Const y0RefName As String = NameOf(__vars)

        ''' <summary>
        ''' The reflection cache of the parameter fields of each ODEs model type, the
        ''' <see cref="Parameters"/> property is evaluated in every <see cref="Solve"/> call,
        ''' so the expensive reflection metadata should be cached by the model type.
        ''' </summary>
        Private Shared ReadOnly parameterFields As New ConcurrentDictionary(Of Type, FieldInfo())
        ''' <summary>
        ''' The reflection cache of the state variable fields of each ODEs model type.
        ''' </summary>
        Private Shared ReadOnly variableFields As New ConcurrentDictionary(Of Type, FieldInfo())
        ''' <summary>
        ''' The parameter names cache which is required by the Monte Carlo fitting.
        ''' </summary>
        Private Shared ReadOnly parameterNames As New ConcurrentDictionary(Of Type, String())
        ''' <summary>
        ''' The state variable names cache.
        ''' </summary>
        Private Shared ReadOnly variableNames As New ConcurrentDictionary(Of Type, String())

        ''' <summary>
        ''' The number of the state variables in current ODEs system.(当前方程组的状态变量个数)
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Size As Integer
            Get
                Return If(vars Is Nothing, 0, vars.Length)
            End Get
        End Property

        ''' <summary>
        ''' The index of the current integration step which is maintained by the solver, it
        ''' starts from zero at the initial state.
        ''' (当前积分步的序号，由求解器维护，初始状态的序号为0)
        ''' </summary>
        ''' <returns></returns>
        Friend Property CurrentStep As Integer

        ''' <summary>
        ''' get variable by given unique name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property GetVar(name As String) As var
            Get
                Return __vars(name)
            End Get
        End Property

        ''' <summary>
        ''' 返回的值包括<see cref="Double"/>类型的Field或者<see cref="float"/>类型的field
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Parameters() As Dictionary(Of String, Double)
            Get
                Dim fields As FieldInfo() = parameterFields.GetOrAdd(Me.GetType, AddressOf GetParameterFields)
                Dim out As New Dictionary(Of String, Double)(fields.Length)

                For i As Integer = 0 To fields.Length - 1
                    Dim f As FieldInfo = fields(i)
                    Dim value As Object = f.GetValue(Me)

                    If f.FieldType.Equals(GetType(Double)) Then
                        out.Add(f.Name, CDbl(value))
                    Else
                        out.Add(f.Name, DirectCast(value, f64).Value)
                    End If
                Next

                Return out
            End Get
        End Property

        ''' <summary>
        ''' Get the parameter fields of the given ODEs model type: the <see cref="Double"/>
        ''' typed fields and the <see cref="f64"/> typed fields, but the <see cref="var"/>
        ''' typed state variables are excluded.
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        Private Shared Function GetParameterFields(model As Type) As FieldInfo()
            Dim fields As FieldInfo() = CType(model, TypeInfo).DeclaredFields.ToArray
            Dim doubleFields = fields.Where(Function(f) f.FieldType.Equals(GetType(Double)))
            Dim f64Fields = fields _
                .Where(Function(f)
                           Return (Not f.FieldType.Equals(var.type)) AndAlso f.FieldType.Equals(GetType(f64))
                       End Function)

            Return doubleFields.Concat(f64Fields).ToArray
        End Function

        Sub New()
            Dim fields As FieldInfo() = variableFields.GetOrAdd(Me.GetType, AddressOf GetVariableFields)

            If fields.Length = 0 Then
                ' 没有声明任何状态变量的方程组：给一个空数组而不是抛出异常
                vars = New var() {}
            Else
                vars = New var(fields.Length - 1) {}
            End If

            For Each f As SeqValue(Of FieldInfo) In fields.SeqIterator
                Dim x As New var() With {
                    .Name = f.value.Name,
                    .Index = f.i
                }
                vars(f.i) = x

                Call f.value.SetValue(Me, x)
            Next

            __vars = vars.ToDictionary(Function(a) a.Name)
        End Sub

        ''' <summary>
        ''' Get the state variable fields of the given ODEs model type.
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        Private Shared Function GetVariableFields(model As Type) As FieldInfo()
            Return CType(model, TypeInfo).DeclaredFields _
                .Where(Function(x) x.FieldType.Equals(var.type)) _
                .ToArray
        End Function

        Protected Sub New(vars As var())
            Me.vars = vars
            Me.__vars = vars.ToDictionary(Function(a) a.Name)
        End Sub

        ''' <summary>
        ''' 初值
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function y0() As var()

        ''' <summary>
        ''' Get the initial values of the state variables, the value array is indexed by
        ''' the <see cref="var.Index"/> of each state variable.
        ''' </summary>
        ''' <param name="incept">是否是为蒙特卡洛实验设计的？</param>
        ''' <returns></returns>
        Public Function GetY0(Optional incept As Boolean = False) As Double()
            Dim symbols As var() = If(incept, Me.vars, Me.y0)

            If symbols Is Nothing OrElse symbols.Length = 0 Then
                Return New Double() {}
            End If

            Dim values As Double() = New Double(symbols.Length - 1) {}

            For i As Integer = 0 To symbols.Length - 1
                Dim v As var = symbols(i)

                values(v.Index) = v.Value
            Next

            Return values
        End Function

        ''' <summary>
        ''' Populates the data of <see cref="ODESolution.x"/>
        ''' </summary>
        ''' <param name="n%"></param>
        ''' <param name="a#"></param>
        ''' <param name="b#"></param>
        ''' <returns></returns>
        Public Shared Iterator Function TimePopulator(n%, a#, b#) As IEnumerable(Of Double)
            Dim dh As Double = (b - a) / n  ' 步长
            Dim dx As Double = a

            For i As Integer = 0 To n
                Yield dx
                dx += dh
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="n">A larger value of this parameter, will makes your result more precise.</param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <returns></returns>
        Public Function Solve(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODESolution
            Dim y0 As Double() = GetY0(incept)
            Dim solver As New RungeKutta4(Me)

            Call solver.Solve(y0, n, a, b)

            Return CreateOutput(Me, CreateY0Map(y0), solver.XValues, solver.YValues)
        End Function

        ''' <summary>
        ''' Solve the ODEs system by using the adaptive step size Dormand-Prince 5(4) method.
        ''' (使用自适应步长的 Dormand-Prince 5(4) 方法求解方程组)
        ''' </summary>
        ''' <param name="n">The number of the points on the output grid minus one.</param>
        ''' <param name="a">The lower bound of the integration range.</param>
        ''' <param name="b">The upper bound of the integration range.</param>
        ''' <param name="atol">The absolute error tolerance of each state variable.</param>
        ''' <param name="rtol">The relative error tolerance of each state variable.</param>
        ''' <param name="initialStep">The initial step size, zero means auto select.</param>
        ''' <param name="incept"></param>
        ''' <param name="maxSteps">The maximal number of the integration steps.</param>
        ''' <returns></returns>
        Public Function SolveAdaptive(n As Integer, a As Double, b As Double,
                                      Optional atol As Double = 1.0E-9,
                                      Optional rtol As Double = 1.0E-7,
                                      Optional initialStep As Double = 0.0,
                                      Optional incept As Boolean = False,
                                      Optional maxSteps As Integer = 10000000) As ODESolution

            Dim y0 As Double() = GetY0(incept)
            Dim solver As New RungeKutta45(Me)

            Call solver.Solve(y0, n, a, b, atol, rtol, initialStep, maxSteps)

            Return CreateOutput(Me, CreateY0Map(y0), solver.XValues, solver.YValues)
        End Function

#Region "Solver dispatch"

        ''' <summary>
        ''' Solve the ODEs system by the specified integration method, all of the methods will
        ''' produce the same output shape as <see cref="Solve"/>: ``n + 1`` sampling points which
        ''' include both boundaries of the integration range.
        ''' (使用指定的积分方法求解方程组，所有方法都输出与 Solve 相同形状的结果)
        ''' </summary>
        ''' <param name="n">The number of the integration steps.(积分步数)</param>
        ''' <param name="a">The lower bound of the integration range.(积分下界)</param>
        ''' <param name="b">The upper bound of the integration range.(积分上界)</param>
        ''' <param name="method">The integration algorithm.(积分算法)</param>
        ''' <param name="incept">是否是为蒙特卡洛实验设计的？</param>
        ''' <param name="options">The optional solver settings.(可选的求解器设置)</param>
        ''' <returns></returns>
        Public Function Solve(n As Integer, a As Double, b As Double, method As ODESolverMethod,
                              Optional incept As Boolean = False,
                              Optional options As ODESolverOptions = Nothing) As ODESolution

            Dim y0 As Double() = GetY0(incept)
            Dim points As Double()
            Dim columns As Double()()

            Select Case method
                Case ODESolverMethod.Euler
                    Dim solver As New EulerSolver(Me)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.RK2
                    Dim solver As New RungeKutta2(Me)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.Gill
                    Dim solver As New GillSolver(Me)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.RK4
                    Dim solver As New RungeKutta4(Me)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.ABM4
                    Dim solver As New AdamsBashforthMoulton4(Me)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.BackwardEuler
                    Dim solver As New BackwardEulerSolver(Me, options)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.Trapezoidal
                    Dim solver As New TrapezoidalSolver(Me, options)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.BDF2
                    Dim solver As New Bdf2Solver(Me, options)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.RosenbrockROS2
                    Dim solver As New RosenbrockRos2Solver(Me, options)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.RK23
                    Dim solver As New RungeKutta23(Me)
                    Dim settings As ODESolverOptions = If(options, New ODESolverOptions())

                    Call solver.Solve(y0, n, a, b, settings.Atol, settings.Rtol, settings.InitialStep, settings.MaxSteps)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.RK45
                    Dim solver As New RungeKutta45(Me)
                    Dim settings As ODESolverOptions = If(options, New ODESolverOptions())

                    Call solver.Solve(y0, n, a, b, settings.Atol, settings.Rtol, settings.InitialStep, settings.MaxSteps)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.Verlet
                    Dim solver As New StormerVerletSolver(Me)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case ODESolverMethod.Yoshida4
                    Dim solver As New Yoshida4Solver(Me)

                    Call solver.Solve(y0, n, a, b)
                    points = solver.XValues
                    columns = solver.YValues
                Case Else
                    ' 外部(插件)求解器：CVODE_Adams / CVODE_BDF 以及将来新增的枚举值统一走注册表
                    Return SolveByProvider(method, y0, n, a, b, options)
            End Select

            Return CreateOutput(Me, CreateY0Map(y0), points, columns)
        End Function

        ''' <summary>
        ''' Solve the ODEs system by the external (plugin) solver which is registered with the
        ''' given name, for example ``cvode.bdf``. The built-in methods can also be invoked by
        ''' their enumeration name, e.g. ``"RK4"``.
        ''' (使用以指定名称注册的外部求解器求解方程组；内建方法也可以用枚举名调用)
        ''' </summary>
        ''' <param name="n">The number of the integration steps.(积分步数)</param>
        ''' <param name="a">The lower bound of the integration range.(积分下界)</param>
        ''' <param name="b">The upper bound of the integration range.(积分上界)</param>
        ''' <param name="methodName">The registration name of the solver.(求解器的注册名称)</param>
        ''' <param name="incept">是否是为蒙特卡洛实验设计的？</param>
        ''' <param name="options">The optional solver settings.(可选的求解器设置)</param>
        ''' <returns></returns>
        Public Function Solve(n As Integer, a As Double, b As Double, methodName As String,
                              Optional incept As Boolean = False,
                              Optional options As ODESolverOptions = Nothing) As ODESolution

            If String.IsNullOrWhiteSpace(methodName) Then
                Throw New ArgumentNullException(NameOf(methodName), "The name of the ODEs solver can not be empty!")
            End If

            Dim name As String = methodName.Trim
            Dim method As ODESolverMethod

            ' 优先按枚举名解析：这样 "RK4" 这类内建方法的名称也可以直接使用
            If [Enum].TryParse(name, True, method) Then
                Return Solve(n, a, b, method, incept, options)
            End If

            Dim provider As IODEsolverProvider = Nothing

            If Not ODESolverRegistry.TryGet(name, provider) Then
                Throw New InvalidOperationException(NotRegisteredMessage(name))
            End If

            Return SolveByProvider(provider, GetY0(incept), n, a, b, options)
        End Function

        ''' <summary>
        ''' Resolve the external solver provider from the given enumeration value and run it.
        ''' </summary>
        Private Function SolveByProvider(method As ODESolverMethod, y0 As Double(), n As Integer,
                                         a As Double, b As Double, options As ODESolverOptions) As ODESolution

            Dim provider As IODEsolverProvider = Nothing

            If Not ODESolverRegistry.TryGet(method, provider) Then
                Throw New InvalidOperationException(NotRegisteredMessage(method.ToString()))
            End If

            Return SolveByProvider(provider, y0, n, a, b, options)
        End Function

        ''' <summary>
        ''' Run the external solver provider and convert its result into the standard
        ''' <see cref="ODESolution"/> of the ODEs framework: the result shape, the values and the
        ''' state variable synchronization are all validated here.
        ''' (执行外部求解器，并把结果校验、状态同步之后转换为框架标准的结果对象)
        ''' </summary>
        Private Function SolveByProvider(provider As IODEsolverProvider, y0 As Double(), n As Integer,
                                         a As Double, b As Double, options As ODESolverOptions) As ODESolution

            If n < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(n), "The number of the integration steps must be positive!")
            End If
            If b <= a Then
                Throw New ArgumentOutOfRangeException(NameOf(b), "The upper bound of the integration range must be greater than the lower bound!")
            End If
            If y0 Is Nothing OrElse y0.Length <> Size Then
                Throw New ArgumentException(
                    $"The dimension of the given y0 initial value ({If(y0 Is Nothing, "null", y0.Length.ToString)}) is not matched to the ODEs system ({Size})!",
                    NameOf(y0))
            End If

            Dim solverName As String = If(String.IsNullOrEmpty(provider.Name), provider.GetType().Name, provider.Name)
            Dim result As ODESolution = provider.Solve(Me, y0, n, a, b, If(options, New ODESolverOptions()))

            If result Is Nothing Then
                Throw New InvalidOperationException($"The ODEs solver '{solverName}' returns no result!")
            End If
            If result.x Is Nothing OrElse result.x.Length <> n + 1 Then
                Throw New InvalidOperationException(
                    $"The ODEs solver '{solverName}' returns an invalid time grid: the sample count should be {n + 1}, but {If(result.x Is Nothing, "null", result.x.Length.ToString)} is found!")
            End If
            If result.y Is Nothing OrElse result.y.Count <> Size Then
                Throw New InvalidOperationException(
                    $"The ODEs solver '{solverName}' returns {If(result.y Is Nothing, 0, result.y.Count)} result columns, but the ODEs system contains {Size} state variables!")
            End If

            Dim columns As Double()() = New Double(Size - 1)() {}

            For i As Integer = 0 To Size - 1
                Dim column As Double() = result.GetComponent(i)

                If column Is Nothing OrElse column.Length <> n + 1 Then
                    Throw New InvalidOperationException(
                        $"The ODEs solver '{solverName}' returns an invalid result column at index {i}: the sample count should be {n + 1}, but {If(column Is Nothing, "null", column.Length.ToString)} is found!")
                End If

                columns(i) = column
            Next

            Dim tolerance As Double = 1.0E-9 * System.Math.Max(1.0, System.Math.Abs(b - a))

            If System.Math.Abs(result.x(0) - a) > tolerance OrElse System.Math.Abs(result.x(n) - b) > tolerance Then
                Throw New InvalidOperationException(
                    $"The ODEs solver '{solverName}' does not cover the integration range [{a}, {b}]: the result range is [{result.x(0)}, {result.x(n)}]!")
            End If
            If result.HaveNaN Then
                Throw New InvalidOperationException(
                    $"The ODEs solver '{solverName}' produces NaN/Inf values in the integration range [{a}, {b}]!")
            End If

            ' 同步最终状态，保持与内建求解器一致的外部可观察语义
            Dim last As Double() = New Double(Size - 1) {}

            For i As Integer = 0 To Size - 1
                last(i) = columns(i)(n)
            Next

            Call SyncState(last)

            CurrentStep = n

            Dim output As ODESolution = CreateOutput(Me, CreateY0Map(y0), result.x, columns)

            ' 保留外部求解器的诊断信息（steps / rhs_evaluations / ...）
            output.info = result.info

            Return output
        End Function

        ''' <summary>
        ''' The error message of the unregistered ODEs solver.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Private Shared Function NotRegisteredMessage(name As String) As String
            Dim registered As String() = ODESolverRegistry.RegisteredNames
            Dim names As String = If(registered.Length = 0, "(no external solver is registered)", String.Join(", ", registered))

            Return $"The ODEs solver '{name}' is not registered! The external solvers are provided as plugins: " &
                   $"reference the 'Microsoft.VisualBasic.Math.Sundials.CVODE' assembly and invoke " &
                   $"'Sundials.CVODE.SolverExtensions.Register()' before the integration. " &
                   $"The registered external solvers are: {names}."
        End Function

        ''' <summary>
        ''' Solve the ODEs system by the forward Euler method, 1st order.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <returns></returns>
        Public Function SolveEuler(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODESolution
            Return Solve(n, a, b, ODESolverMethod.Euler, incept)
        End Function

        ''' <summary>
        ''' Solve the ODEs system by the 2nd order Runge-Kutta (Heun) method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <returns></returns>
        Public Function SolveRK2(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODESolution
            Return Solve(n, a, b, ODESolverMethod.RK2, incept)
        End Function

        ''' <summary>
        ''' Solve the ODEs system by the 4th order Gill method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <returns></returns>
        Public Function SolveGill(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODESolution
            Return Solve(n, a, b, ODESolverMethod.Gill, incept)
        End Function

        ''' <summary>
        ''' Solve the ODEs system by the 4th order Adams-Bashforth-Moulton method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <returns></returns>
        Public Function SolveABM4(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODESolution
            Return Solve(n, a, b, ODESolverMethod.ABM4, incept)
        End Function

        ''' <summary>
        ''' Solve the stiff ODEs system by the implicit backward Euler method (BDF1).
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function SolveBackwardEuler(n As Integer, a As Double, b As Double,
                                           Optional incept As Boolean = False,
                                           Optional options As ODESolverOptions = Nothing) As ODESolution

            Return Solve(n, a, b, ODESolverMethod.BackwardEuler, incept, options)
        End Function

        ''' <summary>
        ''' Solve the stiff ODEs system by the implicit trapezoidal rule (Crank-Nicolson).
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function SolveTrapezoidal(n As Integer, a As Double, b As Double,
                                         Optional incept As Boolean = False,
                                         Optional options As ODESolverOptions = Nothing) As ODESolution

            Return Solve(n, a, b, ODESolverMethod.Trapezoidal, incept, options)
        End Function

        ''' <summary>
        ''' Solve the stiff ODEs system by the 2nd order backward differentiation formula.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function SolveBDF2(n As Integer, a As Double, b As Double,
                                  Optional incept As Boolean = False,
                                  Optional options As ODESolverOptions = Nothing) As ODESolution

            Return Solve(n, a, b, ODESolverMethod.BDF2, incept, options)
        End Function

        ''' <summary>
        ''' Solve the stiff ODEs system by the 2nd order linearly implicit Rosenbrock method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function SolveRosenbrockROS2(n As Integer, a As Double, b As Double,
                                            Optional incept As Boolean = False,
                                            Optional options As ODESolverOptions = Nothing) As ODESolution

            Return Solve(n, a, b, ODESolverMethod.RosenbrockROS2, incept, options)
        End Function

        ''' <summary>
        ''' Solve the ODEs system by the adaptive step size Bogacki-Shampine 3(2) method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function SolveRK23(n As Integer, a As Double, b As Double,
                                  Optional incept As Boolean = False,
                                  Optional options As ODESolverOptions = Nothing) As ODESolution

            Return Solve(n, a, b, ODESolverMethod.RK23, incept, options)
        End Function

        ''' <summary>
        ''' Solve the ODEs system by the adaptive step size Dormand-Prince 5(4) method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <param name="options"></param>
        ''' <returns></returns>
        Public Function SolveRK45(n As Integer, a As Double, b As Double,
                                  Optional incept As Boolean = False,
                                  Optional options As ODESolverOptions = Nothing) As ODESolution

            Return Solve(n, a, b, ODESolverMethod.RK45, incept, options)
        End Function

        ''' <summary>
        ''' Solve the Hamiltonian system by the 2nd order Stormer-Verlet symplectic method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <returns></returns>
        Public Function SolveVerlet(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODESolution
            Return Solve(n, a, b, ODESolverMethod.Verlet, incept)
        End Function

        ''' <summary>
        ''' Solve the Hamiltonian system by the 4th order Yoshida composition of the
        ''' Stormer-Verlet method.
        ''' </summary>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="incept"></param>
        ''' <returns></returns>
        Public Function SolveYoshida4(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODESolution
            Return Solve(n, a, b, ODESolverMethod.Yoshida4, incept)
        End Function

#End Region

        ''' <summary>
        ''' Create the ``name -> value`` table of the initial values of the state variables.
        ''' </summary>
        ''' <param name="y0"></param>
        ''' <returns></returns>
        Private Function CreateY0Map(y0 As Double()) As Dictionary(Of String, Double)
            Dim symbols As var() = vars
            Dim yinit As New Dictionary(Of String, Double)(symbols.Length)

            For i As Integer = 0 To symbols.Length - 1
                Dim v As var = symbols(i)

                ' 记录y0
                yinit(v.Name) = y0(v.Index)
            Next

            Return yinit
        End Function

        ''' <summary>
        ''' Create the output model from the pre-allocated integration result columns.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="y0"></param>
        ''' <param name="x"></param>
        ''' <param name="y">The result columns of each state variable.</param>
        ''' <returns></returns>
        Public Shared Function CreateOutput(system As ODEs, y0 As Dictionary(Of String, Double), x As Double(), y As Double()()) As ODESolution
            Dim symbols As var() = system.vars
            Dim out As New Dictionary(Of NamedCollection(Of Double))
            ' 显式指定列顺序：结果字典是按键排序的，不能依赖它的枚举顺序来定位状态变量
            Dim names As String() = New String(symbols.Length - 1) {}

            For i As Integer = 0 To symbols.Length - 1
                Dim v As var = symbols(i)

                names(v.Index) = v.Name
                out(v.Name) = New NamedCollection(Of Double) With {
                    .name = v.Name,
                    .value = y(v.Index)
                }
            Next

            Return New ODESolution With {
                .x = x,
                .y = out,
                .y0 = y0,
                .params = system.Parameters,
                .columnNames = names
            }
        End Function

        ''' <summary>
        ''' Create the output model from the integration result which is stored in the
        ''' <see cref="List(Of Double)"/> format.
        ''' </summary>
        ''' <param name="system"></param>
        ''' <param name="y0"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function CreateOutput(system As ODEs, y0 As Dictionary(Of String, Double), x As Double(), y As List(Of Double)()) As ODESolution
            Dim symbols As var() = system.vars
            Dim out As New Dictionary(Of NamedCollection(Of Double))

            For i As Integer = 0 To symbols.Length - 1
                Dim v As var = symbols(i)

                out(v.Name) = New NamedCollection(Of Double) With {
                    .name = v.Name,
                    .value = y(v.Index).ToArray
                }
            Next

            Return New ODESolution With {
                .x = x,
                .y = out,
                .y0 = y0,
                .params = system.Parameters
            }
        End Function

        ''' <summary>
        ''' 在这里计算具体的微分方程组
        ''' </summary>
        ''' <param name="dx"></param>
        ''' <param name="dy"></param>
        Protected MustOverride Sub func(dx#, ByRef dy As Vector)

        ''' <summary>
        ''' The optional array based derivative evaluator: when this delegate is presented,
        ''' the solver will invoke it directly with the raw state array and the raw derivative
        ''' buffer, so that the per-stage synchronization of the <see cref="var"/> state
        ''' variables will be skipped. (可选的数组式直通导数求值器)
        ''' </summary>
        ''' <remarks>
        ''' 使用这个入口可以完全绕过 <see cref="var"/> 包装与反射带来的开销，适合 Lorenz 仿真、
        ''' 蒙特卡洛/遗传算法参数拟合这类需要成千上万次求解的高频场景：
        ''' 
        ''' ```vbnet
        ''' model.FastDerivative = Sub(t, y, dy)
        '''                            dy(0) = sigma * (y(1) - y(0))
        '''                            dy(1) = y(0) * (rho - y(2)) - y(1)
        '''                            dy(2) = y(0) * y(1) - beta * y(2)
        '''                        End Sub
        ''' ```
        ''' 
        ''' 注意：使用这个入口的时候，求解过程中不会更新各个 <see cref="var"/> 变量的数值。
        ''' </remarks>
        ''' <returns></returns>
        Public Property FastDerivative As Action(Of Double, Double(), Double())

        ''' <summary>
        ''' The reused <see cref="Vector"/> wrappers of the <see cref="Derivatives(Double, Double(), Double())"/>
        ''' API, they are allocated at the first access only.
        ''' </summary>
        Private derivativeState As Vector
        Private derivativeBuffer As Vector

        ''' <summary>
        ''' The optional analytic Jacobian ``J = ∂f/∂y`` of the ODEs system, when this delegate
        ''' is not presented the implicit integrators will use the central finite difference.
        ''' (可选的解析 Jacobian；未提供的时候隐式求解器使用中心差商)
        ''' </summary>
        ''' <remarks>
        ''' 对于状态变量维度较大、或者需要高频调用隐式求解器的模型，提供解析 Jacobian 可以
        ''' 显著降低每一次隐式积分步的开销（中心差商需要 ``2N`` 次右端函数求值）：
        ''' 
        ''' ```vbnet
        ''' model.Jacobian = Sub(t, y, j)
        '''                      ' j(i, k) = ∂f_i / ∂y_k
        '''                      j(0, 0) = -0.5
        '''                  End Sub
        ''' ```
        ''' </remarks>
        ''' <returns></returns>
        Public Property Jacobian As Action(Of Double, Double(), Double(,))

        ''' <summary>
        ''' Evaluate the right-hand side function of the ODEs system: the state variables
        ''' are synchronized from the state vector first, then the user overrided
        ''' <see cref="func"/> will be invoked for computing the derivatives.
        ''' (方程组的右端函数求值：先从状态向量同步各个变量的数值，再调用用户重写的 func 计算导数)
        ''' </summary>
        ''' <param name="dx">The current integration time.</param>
        ''' <param name="y">The state vector.</param>
        ''' <param name="k">The derivative vector buffer, it will be cleared before the evaluation.</param>
        Friend Sub ODEs(dx As Double, y As Vector, ByRef k As Vector)
            Dim values As Double() = y.Array
            ' 导数缓冲清零：避免用户函数没有显式赋值的分量残留上一阶段的数值
            Dim derivatives As Double() = k.Array
            System.Array.Clear(derivatives, 0, derivatives.Length)

            Dim fast As Action(Of Double, Double(), Double()) = FastDerivative

            If fast IsNot Nothing Then
                ' 数组式直通：跳过 var 状态变量的同步，没有任何包装开销
                Call fast(dx, values, derivatives)
                Return
            End If

            ' 更新设置y的值
            Call SyncState(values)

            Call func(dx, dy:=k)
        End Sub

        ''' <summary>
        ''' Synchronize the ``var`` state variables from the given state vector.
        ''' (将状态向量的数值同步到各个状态变量对象之上)
        ''' </summary>
        ''' <param name="values"></param>
        Friend Sub SyncState(values As Double())
            Dim symbols As var() = vars

            For i As Integer = 0 To symbols.Length - 1
                Dim symbol As var = symbols(i)

                symbol.Value = values(symbol.Index)
            Next
        End Sub

        ''' <summary>
        ''' Evaluate the derivatives of the ODEs system at the given time and state, this is
        ''' a diagnostic API which is useful for the finite difference Jacobian and the
        ''' external custom integrators. (在给定的时间与状态下计算方程组的导数值)
        ''' </summary>
        ''' <param name="t">The evaluation time.</param>
        ''' <param name="y">The state vector.</param>
        ''' <param name="dy">The derivative output buffer.</param>
        Public Sub Derivatives(t As Double, y As Vector, dy As Vector)
            Call ODEs(t, y, dy)
        End Sub

        ''' <summary>
        ''' Evaluate the derivatives of the ODEs system with the raw <see cref="Double"/>
        ''' array buffers.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="y"></param>
        ''' <param name="dy"></param>
        Public Sub Derivatives(t As Double, y As Double(), dy As Double())
            If y Is Nothing OrElse y.Length <> Size Then
                Throw New ArgumentException($"The state vector should contains {Size} elements!", NameOf(y))
            End If
            If dy Is Nothing OrElse dy.Length <> y.Length Then
                Throw new ArgumentException($"The derivative buffer should contains {y.Length} elements!", NameOf(dy))
            End If

            ' 复用状态的 Vector 包装，避免中心差商 Jacobian 每个积分步产生额外的内存分配
            If derivativeState Is Nothing OrElse derivativeState.Length <> y.Length Then
                derivativeState = New Vector(y.Length)
                derivativeBuffer = New Vector(y.Length)
            End If

            Call System.Array.Copy(y, derivativeState.Array, y.Length)
            Call ODEs(t, derivativeState, derivativeBuffer)
            Call System.Array.Copy(derivativeBuffer.Array, dy, dy.Length)
        End Sub

        ''' <summary>
        ''' Get function parameters name collection
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        Public Shared Function GetParameters(model As Type) As IEnumerable(Of String)
            Return parameterNames.GetOrAdd(model, AddressOf GetParameterNames)
        End Function

        Private Shared Function GetParameterNames(model As Type) As String()
            Return CType(model, TypeInfo).DeclaredFields _
                .Where(Function(f)
                           Return (Not f.IsLiteral) AndAlso f.FieldType.Equals(GetType(Double))
                       End Function) _
                .Select(Function(f) f.Name) _
                .ToArray
        End Function

        ''' <summary>
        ''' Get Y names
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        Public Shared Function GetVariables(model As Type) As IEnumerable(Of String)
            Return variableNames.GetOrAdd(model, AddressOf GetVariableNames)
        End Function

        Private Shared Function GetVariableNames(model As Type) As String()
            Return GetVariableFields(model).Select(Function(f) f.Name).ToArray
        End Function
    End Class
End Namespace
