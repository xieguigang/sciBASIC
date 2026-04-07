#Region "Microsoft.VisualBasic::e7c72292739acbcff71e60587203a86e, Data_science\DataMining\PaCMAP\TensorExtensions.vb"

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

    '   Total Lines: 890
    '    Code Lines: 606 (68.09%)
    ' Comment Lines: 146 (16.40%)
    '    - Xml Docs: 76.71%
    ' 
    '   Blank Lines: 138 (15.51%)
    '     File Size: 32.95 KB


    '     Module TensorExtensions
    ' 
    '         Function: (+2 Overloads) [Sub], (+2 Overloads) Add, BroadcastSub, Concat, (+2 Overloads) Div
    '                   ExpandDims, Gather, GatherND, MatMul, (+2 Overloads) Mul
    '                   Neg, RandInt, Reshape, Slice, Sqrt
    '                   Square, Squeeze, Stack, Sum, SumKeepDims
    '                   Tile, TopK
    ' 
    '         Sub: CopyAlongAxis, CopyToTiledPositions, GatherAlongAxis, PrintTensor, SliceRecursive
    '              SumAlongAxis, TileRecursive
    '         Class TopKResult
    ' 
    '             Properties: Indices, Values
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace PaCMAP
    ''' <summary>
    ''' 张量扩展方法，实现链式调用
    ''' Tensor extension methods for chainable operations
    ''' </summary>
    Public Module TensorExtensions
