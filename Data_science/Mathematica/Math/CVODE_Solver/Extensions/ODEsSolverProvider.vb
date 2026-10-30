Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

''' <summary>
''' 把托管版的 CVODE 求解器（Adams / BDF）适配成 ODE 求解器框架的插件。
''' </summary>
''' <remarks>
''' 这个类型只负责「桥接」：
''' 
''' 1. 把 <see cref="ODEs.Derivatives(Double, Double(), Double())"/> 适配成
'''    <see cref="RHSFunction"/>（直接使用 <see cref="Vector.Array"/> 的内部缓冲，零拷贝）；
''' 2. 把 <see cref="ODEs.Jacobian"/> 适配成 <see cref="JacobianFunction"/>
'''    （借助一个复用的矩形缓冲，把 <see cref="NumericMatrix"/> 的 jagged 行数组与外部回调
'''    要求的 ``Double(,)`` 之间做「读入 → 写入 → 写回」的显式搬运）；
''' 3. 把通用的 <see cref="ODESolverOptions"/> 与专有的 <see cref="CVODEOptions"/> 合并；
''' 4. 调用 <see cref="CVODESolverExtensions.Solve(CVODESolver, Double, Double, Vector, Integer, String())"/>
'''    输出统一的 <see cref="ODESolution"/>。
''' </remarks>
Public Class CvodeSolverProvider : Implements IODEsolverProvider

    ''' <summary>
    ''' The CVODE method which is used by this provider.(这个适配器所使用的 CVODE 方法)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Method As CVODEMethod

    ''' <summary>
    ''' The registration name of this provider.(注册名称)
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Name As String Implements IODEsolverProvider.Name

    ''' <summary>
    ''' The description which is used in the log and the exception message.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Description As String Implements IODEsolverProvider.Description

    ''' <summary>
    ''' Create the provider of the given CVODE method.
    ''' </summary>
    ''' <param name="method"></param>
    ''' <param name="name">The registration name, the default value is ``cvode.adams`` / ``cvode.bdf``.</param>
    ''' <param name="description"></param>
    Public Sub New(method As CVODEMethod, Optional name As String = Nothing, Optional description As String = Nothing)
        Me.Method = method
        Me.Name = If(String.IsNullOrEmpty(name), $"cvode.{method.ToString().ToLowerInvariant()}", name)
        Me.Description = If(String.IsNullOrEmpty(description),
                            $"The managed Sundials.CVODE {method} integrator (variable order, variable step size).",
                            description)
    End Sub

    ''' <summary>
    ''' Solve the ODEs system by the CVODE integrator.
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="y0"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    Public Function Solve(model As ODEs, y0 As Double(), n As Integer, a As Double, b As Double,
                          options As ODESolverOptions) As ODESolution Implements IODEsolverProvider.Solve

        If model Is Nothing Then
            Throw New ArgumentNullException(NameOf(model))
        End If
        If y0 Is Nothing OrElse y0.Length = 0 Then
            Throw New ArgumentException("The initial state vector can not be empty!", NameOf(y0))
        End If

        Dim size As Integer = y0.Length
        Dim cvodeOptions As CVODEOptions = BuildOptions(n, options)
        Dim state As New Vector(y0)
        Dim rhs As RHSFunction = Sub(t, y, ydot) Call model.Derivatives(t, y.Array, ydot.Array)
        Dim names As String() = ResolveNames(model, size)
        Dim result As ODESolution

        Using solver As New CVODESolver(Me.Method, rhs, size, cvodeOptions)
            Dim analytic As Action(Of Double, Double(), Double(,)) = model.Jacobian

            If analytic IsNot Nothing Then
                ' NumericMatrix 内部是行数组（jagged），而 ODEs.Jacobian 需要 Double(,)，
                ' 因此用一个只在首次回调时分配的缓冲完成「读入 -> 回调写入 -> 写回」的搬运
                Dim jacobianBuffer As Double(,) = Nothing

                solver.SetJacobianFunction(
                    Sub(t, y, fy, j)
                        If jacobianBuffer Is Nothing Then
                            jacobianBuffer = New Double(j.RowDimension - 1, j.ColumnDimension - 1) {}
                        End If

                        Call j.ToRectangular(jacobianBuffer)
                        Call analytic(t, y.Array, jacobianBuffer)
                        Call j.CopyFrom(jacobianBuffer)
                    End Sub)
            End If

            result = solver.Solve(a, b, state, n, names)
        End Using

        Return result
    End Function

    ''' <summary>
    ''' Merge the generic solver options with the CVODE specific options, the instance which is
    ''' provided by the caller will never be modified.
    ''' (合并通用设置与 CVODE 专有设置，不会修改调用方传入的实例)
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    Private Shared Function BuildOptions(n As Integer, options As ODESolverOptions) As CVODEOptions
        Dim source As CVODEOptions = Nothing
        Dim cvode As New CVODEOptions()

        If options IsNot Nothing Then
            source = TryCast(options.ProviderOptions, CVODEOptions)
        End If

        If source IsNot Nothing Then
            cvode.RelativeTolerance = source.RelativeTolerance
            cvode.AbsoluteTolerance = source.AbsoluteTolerance
            cvode.MaxOrder = source.MaxOrder
            cvode.MaxSteps = source.MaxSteps
            cvode.MaxStep = source.MaxStep
            cvode.MinStep = source.MinStep
            cvode.InitialStep = source.InitialStep
            cvode.MaxNewtonIterations = source.MaxNewtonIterations
            cvode.NewtonConvergenceFactor = source.NewtonConvergenceFactor
            cvode.MaxGrowthFactor = source.MaxGrowthFactor
            cvode.MinReductionFactor = source.MinReductionFactor
            cvode.SafetyFactor = source.SafetyFactor
            cvode.JacobianUpdateFrequency = source.JacobianUpdateFrequency
        End If

        If options IsNot Nothing Then
            If options.Rtol > 0 Then
                cvode.RelativeTolerance = options.Rtol
            End If
            If options.Atol > 0 Then
                cvode.AbsoluteTolerance = options.Atol
            End If
            If options.InitialStep > 0 Then
                cvode.InitialStep = options.InitialStep
            End If
            If options.MaxSteps > 0 Then
                cvode.MaxSteps = options.MaxSteps
            End If
        End If

        ' 每一个输出采样点至少需要推进一个内部步，故 MaxSteps 至少要覆盖整个输出网格，
        ' 否则 CVODE 会在最后一个采样点之前返回 TooManySteps
        If cvode.MaxSteps < n Then
            cvode.MaxSteps = n
        End If

        Return cvode
    End Function

    ''' <summary>
    ''' Resolve the variable names of the ODEs model, when it is not available the name will be
    ''' generated as ``y0``, ``y1``, ...
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="size"></param>
    ''' <returns></returns>
    Private Shared Function ResolveNames(model As ODEs, size As Integer) As String()
        Try
            Dim names As String() = ODEs.GetVariables(model.GetType()).ToArray

            If names IsNot Nothing AndAlso names.Length = size Then
                Return names
            End If
        Catch ex As Exception
            ' 变量名称只是用于结果的展示，解析失败的时候回退到默认名称
        End Try

        Dim fallback As String() = New String(size - 1) {}

        For i As Integer = 0 To size - 1
            fallback(i) = $"y{i}"
        Next

        Return fallback
    End Function
End Class
