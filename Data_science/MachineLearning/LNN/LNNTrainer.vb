Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 液态神经网络训练器
''' 实现基于时间的反向传播(BPTT)训练算法
''' </summary>
Public Class LNNTrainer

#Region "属性"

    ''' <summary>
    ''' 要训练的网络
    ''' </summary>
    Public Property Network As LiquidNeuralNetwork

    ''' <summary>
    ''' 学习率
    ''' </summary>
    Public Property LearningRate As Double = 0.001

    ''' <summary>
    ''' 优化器类型
    ''' </summary>
    Public Property OptimizerType As String = "adam"

    ''' <summary>
    ''' Adam优化器参数 - 一阶矩估计
    ''' </summary>
    Private _AdamM As Dictionary(Of String, Tensor)

    ''' <summary>
    ''' Adam优化器参数 - 二阶矩估计
    ''' </summary>
    Private _AdamV As Dictionary(Of String, Tensor)

    ''' <summary>
    ''' Adam优化器时间步
    ''' </summary>
    Private _AdamT As Integer = 0

    ''' <summary>
    ''' Adam beta1参数
    ''' </summary>
    Public Property AdamBeta1 As Double = 0.9

    ''' <summary>
    ''' Adam beta2参数
    ''' </summary>
    Public Property AdamBeta2 As Double = 0.999

    ''' <summary>
    ''' Adam epsilon参数
    ''' </summary>
    Public Property AdamEpsilon As Double = 0.00000001

    ''' <summary>
    ''' 梯度裁剪阈值
    ''' </summary>
    Public Property GradientClipValue As Double = 1.0

    ''' <summary>
    ''' 是否使用梯度裁剪
    ''' </summary>
    Public Property UseGradientClipping As Boolean = True

#End Region

#Region "构造函数"

    Public Sub New(network As LiquidNeuralNetwork, Optional learningRate As Double = 0.001)
        _Network = network
        _LearningRate = learningRate

        ' 初始化Adam优化器状态
        InitializeAdamState()
    End Sub

    Private Sub InitializeAdamState()
        _AdamM = New Dictionary(Of String, Tensor)()
        _AdamV = New Dictionary(Of String, Tensor)()

        Dim params = _Network.GetParameters()
        For Each kvp In params
            _AdamM.Add(kvp.Key, Tensor.Zeros(kvp.Value.Shape))
            _AdamV.Add(kvp.Key, Tensor.Zeros(kvp.Value.Shape))
        Next
    End Sub

#End Region

#Region "损失函数"

    ''' <summary>
    ''' 计算均方误差损失
    ''' </summary>
    Public Shared Function MSE(predicted As Tensor, target As Tensor) As Double
        If Not predicted.Shape.SequenceEqual(target.Shape) Then
            Throw New ArgumentException("预测值和目标值形状必须相同")
        End If

        Dim sum As Double = 0
        For i = 0 To predicted.Length - 1
            Dim diff = predicted(i) - target(i)
            sum += diff * diff
        Next

        Return sum / predicted.Length
    End Function

    ''' <summary>
    ''' 计算均方误差损失的梯度
    ''' </summary>
    Public Shared Function MSEGradient(predicted As Tensor, target As Tensor) As Tensor
        Dim gradient = Tensor.Zeros(predicted.Shape)
        Dim n = predicted.Length

        For i = 0 To n - 1
            gradient(i) = 2.0 * (predicted(i) - target(i)) / n
        Next

        Return gradient
    End Function

    ''' <summary>
    ''' 计算平均绝对误差损失
    ''' </summary>
    Public Shared Function MAE(predicted As Tensor, target As Tensor) As Double
        Dim sum As Double = 0
        For i = 0 To predicted.Length - 1
            sum += std.Abs(predicted(i) - target(i))
        Next
        Return sum / predicted.Length
    End Function

#End Region