#Region "基础数学运算 / Basic Math Operations"

        ''' <summary>
        ''' 元素取反
        ''' Element-wise negation
        ''' </summary>
        <Extension()>
        Public Function Neg(a As Tensor) As Tensor
            Dim result = New Tensor(a.Shape)
            For i As Integer = 0 To a.Length - 1
                result.Data(i) = -a.Data(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 元素平方
        ''' Element-wise square
        ''' </summary>
        <Extension()>
        Public Function Square(a As Tensor) As Tensor
            Dim result = New Tensor(a.Shape)
            For i As Integer = 0 To a.Length - 1
                result.Data(i) = a.Data(i) * a.Data(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 元素平方根
        ''' Element-wise square root
        ''' </summary>
        <Extension()>
        Public Function Sqrt(a As Tensor) As Tensor
            Dim result = New Tensor(a.Shape)
            For i As Integer = 0 To a.Length - 1
                result.Data(i) = std.Sqrt(a.Data(i))
            Next
            Return result
        End Function

        ''' <summary>
        ''' 元素加法
        ''' Element-wise addition
        ''' </summary>
        <Extension()>
        Public Function Add(a As Tensor, b As Tensor) As Tensor
            ' 标量广播
            If b.Length = 1 Then
                Dim result = New Tensor(a.Shape)
                Dim scalar As Double = b.Data(0)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) + scalar
                Next
                Return result
            End If

            ' 相同形状加法
            If a.Length = b.Length Then
                Dim result = New Tensor(a.Shape)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) + b.Data(i)
                Next
                Return result
            End If

            Throw New ArgumentException($"Shapes [{String.Join(",", a.Shape)}] and [{String.Join(",", b.Shape)}] are not compatible for addition")
        End Function

        ''' <summary>
        ''' 元素加标量
        ''' Add scalar to all elements
        ''' </summary>
        <Extension()>
        Public Function Add(a As Tensor, scalar As Double) As Tensor
            Dim result = New Tensor(a.Shape)
            For i As Integer = 0 To a.Length - 1
                result.Data(i) = a.Data(i) + scalar
            Next
            Return result
        End Function

        ''' <summary>
        ''' 元素减法
        ''' Element-wise subtraction
        ''' </summary>
        <Extension()>
        Public Function [Sub](a As Tensor, b As Tensor) As Tensor
            ' 标量广播
            If b.Length = 1 Then
                Dim result = New Tensor(a.Shape)
                Dim scalar As Double = b.Data(0)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) - scalar
                Next
                Return result
            End If

            ' 相同形状减法
            If a.Length = b.Length Then
                Dim result = New Tensor(a.Shape)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) - b.Data(i)
                Next
                Return result
            End If

            ' 广播减法 (a: [N, M], b: [N, M])
            If a.Rank = 2 AndAlso b.Rank = 2 AndAlso a.Shape(0) = b.Shape(0) AndAlso a.Shape(1) = b.Shape(1) Then
                Return a.Sub(b)
            End If

            Throw New ArgumentException($"Shapes [{String.Join(",", a.Shape)}] and [{String.Join(",", b.Shape)}] are not compatible for subtraction")
        End Function

        ''' <summary>
        ''' 元素减标量
        ''' Subtract scalar from all elements
        ''' </summary>
        <Extension()>
        Public Function [Sub](a As Tensor, scalar As Double) As Tensor
            Dim result = New Tensor(a.Shape)
            For i As Integer = 0 To a.Length - 1
                result.Data(i) = a.Data(i) - scalar
            Next
            Return result
        End Function

        ''' <summary>
        ''' 元素乘法
        ''' Element-wise multiplication
        ''' </summary>
        <Extension()>
        Public Function Mul(a As Tensor, b As Tensor) As Tensor
            ' 标量乘法
            If b.Length = 1 Then
                Dim result = New Tensor(a.Shape)
                Dim scalar As Double = b.Data(0)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) * scalar
                Next
                Return result
            End If

            ' 相同形状乘法
            If a.Length = b.Length Then
                Dim result = New Tensor(a.Shape)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) * b.Data(i)
                Next
                Return result
            End If

            Throw New ArgumentException($"Shapes [{String.Join(",", a.Shape)}] and [{String.Join(",", b.Shape)}] are not compatible for multiplication")
        End Function

        ''' <summary>
        ''' 元素乘标量
        ''' Multiply all elements by scalar
        ''' </summary>
        <Extension()>
        Public Function Mul(a As Tensor, scalar As Double) As Tensor
            Dim result = New Tensor(a.Shape)
            For i As Integer = 0 To a.Length - 1
                result.Data(i) = a.Data(i) * scalar
            Next
            Return result
        End Function

        ''' <summary>
        ''' 元素除法
        ''' Element-wise division
        ''' </summary>
        <Extension()>
        Public Function Div(a As Tensor, b As Tensor) As Tensor
            ' 标量除法
            If b.Length = 1 Then
                Dim result = New Tensor(a.Shape)
                Dim scalar As Double = b.Data(0)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) / scalar
                Next
                Return result
            End If

            ' 相同形状除法
            If a.Length = b.Length Then
                Dim result = New Tensor(a.Shape)
                For i As Integer = 0 To a.Length - 1
                    result.Data(i) = a.Data(i) / b.Data(i)
                Next
                Return result
            End If

            ' 广播除法 (a: [N, M], b: [N, 1])
            If a.Rank = 2 AndAlso b.Rank = 2 AndAlso a.Shape(0) = b.Shape(0) AndAlso b.Shape(1) = 1 Then
                Dim result = New Tensor(a.Shape)
                For i As Integer = 0 To a.Shape(0) - 1
                    For j As Integer = 0 To a.Shape(1) - 1
                        result.Data(i * a.Shape(1) + j) = a.Data(i * a.Shape(1) + j) / b.Data(i)
                    Next
                Next
                Return result
            End If

            Throw New ArgumentException($"Shapes [{String.Join(",", a.Shape)}] and [{String.Join(",", b.Shape)}] are not compatible for division")
        End Function

        ''' <summary>
        ''' 元素除标量
        ''' Divide all elements by scalar
        ''' </summary>
        <Extension()>
        Public Function Div(a As Tensor, scalar As Double) As Tensor
            Dim result = New Tensor(a.Shape)
            For i As Integer = 0 To a.Length - 1
                result.Data(i) = a.Data(i) / scalar
            Next
            Return result
        End Function

