
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 激活层
''' 对输入应用非线性激活函数
''' </summary>
Public Class ActivationLayer
    Inherits Layer
    Private ReadOnly _activationType As ActivationType
    Private _lastInput As Tensor

    Public Sub New(type As ActivationType, Optional name As String = Nothing)
        _activationType = type
        MyBase.Name = If(name, $"Activation_{type}")
    End Sub

    Public Overrides Function Forward(input As Tensor) As Tensor
        _lastInput = input
        Return Apply(input, _activationType)
    End Function

    Public Overrides Function Backward(gradient As Tensor) As Tensor
        Dim activationDerivative = Derivative(_lastInput, _activationType)
        Return gradient.ElementwiseMultiply(activationDerivative)
    End Function

    Public Overrides Function GetParameters() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
    Public Overrides Function GetGradients() As List(Of Tensor)
        Return New List(Of Tensor)()
    End Function
End Class