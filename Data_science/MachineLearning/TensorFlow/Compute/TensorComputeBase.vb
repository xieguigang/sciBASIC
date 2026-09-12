#Region "Microsoft.VisualBasic::109fa2e9a395f60c6559d6d5014e7f58, Data_science\MachineLearning\TensorFlow\Compute\TensorComputeBase.vb"

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

    '   Total Lines: 657
    '    Code Lines: 509 (77.47%)
    ' Comment Lines: 19 (2.89%)
    '    - Xml Docs: 47.37%
    ' 
    '   Blank Lines: 129 (19.63%)
    '     File Size: 26.34 KB


    '     Class TensorComputeBase
    ' 
    '         Function: Abs, Add, AddScalar, ArgGlobal, ArgMax
    '                   ArgMin, Clip, Cos, Divide, DivideScalar
    '                   Elu, Exp, Gelu, HuberLoss, L2Loss
    '                   L2Norm, LeakyRelu, Log, LogSoftmax, MapBinary
    '                   MapUnary, MatMul, Max, Maximum, Mean
    '                   MeanAll, Min, Minimum, MseLoss, Multiply
    '                   MultiplyScalar, Negate, Pow, Prod, Reciprocal
    '                   ReduceAlongAxis, ReduceArgAxis, ReduceGlobal, Relu, Sigmoid
    '                   SigmoidCrossEntropyWithLogits, Sin, Softmax, Sqrt, Square
    '                   StdDev, Subtract, Sum, SumAll, Swish
    '                   Tanh, Transpose, Wrap
    ' 
    '         Sub: RequireSameShape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' 标量参考实现：把原本散落在 Tensor.vb / Math.vb / nn.vb 里的循环算法集中到这里，