#End Region

#Region "形状操作 / Shape Operations"

        ''' <summary>
        ''' 重塑张量形状
        ''' Reshape tensor
        ''' </summary>
        <Extension()>
        Public Function Reshape(a As Tensor, ParamArray newShape As Integer()) As Tensor
            ' 处理-1维度
            Dim negIndex = Array.IndexOf(newShape, -1)
            If negIndex >= 0 Then
                Dim knownSize = 1
                For i = 0 To newShape.Length - 1
                    If i <> negIndex AndAlso newShape(i) > 0 Then
                        knownSize *= newShape(i)
                    End If
                Next
                newShape(negIndex) = a.Length / knownSize
            End If

            Dim newSize = newShape.Aggregate(1, Function(x, y) x * y)
            If newSize <> a.Length Then
                Throw New ArgumentException($"Cannot reshape tensor of size {a.Length} to shape [{String.Join(",", newShape)}]")
            End If

            Return New Tensor(a.Data, newShape)
        End Function

        ''' <summary>
        ''' 扩展维度
        ''' Expand dimensions at specified axis
        ''' </summary>
        <Extension()>
        Public Function ExpandDims(a As Tensor, Optional axis As Integer = 0) As Tensor
            If axis < 0 Then
                axis = a.Rank + axis + 1
            End If

            Dim newShape = New Integer(a.Rank + 1 - 1) {}
            Dim j = 0
            For i = 0 To newShape.Length - 1
                If i = axis Then
                    newShape(i) = 1
                Else
                    newShape(i) = a.Shape(std.Min(Threading.Interlocked.Increment(j), j - 1))
                End If
            Next

            Return New Tensor(a.Data, newShape)
        End Function

        ''' <summary>
        ''' 移除大小为1的维度
        ''' Remove dimensions of size 1
        ''' </summary>
        <Extension()>
        Public Function Squeeze(a As Tensor, Optional axis As Integer? = Nothing) As Tensor
            If axis.HasValue Then
                Dim ax = If(axis.Value < 0, a.Rank + axis.Value, axis.Value)
                If a.Shape(ax) <> 1 Then
                    Throw New ArgumentException($"Cannot squeeze dimension {ax} with size {a.Shape(ax)}")
                End If

                Dim newShape = a.Shape.Where(Function(__, i) i <> ax).ToArray()
                Return New Tensor(a.Data, newShape)
            Else
                Dim newShape = a.Shape.Where(Function(s) s <> 1).ToArray()
                If newShape.Length = 0 Then newShape = New Integer() {1}
                Return New Tensor(a.Data, newShape)
            End If
        End Function

        ''' <summary>
        ''' 平铺张量
        ''' Tile tensor along each dimension
        ''' </summary>
        <Extension()>
        Public Function Tile(a As Tensor, reps As Integer()) As Tensor
            ' 计算新形状
            Dim newShape = New Integer(std.Max(a.Rank, reps.Length) - 1) {}

            For i = 0 To newShape.Length - 1
                Dim aDim = If(i < a.Rank, a.Shape(a.Rank - 1 - i), 1)
                Dim rDim = If(i < reps.Length, reps(reps.Length - 1 - i), 1)
                newShape(newShape.Length - 1 - i) = aDim * rDim
            Next

            Dim result = New Tensor(newShape)

            ' 执行平铺
            TensorExtensions.TileRecursive(a.Data, result.Data, a.Shape, reps, New Integer(a.Rank - 1) {}, 0, 0)

            Return result
        End Function

        Private Sub TileRecursive(src As Double(), dst As Double(), srcShape As Integer(), reps As Integer(), indices As Integer(), srcIdx As Integer, dstIdx As Integer)
            If srcIdx = srcShape.Length Then
                ' 计算源扁平索引
                Dim srcFlat = 0
                Dim multiplier = 1
                For i = srcShape.Length - 1 To 0 Step -1
                    srcFlat += indices(i) * multiplier
                    multiplier *= srcShape(i)
                Next

                ' 复制到所有平铺位置
                TensorExtensions.CopyToTiledPositions(src, dst, srcShape, reps, indices, srcFlat, 0, 0)
                Return
            End If

            For i = 0 To srcShape(srcIdx) - 1
                indices(srcIdx) = i
                TensorExtensions.TileRecursive(src, dst, srcShape, reps, indices, srcIdx + 1, dstIdx)
            Next
        End Sub

        Private Sub CopyToTiledPositions(src As Double(), dst As Double(), srcShape As Integer(), reps As Integer(), indices As Integer(), srcFlat As Integer, repIdx As Integer, baseOffset As Integer)
            If repIdx = reps.Length Then
                dst(baseOffset) = src(srcFlat)
                Return
            End If

            Dim dimSize = 1
            For i = repIdx + 1 To srcShape.Length - 1
                dimSize *= srcShape(i)
            Next

            For r = 0 To reps(repIdx) - 1
                Dim newOffset = baseOffset + r * dimSize
                TensorExtensions.CopyToTiledPositions(src, dst, srcShape, reps, indices, srcFlat, repIdx + 1, newOffset)
            Next
        End Sub

