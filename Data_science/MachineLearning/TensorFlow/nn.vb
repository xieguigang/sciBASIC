#Region "Microsoft.VisualBasic::fc05cf8874969522ddf1f080e5d1ea88, Data_science\MachineLearning\TensorFlow\nn.vb"

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

    '   Total Lines: 486
    '    Code Lines: 308 (63.37%)
    ' Comment Lines: 105 (21.60%)
    '    - Xml Docs: 73.33%
    ' 
    '   Blank Lines: 73 (15.02%)
    '     File Size: 16.66 KB


    ' Module nn
    ' 
    '     Function: dropout, elu, gelu, huber_loss, l2_loss
    '               leaky_relu, log_softmax, mse_loss, relu, sigmoid
    '               sigmoid_cross_entropy_with_logits, softmax, softmax_cross_entropy_with_logits, sparse_softmax_cross_entropy_with_logits, swish
    '               tanh
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math
Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow

''' <summary>
''' TensorFlow 风格的神经网络模块
''' 提供激活函数、损失函数和正则化操作，全部基于 Tensor 实现
''' </summary>
Public Module nn

#Region "激活函数"

    ''' <summary>
    ''' ReLU 激活函数: max(0, x)
    ''' </summary>
    Public Function relu(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Max(0.0, src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' Leaky ReLU: max(alpha * x, x)
    ''' 负值部分使用 alpha 斜率，默认 0.01
    ''' </summary>
    Public Function leaky_relu(t As Tensor, Optional alpha As Double = 0.01) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = If(src(i) >= 0.0, src(i), alpha * src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' ELU 激活函数: x if x > 0 else alpha * (exp(x) - 1)
    ''' </summary>
    Public Function elu(t As Tensor, Optional alpha As Double = 1.0) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = If(src(i) >= 0.0, src(i), alpha * (std.Exp(src(i)) - 1.0))
        Next
        Return result
    End Function

    ''' <summary>
    ''' Sigmoid 激活函数: 1 / (1 + e^(-x))
    ''' </summary>
    Public Function sigmoid(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = 1.0 / (1.0 + std.Exp(-src(i)))
        Next
        Return result
    End Function

    ''' <summary>
    ''' Tanh 激活函数: (e^x - e^(-x)) / (e^x + e^(-x))
    ''' </summary>
    Public Function tanh(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Tanh(src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' GELU 激活函数（高斯误差线性单元）: x * Phi(x)
    ''' 使用近似公式: 0.5 * x * (1 + tanh(sqrt(2/pi) * (x + 0.044715 * x^3)))
    ''' </summary>
    Public Function gelu(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        Dim c = std.Sqrt(2.0 / std.PI)
        For i = 0 To src.Length - 1
            Dim x = src(i)
            dst(i) = 0.5 * x * (1.0 + std.Tanh(c * (x + 0.044715 * x * x * x)))
        Next
        Return result
    End Function

    ''' <summary>
    ''' Swish / SiLU 激活函数: x * sigmoid(x)
    ''' </summary>
    Public Function swish(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            Dim x = src(i)
            dst(i) = x / (1.0 + std.Exp(-x))
        Next
        Return result
    End Function

    ''' <summary>
    ''' Softmax 函数: exp(x_i) / sum(exp(x_j))
    ''' 使用 max-shift 技巧保证数值稳定性
    ''' </summary>
    ''' <param name="logits">输入张量</param>
    ''' <param name="axis">沿指定轴计算 softmax，默认为最后一维 (-1)</param>
    Public Function softmax(logits As Tensor, Optional axis As Integer = -1) As Tensor
        Dim rank = logits.Rank
        If axis < 0 Then axis = rank + axis

        ' 处理一维张量的简单情况
        If rank = 1 Then
            Dim srcData = logits.Data
            Dim len = srcData.Length
            Dim result = New Tensor(len)

            ' 找最大值（数值稳定）
            Dim maxVal = srcData(0)
            For i = 1 To len - 1
                If srcData(i) > maxVal Then maxVal = srcData(i)
            Next

            ' 计算 exp(x - max) 和 sum
            Dim sumExp = 0.0
            Dim dstData = result.Data
            For i = 0 To len - 1
                Dim e = std.Exp(srcData(i) - maxVal)
                dstData(i) = e
                sumExp += e
            Next

            ' 归一化
            For i = 0 To len - 1
                dstData(i) /= sumExp
            Next

            Return result
        End If

        ' 处理高维张量（通用实现）
        Dim origShape = logits.Shape
        Dim axisSize = origShape(axis)

        ' outerSize：轴之前各维度乘积
        Dim outerSize = 1
        For i = 0 To axis - 1
            outerSize *= origShape(i)
        Next

        ' stride：轴之后各维度乘积（沿轴遍历的步长）
        Dim stride = 1
        For i = axis + 1 To rank - 1
            stride *= origShape(i)
        Next

        Dim resultTensor = New Tensor(origShape)
        Dim srcArr = logits.Data
        Dim dstArr = resultTensor.Data

        For outer = 0 To outerSize - 1
            Dim baseIdx = outer * axisSize * stride
            For innerPtr = 0 To stride - 1
                ' 阶段 1: 找该切片的最大值
                Dim maxVal = srcArr(baseIdx + innerPtr)
                For k = 1 To axisSize - 1
                    Dim val = srcArr(baseIdx + k * stride + innerPtr)
                    If val > maxVal Then maxVal = val
                Next

                ' 阶段 2: 计算 exp(x - max) 和 sum
                Dim sumExp = 0.0
                Dim exps = New Double(axisSize - 1) {}
                For k = 0 To axisSize - 1
                    Dim val = srcArr(baseIdx + k * stride + innerPtr)
                    exps(k) = std.Exp(val - maxVal)
                    sumExp += exps(k)
                Next

                ' 阶段 3: 归一化并写回
                For k = 0 To axisSize - 1
                    dstArr(baseIdx + k * stride + innerPtr) = exps(k) / sumExp
                Next
            Next
        Next

        Return resultTensor
    End Function

    ''' <summary>
    ''' Log Softmax: log(softmax(x))
    ''' 使用数值稳定的 log-sum-exp 技巧
    ''' </summary>
    Public Function log_softmax(logits As Tensor, Optional axis As Integer = -1) As Tensor
        Dim rank = logits.Rank
        If axis < 0 Then axis = rank + axis

        If rank = 1 Then
            Dim srcData = logits.Data
            Dim len = srcData.Length
            Dim result = New Tensor(len)

            ' 找最大值
            Dim maxVal = srcData(0)
            For i = 1 To len - 1
                If srcData(i) > maxVal Then maxVal = srcData(i)
            Next

            ' 计算 log-sum-exp
            Dim sumExp = 0.0
            For i = 0 To len - 1
                sumExp += std.Exp(srcData(i) - maxVal)
            Next
            Dim logSumExp = maxVal + std.Log(sumExp)

            ' log_softmax = x - log_sum_exp
            Dim dstData = result.Data
            For i = 0 To len - 1
                dstData(i) = srcData(i) - logSumExp
            Next

            Return result
        End If

        ' 高维通用实现
        Dim origShape = logits.Shape
        Dim axisSize = origShape(axis)

        Dim outerSize = 1
        For i = 0 To axis - 1
            outerSize *= origShape(i)
        Next

        Dim stride = 1
        For i = axis + 1 To rank - 1
            stride *= origShape(i)
        Next

        Dim resultTensor = New Tensor(origShape)
        Dim srcArr = logits.Data
        Dim dstArr = resultTensor.Data

        For outer = 0 To outerSize - 1
            Dim baseIdx = outer * axisSize * stride
            For innerPtr = 0 To stride - 1
                ' 找最大值
                Dim maxVal = srcArr(baseIdx + innerPtr)
                For k = 1 To axisSize - 1
                    Dim val = srcArr(baseIdx + k * stride + innerPtr)
                    If val > maxVal Then maxVal = val
                Next

                ' 计算 log-sum-exp
                Dim sumExp = 0.0
                For k = 0 To axisSize - 1
                    sumExp += std.Exp(srcArr(baseIdx + k * stride + innerPtr) - maxVal)
                Next
                Dim logSumExp = maxVal + std.Log(sumExp)

                ' x - log_sum_exp
                For k = 0 To axisSize - 1
                    dstArr(baseIdx + k * stride + innerPtr) = srcArr(baseIdx + k * stride + innerPtr) - logSumExp
                Next
            Next
        Next

        Return resultTensor
    End Function

#End Region

#Region "损失函数"

    ''' <summary>
    ''' Sigmoid 交叉熵损失（数值稳定版本）
    ''' 对每个元素独立计算: max(x,0) - x*z + log(1 + exp(-|x|))
    ''' </summary>
    ''' <param name="labels">真实标签 (0 或 1)</param>
    ''' <param name="logits">模型输出的 logits</param>
    ''' <returns>逐元素损失，形状与输入相同</returns>
    Public Function sigmoid_cross_entropy_with_logits(labels As Tensor, logits As Tensor) As Tensor
        If Not labels.Shape.SequenceEqual(logits.Shape) Then
            Throw New ArgumentException($"labels 和 logits 形状必须相同: [{String.Join(",", labels.Shape)}] vs [{String.Join(",", logits.Shape)}]")
        End If

        Dim result = New Tensor(logits.Shape)
        Dim srcLabels = labels.Data
        Dim srcLogits = logits.Data
        Dim dst = result.Data

        ' 数值稳定的实现: max(x,0) - x*z + log(1 + exp(-|x|))
        For i = 0 To dst.Length - 1
            Dim x = srcLogits(i)
            Dim z = srcLabels(i)
            dst(i) = std.Max(x, 0.0) - x * z + std.Log(1.0 + std.Exp(-std.Abs(x)))
        Next

        Return result
    End Function

    ''' <summary>
    ''' Softmax 交叉熵损失
    ''' loss = -sum(labels * log(softmax(logits)))
    ''' </summary>
    ''' <param name="labels">真实标签（one-hot 或 概率分布），形状与 logits 相同</param>
    ''' <param name="logits">模型输出 logits</param>
    ''' <param name="axis">类别所在的轴，默认为最后一维</param>
    ''' <returns>每个样本的损失，形状为去除 axis 维度的张量</returns>
    Public Function softmax_cross_entropy_with_logits(labels As Tensor, logits As Tensor, Optional axis As Integer = -1) As Tensor
        If Not labels.Shape.SequenceEqual(logits.Shape) Then
            Throw New ArgumentException($"labels 和 logits 形状必须相同: [{String.Join(",", labels.Shape)}] vs [{String.Join(",", logits.Shape)}]")
        End If

        Dim rank = logits.Rank
        If axis < 0 Then axis = rank + axis

        ' log_softmax 然后逐元素乘 labels，再沿 axis 求和取负
        Dim logProbs = log_softmax(logits, axis)

        ' 逐元素乘法: labels * log_softmax
        Dim product = New Tensor(logits.Shape)
        Dim srcLabels = labels.Data
        Dim srcLogProbs = logProbs.Data
        Dim dstProduct = product.Data
        For i = 0 To dstProduct.Length - 1
            dstProduct(i) = srcLabels(i) * srcLogProbs(i)
        Next

        ' 沿 axis 求和
        Dim sumResult = Math.reduce_sum(product, axis, keepdims:=False)

        ' 取负
        Return Math.negative(sumResult)
    End Function

    ''' <summary>
    ''' 稀疏 Softmax 交叉熵损失
    ''' labels 是整数索引（非 one-hot），每个元素表示正确的类别索引
    ''' </summary>
    ''' <param name="labels">真实标签索引，形状与 logits 去掉类别轴相同</param>
    ''' <param name="logits">模型输出 logits</param>
    ''' <returns>每个样本的损失</returns>
    Public Function sparse_softmax_cross_entropy_with_logits(labels As Tensor, logits As Tensor) As Tensor
        Dim rank = logits.Rank
        Dim numClasses = logits.Shape(rank - 1)

        ' labels 应该是整数索引，我们取整
        ' 先计算 log_softmax
        Dim logProbs = log_softmax(logits, rank - 1)

        ' 结果形状：logits 去除最后一维
        Dim outShape = New Integer(rank - 2) {}
        For i = 0 To rank - 2
            outShape(i) = logits.Shape(i)
        Next

        Dim result As Tensor
        If outShape.Length = 0 Then
            ' 标量情况
            result = New Tensor(New Integer() {1})
        Else
            result = New Tensor(outShape)
        End If

        ' 遍历每个样本
        Dim sampleSize = 1
        For i = 0 To rank - 2
            sampleSize *= logits.Shape(i)
        Next

        Dim logProbsData = logProbs.Data
        Dim labelsData = labels.Data
        Dim resultData = result.Data

        For i = 0 To sampleSize - 1
            Dim baseIdx = i * numClasses
            Dim labelIdx = CInt(labelsData(i))
            ' 边界检查
            If labelIdx < 0 OrElse labelIdx >= numClasses Then
                Throw New ArgumentException($"标签索引 {labelIdx} 超出范围 [0, {numClasses - 1}]")
            End If
            resultData(i) = -logProbsData(baseIdx + labelIdx)
        Next

        Return result
    End Function

    ''' <summary>
    ''' 均方误差损失: mean((predictions - targets)^2)
    ''' </summary>
    Public Function mse_loss(predictions As Tensor, targets As Tensor) As Tensor
        If Not predictions.Shape.SequenceEqual(targets.Shape) Then
            Throw New ArgumentException($"predictions 和 targets 形状必须相同")
        End If

        Dim sumSq As Double = 0
        Dim srcPred = predictions.Data
        Dim srcTarget = targets.Data
        For i = 0 To srcPred.Length - 1
            Dim diff = srcPred(i) - srcTarget(i)
            sumSq += diff * diff
        Next

        Return Tensor.Scalar(sumSq / srcPred.Length)
    End Function

    ''' <summary>
    ''' L2 损失: sum(x^2) / 2
    ''' </summary>
    Public Function l2_loss(t As Tensor) As Tensor
        Dim sumSq As Double = 0
        Dim src = t.Data
        For i = 0 To src.Length - 1
            sumSq += src(i) * src(i)
        Next
        Return Tensor.Scalar(sumSq / 2.0)
    End Function

    ''' <summary>
    ''' Huber 损失（Smooth L1 Loss）
    ''' 当 |x| 不超过 delta 时为平方损失，否则为线性损失
    ''' </summary>
    Public Function huber_loss(predictions As Tensor, targets As Tensor, Optional delta As Double = 1.0) As Tensor
        If Not predictions.Shape.SequenceEqual(targets.Shape) Then
            Throw New ArgumentException($"predictions 和 targets 形状必须相同")
        End If

        Dim totalLoss As Double = 0
        Dim srcPred = predictions.Data
        Dim srcTarget = targets.Data
        For i = 0 To srcPred.Length - 1
            Dim diff = std.Abs(srcPred(i) - srcTarget(i))
            If diff <= delta Then
                totalLoss += 0.5 * diff * diff
            Else
                totalLoss += delta * (diff - 0.5 * delta)
            End If
        Next

        Return Tensor.Scalar(totalLoss / srcPred.Length)
    End Function

#End Region

#Region "正则化"

    ''' <summary>
    ''' Dropout 正则化
    ''' 以概率 keepProb 保留每个神经元，其余置零
    ''' 训练时将保留的神经元值放大 1/keepProb 以保持期望不变
    ''' </summary>
    ''' <param name="t">输入张量</param>
    ''' <param name="keepProb">保留概率 (0~1)</param>
    ''' <param name="seed">随机种子（可选）</param>
    Public Function dropout(t As Tensor, keepProb As Double, Optional seed As Integer? = Nothing) As Tensor
        If keepProb <= 0 OrElse keepProb > 1 Then
            Throw New ArgumentException($"keepProb 必须在 (0, 1] 范围内，当前值: {keepProb}")
        End If

        Dim rnd = If(seed.HasValue, New Random(seed.Value), New Random())
        Dim scale = 1.0 / keepProb

        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data

        For i = 0 To src.Length - 1
            If rnd.NextDouble() < keepProb Then
                dst(i) = src(i) * scale
            Else
                dst(i) = 0.0
            End If
        Next

        Return result
    End Function

#End Region

End Module
