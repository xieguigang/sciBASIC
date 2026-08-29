Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' Adam优化器
''' 自适应学习率优化器，结合了动量和RMSprop的优点
''' 论文: Adam: A Method for Stochastic Optimization (Kingma & Ba, ICLR 2015)
''' </summary>
Public Class AdamOptimizer
    Inherits Optimizer
    ''' <summary>
    ''' 一阶矩估计的指数衰减率
    ''' </summary>
    Public Property Beta1 As Single

    ''' <summary>
    ''' 二阶矩估计的指数衰减率
    ''' </summary>
    Public Property Beta2 As Single

    ''' <summary>
    ''' 数值稳定性常数
    ''' </summary>
    Public Property Epsilon As Single

    ''' <summary>
    ''' 当前时间步
    ''' </summary>
    Private _t As Integer

    ''' <summary>
    ''' 一阶矩估计（梯度的移动平均）
    ''' </summary>
    Private _m As List(Of Tensor)

    ''' <summary>
    ''' 二阶矩估计（梯度平方的移动平均）
    ''' </summary>
    Private _v As List(Of Tensor)

    ''' <summary>
    ''' 创建Adam优化器
    ''' </summary>
    Public Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), Optional learningRate As Single = 0.001F, Optional beta1 As Single = 0.9F, Optional beta2 As Single = 0.999F, Optional epsilon As Single = 0.00000001F)

        MyBase.New(parameters, gradients, learningRate)
        Me.Beta1 = beta1
        Me.Beta2 = beta2
        Me.Epsilon = epsilon
        _t = 0

        ' 初始化矩估计
        _m = New List(Of Tensor)()
        _v = New List(Of Tensor)()
        For Each param In parameters
            _m.Add(New Tensor(param.Shape))
            _v.Add(New Tensor(param.Shape))
        Next
    End Sub

    Public Overrides Sub [Step]()
        _t += 1

        For i = 0 To _parameters.Count - 1
            Dim param = _parameters(i)
            Dim grad = _gradients(i)
            Dim m = _m(i)
            Dim v = _v(i)

            For j = 0 To param.Length - 1
                Dim g = grad(j)

                ' 更新一阶矩估计
                m(j) = Beta1 * m(j) + (1 - Beta1) * g

                ' 更新二阶矩估计
                v(j) = Beta2 * v(j) + (1 - Beta2) * g * g

                ' 偏差修正
                Dim mHat = m(j) / (1 - CSng(std.Pow(Beta1, _t)))
                Dim vHat = v(j) / (1 - CSng(std.Pow(Beta2, _t)))

                ' 参数更新
                param(j) -= LearningRate * mHat / (CSng(std.Sqrt(vHat)) + Epsilon)
            Next
        Next
    End Sub
End Class