#End Region

#Region "切片和索引 / Slicing and Indexing"

        ''' <summary>
        ''' 切片操作
        ''' Slice tensor
        ''' </summary>
        <Extension()>
        Public Function Slice(a As Tensor, start As Integer(), size As Integer()) As Tensor
            ' 处理-1表示"到末尾"
            For i = 0 To size.Length - 1
                If size(i) = -1 Then
                    size(i) = a.Shape(i) - start(i)
                End If
            Next

            Dim result = New Tensor(size)
            TensorExtensions.SliceRecursive(a.Data, result.Data, a.Shape, start, size, New Integer(a.Rank - 1) {}, 0)
            Return result
        End Function

        Private Sub SliceRecursive(src As Double(), dst As Double(), srcShape As Integer(), start As Integer(), size As Integer(), indices As Integer(), [dim] As Integer)
            If [dim] = srcShape.Length Then
                Dim srcFlat = 0
                Dim multiplier = 1
                For i = srcShape.Length - 1 To 0 Step -1
                    srcFlat += (start(i) + indices(i)) * multiplier
                    multiplier *= srcShape(i)
                Next

                Dim dstFlat = 0
                multiplier = 1
                For i = size.Length - 1 To 0 Step -1
                    dstFlat += indices(i) * multiplier
                    multiplier *= size(i)
                Next

                dst(dstFlat) = src(srcFlat)
                Return
            End If

            For i = 0 To size([dim]) - 1
                indices([dim]) = i
                TensorExtensions.SliceRecursive(src, dst, srcShape, start, size, indices, [dim] + 1)
            Next
        End Sub

        ''' <summary>
        ''' 沿轴连接张量
        ''' Concatenate tensors along axis
        ''' </summary>
        <Extension()>
        Public Function Concat(a As Tensor, b As Tensor, Optional axis As Integer = 0) As Tensor
            If a.Rank <> b.Rank Then
                Throw New ArgumentException("Tensors must have the same rank for concatenation")
            End If

            For i As Integer = 0 To a.Rank - 1
                If i <> axis AndAlso a.Shape(i) <> b.Shape(i) Then
                    Throw New ArgumentException($"Shape mismatch at dimension {i}: {a.Shape(i)} vs {b.Shape(i)}")
                End If
            Next

            Dim newShape = CType(a.Shape.Clone(), Integer())
            newShape(axis) = a.Shape(axis) + b.Shape(axis)

            Dim result = New Tensor(newShape)

            ' 复制a的数据
            TensorExtensions.CopyAlongAxis(a.Data, result.Data, a.Shape, newShape, axis, 0)
            ' 复制b的数据
            TensorExtensions.CopyAlongAxis(b.Data, result.Data, b.Shape, newShape, axis, a.Shape(axis))

            Return result
        End Function

        Private Sub CopyAlongAxis(src As Double(), dst As Double(), srcShape As Integer(), dstShape As Integer(), axis As Integer, offset As Integer)
            Dim totalElements = srcShape.Aggregate(1, Function(x, y) x * y)
            Dim stride = 1
            For i = axis + 1 To srcShape.Length - 1
                stride *= srcShape(i)
            Next

            Dim blockSize = stride
            Dim numBlocks As Integer = totalElements / blockSize

            For block = 0 To numBlocks - 1
                Dim srcStart = block * blockSize
                Dim dstStart As Integer = block / srcShape(axis) * dstShape(axis) * blockSize + (block Mod srcShape(axis) + offset) * blockSize

                If axis = 0 Then
                    dstStart = (block / srcShape(axis) * dstShape(axis) + block Mod srcShape(axis) + offset) * blockSize
                Else
                    dstStart = block * blockSize + offset * blockSize
                End If

                Array.Copy(src, srcStart, dst, dstStart, blockSize)
            Next
        End Sub

        ''' <summary>
        ''' 堆叠张量数组
        ''' Stack tensor array along new axis
        ''' </summary>
        <Extension()>
        Public Function Stack(tensors As Tensor(), Optional axis As Integer = 0) As Tensor
            If tensors Is Nothing OrElse tensors.Length = 0 Then
                Throw New ArgumentException("Cannot stack empty array")
            End If

            Dim first = tensors(0)
            For Each t In tensors
                If Not t.Shape.SequenceEqual(first.Shape) Then
                    Throw New ArgumentException("All tensors must have the same shape for stacking")
                End If
            Next

            Dim newShape = New Integer(first.Rank + 1 - 1) {}
            Dim j = 0
            For i = 0 To newShape.Length - 1
                If i = axis Then
                    newShape(i) = tensors.Length
                Else
                    newShape(i) = first.Shape(std.Min(Threading.Interlocked.Increment(j), j - 1))
                End If
            Next

            Dim result = New Tensor(newShape)

            For t = 0 To tensors.Length - 1
                For i As Integer = 0 To tensors(t).Length - 1
                    Dim indices = tensors(t).GetIndices(i)
                    Dim newIndices = New Integer(newShape.Length - 1) {}
                    j = 0
                    For k = 0 To newIndices.Length - 1
                        If k = axis Then
                            newIndices(k) = t
                        Else
                            newIndices(k) = indices(std.Min(Threading.Interlocked.Increment(j), j - 1))
                        End If
                    Next
                    result.SetValue(tensors(t).Data(i), newIndices)
                Next
            Next

            Return result
        End Function

