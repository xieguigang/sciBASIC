Option Strict On
Option Explicit On

Imports System
Imports std = System.Math

''' <summary>
''' 前提参数的优化器（SGD / 动量 / Adam / AdamW）。
''' </summary>
''' <remarks>
''' <para>
''' 关键设计：**必须为正的参数（各隶属函数的宽度、斜率）在内部以对数参数化**。
''' 设 <c>s = ln p</c>，则 <c>dE/ds = p * dE/dp</c>，更新在 <c>s</c> 空间完成后再
''' <c>p = exp(s)</c> 回写。这样宽度天然保持为正，不需要每步做 <c>max(p, eps)</c> 的
''' 投影（那种投影在 Adam 下会把二阶矩估计污染掉，导致参数卡在下界附近不动）。
''' </para>
''' <para>
''' 一阶/二阶矩在 <c>s</c> 空间维护，因此跨轮次是自洽的；
''' 学习率与权重衰减也都在 <c>s</c> 空间施加。
''' </para>
''' </remarks>
Public Class AnfisOptimizer

    Private ReadOnly _kind As AnfisOptimizerKind
    Private ReadOnly _positive As Boolean()

    Private ReadOnly _momentumBuffer As Double()
    Private ReadOnly _firstMoment As Double()
    Private ReadOnly _secondMoment As Double()

    Private _step As Integer

    Private ReadOnly _momentum As Double
    Private ReadOnly _beta1 As Double
    Private ReadOnly _beta2 As Double
    Private ReadOnly _epsilon As Double
    Private ReadOnly _weightDecay As Double

    ''' <summary>创建优化器。</summary>
    ''' <param name="parameterCount">参数个数</param>
    ''' <param name="positiveMask">哪些参数必须保持为正（内部走对数空间）</param>
    ''' <param name="options">配置（优化器类型、动量/Adam 超参数、权重衰减）</param>
    Public Sub New(parameterCount As Integer, positiveMask As Boolean(), options As AnfisOptions)
        If parameterCount <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(parameterCount), parameterCount, "the parameter count must be positive")
        End If
        If positiveMask Is Nothing OrElse positiveMask.Length <> parameterCount Then
            Throw New ArgumentException($"the positive mask must have {parameterCount} elements", NameOf(positiveMask))
        End If
        If options Is Nothing Then
            Throw New ArgumentNullException(NameOf(options))
        End If

        _kind = options.Optimizer
        _positive = positiveMask
        _momentum = options.Momentum
        _beta1 = options.Beta1
        _beta2 = options.Beta2
        _epsilon = options.Epsilon
        _weightDecay = options.WeightDecay

        _momentumBuffer = New Double(parameterCount - 1) {}
        _firstMoment = New Double(parameterCount - 1) {}
        _secondMoment = New Double(parameterCount - 1) {}
        _step = 0
    End Sub

    ''' <summary>已经执行的更新步数。</summary>
    Public ReadOnly Property Step As Integer
        Get
            Return _step
        End Get
    End Property

    ''' <summary>
    ''' 按梯度就地更新参数。
    ''' </summary>
    ''' <param name="parameters">当前参数（会被就地更新）</param>
    ''' <param name="gradients">损失对参数的梯度</param>
    ''' <param name="learningRate">本步学习率</param>
    ''' <param name="gradientClip">梯度 L2 范数上限，&lt;= 0 表示不裁剪</param>
    Public Sub ApplyGradients(parameters As Double(), gradients As Double(), learningRate As Double, Optional gradientClip As Double = 5.0)
        If parameters Is Nothing OrElse parameters.Length <> _positive.Length Then
            Throw New ArgumentException("the parameter vector does not match the optimizer", NameOf(parameters))
        End If
        If gradients Is Nothing OrElse gradients.Length <> _positive.Length Then
            Throw New ArgumentException("the gradient vector does not match the optimizer", NameOf(gradients))
        End If

        Dim count As Integer = parameters.Length

        ' 梯度裁剪：按整体 L2 范数缩放（比逐元素裁剪更能保持方向）
        Dim scale As Double = 1.0

        If gradientClip > 0.0 Then
            Dim norm As Double = 0.0

            For i As Integer = 0 To count - 1
                norm += gradients(i) * gradients(i)
            Next

            norm = std.Sqrt(norm)

            If norm > gradientClip AndAlso norm > 0.0 Then
                scale = gradientClip / norm
            End If
        End If

        _step += 1

        Dim correction1 As Double = 1.0 - std.Pow(_beta1, _step)
        Dim correction2 As Double = 1.0 - std.Pow(_beta2, _step)

        For i As Integer = 0 To count - 1
            Dim p As Double = parameters(i)
            Dim g As Double = gradients(i) * scale
            Dim s As Double
            Dim inLogSpace As Boolean = _positive(i)

            If inLogSpace Then
                If p < 0.000001 Then
                    p = 0.000001
                End If

                s = std.Log(p)
                g = g * p
            Else
                s = p
            End If

            If Double.IsNaN(g) OrElse Double.IsInfinity(g) Then
                Continue For
            End If

            Dim updated As Double = s

            Select Case _kind
                Case AnfisOptimizerKind.GradientDescent
                    updated = s - learningRate * g
                Case AnfisOptimizerKind.Momentum
                    _momentumBuffer(i) = _momentum * _momentumBuffer(i) + g
                    updated = s - learningRate * _momentumBuffer(i)
                Case AnfisOptimizerKind.Adam, AnfisOptimizerKind.AdamW
                    _firstMoment(i) = _beta1 * _firstMoment(i) + (1.0 - _beta1) * g
                    _secondMoment(i) = _beta2 * _secondMoment(i) + (1.0 - _beta2) * g * g

                    Dim mHat As Double = _firstMoment(i) / correction1
                    Dim vHat As Double = _secondMoment(i) / correction2
                    updated = s - learningRate * mHat / (std.Sqrt(vHat) + _epsilon)

                    If _kind = AnfisOptimizerKind.AdamW AndAlso _weightDecay > 0.0 Then
                        ' 解耦权重衰减：直接在参数空间收缩（对数空间里就是减去一个常数）
                        updated -= learningRate * _weightDecay
                    End If
                Case Else
                    updated = s - learningRate * g
            End Select

            If inLogSpace Then
                If updated > 700.0 Then
                    updated = 700.0
                ElseIf updated < -700.0 Then
                    updated = -700.0
                End If

                parameters(i) = std.Exp(updated)
            Else
                parameters(i) = updated
            End If
        Next
    End Sub

    ''' <summary>清零动量 / 矩估计（例如更换数据分布后重新训练）。</summary>
    Public Sub Reset()
        Call Array.Clear(_momentumBuffer, 0, _momentumBuffer.Length)
        Call Array.Clear(_firstMoment, 0, _firstMoment.Length)
        Call Array.Clear(_secondMoment, 0, _secondMoment.Length)
        _step = 0
    End Sub

End Class
