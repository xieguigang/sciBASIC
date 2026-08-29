

''' <summary>
''' 激活函数工具类
''' 提供统一的激活函数调用接口
''' </summary>
Public Module ActivationFunctions
    ''' <summary>
    ''' 应用激活函数
    ''' </summary>
    Public Function Apply(input As Tensor, type As ActivationType) As Tensor
        Select Case type
            Case ActivationType.None : Return input.Clone()
            Case ActivationType.ReLU : Return input.Apply(AddressOf Activation.ReLU)
            Case ActivationType.Sigmoid : Return input.Apply(AddressOf Activation.Sigmoid)
            Case ActivationType.Tanh : Return input.Apply(AddressOf Activation.Tanh)
            Case ActivationType.LeakyReLU : Return input.Apply(Function(x) Activation.LeakyReLU(x))
            Case ActivationType.Softmax : Return Activation.Softmax(input)
            Case Else
                Throw New ArgumentException($"未知的激活函数类型: {type}")
        End Select
    End Function

    ''' <summary>
    ''' 计算激活函数的导数
    ''' </summary>
    Public Function Derivative(input As Tensor, type As ActivationType) As Tensor
        Select Case type
            Case ActivationType.None : Return Tensor.Filled(input.Shape, 1.0F)
            Case ActivationType.ReLU : Return input.Apply(AddressOf Activation.ReLUDerivative)
            Case ActivationType.Sigmoid : Return input.Apply(AddressOf Activation.SigmoidDerivative)
            Case ActivationType.Tanh : Return input.Apply(AddressOf Activation.TanhDerivative)
            Case ActivationType.LeakyReLU : Return input.Apply(Function(x) Activation.LeakyReLUDerivative(x))
            Case Else
                Throw New ArgumentException($"不支持的激活函数导数: {type}")
        End Select
    End Function
End Module
