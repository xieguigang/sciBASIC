#Region "Microsoft.VisualBasic::fdaee716c5f7aa3dc4d30aa443c18700, Data_science\MachineLearning\LNN\LNNTrainer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 397
    '    Code Lines: 194 (48.87%)
    ' Comment Lines: 117 (29.47%)
    '    - Xml Docs: 87.18%
    ' 
    '   Blank Lines: 86 (21.66%)
    '     File Size: 12.24 KB


    ' Class LNNTrainer
    ' 
    '     Properties: AdamBeta1, AdamBeta2, AdamEpsilon, GradientClipValue, LearningRate
    '                 Network, OptimizerType, UseGradientClipping, Verbose
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Backward, Fit, MAE, MSE, MSEGradient
    '               RowVector, TrainSequence, TrainStep
    ' 
    '     Sub: [Step], ClipGradients, InitializeAdamState, UpdateParametersAdam, UpdateParametersSGD
    '          ZeroGradients
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 液态神经网络训练器
''' 实现基于时间的反向传播(BPTT)训练算法
''' </summary>
''' <remarks>
''' 与旧实现的关键区别：
''' 1. 液态层（τ、W、U、b 以及 LTC/CfC 的门控参数）的梯度由
'''    <see cref="LiquidCell.Backward"/> 精确计算，不再使用硬编码的伪梯度；
''' 2. Adam 的动量状态按参数的<strong>全局唯一名</strong>索引，多层堆叠时不再互相覆盖；
''' 3. 梯度累积与参数更新被拆成 <see cref="Backward"/> 与 <see cref="Step"/>，
'''    便于外部（如代谢网络训练器）先合并多个损失头的梯度再统一更新。
''' </remarks>
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
    ''' 优化器类型（"adam" / "sgd"）
    ''' </summary>
    Public Property OptimizerType As String = "adam"

    ''' <summary>
    ''' 是否在 <see cref="Fit"/> 中打印训练进度
    ''' </summary>
    Public Property Verbose As Boolean = True

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
    ''' 梯度裁剪阈值（按全部参数梯度的全局 L2 范数裁剪）
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

        For Each pair In _Network.GetParameterPairs()
            _AdamM.Add(pair.Name, Tensor.Zeros(pair.Value.Shape))
            _AdamV.Add(pair.Name, Tensor.Zeros(pair.Value.Shape))
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

#Region "梯度累积与参数更新"

    ''' <summary>
    ''' 只累积梯度，不更新参数
    ''' </summary>
    ''' <param name="outputGradient">对网络输出的梯度 dL/doutput</param>
    ''' <returns>对网络外部输入的梯度（一般可忽略）</returns>
    Public Function Backward(outputGradient As Tensor) As Tensor
        Dim adjH = _Network.BackwardOutput(outputGradient)

        Return _Network.BackwardLiquid(adjH)
    End Function

    ''' <summary>
    ''' 应用优化器更新并清零梯度
    ''' </summary>
    Public Sub [Step]()
        If UseGradientClipping Then
            Call ClipGradients()
        End If

        Select Case OptimizerType.ToLower()
            Case "sgd"
                Call UpdateParametersSGD()
            Case Else
                Call UpdateParametersAdam()
        End Select

        Call ZeroGradients()
    End Sub

    ''' <summary>
    ''' 按全部参数梯度的全局 L2 范数做裁剪
    ''' </summary>
    Private Sub ClipGradients()
        Dim sq As Double = 0.0

        For Each pair In _Network.GetParameterPairs()
            Dim g = pair.Gradient
            For i = 0 To g.Length - 1
                sq += g(i) * g(i)
            Next
        Next

        Dim norm = std.Sqrt(sq)

        If norm <= GradientClipValue OrElse norm = 0.0 Then
            Return
        End If

        Dim scale = GradientClipValue / norm

        For Each pair In _Network.GetParameterPairs()
            Dim g = pair.Gradient
            For i = 0 To g.Length - 1
                g(i) = g(i) * scale
            Next
        Next
    End Sub

    ''' <summary>
    ''' 使用SGD更新参数
    ''' </summary>
    Private Sub UpdateParametersSGD()
        For Each pair In _Network.GetParameterPairs()
            Dim p = pair.Value
            Dim g = pair.Gradient

            For i = 0 To p.Length - 1
                p(i) -= LearningRate * g(i)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 使用Adam优化器更新参数
    ''' </summary>
    Private Sub UpdateParametersAdam()
        _AdamT += 1

        Dim beta1Pow = std.Pow(AdamBeta1, _AdamT)
        Dim beta2Pow = std.Pow(AdamBeta2, _AdamT)

        For Each pair In _Network.GetParameterPairs()
            Dim key = pair.Name

            ' 参数集合可能在运行中变化（例如切换到 LTC 模式后新增门控参数）
            If Not _AdamM.ContainsKey(key) Then
                _AdamM.Add(key, Tensor.Zeros(pair.Value.Shape))
                _AdamV.Add(key, Tensor.Zeros(pair.Value.Shape))
            End If

            Dim m = _AdamM(key)
            Dim v = _AdamV(key)
            Dim p = pair.Value
            Dim g = pair.Gradient

            For i = 0 To p.Length - 1
                ' 更新一阶矩估计
                m(i) = AdamBeta1 * m(i) + (1 - AdamBeta1) * g(i)

                ' 更新二阶矩估计
                v(i) = AdamBeta2 * v(i) + (1 - AdamBeta2) * g(i) * g(i)

                ' 偏差校正
                Dim mHat = m(i) / (1 - beta1Pow)
                Dim vHat = v(i) / (1 - beta2Pow)

                ' 更新参数
                p(i) -= LearningRate * mHat / (std.Sqrt(vHat) + AdamEpsilon)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 清零所有梯度
    ''' </summary>
    Public Sub ZeroGradients()
        _Network.ZeroGradients()
    End Sub

