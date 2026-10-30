Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' 把 CVODE 求解器的积分结果统一输出为 <see cref="ODEsOut"/> 的扩展方法。
''' </summary>
''' <remarks>
''' <see cref="ODEsOut"/> 是 ODE 求解器框架的统一结果模型（原来 CVODE 专用的 ``ODESolution``
''' 已经删除），它同时提供 ``x`` / ``y`` / ``y0`` / ``params`` 的原始数据与 ``Count`` /
''' ``Item(index)`` / ``GetComponent(i)`` / ``ToCSV()`` 这类通用访问接口，因此 CVODE 的调用方
''' 可以与内建求解器的结果采用完全一致的处理方式。
''' </remarks>
Public Module CVODESolverExtensions

    ''' <summary>
    ''' Solve the ODE system over ``[t0, tEnd]`` and output ``n + 1`` uniform sampling points.
    ''' (求解 ``[t0, tEnd]`` 并输出 ``n + 1`` 个等距采样点)
    ''' </summary>
    ''' <param name="solver">The initialized CVODE solver.(CVODE 求解器)</param>
    ''' <param name="t0">The lower bound of the integration range.(积分下界)</param>
    ''' <param name="tEnd">The upper bound of the integration range.(积分上界)</param>
    ''' <param name="y0">The initial state vector.(初始状态)</param>
    ''' <param name="n">The number of the integration steps.(积分步数)</param>
    ''' <param name="names">
    ''' The variable names of the result columns, when this value is not presented the name will
    ''' be generated as ``y0``, ``y1``, ...(结果列的名称，缺省为 y0, y1, ...)
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function Solve(solver As CVODESolver, t0 As Double, tEnd As Double, y0 As Vector,
                          n As Integer, Optional names As String() = Nothing) As ODEsOut

        If solver Is Nothing Then
            Throw New ArgumentNullException(NameOf(solver))
        End If
        If y0 Is Nothing OrElse y0.Length = 0 Then
            Throw New ArgumentException("The initial state vector can not be empty!", NameOf(y0))
        End If
        If n < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(n), "The number of the integration steps must be positive!")
        End If
        If tEnd <= t0 Then
            Throw New ArgumentOutOfRangeException(NameOf(tEnd), "The upper bound of the integration range must be greater than the lower bound!")
        End If

        Dim size As Integer = y0.Length
        Dim grid As Double() = ODESequence.CreatePoints(t0, tEnd, n)
        Dim columns As Double()() = New Double(size - 1)() {}

        For i As Integer = 0 To size - 1
            columns(i) = New Double(n) {}
            columns(i)(0) = y0(i)
        Next

        Dim status As CVODEStatus = solver.Initialize(t0, y0)

        If status <> CVODEStatus.Success Then
            Throw Fail(status, t0, solver)
        End If

        Dim yout As New Vector(size)

        For i As Integer = 1 To n
            status = solver.Integrate(grid(i), yout)

            If status <> CVODEStatus.Success AndAlso status <> CVODEStatus.TStopReturn Then
                Throw Fail(status, grid(i), solver)
            End If

            For k As Integer = 0 To size - 1
                columns(k)(i) = yout(k)
            Next
        Next

        Dim result As ODEsOut = ODEsOut.FromColumns(grid, columns, names)

        result.info = New Dictionary(Of String, String) From {
            {"solver", NameOf(CVODESolver)},
            {"steps", solver.TotalSteps.ToString()},
            {"rhs_evaluations", solver.RHSFunctionEvaluations.ToString()},
            {"newton_iterations", solver.NewtonIterations.ToString()},
            {"linear_solves", solver.LinearSolves.ToString()},
            {"max_order", solver.CurrentOrder.ToString()}
        }

        Return result
    End Function

    ''' <summary>
    ''' Create a CVODE solver for a single integration run and output the result as
    ''' <see cref="ODEsOut"/>, the solver will be disposed automatically.
    ''' (一次性创建求解器并输出统一结果，求解器会自动释放)
    ''' </summary>
    ''' <param name="method"></param>
    ''' <param name="rhs"></param>
    ''' <param name="t0"></param>
    ''' <param name="tEnd"></param>
    ''' <param name="y0"></param>
    ''' <param name="n"></param>
    ''' <param name="names"></param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    Public Function Solve(method As CVODEMethod, rhs As RHSFunction, t0 As Double, tEnd As Double,
                          y0 As Vector, n As Integer, Optional names As String() = Nothing,
                          Optional options As CVODEOptions = Nothing) As ODEsOut

        If rhs Is Nothing Then
            Throw New ArgumentNullException(NameOf(rhs))
        End If
        If y0 Is Nothing Then
            Throw New ArgumentNullException(NameOf(y0))
        End If

        Using solver As New CVODESolver(method, rhs, y0.Length, options)
            Return solver.Solve(t0, tEnd, y0, n, names)
        End Using
    End Function

    ''' <summary>
    ''' Create the exception which describes the CVODE failure status.
    ''' </summary>
    ''' <param name="status"></param>
    ''' <param name="t"></param>
    ''' <param name="solver"></param>
    ''' <returns></returns>
    Private Function Fail(status As CVODEStatus, t As Double, solver As CVODESolver) As InvalidOperationException
        Return New InvalidOperationException(
            $"The CVODE integration is failed at t = {t} (status = {status}, steps = {solver.TotalSteps}, rhs evaluations = {solver.RHSFunctionEvaluations})!")
    End Function
End Module
