#Region "Microsoft.VisualBasic::014f5bcd17a5ff37909ce3d57b91c55f, Data_science\MachineLearning\TensorFlow\Math.vb"

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

    '   Total Lines: 30
    '    Code Lines: 23 (76.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (23.33%)
    '     File Size: 762 B


    ' Module Math
    ' 
    '     Function: exp, log, sqrt, square, abs, sin, cos, tanh, sigmoid, pow,
    '               reduce_sum, reduce_mean, reduce_max, reduce_min,
    '               argmax, argmin, clip_by_value, clip_by_norm
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

''' <summary>
''' TensorFlow 风格的数学运算模块
''' 提供逐元素运算、规约操作和裁剪功能，全部基于 Tensor 实现
''' </summary>
Public Module Math

#Region "逐元素一元运算"

    ''' <summary>
    ''' 逐元素指数函数 e^x
    ''' </summary>
    Public Function exp(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Exp(src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素自然对数 ln(x)
    ''' </summary>
    Public Function log(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Log(src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素平方根 sqrt(x)
    ''' </summary>
    Public Function sqrt(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Sqrt(src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素平方 x^2
    ''' </summary>
    Public Function square(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = src(i) * src(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素绝对值 |x|
    ''' </summary>
    Public Function abs(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Abs(src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素正弦 sin(x)
    ''' </summary>
    Public Function sin(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Sin(src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素余弦 cos(x)
    ''' </summary>
    Public Function cos(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Cos(src(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素双曲正切 tanh(x)
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
    ''' 逐元素 Sigmoid 函数 1/(1+e^(-x))
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
    ''' 逐元素幂运算 x^exponent
    ''' </summary>
    Public Function pow(t As Tensor, exponent As Double) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = std.Pow(src(i), exponent)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素取负 -x
    ''' </summary>
    Public Function negative(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = -src(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素取倒数 1/x
    ''' </summary>
    Public Function reciprocal(t As Tensor) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = 1.0 / src(i)
        Next
        Return result
    End Function

#End Region

#Region "逐元素二元运算"

    ''' <summary>
    ''' 逐元素加法 a + b
    ''' </summary>
    Public Function add(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = srcA(i) + srcB(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素减法 a - b
    ''' </summary>
    Public Function subtract(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = srcA(i) - srcB(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素乘法 a * b（Hadamard 积）
    ''' </summary>
    Public Function multiply(a As Tensor, b As Tensor) As Tensor
        Return a.ElementwiseMultiply(b)
    End Function

    ''' <summary>
    ''' 逐元素除法 a / b
    ''' </summary>
    Public Function divide(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = srcA(i) / srcB(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素最大值 max(a, b)
    ''' </summary>
    Public Function maximum(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = std.Max(srcA(i), srcB(i))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素最小值 min(a, b)
    ''' </summary>
    Public Function minimum(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = std.Min(srcA(i), srcB(i))
        Next
        Return result
    End Function

#End Region

#Region "标量运算"

    ''' <summary>
    ''' 张量与标量相加 t + scalar
    ''' </summary>
    Public Function add_scalar(t As Tensor, scalar As Double) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = src(i) + scalar
        Next
        Return result
    End Function

    ''' <summary>
    ''' 张量与标量相乘 t * scalar
    ''' </summary>
    Public Function multiply_scalar(t As Tensor, scalar As Double) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            dst(i) = src(i) * scalar
        Next
        Return result
    End Function

#End Region

#Region "规约操作 - 全部元素"

    ''' <summary>
    ''' 计算所有元素的和
    ''' </summary>
    Public Function reduce_sum(t As Tensor, Optional axis As Integer? = Nothing, Optional keepdims As Boolean = False) As Tensor
        If Not axis.HasValue Then
            Dim sum As Double = 0
            Dim src = t.Data
            For i = 0 To src.Length - 1
                sum += src(i)
            Next
            Return If(keepdims, Tensor.Scalar(sum), Tensor.Scalar(sum))
        End If
        Return ReduceAlongAxis(t, axis.Value, keepdims, 0.0, Function(acc, val) acc + val)
    End Function

    ''' <summary>
    ''' 计算所有元素的平均值
    ''' </summary>
    Public Function reduce_mean(t As Tensor, Optional axis As Integer? = Nothing, Optional keepdims As Boolean = False) As Tensor
        If Not axis.HasValue Then
            Dim sum As Double = 0
            Dim src = t.Data
            For i = 0 To src.Length - 1
                sum += src(i)
            Next
            Return Tensor.Scalar(sum / src.Length)
        End If
        Dim sumResult = ReduceAlongAxis(t, axis.Value, keepdims, 0.0, Function(acc, val) acc + val)
        Dim count = t.Shape(axis.Value)
        Return multiply_scalar(sumResult, 1.0 / count)
    End Function

    ''' <summary>
    ''' 计算所有元素的最大值
    ''' </summary>
    Public Function reduce_max(t As Tensor, Optional axis As Integer? = Nothing, Optional keepdims As Boolean = False) As Tensor
        If Not axis.HasValue Then
            Dim src = t.Data
            Dim maxVal = src(0)
            For i = 1 To src.Length - 1
                If src(i) > maxVal Then maxVal = src(i)
            Next
            Return Tensor.Scalar(maxVal)
        End If
        Return ReduceAlongAxis(t, axis.Value, keepdims, Double.NegativeInfinity, Function(acc, val) std.Max(acc, val))
    End Function

    ''' <summary>
    ''' 计算所有元素的最小值
    ''' </summary>
    Public Function reduce_min(t As Tensor, Optional axis As Integer? = Nothing, Optional keepdims As Boolean = False) As Tensor
        If Not axis.HasValue Then
            Dim src = t.Data
            Dim minVal = src(0)
            For i = 1 To src.Length - 1
                If src(i) < minVal Then minVal = src(i)
            Next
            Return Tensor.Scalar(minVal)
        End If
        Return ReduceAlongAxis(t, axis.Value, keepdims, Double.PositiveInfinity, Function(acc, val) std.Min(acc, val))
    End Function

    ''' <summary>
    ''' 计算所有元素的乘积
    ''' </summary>
    Public Function reduce_prod(t As Tensor, Optional axis As Integer? = Nothing, Optional keepdims As Boolean = False) As Tensor
        If Not axis.HasValue Then
            Dim prod As Double = 1
            Dim src = t.Data
            For i = 0 To src.Length - 1
                prod *= src(i)
            Next
            Return Tensor.Scalar(prod)
        End If
        Return ReduceAlongAxis(t, axis.Value, keepdims, 1.0, Function(acc, val) acc * val)
    End Function

    ''' <summary>
    ''' 计算所有元素的标准差（总体）
    ''' </summary>
    Public Function reduce_std(t As Tensor) As Tensor
        Dim meanVal = reduce_mean(t)
        Dim src = t.Data
        Dim meanD = meanVal.Data(0)
        Dim sumSq As Double = 0
        For i = 0 To src.Length - 1
            Dim diff = src(i) - meanD
            sumSq += diff * diff
        Next
        Return Tensor.Scalar(std.Sqrt(sumSq / src.Length))
    End Function

#End Region

#Region "规约操作 - 沿轴"

    ''' <summary>
    ''' 沿指定轴找到最大值的索引（返回 Double 类型以兼容 Tensor）
    ''' </summary>
    Public Function argmax(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        If Not axis.HasValue Then
            ' 在整个展平数组上找最大值索引
            Dim src = t.Data
            Dim maxIdx = 0
            Dim maxVal = src(0)
            For i = 1 To src.Length - 1
                If src(i) > maxVal Then
                    maxVal = src(i)
                    maxIdx = i
                End If
            Next
            Return Tensor.Scalar(maxIdx)
        End If

        Return ReduceArgAxis(t, axis.Value, False)
    End Function

    ''' <summary>
    ''' 沿指定轴找到最小值的索引（返回 Double 类型以兼容 Tensor）
    ''' </summary>
    Public Function argmin(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        If Not axis.HasValue Then
            Dim src = t.Data
            Dim minIdx = 0
            Dim minVal = src(0)
            For i = 1 To src.Length - 1
                If src(i) < minVal Then
                    minVal = src(i)
                    minIdx = i
                End If
            Next
            Return Tensor.Scalar(minIdx)
        End If

        Return ReduceArgAxis(t, axis.Value, True)
    End Function

#End Region

#Region "规约辅助函数"

    ''' <summary>
    ''' 沿指定轴执行规约操作（通用实现）
    ''' </summary>
    Private Function ReduceAlongAxis(t As Tensor, axis As Integer, keepdims As Boolean,
                                     initValue As Double,
                                     reduceFunc As Func(Of Double, Double, Double)) As Tensor
        Dim rank = t.Rank
        Dim origShape = t.Shape

        ' 计算 outerSize：轴之前各维度的乘积
        Dim outerSize = 1
        For i = 0 To axis - 1
            outerSize *= origShape(i)
        Next

        ' 计算 stride：轴之后各维度的乘积（沿轴遍历时的步长）
        Dim stride = 1
        For i = axis + 1 To rank - 1
            stride *= origShape(i)
        Next

        Dim axisSize = origShape(axis)

        ' 构造输出形状
        Dim outShapeList = New List(Of Integer)
        For i = 0 To rank - 1
            If i = axis Then
                If keepdims Then outShapeList.Add(1)
            Else
                outShapeList.Add(origShape(i))
            End If
        Next

        ' 如果输出形状为空（对标量规约），返回标量
        If outShapeList.Count = 0 Then
            Dim totalVal = initValue
            Dim srcAll = t.Data
            For i = 0 To srcAll.Length - 1
                totalVal = reduceFunc(totalVal, srcAll(i))
            Next
            Return Tensor.Scalar(totalVal)
        End If

        Dim result = New Tensor(outShapeList.ToArray())
        Dim srcData = t.Data
        Dim dstData = result.Data
        Dim dstIdx = 0

        For outer = 0 To outerSize - 1
            Dim baseIdx = outer * axisSize * stride
            For inner = 0 To stride - 1
                Dim value = initValue
                For k = 0 To axisSize - 1
                    Dim srcIdx = baseIdx + k * stride + inner
                    value = reduceFunc(value, srcData(srcIdx))
                Next
                dstData(dstIdx) = value
                dstIdx += 1
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 沿指定轴找到极值的索引（argmax / argmin）
    ''' </summary>
    Private Function ReduceArgAxis(t As Tensor, axis As Integer, findMin As Boolean) As Tensor
        Dim rank = t.Rank
        Dim origShape = t.Shape

        Dim outerSize = 1
        For i = 0 To axis - 1
            outerSize *= origShape(i)
        Next

        Dim stride = 1
        For i = axis + 1 To rank - 1
            stride *= origShape(i)
        Next

        Dim axisSize = origShape(axis)

        Dim outShapeList = New List(Of Integer)
        For i = 0 To rank - 1
            If i <> axis Then outShapeList.Add(origShape(i))
        Next

        Dim result = New Tensor(outShapeList.ToArray())
        Dim srcData = t.Data
        Dim dstData = result.Data
        Dim dstIdx = 0

        For outer = 0 To outerSize - 1
            Dim baseIdx = outer * axisSize * stride
            For inner = 0 To stride - 1
                Dim bestVal = srcData(baseIdx + inner)
                Dim bestK = 0
                For k = 1 To axisSize - 1
                    Dim srcIdx = baseIdx + k * stride + inner
                    Dim val = srcData(srcIdx)
                    If findMin Then
                        If val < bestVal Then
                            bestVal = val
                            bestK = k
                        End If
                    Else
                        If val > bestVal Then
                            bestVal = val
                            bestK = k
                        End If
                    End If
                Next
                dstData(dstIdx) = bestK
                dstIdx += 1
            Next
        Next

        Return result
    End Function

#End Region

#Region "裁剪操作"

    ''' <summary>
    ''' 将张量值裁剪到 [minValue, maxValue] 范围内
    ''' </summary>
    Public Function clip_by_value(t As Tensor, minValue As Double, maxValue As Double) As Tensor
        Dim result = New Tensor(t.Shape)
        Dim src = t.Data
        Dim dst = result.Data
        For i = 0 To src.Length - 1
            If src(i) < minValue Then
                dst(i) = minValue
            ElseIf src(i) > maxValue Then
                dst(i) = maxValue
            Else
                dst(i) = src(i)
            End If
        Next
        Return result
    End Function

    ''' <summary>
    ''' 将张量按 L2 范数裁剪，确保整体范数不超过 clipNorm
    ''' </summary>
    Public Function clip_by_norm(t As Tensor, clipNorm As Double) As Tensor
        Dim l2norm = t.L2Norm()
        If l2norm > clipNorm Then
            Dim scale = clipNorm / l2norm
            Return multiply_scalar(t, scale)
        End If
        Return CType(t.Clone(), Tensor)
    End Function

#End Region

#Region "条件与比较"

    ''' <summary>
    ''' 逐元素比较 a > b，返回 1.0/0.0 的 Tensor
    ''' </summary>
    Public Function greater(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = If(srcA(i) > srcB(i), 1.0, 0.0)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素比较 a >= b，返回 1.0/0.0 的 Tensor
    ''' </summary>
    Public Function greater_equal(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = If(srcA(i) >= srcB(i), 1.0, 0.0)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素比较 a < b，返回 1.0/0.0 的 Tensor
    ''' </summary>
    Public Function less(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = If(srcA(i) < srcB(i), 1.0, 0.0)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素比较 a <= b，返回 1.0/0.0 的 Tensor
    ''' </summary>
    Public Function less_equal(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = If(srcA(i) <= srcB(i), 1.0, 0.0)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 逐元素比较 a == b，返回 1.0/0.0 的 Tensor
    ''' </summary>
    Public Function equal(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException($"张量形状必须相同: [{String.Join(",", a.Shape)}] vs [{String.Join(",", b.Shape)}]")
        End If
        Dim result = New Tensor(a.Shape)
        Dim srcA = a.Data
        Dim srcB = b.Data
        Dim dst = result.Data
        For i = 0 To dst.Length - 1
            dst(i) = If(std.Abs(srcA(i) - srcB(i)) < 1e-9, 1.0, 0.0)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 所有元素都为非零（> 0）时返回 True（以 1.0/0.0 标量 Tensor 表示）
    ''' </summary>
    Public Function reduce_all(t As Tensor, Optional axis As Integer? = Nothing, Optional keepdims As Boolean = False) As Tensor
        If Not axis.HasValue Then
            Dim src = t.Data
            For i = 0 To src.Length - 1
                If src(i) = 0.0 Then Return Tensor.Scalar(0.0)
            Next
            Return Tensor.Scalar(1.0)
        End If
        Return ReduceAlongAxis(t, axis.Value, keepdims, 1.0, Function(acc, val) If(acc <> 0.0 AndAlso val <> 0.0, 1.0, 0.0))
    End Function

    ''' <summary>
    ''' 任一元素为非零（> 0）时返回 True（以 1.0/0.0 标量 Tensor 表示）
    ''' </summary>
    Public Function reduce_any(t As Tensor, Optional axis As Integer? = Nothing, Optional keepdims As Boolean = False) As Tensor
        If Not axis.HasValue Then
            Dim src = t.Data
            For i = 0 To src.Length - 1
                If src(i) <> 0.0 Then Return Tensor.Scalar(1.0)
            Next
            Return Tensor.Scalar(0.0)
        End If
        Return ReduceAlongAxis(t, axis.Value, keepdims, 0.0, Function(acc, val) If(acc <> 0.0 OrElse val <> 0.0, 1.0, 0.0))
    End Function

#End Region

#Region "矩阵运算"

    ''' <summary>
    ''' 矩阵乘法（二维张量），等价于 MatMul
    ''' </summary>
    Public Function matmul(a As Tensor, b As Tensor) As Tensor
        Return a.MatMul(b)
    End Function

#End Region

End Module