#End Region

#Region "训练方法"

    ''' <summary>
    ''' 训练单个时间步（BPTT 截断长度为 1）
    ''' </summary>
    ''' <param name="input">输入</param>
    ''' <param name="target">目标输出</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>损失值</returns>
    Public Function TrainStep(input As Tensor, target As Tensor, Optional dt As Double? = Nothing) As Double
        _Network.Training = True

        ' 前向传播
        Dim output = _Network.Forward(input, dt)

        ' 计算损失
        Dim loss = MSE(output, target)

        ' 反向传播（精确反向模式 AD）
        Call Backward(MSEGradient(output, target))

        _Network.Training = False

        ' 更新参数
        Call [Step]()

        Return loss
    End Function

    ''' <summary>
    ''' 在完整序列上做一次 BPTT（前向整段 → 损失 → 逆序回传 → 更新）
    ''' </summary>
    ''' <param name="inputSequence">输入序列，形状 (seqLength, inputSize)</param>
    ''' <param name="targetSequence">目标序列，形状 (seqLength, outputSize)</param>
    ''' <param name="dt">时间步长</param>
    ''' <returns>平均损失</returns>
    Public Function TrainSequence(inputSequence As Tensor, targetSequence As Tensor, Optional dt As Double? = Nothing) As Double
        Dim actualDt = If(dt, _Network.DefaultDt)
        Dim seqLength = inputSequence.Shape(0)
        Dim totalLoss As Double = 0

        ' 重置网络状态并丢弃历史前向记录
        _Network.ResetState()
        _Network.Training = True

        ' ---- 前向 ----
        Dim outputs(seqLength - 1) As Tensor

        For t = 0 To seqLength - 1
            outputs(t) = _Network.Forward(RowVector(inputSequence, t, _Network.InputSize), actualDt)
            totalLoss += MSE(outputs(t), RowVector(targetSequence, t, _Network.OutputSize))
        Next

        ' ---- 反向（逆时间序；跨时间步的伴随由 LiquidLayer 内部维护） ----
        For t = seqLength - 1 To 0 Step -1
            Dim dOut = MSEGradient(outputs(t), RowVector(targetSequence, t, _Network.OutputSize))
            Dim adjH = _Network.BackwardOutput(dOut)

            Call _Network.BackwardLiquid(adjH)
        Next

        _Network.Training = False

        ' ---- 更新 ----
        Call [Step]()

        Return totalLoss / seqLength
    End Function

    ''' <summary>
    ''' 取出二维张量的第 rowIndex 行
    ''' </summary>
    Private Shared Function RowVector(sequence As Tensor, rowIndex As Integer, width As Integer) As Tensor
        Dim row = Tensor.Zeros({width})

        For i = 0 To width - 1
            row(i) = sequence(rowIndex, i)
        Next

        Return row
    End Function

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
            If Verbose AndAlso (epoch Mod 10 = 0 OrElse epoch = 1) Then
                Console.WriteLine($"Epoch {epoch}/{epochs}, Loss: {epochLoss:F6}")
            End If
        Next

        Return losses
    End Function

#End Region

End Class
