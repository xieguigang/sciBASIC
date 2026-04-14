
''' <summary>
''' CVODE求解器构建器
''' 提供流式API创建求解器
''' </summary>
Public Class CVODEBuilder

    Private _method As CVODEMethod = CVODEMethod.BDF
    Private _rhsFunc As RHSFunction
    Private _jacobianFunc As JacobianFunction
    Private _dimension As Integer
    Private _options As New CVODEOptions()

    ''' <summary>
    ''' 设置求解方法
    ''' </summary>
    Public Function UseMethod(method As CVODEMethod) As CVODEBuilder
        _method = method
        Return Me
    End Function

    ''' <summary>
    ''' 设置右端函数
    ''' </summary>
    Public Function WithRHS(rhsFunc As RHSFunction, dimension As Integer) As CVODEBuilder
        _rhsFunc = rhsFunc
        _dimension = dimension
        Return Me
    End Function

    ''' <summary>
    ''' 设置Jacobian函数
    ''' </summary>
    Public Function WithJacobian(jacFunc As JacobianFunction) As CVODEBuilder
        _jacobianFunc = jacFunc
        Return Me
    End Function

    ''' <summary>
    ''' 设置相对误差容差
    ''' </summary>
    Public Function WithRelativeTolerance(rtol As Double) As CVODEBuilder
        _options.RelativeTolerance = rtol
        Return Me
    End Function

    ''' <summary>
    ''' 设置绝对误差容差
    ''' </summary>
    Public Function WithAbsoluteTolerance(atol As Double) As CVODEBuilder
        _options.AbsoluteTolerance = atol
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大阶数
    ''' </summary>
    Public Function WithMaxOrder(maxOrder As Integer) As CVODEBuilder
        _options.MaxOrder = maxOrder
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大步数
    ''' </summary>
    Public Function WithMaxSteps(maxSteps As Integer) As CVODEBuilder
        _options.MaxSteps = maxSteps
        Return Me
    End Function

    ''' <summary>
    ''' 设置最大步长
    ''' </summary>
    Public Function WithMaxStep(maxStep As Double) As CVODEBuilder
        _options.MaxStep = maxStep
        Return Me
    End Function

    ''' <summary>
    ''' 设置初始步长
    ''' </summary>
    Public Function WithInitialStep(h0 As Double) As CVODEBuilder
        _options.InitialStep = h0
        Return Me
    End Function

    ''' <summary>
    ''' 构建求解器
    ''' </summary>
    Public Function Build() As CVODESolver
        If _rhsFunc Is Nothing Then
            Throw New InvalidOperationException("必须设置右端函数")
        End If
        If _dimension <= 0 Then
            Throw New InvalidOperationException("必须设置问题维度")
        End If

        Dim solver As New CVODESolver(_method, _rhsFunc, _dimension, _options)

        If _jacobianFunc IsNot Nothing Then
            solver.SetJacobianFunction(_jacobianFunc)
        End If

        Return solver
    End Function

End Class
