Imports std = System.Math

''' <summary>
''' Adam 优化器
''' 
''' Adam Optimizer
''' ==============
''' 参数更新规则：
'''   m_t = β₁ * m_{t-1} + (1 - β₁) * g_t
'''   v_t = β₂ * v_{t-1} + (1 - β₂) * g_t²
'''   m̂_t = m_t / (1 - β₁^t)
'''   v̂_t = v_t / (1 - β₂^t)
'''   θ_t = θ_{t-1} - lr * m̂_t / (√v̂_t + ε)
''' 
''' 每个参数张量维护自己的 m 和 v 状态。
''' </summary>
Public Class AdamOptimizer

    ''' <summary>学习率</summary>
    Public Property LearningRate As Double = 0.01

    ''' <summary>一阶矩衰减率</summary>
    Public Property Beta1 As Double = 0.9

    ''' <summary>二阶矩衰减率</summary>
    Public Property Beta2 As Double = 0.999

    ''' <summary>数值稳定项</summary>
    Public Property Epsilon As Double = 1e-8

    ''' <summary>时间步</summary>
    Private _t As Integer = 0

    Public Sub New(Optional lr As Double = 0.01)
        Me.LearningRate = lr
    End Sub

    Public Sub IncrementStep()
        _t += 1
    End Sub

    ''' <summary>
    ''' 对一个参数张量执行 Adam 更新
    ''' Perform an Adam update on a single parameter tensor.
    ''' </summary>
    ''' <param name="param">参数张量（会被原地修改）</param>
    ''' <param name="grad">梯度张量</param>
    ''' <param name="m">一阶矩状态（会被原地修改）</param>
    ''' <param name="v">二阶矩状态（会被原地修改）</param>
    ''' <param name="lrScale">学习率缩放因子（用于不同参数组的不同学习率）</param>
    Public Sub [Step](param As Tensor, grad As Tensor, m As Tensor, v As Tensor, Optional lrScale As Double = 1.0)
        Dim n = param.Length
        Dim bc1 = 1.0 - std.Pow(Beta1, _t)
        Dim bc2 = 1.0 - std.Pow(Beta2, _t)
        Dim lr = LearningRate * lrScale

        For i = 0 To n - 1
            Dim g = grad(i)
            ' m = β₁ * m + (1 - β₁) * g
            m(i) = Beta1 * m(i) + (1 - Beta1) * g
            ' v = β₂ * v + (1 - β₂) * g²
            v(i) = Beta2 * v(i) + (1 - Beta2) * g * g
            ' 偏差校正
            Dim mHat = m(i) / bc1
            Dim vHat = v(i) / bc2
            ' 更新参数
            param(i) = param(i) - lr * mHat / (std.Sqrt(vHat) + Epsilon)
        Next
    End Sub

    ''' <summary>
    ''' 对整个高斯模型执行一次 Adam 更新
    ''' </summary>
    Public Sub StepModel(model As GaussianModel)
        IncrementStep()
        ' 位置：标准学习率
        [Step](model.Positions, model.PositionsGrad, model.PositionsM, model.PositionsV, 1.0)
        ' 缩放：较低学习率（防止数值不稳定）
        [Step](model.Scales, model.ScalesGrad, model.ScalesM, model.ScalesV, 0.1)
        ' 旋转：较低学习率
        [Step](model.Rotations, model.RotationsGrad, model.RotationsM, model.RotationsV, 0.1)
        ' 颜色：标准学习率
        [Step](model.Colors, model.ColorsGrad, model.ColorsM, model.ColorsV, 1.0)
        ' 不透明度：较低学习率
        [Step](model.Opacities, model.OpacitiesGrad, model.OpacitiesM, model.OpacitiesV, 0.1)
    End Sub

End Class


''' <summary>
''' 学习率调度器（余弦退火 + 阶梯衰减）
''' </summary>
Public Class LRScheduler

    Private _initialLr As Double
    Private _finalLr As Double
    Private _totalSteps As Integer

    Public Sub New(initialLr As Double, finalLr As Double, totalSteps As Integer)
        _initialLr = initialLr
        _finalLr = finalLr
        _totalSteps = totalSteps
    End Sub

    ''' <summary>
    ''' 获取第 step 步的学习率（余弦退火）
    ''' </summary>
    Public Function GetLr([step] As Integer) As Double
        If [step] >= _totalSteps Then Return _finalLr
        Dim progress = [step] / _totalSteps
        Dim cosine = 0.5 * (1 + std.Cos(std.PI * progress))
        Return _finalLr + (_initialLr - _finalLr) * cosine
    End Function

End Class
