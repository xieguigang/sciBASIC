Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 神经网络激活函数集合
''' </summary>
Public Module ActivationFunctions

    ''' <summary>
    ''' Sigmoid激活函数: σ(x) = 1 / (1 + e^(-x))
    ''' 输出范围: (0, 1)
    ''' </summary>
    Public Function Sigmoid(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) As Double
                           If v < -20 Then Return 0.0
                           If v > 20 Then Return 1.0
                           Return 1.0 / (1.0 + std.Exp(-v))
                       End Function)
    End Function

    ''' <summary>
    ''' Sigmoid导数: σ'(x) = σ(x) * (1 - σ(x))
    ''' </summary>
    Public Function SigmoidDerivative(sigmoidOutput As Tensor) As Tensor
        Return sigmoidOutput.Apply(Function(v As Double) v * (1.0 - v))
    End Function

    ''' <summary>
    ''' Tanh激活函数: tanh(x)
    ''' 输出范围: (-1, 1)
    ''' </summary>
    Public Function Tanh(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) std.Tanh(v))
    End Function

    ''' <summary>
    ''' Tanh导数: tanh'(x) = 1 - tanh(x)^2
    ''' </summary>
    Public Function TanhDerivative(tanhOutput As Tensor) As Tensor
        Return tanhOutput.Apply(Function(v As Double) 1.0 - v * v)
    End Function

    ''' <summary>
    ''' ReLU激活函数: max(0, x)
    ''' </summary>
    Public Function ReLU(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) std.Max(0.0, v))
    End Function

    ''' <summary>
    ''' ReLU导数
    ''' </summary>
    Public Function ReLUDerivative(x As Tensor) As Tensor
        Return x.Apply(Function(v As Double) If(v > 0, 1.0, 0.0))
    End Function

    ''' <summary>
    ''' Leaky ReLU: max(αx, x)，其中α通常为0.01
    ''' </summary>
    Public Function LeakyReLU(x As Tensor, Optional alpha As Double = 0.01) As Tensor
        Return x.Apply(Function(v As Double) If(v > 0, v, alpha * v))
    End Function

    ''' <summary>
    ''' Softmax激活函数（用于多分类输出层）
    ''' </summary>
    Public Function Softmax(x As Tensor) As Tensor
        ' 减去最大值以提高数值稳定性
        Dim maxVal = Double.MinValue
        For i = 0 To x.Length - 1
            maxVal = std.Max(maxVal, x(i))
        Next

        Dim expSum = 0.0
        Dim expValues = New Double(x.Length - 1) {}

        For i = 0 To x.Length - 1
            expValues(i) = std.Exp(x(i) - maxVal)
            expSum += expValues(i)
        Next

        Dim result = New Tensor(x.Shape)
        For i = 0 To x.Length - 1
            result(i) = expValues(i) / expSum
        Next

        Return result
    End Function

End Module
