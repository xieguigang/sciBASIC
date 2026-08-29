
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 随机梯度下降优化器 (SGD)
''' 最基础的优化器：θ = θ - lr * ∇θ
''' </summary>
Public Class SGDOptimizer
    Inherits Optimizer
    ''' <summary>
    ''' 动量系数
    ''' </summary>
    Public Property Momentum As Single

    ''' <summary>
    ''' 速度（动量）缓存
    ''' </summary>
    Private _velocities As List(Of Tensor)

    ''' <summary>
    ''' 创建SGD优化器
    ''' </summary>
    ''' <param name="parameters">需要优化的参数</param>
    ''' <param name="gradients">参数对应的梯度</param>
    ''' <param name="learningRate">学习率</param>
    ''' <param name="momentum">动量系数（0表示不使用动量）</param>
    Public Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), Optional learningRate As Single = 0.01F, Optional momentum As Single = 0.0F)

        MyBase.New(parameters, gradients, learningRate)
        Me.Momentum = momentum

        ' 初始化速度缓存
        _velocities = New List(Of Tensor)()
        For Each param In parameters
            _velocities.Add(New Tensor(param.Shape))
        Next
    End Sub

    Public Overrides Sub [Step]()
        For i = 0 To _parameters.Count - 1
            Dim param = _parameters(i)
            Dim grad = _gradients(i)
            Dim velocity = _velocities(i)

            For j = 0 To param.Length - 1
                If Momentum > 0 Then
                    ' 带动量的更新
                    ' v = momentum * v - lr * grad
                    ' param = param + v
                    velocity(j) = Momentum * velocity(j) - LearningRate * grad(j)
                    param(j) += velocity(j)
                Else
                    ' 普通SGD
                    param(j) -= LearningRate * grad(j)
                End If
            Next
        Next
    End Sub
End Class