#End Region

#Region "归约操作 / Reduction Operations"

        <Extension()>
        Private Function SumKeepDims(a As Tensor, sum As Double) As Tensor
            Dim shape = Enumerable.Repeat(1, a.Rank).ToArray()
            Dim result = New Tensor(shape)
            result.Data(0) = sum
            Return result
        End Function

        ''' <summary>
        ''' 沿轴求和
        ''' Sum along axis
        ''' </summary>
        <Extension()>
        Public Function Sum(a As Tensor, Optional axis As Integer? = Nothing, Optional keepDims As Boolean = False) As Tensor
            If Not axis.HasValue Then
                ' 全局求和
                Dim lSum As Double = a.Data.Sum()
                If keepDims Then
                    Return a.SumKeepDims(lSum)
                End If
                Return Tensor.Scalar(lSum)
            End If

            Dim ax = If(axis.Value < 0, a.Rank + axis.Value, axis.Value)

            ' 计算结果形状
            Dim resultShape = New List(Of Integer)()
            For i As Integer = 0 To a.Rank - 1
                If i = ax Then
                    If keepDims Then resultShape.Add(1)
                Else
                    resultShape.Add(a.Shape(i))
                End If
            Next

            If resultShape.Count = 0 Then resultShape.Add(1)

            Dim result = New Tensor(resultShape.ToArray())

            ' 执行求和
            TensorExtensions.SumAlongAxis(a.Data, result.Data, a.Shape, ax)

            Return result
        End Function

        Private Sub SumAlongAxis(src As Double(), dst As Double(), shape As Integer(), axis As Integer)
            Dim outerSize = 1
            For i = 0 To axis - 1
                outerSize *= shape(i)
            Next

            Dim axisSize = shape(axis)

            Dim innerSize = 1
            For i = axis + 1 To shape.Length - 1
                innerSize *= shape(i)
            Next

            For outer = 0 To outerSize - 1
                For inner = 0 To innerSize - 1
                    Dim sum As Double = 0
                    For ax = 0 To axisSize - 1
                        Dim idx = outer * axisSize * innerSize + ax * innerSize + inner
                        sum += src(idx)
                    Next
                    dst(outer * innerSize + inner) = sum
                Next
            Next
        End Sub

