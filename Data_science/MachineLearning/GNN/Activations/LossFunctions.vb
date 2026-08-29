
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' 损失函数类型枚举
''' </summary>
Public Enum LossType
    MeanSquaredError   ' 均方误差
    CrossEntropy       ' 交叉熵
    BinaryCrossEntropy ' 二元交叉熵
    SoftmaxCrossEntropy ' Softmax交叉熵
End Enum

''' <summary>
''' 损失函数工具类
''' </summary>
Public Module LossFunctions
    ''' <summary>
    ''' 计算损失值
    ''' </summary>
    Public Function Compute(predicted As Tensor, target As Tensor, type As LossType) As Single
        Select Case type
            Case LossType.MeanSquaredError : Return Loss.MeanSquaredError(predicted, target)
            Case LossType.CrossEntropy : Return Loss.CrossEntropy(predicted, target)
            Case Else
                Throw New ArgumentException($"不支持的损失函数类型: {type}")
        End Select
    End Function

    ''' <summary>
    ''' 计算损失梯度
    ''' </summary>
    Public Function Gradient(predicted As Tensor, target As Tensor, type As LossType) As Tensor
        Select Case type
            Case LossType.MeanSquaredError : Return Loss.MeanSquaredErrorGradient(predicted, target)
            Case Else
                Throw New ArgumentException($"不支持的损失函数梯度: {type}")
        End Select
    End Function
End Module
