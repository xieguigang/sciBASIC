
''' <summary>
''' 优化器基类
''' 定义了参数更新的接口
''' </summary>
Public MustInherit Class Optimizer
    ''' <summary>
    ''' 学习率
    ''' </summary>
    Public Property LearningRate As Single

    ''' <summary>
    ''' 需要优化的参数列表
    ''' </summary>
    Protected _parameters As List(Of Tensor)

    ''' <summary>
    ''' 参数对应的梯度列表
    ''' </summary>
    Protected _gradients As List(Of Tensor)

    Protected Sub New(parameters As List(Of Tensor), gradients As List(Of Tensor), learningRate As Single)
        If parameters.Count <> gradients.Count Then Throw New ArgumentException("参数数量和梯度数量必须相同")

        _parameters = parameters
        _gradients = gradients
        Me.LearningRate = learningRate
    End Sub

    ''' <summary>
    ''' 执行一步参数更新
    ''' </summary>
    Public MustOverride Sub [Step]()

    ''' <summary>
    ''' 清零梯度
    ''' </summary>
    Public Overridable Sub ZeroGrad()
        For Each grad In _gradients
            For i = 0 To grad.Length - 1
                grad(i) = 0
            Next
        Next
    End Sub
End Class