#End Region

#Region "矩阵运算 / Matrix Operations"

        ''' <summary>
        ''' 矩阵乘法
        ''' Matrix multiplication
        ''' </summary>
        <Extension()>
        Public Function MatMul(a As Tensor, b As Tensor, Optional transposeA As Boolean = False, Optional transposeB As Boolean = False) As Tensor
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("MatMul requires 2D tensors")
            End If

            Dim m As Integer = If(transposeA, a.Shape(1), a.Shape(0))
            Dim k As Integer = If(transposeA, a.Shape(0), a.Shape(1))
            Dim k2 As Integer = If(transposeB, b.Shape(1), b.Shape(0))
            Dim n As Integer = If(transposeB, b.Shape(0), b.Shape(1))

            If k <> k2 Then
                Throw New ArgumentException($"Matrix dimensions incompatible: {m}x{k} and {k2}x{n}")
            End If

            Dim result = New Tensor(New Integer() {m, n})

            For i = 0 To m - 1
                For j = 0 To n - 1
                    Dim sum As Double = 0
                    For l = 0 To k - 1
                        Dim aIdx As Integer = If(transposeA, l * a.Shape(1) + i, i * a.Shape(1) + l)
                        Dim bIdx As Integer = If(transposeB, j * b.Shape(1) + l, l * b.Shape(1) + j)
                        sum += a.Data(aIdx) * b.Data(bIdx)
                    Next
                    result.Data(i * n + j) = sum
                Next
            Next

            Return result
        End Function

#End Region

#Region "TopK操作 / TopK Operations"

        ''' <summary>
        ''' TopK结果
        ''' TopK result structure
        ''' </summary>
        Public Class TopKResult
            Public Property Values As Tensor
            Public Property Indices As Tensor
        End Class

        ''' <summary>
        ''' 获取最大的K个元素
        ''' Get top K elements
        ''' </summary>
        <Extension()>
        Public Function TopK(a As Tensor, k As Integer, Optional sorted As Boolean = True) As TensorExtensions.TopKResult
            If a.Rank < 1 Then
                Throw New ArgumentException("TopK requires at least 1D tensor")
            End If

            Dim lastDim As Integer = a.Shape(a.Rank - 1)
            If k > lastDim Then
                Throw New ArgumentException($"k ({k}) cannot be larger than last dimension ({lastDim})")
            End If

            ' 计算结果形状
            Dim resultShape = CType(a.Shape.Clone(), Integer())
            resultShape(a.Rank - 1) = k

            Dim values = New Tensor(resultShape)
            Dim indices = New Tensor(resultShape)

            ' 沿最后一维执行TopK
            Dim numVectors As Integer = a.Length / lastDim

            For v = 0 To numVectors - 1
                Dim startIdx = v * lastDim

                ' 创建索引值对
                Dim pairs = New List(Of (value As Double, index As Integer))()
                For i = 0 To lastDim - 1
                    pairs.Add((a.Data(startIdx + i), i))
                Next

                ' 排序获取TopK
                Dim sortedPairs = If(sorted, pairs.OrderByDescending(Function(p) p.value).Take(k).ToList(), pairs.OrderByDescending(Function(p) p.value).Take(k).ToList())

                ' 写入结果
                For i = 0 To k - 1
                    values.Data(v * k + i) = sortedPairs(i).value
                    indices.Data(v * k + i) = sortedPairs(i).index
                Next
            Next

            Return New TensorExtensions.TopKResult With {
                .Values = values,
                .Indices = indices
            }
        End Function