' 作为所有后端的兜底实现。
'
'   * SIMDTensor 只重写“有向量指令收益”的热点算子，其余自动继承这里；
'   * CudaTensor 只重写“有 CUDA 内核”的算子，其余自动继承这里。
'
' 这样即使某个后端没有覆盖某个算子，语义也始终正确（退化为 CPU 计算），
' 不会出现“切换后端之后某个算子不可用”的情况。
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' <see cref="ITensorCompute"/> 的标量兜底实现。
    ''' </summary>
    Public MustInherit Class TensorComputeBase
        Implements ITensorCompute

        Public MustOverride ReadOnly Property Name As String Implements ITensorCompute.Name

#Region "helpers"

        ''' <summary>要求两个张量形状一致</summary>
        Protected Shared Sub RequireSameShape(a As Tensor, b As Tensor, opName As String)
            If Not a.Shape.SequenceEqual(b.Shape) Then
                Throw New ArgumentException(
                    $"张量形状必须相同才能{opName}: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
            End If
        End Sub

        ''' <summary>由计算结果的一维数组与形状包装出新的张量</summary>
        Protected Shared Function Wrap(data As Double(), shape As Integer()) As Tensor
            Return New Tensor(data, shape)
        End Function

        ''' <summary>逐元素一元运算的通用标量实现</summary>
        Protected Shared Function MapUnary(t As Tensor, op As Func(Of Double, Double)) As Tensor
            Dim src = t.Data
            Dim dst(src.Length - 1) As Double
            For i As Integer = 0 To src.Length - 1
                dst(i) = op(src(i))
            Next
            Return New Tensor(dst, t.Shape)
        End Function

        ''' <summary>逐元素二元运算的通用标量实现</summary>
        Protected Shared Function MapBinary(a As Tensor, b As Tensor, op As Func(Of Double, Double, Double)) As Tensor
            RequireSameShape(a, b, "进行逐元素运算")
            Dim srcA = a.Data
            Dim srcB = b.Data
            Dim dst(srcA.Length - 1) As Double
            For i As Integer = 0 To srcA.Length - 1
                dst(i) = op(srcA(i), srcB(i))
            Next
            Return New Tensor(dst, a.Shape)
        End Function

#End Region

#Region "逐元素 - 二元"

        Public Overridable Function Add(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Add
            Return MapBinary(a, b, Function(x, y) x + y)
        End Function

        Public Overridable Function Subtract(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Subtract
            Return MapBinary(a, b, Function(x, y) x - y)
        End Function

        Public Overridable Function Multiply(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Multiply
            Return MapBinary(a, b, Function(x, y) x * y)
        End Function

        Public Overridable Function Divide(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Divide
            Return MapBinary(a, b, Function(x, y) x / y)
        End Function

        Public Overridable Function Maximum(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Maximum
            Return MapBinary(a, b, Function(x, y) std.Max(x, y))
        End Function

        Public Overridable Function Minimum(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.Minimum
            Return MapBinary(a, b, Function(x, y) std.Min(x, y))
        End Function

#End Region

#Region "逐元素 - 一元"

        Public Overridable Function Exp(t As Tensor) As Tensor Implements ITensorCompute.Exp
            Return MapUnary(t, Function(x) std.Exp(x))
        End Function

        Public Overridable Function Log(t As Tensor) As Tensor Implements ITensorCompute.Log
            Return MapUnary(t, Function(x) std.Log(x))
        End Function

        Public Overridable Function Sqrt(t As Tensor) As Tensor Implements ITensorCompute.Sqrt
            Return MapUnary(t, Function(x) std.Sqrt(x))
        End Function

        Public Overridable Function Square(t As Tensor) As Tensor Implements ITensorCompute.Square
            Return MapUnary(t, Function(x) x * x)
        End Function

        Public Overridable Function Abs(t As Tensor) As Tensor Implements ITensorCompute.Abs
            Return MapUnary(t, Function(x) std.Abs(x))
        End Function

        Public Overridable Function Sin(t As Tensor) As Tensor Implements ITensorCompute.Sin
            Return MapUnary(t, Function(x) std.Sin(x))
        End Function

        Public Overridable Function Cos(t As Tensor) As Tensor Implements ITensorCompute.Cos
            Return MapUnary(t, Function(x) std.Cos(x))
        End Function

        Public Overridable Function Tanh(t As Tensor) As Tensor Implements ITensorCompute.Tanh
            Return MapUnary(t, Function(x) std.Tanh(x))
        End Function

        Public Overridable Function Sigmoid(t As Tensor) As Tensor Implements ITensorCompute.Sigmoid
            Return MapUnary(t, Function(x) 1.0 / (1.0 + std.Exp(-x)))
        End Function

        Public Overridable Function Negate(t As Tensor) As Tensor Implements ITensorCompute.Negate
            Return MapUnary(t, Function(x) -x)
        End Function

        Public Overridable Function Reciprocal(t As Tensor) As Tensor Implements ITensorCompute.Reciprocal
            Return MapUnary(t, Function(x) 1.0 / x)
        End Function

        Public Overridable Function Relu(t As Tensor) As Tensor Implements ITensorCompute.Relu
            Return MapUnary(t, Function(x) std.Max(0.0, x))
        End Function

        Public Overridable Function LeakyRelu(t As Tensor, alpha As Double) As Tensor Implements ITensorCompute.LeakyRelu
            Return MapUnary(t, Function(x) If(x >= 0.0, x, alpha * x))
        End Function

        Public Overridable Function Elu(t As Tensor, alpha As Double) As Tensor Implements ITensorCompute.Elu
            Return MapUnary(t, Function(x) If(x >= 0.0, x, alpha * (std.Exp(x) - 1.0)))
        End Function

        Public Overridable Function Gelu(t As Tensor) As Tensor Implements ITensorCompute.Gelu
            Dim c = std.Sqrt(2.0 / std.PI)
            Return MapUnary(t, Function(x) 0.5 * x * (1.0 + std.Tanh(c * (x + 0.044715 * x * x * x))))
        End Function

        Public Overridable Function Swish(t As Tensor) As Tensor Implements ITensorCompute.Swish
            Return MapUnary(t, Function(x) x / (1.0 + std.Exp(-x)))
        End Function

#End Region

#Region "标量运算"

        Public Overridable Function AddScalar(t As Tensor, scalar As Double) As Tensor Implements ITensorCompute.AddScalar
            Return MapUnary(t, Function(x) x + scalar)
        End Function

        Public Overridable Function MultiplyScalar(t As Tensor, scalar As Double) As Tensor Implements ITensorCompute.MultiplyScalar
            Return MapUnary(t, Function(x) x * scalar)
        End Function

        Public Overridable Function DivideScalar(t As Tensor, scalar As Double) As Tensor Implements ITensorCompute.DivideScalar
            Return MapUnary(t, Function(x) x / scalar)
        End Function

        Public Overridable Function Pow(t As Tensor, exponent As Double) As Tensor Implements ITensorCompute.Pow
            Return MapUnary(t, Function(x) std.Pow(x, exponent))
        End Function

        Public Overridable Function Clip(t As Tensor, low As Double, high As Double) As Tensor Implements ITensorCompute.Clip
            Return MapUnary(t, Function(x) std.Min(std.Max(x, low), high))
        End Function

#End Region

#Region "矩阵运算"

        Public Overridable Function MatMul(a As Tensor, b As Tensor) As Tensor Implements ITensorCompute.MatMul
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0)
            Dim n = b.Shape(1)
            Dim k = a.Shape(1)

            Dim result = New Tensor(m, n)
            Dim srcA = a.Data
            Dim srcB = b.Data
            Dim dst = result.Data

            For i As Integer = 0 To m - 1
                Dim rowA = i * k
                Dim rowC = i * n
                For j As Integer = 0 To n - 1
                    Dim sum As Double = 0
                    For p As Integer = 0 To k - 1
                        sum += srcA(rowA + p) * srcB(p * n + j)
                    Next
                    dst(rowC + j) = sum
                Next
            Next

            Return result
        End Function

        Public Overridable Function Transpose(t As Tensor) As Tensor Implements ITensorCompute.Transpose
            If t.Rank <> 2 Then
                Throw New ArgumentException("只支持二维张量转置")
            End If

            Dim rows = t.Shape(0)
            Dim cols = t.Shape(1)
            Dim result = New Tensor(cols, rows)
            Dim src = t.Data
            Dim dst = result.Data

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    dst(j * rows + i) = src(i * cols + j)
                Next
            Next

            Return result
        End Function

#End Region

#Region "归约运算"

        Public Overridable Function SumAll(t As Tensor) As Double Implements ITensorCompute.SumAll
            Dim src = t.Data
            Dim sum As Double = 0
            For i As Integer = 0 To src.Length - 1
                sum += src(i)
            Next
            Return sum
        End Function

        Public Overridable Function MeanAll(t As Tensor) As Double Implements ITensorCompute.MeanAll
            Return SumAll(t) / t.Length
        End Function

        Public Overridable Function Sum(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Sum
            If Not axis.HasValue Then
                Return Tensor.Scalar(SumAll(t))
            End If
            Return ReduceAlongAxis(t, axis.Value, keepdims, 0.0, Function(acc, val) acc + val)
        End Function

        Public Overridable Function Mean(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Mean
            If Not axis.HasValue Then
                Return Tensor.Scalar(MeanAll(t))
            End If

            Dim sumResult = ReduceAlongAxis(t, axis.Value, keepdims, 0.0, Function(acc, val) acc + val)
            Dim count = t.Shape(axis.Value)
            Return MultiplyScalar(sumResult, 1.0 / count)
        End Function

        Public Overridable Function Max(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Max
            If Not axis.HasValue Then
                Return Tensor.Scalar(ReduceGlobal(t, Double.NegativeInfinity, Function(acc, val) std.Max(acc, val)))
            End If
            Return ReduceAlongAxis(t, axis.Value, keepdims, Double.NegativeInfinity, Function(acc, val) std.Max(acc, val))
        End Function

        Public Overridable Function Min(t As Tensor, axis As Integer?, keepdims As Boolean) As Tensor Implements ITensorCompute.Min
            If Not axis.HasValue Then
                Return Tensor.Scalar(ReduceGlobal(t, Double.PositiveInfinity, Function(acc, val) std.Min(acc, val)))
            End If
            Return ReduceAlongAxis(t, axis.Value, keepdims, Double.PositiveInfinity, Function(acc, val) std.Min(acc, val))
        End Function

        Public Overridable Function Prod(t As Tensor, axis As Integer?) As Tensor Implements ITensorCompute.Prod
            If Not axis.HasValue Then
                Return Tensor.Scalar(ReduceGlobal(t, 1.0, Function(acc, val) acc * val))
            End If
            Return ReduceAlongAxis(t, axis.Value, False, 1.0, Function(acc, val) acc * val)
        End Function

        Public Overridable Function StdDev(t As Tensor) As Double Implements ITensorCompute.StdDev
            Dim mean = MeanAll(t)
            Dim src = t.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To src.Length - 1
                Dim diff = src(i) - mean
                sumSq += diff * diff
            Next
            Return std.Sqrt(sumSq / src.Length)
        End Function

        Public Overridable Function L2Norm(t As Tensor) As Double Implements ITensorCompute.L2Norm
            Dim src = t.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To src.Length - 1
                sumSq += src(i) * src(i)
            Next
            Return std.Sqrt(sumSq)
        End Function

        Public Overridable Function ArgMax(t As Tensor, axis As Integer?) As Tensor Implements ITensorCompute.ArgMax
            If Not axis.HasValue Then
                Return Tensor.Scalar(ArgGlobal(t, findMin:=False))
            End If
            Return ReduceArgAxis(t, axis.Value, False)
        End Function

        Public Overridable Function ArgMin(t As Tensor, axis As Integer?) As Tensor Implements ITensorCompute.ArgMin
            If Not axis.HasValue Then
                Return Tensor.Scalar(ArgGlobal(t, findMin:=True))
            End If
            Return ReduceArgAxis(t, axis.Value, True)
        End Function

#End Region

#Region "神经网络"

        Public Overridable Function Softmax(t As Tensor, axis As Integer) As Tensor Implements ITensorCompute.Softmax
            Dim rank = t.Rank
            If axis < 0 Then axis = rank + axis

            If rank = 1 Then
                Dim srcData = t.Data
                Dim dstData(srcData.Length - 1) As Double

                Dim maxVal = srcData(0)
                For i As Integer = 1 To srcData.Length - 1
                    If srcData(i) > maxVal Then maxVal = srcData(i)
                Next

                Dim sumExp As Double = 0
                For i As Integer = 0 To srcData.Length - 1
                    Dim e = std.Exp(srcData(i) - maxVal)
                    dstData(i) = e
                    sumExp += e
                Next

                For i As Integer = 0 To dstData.Length - 1
                    dstData(i) /= sumExp
                Next

                Return New Tensor(dstData, t.Shape)
            End If

            Dim shape = t.Shape
            Dim axisSize = shape(axis)
            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= shape(i)
            Next
            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= shape(i)
            Next

            Dim srcArr = t.Data
            Dim dstArr(srcArr.Length - 1) As Double

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For innerPtr As Integer = 0 To stride - 1
                    Dim maxVal = srcArr(baseIdx + innerPtr)
                    For k As Integer = 1 To axisSize - 1
                        Dim val = srcArr(baseIdx + k * stride + innerPtr)
                        If val > maxVal Then maxVal = val
                    Next

                    Dim sumExp As Double = 0
                    Dim exps(axisSize - 1) As Double
                    For k As Integer = 0 To axisSize - 1
                        Dim val = srcArr(baseIdx + k * stride + innerPtr)
                        exps(k) = std.Exp(val - maxVal)
                        sumExp += exps(k)
                    Next

                    For k As Integer = 0 To axisSize - 1
                        dstArr(baseIdx + k * stride + innerPtr) = exps(k) / sumExp
                    Next
                Next
            Next

            Return New Tensor(dstArr, shape)
        End Function

        Public Overridable Function LogSoftmax(t As Tensor, axis As Integer) As Tensor Implements ITensorCompute.LogSoftmax
            Dim rank = t.Rank
            If axis < 0 Then axis = rank + axis

            If rank = 1 Then
                Dim srcData = t.Data
                Dim maxVal = srcData(0)
                For i As Integer = 1 To srcData.Length - 1
                    If srcData(i) > maxVal Then maxVal = srcData(i)
                Next

                Dim sumExp As Double = 0
                For i As Integer = 0 To srcData.Length - 1
                    sumExp += std.Exp(srcData(i) - maxVal)
                Next
                Dim logSumExp = maxVal + std.Log(sumExp)

                Dim dstData(srcData.Length - 1) As Double
                For i As Integer = 0 To srcData.Length - 1
                    dstData(i) = srcData(i) - logSumExp
                Next

                Return New Tensor(dstData, t.Shape)
            End If

            Dim shape = t.Shape
            Dim axisSize = shape(axis)
            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= shape(i)
            Next
            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= shape(i)
            Next

            Dim srcArr = t.Data
            Dim dstArr(srcArr.Length - 1) As Double

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For innerPtr As Integer = 0 To stride - 1
                    Dim maxVal = srcArr(baseIdx + innerPtr)
                    For k As Integer = 1 To axisSize - 1
                        Dim val = srcArr(baseIdx + k * stride + innerPtr)
                        If val > maxVal Then maxVal = val
                    Next

                    Dim sumExp As Double = 0
                    For k As Integer = 0 To axisSize - 1
                        sumExp += std.Exp(srcArr(baseIdx + k * stride + innerPtr) - maxVal)
                    Next
                    Dim logSumExp = maxVal + std.Log(sumExp)

                    For k As Integer = 0 To axisSize - 1
                        dstArr(baseIdx + k * stride + innerPtr) = srcArr(baseIdx + k * stride + innerPtr) - logSumExp
                    Next
                Next
            Next

            Return New Tensor(dstArr, shape)
        End Function

        Public Overridable Function SigmoidCrossEntropyWithLogits(labels As Tensor, logits As Tensor) As Tensor Implements ITensorCompute.SigmoidCrossEntropyWithLogits
            RequireSameShape(labels, logits, "计算 sigmoid 交叉熵")

            Dim srcLabels = labels.Data
            Dim srcLogits = logits.Data
            Dim dst(srcLogits.Length - 1) As Double

            For i As Integer = 0 To dst.Length - 1
                Dim x = srcLogits(i)
                Dim z = srcLabels(i)
                dst(i) = std.Max(x, 0.0) - x * z + std.Log(1.0 + std.Exp(-std.Abs(x)))
            Next

            Return New Tensor(dst, logits.Shape)
        End Function

        Public Overridable Function MseLoss(predictions As Tensor, targets As Tensor) As Tensor Implements ITensorCompute.MseLoss
            RequireSameShape(predictions, targets, "计算均方误差")

            Dim srcPred = predictions.Data
            Dim srcTarget = targets.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To srcPred.Length - 1
                Dim diff = srcPred(i) - srcTarget(i)
                sumSq += diff * diff
            Next

            Return Tensor.Scalar(sumSq / srcPred.Length)
        End Function

        Public Overridable Function L2Loss(t As Tensor) As Tensor Implements ITensorCompute.L2Loss
            Dim src = t.Data
            Dim sumSq As Double = 0
            For i As Integer = 0 To src.Length - 1
                sumSq += src(i) * src(i)
            Next
            Return Tensor.Scalar(sumSq / 2.0)
        End Function

        Public Overridable Function HuberLoss(predictions As Tensor, targets As Tensor, delta As Double) As Tensor Implements ITensorCompute.HuberLoss
            RequireSameShape(predictions, targets, "计算 Huber 损失")

            Dim srcPred = predictions.Data
            Dim srcTarget = targets.Data
            Dim totalLoss As Double = 0
            For i As Integer = 0 To srcPred.Length - 1
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

#Region "内部工具"

        Private Shared Function ReduceGlobal(t As Tensor, initValue As Double,
                                             reduceFunc As Func(Of Double, Double, Double)) As Double
            Dim src = t.Data
            Dim value = initValue
            For i As Integer = 0 To src.Length - 1
                value = reduceFunc(value, src(i))
            Next
            Return value
        End Function

        Private Shared Function ArgGlobal(t As Tensor, findMin As Boolean) As Double
            Dim src = t.Data
            Dim bestIdx = 0
            Dim bestVal = src(0)

            For i As Integer = 1 To src.Length - 1
                If findMin Then
                    If src(i) < bestVal Then
                        bestVal = src(i)
                        bestIdx = i
                    End If
                Else
                    If src(i) > bestVal Then
                        bestVal = src(i)
                        bestIdx = i
                    End If
                End If
            Next

            Return bestIdx
        End Function

        ''' <summary>沿指定轴执行规约操作（通用实现）</summary>
        Protected Shared Function ReduceAlongAxis(t As Tensor, axis As Integer, keepdims As Boolean,
                                                  initValue As Double,
                                                  reduceFunc As Func(Of Double, Double, Double)) As Tensor
            Dim rank = t.Rank
            Dim origShape = t.Shape

            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= origShape(i)
            Next

            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= origShape(i)
            Next

            Dim axisSize = origShape(axis)

            Dim outShapeList = New List(Of Integer)
            For i As Integer = 0 To rank - 1
                If i = axis Then
                    If keepdims Then outShapeList.Add(1)
                Else
                    outShapeList.Add(origShape(i))
                End If
            Next

            If outShapeList.Count = 0 Then
                Return Tensor.Scalar(ReduceGlobal(t, initValue, reduceFunc))
            End If

            Dim result = New Tensor(outShapeList.ToArray())
            Dim srcData = t.Data
            Dim dstData = result.Data
            Dim dstIdx = 0

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For inner As Integer = 0 To stride - 1
                    Dim value = initValue
                    For k As Integer = 0 To axisSize - 1
                        value = reduceFunc(value, srcData(baseIdx + k * stride + inner))
                    Next
                    dstData(dstIdx) = value
                    dstIdx += 1
                Next
            Next

            Return result
        End Function

        ''' <summary>沿指定轴找到极值的索引（argmax / argmin）</summary>
        Protected Shared Function ReduceArgAxis(t As Tensor, axis As Integer, findMin As Boolean) As Tensor
            Dim rank = t.Rank
            Dim origShape = t.Shape

            Dim outerSize = 1
            For i As Integer = 0 To axis - 1
                outerSize *= origShape(i)
            Next

            Dim stride = 1
            For i As Integer = axis + 1 To rank - 1
                stride *= origShape(i)
            Next

            Dim axisSize = origShape(axis)

            Dim outShapeList = New List(Of Integer)
            For i As Integer = 0 To rank - 1
                If i <> axis Then outShapeList.Add(origShape(i))
            Next

            Dim result = New Tensor(outShapeList.ToArray())
            Dim srcData = t.Data
            Dim dstData = result.Data
            Dim dstIdx = 0

            For outer As Integer = 0 To outerSize - 1
                Dim baseIdx = outer * axisSize * stride
                For inner As Integer = 0 To stride - 1
                    Dim bestVal = srcData(baseIdx + inner)
                    Dim bestK = 0
                    For k As Integer = 1 To axisSize - 1
                        Dim val = srcData(baseIdx + k * stride + inner)
                        If (findMin AndAlso val < bestVal) OrElse (Not findMin AndAlso val > bestVal) Then
                            bestVal = val
                            bestK = k
                        End If
                    Next
                    dstData(dstIdx) = bestK
                    dstIdx += 1
                Next
            Next

            Return result
        End Function

#End Region

    End Class

End Namespace
