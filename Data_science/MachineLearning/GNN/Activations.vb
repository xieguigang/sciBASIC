Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 激活函数集合
''' 激活函数为神经网络引入非线性，使网络能够学习复杂的模式
''' </summary>
Public Module Activation
    ''' <summary>
    ''' ReLU激活函数: f(x) = max(0, x)
    ''' 优点：计算简单，缓解梯度消失问题
    ''' 缺点：存在"死亡ReLU"问题（负值永远为0）
    ''' </summary>
    Public Function ReLU(x As Single) As Single
        Return std.Max(0, x)
    End Function

    ''' <summary>
    ''' ReLU的导数
    ''' </summary>
    Public Function ReLUDerivative(x As Single) As Single
        Return If(x > 0, 1.0F, 0.0F)
    End Function

    ''' <summary>
    ''' Sigmoid激活函数: f(x) = 1 / (1 + e^(-x))
    ''' 输出范围: (0, 1)
    ''' 常用于二分类问题的输出层
    ''' </summary>
    Public Function Sigmoid(x As Single) As Single
        ' 防止数值溢出
        If x > 20 Then Return 1.0F
        If x < -20 Then Return 0.0F
        Return 1.0F / (1.0F + CSng(std.Exp(-x)))
    End Function

    ''' <summary>
    ''' Sigmoid的导数
    ''' </summary>
    Public Function SigmoidDerivative(x As Single) As Single
        Dim s = Sigmoid(x)
        Return s * (1 - s)
    End Function

    ''' <summary>
    ''' Tanh激活函数: f(x) = (e^x - e^(-x)) / (e^x + e^(-x))
    ''' 输出范围: (-1, 1)
    ''' 零中心化，收敛速度通常比sigmoid快
    ''' </summary>
    Public Function Tanh(x As Single) As Single
        Return std.Tanh(x)
    End Function

    ''' <summary>
    ''' Tanh的导数
    ''' </summary>
    Public Function TanhDerivative(x As Single) As Single
        Dim t = Tanh(x)
        Return 1 - t * t
    End Function

    ''' <summary>
    ''' LeakyReLU激活函数: f(x) = x if x > 0 else alpha * x
    ''' 解决ReLU的"死亡"问题
    ''' </summary>
    Public Function LeakyReLU(x As Single, Optional alpha As Single = 0.01F) As Single
        Return If(x > 0, x, alpha * x)
    End Function

    ''' <summary>
    ''' LeakyReLU的导数
    ''' </summary>
    Public Function LeakyReLUDerivative(x As Single, Optional alpha As Single = 0.01F) As Single
        Return If(x > 0, 1.0F, alpha)
    End Function

    ''' <summary>
    ''' Softmax函数
    ''' 将向量转换为概率分布，所有元素和为1
    ''' 常用于多分类问题的输出层
    ''' </summary>
    ''' <param name="input">输入向量</param>
    ''' <returns>概率分布向量</returns>
    Public Function Softmax(input As Single()) As Single()
        ' 数值稳定性：减去最大值
        Dim maxVal As Single = input.Max()
        Dim expValues = New Single(input.Length - 1) {}
        Dim sumExp As Single = 0

        For i = 0 To input.Length - 1
            expValues(i) = CSng(std.Exp(input(i) - maxVal))
            sumExp += expValues(i)
        Next

        Dim result = New Single(input.Length - 1) {}
        For i = 0 To input.Length - 1
            result(i) = expValues(i) / sumExp
        Next

        Return result
    End Function

    ''' <summary>
    ''' 对张量的每一行应用Softmax
    ''' </summary>
    Public Function Softmax(input As Tensor) As Tensor
        If input.Rank <> 2 Then Throw New ArgumentException("Softmax只支持二维张量")

        Dim result = New Tensor(input.Shape)
        Dim rows = input.Shape(0)
        Dim cols = input.Shape(1)

        For i = 0 To rows - 1
            ' 提取一行
            Dim row = New Single(cols - 1) {}
            For j = 0 To cols - 1
                row(j) = input(i, j)
            Next

            ' 应用Softmax
            Dim softmaxRow = Softmax(row)

            ' 写入结果
            For j = 0 To cols - 1
                result(i, j) = softmaxRow(j)
            Next
        Next

        Return result
    End Function
End Module

''' <summary>
''' 激活函数类型枚举
''' </summary>
Public Enum ActivationType
    None       ' 不使用激活函数
    ReLU       ' ReLU
    Sigmoid    ' Sigmoid
    Tanh       ' Tanh
    LeakyReLU  ' LeakyReLU
    Softmax     ' Softmax
End Enum

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