#End Region

#Region "Gather操作 / Gather Operations"

        ''' <summary>
        ''' 沿轴收集元素
        ''' Gather elements along axis
        ''' </summary>
        <Extension()>
        Public Function Gather(a As Tensor, indices As Tensor, Optional axis As Integer = 0) As Tensor
            If axis < 0 OrElse axis >= a.Rank Then
                Throw New ArgumentException($"Invalid axis {axis} for tensor with rank {a.Rank}")
            End If

            ' 计算结果形状
            Dim resultShape = New List(Of Integer)()
            For i As Integer = 0 To a.Rank - 1
                If i = axis Then
                    resultShape.AddRange(indices.Shape)
                Else
                    resultShape.Add(a.Shape(i))
                End If
            Next

            Dim result = New Tensor(resultShape.ToArray())

            ' 执行Gather
            TensorExtensions.GatherAlongAxis(a.Data, result.Data, a.Shape, indices.Data, axis)

            Return result
        End Function

        Private Sub GatherAlongAxis(src As Double(), dst As Double(), srcShape As Integer(), indices As Double(), axis As Integer)
            Dim outerSize = 1
            For i = 0 To axis - 1
                outerSize *= srcShape(i)
            Next

            Dim axisSize = srcShape(axis)

            Dim innerSize = 1
            For i = axis + 1 To srcShape.Length - 1
                innerSize *= srcShape(i)
            Next

            Dim numIndices = indices.Length

            For outer = 0 To outerSize - 1
                For idx = 0 To numIndices - 1
                    Dim srcIdx As Integer = indices(idx)
                    For inner = 0 To innerSize - 1
                        Dim srcPos = outer * axisSize * innerSize + srcIdx * innerSize + inner
                        Dim dstPos = (outer * numIndices + idx) * innerSize + inner
                        dst(dstPos) = src(srcPos)
                    Next
                Next
            Next
        End Sub

        ''' <summary>
        ''' 使用多维索引收集元素
        ''' Gather elements using multi-dimensional indices
        ''' </summary>
        <Extension()>
        Public Function GatherND(a As Tensor, indices As Tensor) As Tensor
            If indices.Rank < 1 Then
                Throw New ArgumentException("Indices must have at least 1 dimension")
            End If

            Dim lastDim As Integer = indices.Shape(indices.Rank - 1)
            If lastDim > a.Rank Then
                Throw New ArgumentException($"Index depth {lastDim} exceeds tensor rank {a.Rank}")
            End If

            ' 计算结果形状
            Dim resultShape = New List(Of Integer)()
            For i As Integer = 0 To indices.Rank - 1 - 1
                resultShape.Add(indices.Shape(i))
            Next
            For i As Integer = lastDim To a.Rank - 1
                resultShape.Add(a.Shape(i))
            Next

            If resultShape.Count = 0 Then resultShape.Add(1)

            Dim result = New Tensor(resultShape.ToArray())

            ' 执行GatherND
            Dim numIndices As Integer = indices.Length / lastDim
            Dim elementSize = 1
            For i As Integer = lastDim To a.Rank - 1
                elementSize *= a.Shape(i)
            Next

            For i = 0 To numIndices - 1
                ' 计算源位置
                Dim srcPos = 0
                Dim multiplier = 1
                For j = lastDim - 1 To 0 Step -1
                    Dim idx = CInt(indices.Data(i * lastDim + j))
                    srcPos += idx * multiplier
                    multiplier *= a.Shape(j)
                Next
                srcPos *= elementSize

                ' 复制元素
                For j = 0 To elementSize - 1
                    result.Data(i * elementSize + j) = a.Data(srcPos + j)
                Next
            Next

            Return result
        End Function

