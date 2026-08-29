
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 损失函数集合
''' 损失函数衡量模型预测与真实值之间的差距
''' </summary>
Public Module Loss
    ''' <summary>
    ''' 均方误差损失 (MSE): L = (1/n) * Σ(y_pred - y_true)²
    ''' 常用于回归问题
    ''' </summary>
    Public Function MeanSquaredError(predicted As Tensor, target As Tensor) As Single
        If Not predicted.Shape.SequenceEqual(target.Shape) Then Throw New ArgumentException("预测值和目标值形状必须相同")

        Dim sumSquaredError As Single = 0
        For i = 0 To predicted.Length - 1
            Dim diff = predicted(i) - target(i)
            sumSquaredError += diff * diff
        Next

        Return sumSquaredError / predicted.Length
    End Function

    ''' <summary>
    ''' MSE损失的梯度
    ''' </summary>
    Public Function MeanSquaredErrorGradient(predicted As Tensor, target As Tensor) As Tensor
        Return (predicted - target) * (2.0F / predicted.Length)
    End Function

    ''' <summary>
    ''' 交叉熵损失 (Cross Entropy)
    ''' 常用于分类问题
    ''' L = -Σ y_true * log(y_pred)
    ''' </summary>
    Public Function CrossEntropy(predicted As Tensor, target As Tensor) As Single
        Dim loss As Single = 0
        Dim epsilon = 0.0000001F ' 防止log(0)

        For i = 0 To predicted.Length - 1
            Dim p = std.Max(std.Min(predicted(i), 1 - epsilon), epsilon)
            loss -= target(i) * CSng(std.Log(p))
        Next

        Return loss / predicted.Shape(0) ' 平均每个样本的损失
    End Function

    ''' <summary>
    ''' 二元交叉熵损失
    ''' 用于二分类问题
    ''' </summary>
    Public Function BinaryCrossEntropy(predicted As Single, target As Single) As Single
        Dim epsilon = 0.0000001F
        Dim p = std.Max(std.Min(predicted, 1 - epsilon), epsilon)
        Return -(target * CSng(std.Log(p)) + (1 - target) * CSng(std.Log(1 - p)))
    End Function

    ''' <summary>
    ''' 负对数似然损失
    ''' 用于多分类问题（配合Softmax使用）
    ''' </summary>
    Public Function NegativeLogLikelihood(logits As Tensor, targetClass As Integer) As Single
        ' 使用log-softmax提高数值稳定性
        Dim maxLogit = Single.MinValue
        For i = 0 To logits.Shape(1) - 1
            If logits(0, i) > maxLogit Then maxLogit = logits(0, i)
        Next

        Dim sumExp As Single = 0
        For i = 0 To logits.Shape(1) - 1
            sumExp += CSng(std.Exp(logits(0, i) - maxLogit))
        Next

        Dim logSumExp = maxLogit + CSng(std.Log(sumExp))
        Return logSumExp - logits(0, targetClass)
    End Function

    ''' <summary>
    ''' Softmax交叉熵损失（组合函数，更高效）
    ''' </summary>
    Public Function SoftmaxCrossEntropy(logits As Tensor, targetClass As Integer) As Single
        Return NegativeLogLikelihood(logits, targetClass)
    End Function
End Module