#Region "训练方法"

    ''' <summary>
    ''' 训练单个时间步
    ''' </summary>
    ''' <param name="input">输入</param>
    ''' <param name="target">目标输出</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>损失值</returns>
    Public Function TrainStep(input As Tensor, target As Tensor, Optional dt As Double? = Nothing) As Double
        ' 前向传播
        Dim output = _Network.Forward(input, dt)

        ' 计算损失
        Dim loss = MSE(output, target)

        ' 计算输出层梯度
        Dim outputGradient = MSEGradient(output, target)

        ' 反向传播（简化版本）
        Backpropagate(outputGradient)

        ' 更新参数
        UpdateParameters()

        Return loss
    End Function

    ''' <summary>
    ''' 训练完整序列
    ''' </summary>
    ''' <param name="inputSequence">输入序列</param>
    ''' <param name="targetSequence">目标序列</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>平均损失</returns>
    Public Function TrainSequence(inputSequence As Tensor, targetSequence As Tensor, Optional dt As Double? = Nothing) As Double
        Dim actualDt = If(dt, _Network.DefaultDt)
        Dim seqLength = inputSequence.Shape(0)
        Dim totalLoss As Double = 0

        ' 重置网络状态
        _Network.ResetState()

        ' 逐时间步训练
        For t = 0 To seqLength - 1
            ' 获取当前输入和目标
            Dim currentInput = Tensor.Zeros({_Network.InputSize})
            Dim currentTarget = Tensor.Zeros({_Network.OutputSize})

            For i = 0 To _Network.InputSize - 1
                currentInput(i) = inputSequence(t, i)
            Next

            For i = 0 To _Network.OutputSize - 1
                currentTarget(i) = targetSequence(t, i)
            Next

            ' 训练当前时间步
            totalLoss += TrainStep(currentInput, currentTarget, actualDt)
        Next

        Return totalLoss / seqLength
    End Function

    ''' <summary>
    ''' 反向传播（简化实现）
    ''' </summary>
    Private Sub Backpropagate(outputGradient As Tensor)
        ' 获取网络参数
        Dim hiddenSize = _Network.HiddenSize
        Dim outputSize = _Network.OutputSize

        ' 计算输出层权重梯度
        ' dL/dW_out = hidden^T @ outputGradient
        Dim hiddenState = _Network.LiquidLayer.GetOutputState()
        Dim hiddenReshaped = New Tensor(hiddenState.Data, 1, hiddenSize)
        Dim gradReshaped = New Tensor(outputGradient.Data, outputSize, 1)

        ' 输出权重梯度
        For i = 0 To hiddenSize - 1
            For j = 0 To outputSize - 1
                _Network.OutputWeightGradient(i, j) += hiddenState(i) * outputGradient(j)
            Next
        Next

        ' 输出偏置梯度
        For i = 0 To outputSize - 1
            _Network.OutputBiasGradient(i) += outputGradient(i)
        Next

        ' 传播到隐藏层（简化：使用固定的时间常数导数）
        Dim hiddenGradient = Tensor.Zeros({hiddenSize})
        For i = 0 To hiddenSize - 1
            For j = 0 To outputSize - 1
                hiddenGradient(i) += outputGradient(j) * _Network.OutputWeight(i, j)
            Next
        Next

        ' 梯度裁剪
        If UseGradientClipping Then
            Dim norm = hiddenGradient.L2Norm()
            If norm > GradientClipValue Then
                Dim scale = GradientClipValue / norm
                hiddenGradient = hiddenGradient * CSng(scale)
            End If
        End If

        ' 更新液态层梯度（简化版本）
        UpdateLiquidLayerGradients(hiddenGradient)
    End Sub

    ''' <summary>
    ''' 更新液态层梯度
    ''' </summary>
    Private Sub UpdateLiquidLayerGradients(hiddenGradient As Tensor)
        ' 获取最后一层的cell
        Dim lastCell = _Network.LiquidLayer.Cells(_Network.LiquidLayer.Cells.Count - 1)

        ' 更新循环权重梯度
        Dim state = lastCell.State
        For i = 0 To lastCell.HiddenSize - 1
            For j = 0 To lastCell.HiddenSize - 1
                lastCell.WeightRecurrentGradient(i, j) += hiddenGradient(i) * state(j) * 0.1
            Next
        Next

        ' 更新偏置梯度
        For i = 0 To lastCell.HiddenSize - 1
            lastCell.BiasGradient(i) += hiddenGradient(i) * 0.1
        Next
    End Sub

    ''' <summary>
    ''' 更新参数
    ''' </summary>
    Private Sub UpdateParameters()
        Select Case OptimizerType.ToLower()
            Case "adam"
                UpdateParametersAdam()
            Case "sgd"
                UpdateParametersSGD()
            Case Else
                UpdateParametersAdam()
        End Select
    End Sub

    ''' <summary>
    ''' 使用SGD更新参数
    ''' </summary>
    Private Sub UpdateParametersSGD()
        ' 更新输出层权重
        For i = 0 To _Network.OutputWeight.Shape(0) - 1
            For j = 0 To _Network.OutputWeight.Shape(1) - 1
                _Network.OutputWeight(i, j) -= LearningRate * _Network.OutputWeightGradient(i, j)
            Next
        Next

        ' 更新输出层偏置
        For i = 0 To _Network.OutputBias.Length - 1
            _Network.OutputBias(i) -= LearningRate * _Network.OutputBiasGradient(i)
        Next

        ' 清零梯度
        ZeroGradients()
    End Sub

    ''' <summary>
    ''' 使用Adam优化器更新参数
    ''' </summary>
    Private Sub UpdateParametersAdam()
        _AdamT += 1
        Dim params = _Network.GetParameters()

        ' 更新输出权重
        UpdateParamAdam(_Network.OutputWeight, _Network.OutputWeightGradient, "output_weight")

        ' 更新输出偏置
        UpdateParamAdam(_Network.OutputBias, _Network.OutputBiasGradient, "output_bias")

        ' 更新液态层参数
        For Each cell In _Network.LiquidLayer.Cells
            UpdateParamAdam(cell.Tau, cell.TauGradient, "tau")
            UpdateParamAdam(cell.WeightInput, cell.WeightInputGradient, "weight_input")
            UpdateParamAdam(cell.WeightRecurrent, cell.WeightRecurrentGradient, "weight_recurrent")
            UpdateParamAdam(cell.Bias, cell.BiasGradient, "bias")
        Next

        ' 清零梯度
        ZeroGradients()
    End Sub

    ''' <summary>
    ''' 使用Adam更新单个参数
    ''' </summary>
    Private Sub UpdateParamAdam(param As Tensor, gradient As Tensor, name As String)
        Dim key = name
        If Not _AdamM.ContainsKey(key) Then
            _AdamM.Add(key, Tensor.Zeros(param.Shape))
            _AdamV.Add(key, Tensor.Zeros(param.Shape))
        End If

        Dim m = _AdamM(key)
        Dim v = _AdamV(key)

        For i = 0 To param.Length - 1
            ' 更新一阶矩估计
            m(i) = AdamBeta1 * m(i) + (1 - AdamBeta1) * gradient(i)

            ' 更新二阶矩估计
            v(i) = AdamBeta2 * v(i) + (1 - AdamBeta2) * gradient(i) * gradient(i)

            ' 偏差校正
            Dim mHat = m(i) / (1 - std.Pow(AdamBeta1, _AdamT))
            Dim vHat = v(i) / (1 - std.Pow(AdamBeta2, _AdamT))

            ' 更新参数
            param(i) -= LearningRate * mHat / (std.Sqrt(vHat) + AdamEpsilon)
        Next
    End Sub

    ''' <summary>
    ''' 清零所有梯度
    ''' </summary>
    Private Sub ZeroGradients()
        ' 清零输出层梯度
        For i = 0 To _Network.OutputWeightGradient.Length - 1
            _Network.OutputWeightGradient(i) = 0
        Next
        For i = 0 To _Network.OutputBiasGradient.Length - 1
            _Network.OutputBiasGradient(i) = 0
        Next

        ' 清零液态层梯度
        For Each cell In _Network.LiquidLayer.Cells
            For i = 0 To cell.TauGradient.Length - 1
                cell.TauGradient(i) = 0
            Next
            For i = 0 To cell.WeightInputGradient.Length - 1
                cell.WeightInputGradient(i) = 0
            Next
            For i = 0 To cell.WeightRecurrentGradient.Length - 1
                cell.WeightRecurrentGradient(i) = 0
            Next
            For i = 0 To cell.BiasGradient.Length - 1
                cell.BiasGradient(i) = 0
            Next
        Next
    End Sub

#End Region

#Region "训练循环"

    ''' <summary>
    ''' 训练多个epoch
    ''' </summary>
    ''' <param name="inputSequences">输入序列列表</param>
    ''' <param name="targetSequences">目标序列列表</param>
    ''' <param name="epochs">训练轮数</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>每轮的平均损失</returns>
    Public Function Fit(inputSequences As List(Of Tensor), targetSequences As List(Of Tensor),
                        epochs As Integer, Optional dt As Double? = Nothing) As List(Of Double)
        If inputSequences.Count <> targetSequences.Count Then
            Throw New ArgumentException("输入序列和目标序列数量必须相同")
        End If

        Dim losses As New List(Of Double)()

        For epoch = 1 To epochs
            Dim epochLoss As Double = 0

            For i = 0 To inputSequences.Count - 1
                Dim seqLoss = TrainSequence(inputSequences(i), targetSequences(i), dt)
                epochLoss += seqLoss
            Next

            epochLoss /= inputSequences.Count
            losses.Add(epochLoss)

            ' 输出训练进度
            If epoch Mod 10 = 0 OrElse epoch = 1 Then
                Console.WriteLine($"Epoch {epoch}/{epochs}, Loss: {epochLoss:F6}")
            End If
        Next

        Return losses
    End Function

#End Region

End Class