#End Region

#Region "随机操作 / Random Operations"

        ''' <summary>
        ''' 创建随机整数张量
        ''' Create random integer tensor
        ''' </summary>
        Public Function RandInt(shape As Integer(), min As Integer, max As Integer, Optional seed As Integer? = Nothing) As Tensor
            Dim random = If(seed.HasValue, New Random(seed.Value), New Random())
            Dim tensor = New Tensor(shape)

            For i As Integer = 0 To tensor.Length - 1
                tensor.Data(i) = random.Next(min, max)
            Next

            Return tensor
        End Function

#End Region

#Region "广播操作 / Broadcasting Operations"

        ''' <summary>
        ''' 广播减法 (a: [N, 1, D], b: [N, M, D])
        ''' Broadcast subtraction for distance calculation
        ''' </summary>
        <Extension()>
        Public Function BroadcastSub(a As Tensor, b As Tensor) As Tensor
            ' 特殊处理: a.Shape = [N, 1, D], b.Shape = [N, M, D]
            If a.Rank = 3 AndAlso b.Rank = 3 AndAlso a.Shape(0) = b.Shape(0) AndAlso a.Shape(1) = 1 AndAlso a.Shape(2) = b.Shape(2) Then
                Dim n As Integer = a.Shape(0)
                Dim m As Integer = b.Shape(1)
                Dim d As Integer = a.Shape(2)

                Dim result = New Tensor(New Integer() {n, m, d})

                For i = 0 To n - 1
                    For j = 0 To m - 1
                        For k = 0 To d - 1
                            Dim aIdx = i * d + k
                            Dim bIdx = i * m * d + j * d + k
                            Dim rIdx = i * m * d + j * d + k
                            result.Data(rIdx) = a.Data(aIdx) - b.Data(bIdx)
                        Next
                    Next
                Next

                Return result
            End If

            ' 一般广播减法
            Throw New NotImplementedException($"Broadcast subtraction not implemented for shapes [{String.Join(",", a.Shape)}] and [{String.Join(",", b.Shape)}]")
        End Function

#End Region

        ''' <summary>
        ''' 打印张量内容
        ''' Print tensor contents
        ''' </summary>
        <Extension>
        Public Sub PrintTensor(t As Tensor)
            If t.Rank = 1 Then
                Console.WriteLine($"[{String.Join(", ", t.Data.Select(Function(x) x.ToString("F4")))}]")
            ElseIf t.Rank = 2 Then
                Dim rows As Integer = t.Shape(0)
                Dim cols As Integer = t.Shape(1)
                For i = 0 To std.Min(5, rows) - 1
                    Console.Write("[")
                    For j = 0 To std.Min(5, cols) - 1
                        Console.Write($"{t.Data(i * cols + j):F4}")
                        If j < cols - 1 Then Console.Write(", ")
                    Next
                    If cols > 5 Then Console.Write(", ...")
                    Console.WriteLine("]")
                Next
                If rows > 5 Then Console.WriteLine("...")
            Else
                Console.WriteLine($"Tensor with shape [{String.Join(",", t.Shape)}]")
            End If
        End Sub
    End Module
End Namespace
