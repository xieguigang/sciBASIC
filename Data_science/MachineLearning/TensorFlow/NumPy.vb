#Region "Microsoft.VisualBasic::c3dc987273b8bf22b1adc72fd0efec0f, Data_science\MachineLearning\TensorFlow\NumPy.vb"

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

    '   Total Lines: 734
    '    Code Lines: 412 (56.13%)
    ' Comment Lines: 200 (27.25%)
    '    - Xml Docs: 91.50%
    ' 
    '   Blank Lines: 122 (16.62%)
    '     File Size: 23.36 KB


    ' Module NumPyModule
    ' 
    '     Function: [where], abs, arange, argmax, argmin
    '               array, as_double_array, clip, concatenate, cos
    '               dot, equal, exp, expand_dims, eye
    '               flatten, full, greater, hstack, identity
    '               less, linspace, log, logspace, matmul
    '               max, maximum, mean, min, minimum
    '               ndim, ones, power, prod, reshape
    '               shape, sin, size, split, sqrt
    '               square, squeeze, stack, std, sum
    '               tanh, transpose, vstack, zeros
    '     Class RandomState
    ' 
    '         Function: rand, randint, randn
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports tf = Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace NumPy

''' <summary>
''' NumPy 兼容 API 模块
''' 提供类 NumPy 的静态方法，全部基于 Tensor 实现
''' </summary>
''' <remarks>
''' 典型用法:
''' Imports np = Microsoft.VisualBasic.MachineLearning.TensorFlow.NumPy
''' Dim a = np.zeros(3, 4)
''' Dim b = np.dot(a, a.transpose())
''' </remarks>
Public Module NumPyModule

#Region "随机数"

    ''' <summary>
    ''' NumPy 风格的随机数子模块
    ''' np.random.rand(...) 和 np.random.randn(...) 对应 NumPy.RandomState.rand(...)
    ''' </summary>
    Public Class RandomState

        ''' <summary>
        ''' 生成均匀分布 [0, 1) 的随机张量 (np.random.rand)
        ''' </summary>
        Public Shared Function rand(ParamArray shape As Integer()) As Tensor
            Return Tensor.Random(shape, 0, 1)
        End Function

        ''' <summary>
        ''' 生成标准正态分布 N(0,1) 的随机张量 (np.random.randn)
        ''' </summary>
        Public Shared Function randn(ParamArray shape As Integer()) As Tensor
            Return Tensor.RandomNormal(shape, 0, 1)
        End Function

        ''' <summary>
        ''' 生成 [low, high) 均匀分布的随机整数张量 (np.random.randint)
        ''' </summary>
        Public Shared Function randint(low As Integer, high As Integer, ParamArray shape As Integer()) As Tensor
            Dim rnd = New System.Random()
            Dim t = New Tensor(shape)
            Dim dst = t.Data
            For i = 0 To dst.Length - 1
                dst(i) = rnd.[Next](low, high)
            Next
            Return t
        End Function

    End Class

#End Region

#Region "数组创建"

    ''' <summary>
    ''' 从数据和形状创建张量 (np.array)
    ''' </summary>
    Public Function array(data As Double(), ParamArray shape As Integer()) As Tensor
        Return New Tensor(data, shape)
    End Function

    ''' <summary>
    ''' 创建全零张量 (np.zeros)
    ''' </summary>
    Public Function zeros(ParamArray shape As Integer()) As Tensor
        Return Tensor.Zeros(shape)
    End Function

    ''' <summary>
    ''' 创建全一张量 (np.ones)
    ''' </summary>
    Public Function ones(ParamArray shape As Integer()) As Tensor
        Return Tensor.Ones(shape)
    End Function

    ''' <summary>
    ''' 创建填充指定值的张量 (np.full)
    ''' </summary>
    Public Function full(shape As Integer(), value As Double) As Tensor
        Dim t = New Tensor(shape)
        System.Array.Fill(t.Data, value)
        Return t
    End Function

    ''' <summary>
    ''' 创建单位矩阵 (np.eye)
    ''' </summary>
    ''' <param name="n">行数</param>
    ''' <param name="m">列数（可选，默认为 n）</param>
    Public Function eye(n As Integer, Optional m As Integer? = Nothing) As Tensor
        Dim cols = If(m.HasValue, m.Value, n)
        If cols = n Then
            Return Tensor.Identity(n)
        End If
        Dim result = New Tensor(n, cols)
        Dim minDim = System.Math.Min(n, cols)
        For i = 0 To minDim - 1
            result(i, i) = 1.0
        Next
        Return result
    End Function

    ''' <summary>
    ''' 创建单位矩阵 (np.identity)
    ''' </summary>
    Public Function identity(size As Integer) As Tensor
        Return Tensor.Identity(size)
    End Function

    ''' <summary>
    ''' 创建等差数列张量 (np.arange)
    ''' </summary>
    Public Function arange(startOrStop As Double, Optional [stop] As Double? = Nothing, Optional [step] As Double = 1.0) As Tensor
        Dim [end] As Double
        Dim start As Double

        If [stop].HasValue Then
            start = startOrStop
            [end] = [stop].Value
        Else
            start = 0
            [end] = startOrStop
        End If

        ' 计算元素个数
        Dim count = CInt(System.Math.Ceiling(([end] - start) / [step]))
        If count <= 0 Then count = 0

        Dim t = New Tensor(count)
        Dim dst = t.Data
        For i = 0 To count - 1
            dst(i) = start + i * [step]
        Next
        Return t
    End Function

    ''' <summary>
    ''' 创建等间距张量 (np.linspace)
    ''' </summary>
    ''' <param name="start">起始值</param>
    ''' <param name="[stop]">终止值（包含）</param>
    ''' <param name="num">元素个数，默认 50</param>
    Public Function linspace(start As Double, [stop] As Double, Optional num As Integer = 50) As Tensor
        If num < 2 Then
            Return Tensor.Scalar(start)
        End If

        Dim t = New Tensor(num)
        Dim dst = t.Data
        Dim stepVal = ([stop] - start) / (num - 1)
        For i = 0 To num - 1
            dst(i) = start + i * stepVal
        Next
        Return t
    End Function

    ''' <summary>
    ''' 创建对数等间距张量 (np.logspace)
    ''' </summary>
    ''' <param name="start">10^start 起始</param>
    ''' <param name="[stop]">10^stop 终止</param>
    ''' <param name="num">元素个数，默认 50</param>
    ''' <param name="base">对数底数，默认 10.0</param>
    Public Function logspace(start As Double, [stop] As Double, Optional num As Integer = 50, Optional base As Double = 10.0) As Tensor
        Dim lin = linspace(start, [stop], num)
        Dim result = New Tensor(num)
        Dim dst = result.Data
        Dim src = lin.Data
        For i = 0 To num - 1
            dst(i) = System.Math.Pow(base, src(i))
        Next
        Return result
    End Function

#End Region

#Region "形状操作"

    ''' <summary>
    ''' 重塑张量形状 (np.reshape)
    ''' 返回新的张量，不修改原张量
    ''' </summary>
    Public Function reshape(t As Tensor, ParamArray shape As Integer()) As Tensor
        Dim newShape = shape
        ' 支持 -1 自动推断维度
        Dim negIdx = -1
        For i = 0 To newShape.Length - 1
            If newShape(i) = -1 Then
                If negIdx >= 0 Then Throw New ArgumentException("只能有一个维度为 -1")
                negIdx = i
            End If
        Next

        If negIdx >= 0 Then
            Dim knownProduct = 1
            For i = 0 To newShape.Length - 1
                If i <> negIdx Then knownProduct *= newShape(i)
            Next
            newShape = CType(newShape.Clone(), Integer())
            newShape(negIdx) = t.Length \ knownProduct
        End If

        Dim knownSize = 1
        For i = 0 To newShape.Length - 1
            knownSize *= newShape(i)
        Next

        If knownSize <> t.Length Then
            Throw New ArgumentException($"无法将形状 [{String.Join(",", t.Shape)}] 重塑为 [{String.Join(",", newShape)}]，元素总数不匹配 ({t.Length} vs {knownSize})")
        End If

        Return New Tensor(CType(t.Data.Clone(), Double()), newShape)
    End Function

    ''' <summary>
    ''' 转置张量 (np.transpose)
    ''' </summary>
    Public Function transpose(t As Tensor) As Tensor
        Return t.Transpose()
    End Function

    ''' <summary>
    ''' 在指定位置插入一个大小为 1 的维度 (np.expand_dims)
    ''' </summary>
    Public Function expand_dims(t As Tensor, axis As Integer) As Tensor
        Dim rank = t.Rank
        If axis < 0 Then axis = rank + axis + 1

        ' 构建新形状：在原形状的 axis 位置插入 1
        Dim newShape = New List(Of Integer)
        For i = 0 To rank - 1
            If i = axis Then newShape.Add(1)
            newShape.Add(t.Shape(i))
        Next
        If axis = rank Then newShape.Add(1)

        Return New Tensor(CType(t.Data.Clone(), Double()), newShape.ToArray())
    End Function

    ''' <summary>
    ''' 移除大小为 1 的维度 (np.squeeze)
    ''' </summary>
    ''' <param name="t">输入张量</param>
    ''' <param name="axis">指定要移除的轴，若不指定则移除所有大小为 1 的维度</param>
    Public Function squeeze(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Dim newShape = New List(Of Integer)

        If axis.HasValue Then
            Dim ax = axis.Value
            If ax < 0 Then ax = t.Rank + ax

            If t.Shape(ax) <> 1 Then
                Throw New ArgumentException($"轴 {ax} 的大小为 {t.Shape(ax)}，不是 1，无法 squeeze")
            End If

            For i = 0 To t.Rank - 1
                If i <> ax Then newShape.Add(t.Shape(i))
            Next
        Else
            For i = 0 To t.Rank - 1
                If t.Shape(i) <> 1 Then newShape.Add(t.Shape(i))
            Next

            ' 如果全部是 1，保留一个维度
            If newShape.Count = 0 Then
                newShape.Add(1)
            End If
        End If

        Return New Tensor(CType(t.Data.Clone(), Double()), newShape.ToArray())
    End Function

    ''' <summary>
    ''' 展平张量为一维 (np.flatten / np.ravel)
    ''' </summary>
    Public Function flatten(t As Tensor) As Tensor
        Return New Tensor(CType(t.Data.Clone(), Double()), t.Length)
    End Function

#End Region

#Region "矩阵运算"

    ''' <summary>
    ''' 矩阵乘法 (np.dot / np.matmul)
    ''' </summary>
    Public Function dot(a As Tensor, b As Tensor) As Tensor
        Return a.MatMul(b)
    End Function

    ''' <summary>
    ''' 矩阵乘法 (np.matmul)
    ''' </summary>
    Public Function matmul(a As Tensor, b As Tensor) As Tensor
        Return a.MatMul(b)
    End Function

#End Region

#Region "拼接与分割"

    ''' <summary>
    ''' 沿指定轴拼接多个张量 (np.concatenate)
    ''' </summary>
    Public Function concatenate(tensors As Tensor(), Optional axis As Integer = 0) As Tensor
        If tensors Is Nothing OrElse tensors.Length = 0 Then
            Throw New ArgumentException("至少需要一个张量")
        End If

        If tensors.Length = 1 Then
            Return CType(tensors(0).Clone(), Tensor)
        End If

        Dim rank = tensors(0).Rank
        If axis < 0 Then axis = rank + axis

        ' 验证所有张量非轴维度形状一致
        Dim refShape = tensors(0).Shape
        For idx = 1 To tensors.Length - 1
            Dim shp = tensors(idx).Shape
            If shp.Length <> rank Then
                Throw New ArgumentException($"所有张量的维度数必须相同: 张量 0 有 {rank} 维，张量 {idx} 有 {shp.Length} 维")
            End If
            For d = 0 To rank - 1
                If d <> axis AndAlso shp(d) <> refShape(d) Then
                    Throw New ArgumentException($"非拼接轴维度不匹配: 轴 {d} 上张量 0 大小为 {refShape(d)}，张量 {idx} 大小为 {shp(d)}")
                End If
            Next
        Next

        ' 计算输出形状
        Dim outShape = CType(refShape.Clone(), Integer())
        outShape(axis) = 0
        For Each t In tensors
            outShape(axis) += t.Shape(axis)
        Next

        ' 计算拼接轴前后的尺寸
        Dim preAxisSize = 1
        For i = 0 To axis - 1
            preAxisSize *= outShape(i)
        Next

        Dim postAxisSize = 1
        For i = axis + 1 To rank - 1
            postAxisSize *= outShape(i)
        Next

        Dim result = New Tensor(outShape)
        Dim dstData = result.Data
        Dim dstOffset = 0

        ' 对每个 preAxis 块，依次复制各张量沿轴的数据
        For block = 0 To preAxisSize - 1
            For Each t In tensors
                Dim axisLen = t.Shape(axis)
                Dim srcData = t.Data
                Dim srcBase = block * axisLen * postAxisSize
                Dim copyLen = axisLen * postAxisSize
                System.Array.Copy(srcData, srcBase, dstData, dstOffset, copyLen)
                dstOffset += copyLen
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 沿新轴堆叠多个张量 (np.stack)
    ''' </summary>
    Public Function stack(tensors As Tensor(), Optional axis As Integer = 0) As Tensor
        If tensors Is Nothing OrElse tensors.Length = 0 Then
            Throw New ArgumentException("至少需要一个张量")
        End If

        Dim rank = tensors(0).Rank
        If axis < 0 Then axis = rank + axis + 1

        ' 验证所有张量形状相同
        Dim refShape = tensors(0).Shape
        For idx = 1 To tensors.Length - 1
            If Not refShape.SequenceEqual(tensors(idx).Shape) Then
                Throw New ArgumentException($"所有张量形状必须相同: 张量 0: [{String.Join(",", refShape)}]，张量 {idx}: [{String.Join(",", tensors(idx).Shape)}]")
            End If
        Next

        ' 构建输出形状
        Dim outShape = New List(Of Integer)
        For i = 0 To rank
            If i = axis Then
                outShape.Add(tensors.Length)
            End If
            If i < rank Then
                outShape.Add(refShape(i))
            End If
        Next

        Dim result = New Tensor(outShape.ToArray())
        Dim dstData = result.Data

        ' 计算 preSize: 轴之前各维度的乘积（在原始形状中的分组数）
        Dim preSize = 1
        For i = 0 To axis - 1
            preSize *= refShape(i)
        Next

        ' 计算 postSize: 轴之后各维度的乘积（每个分组内的元素数）
        Dim postSize = 1
        For i = axis To rank - 1
            postSize *= refShape(i)
        Next

        Dim numTensors = tensors.Length

        ' 按分组交错复制数据：
        ' 对每个 preSize 分组，依次从各张量复制 postSize 个元素
        For i = 0 To preSize - 1
            For j = 0 To numTensors - 1
                Dim srcStart = i * postSize
                Dim dstStart = (i * numTensors + j) * postSize
                System.Array.Copy(tensors(j).Data, srcStart, dstData, dstStart, postSize)
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 垂直堆叠 (np.vstack) - 相当于 axis=0 的 concatenate
    ''' </summary>
    Public Function vstack(tensors As Tensor()) As Tensor
        Return concatenate(tensors, axis:=0)
    End Function

    ''' <summary>
    ''' 水平堆叠 (np.hstack) - 对于 1D 相当于 axis=0 的 concatenate，对于 2D+ 相当于 axis=1
    ''' </summary>
    Public Function hstack(tensors As Tensor()) As Tensor
        If tensors(0).Rank = 1 Then
            Return concatenate(tensors, axis:=0)
        End If
        Return concatenate(tensors, axis:=1)
    End Function

    ''' <summary>
    ''' 将张量沿指定轴分割为多个子张量 (np.split)
    ''' </summary>
    Public Function split(t As Tensor, numSplits As Integer, Optional axis As Integer = 0) As Tensor()
        Dim rank = t.Rank
        If axis < 0 Then axis = rank + axis

        Dim axisSize = t.Shape(axis)
        If axisSize Mod numSplits <> 0 Then
            Throw New ArgumentException($"轴 {axis} 的大小 {axisSize} 不能被 {numSplits} 整除")
        End If

        Dim splitSize = axisSize \ numSplits

        ' 计算分割前后的尺寸
        Dim preAxisSize = 1
        For i = 0 To axis - 1
            preAxisSize *= t.Shape(i)
        Next

        Dim postAxisSize = 1
        For i = axis + 1 To rank - 1
            postAxisSize *= t.Shape(i)
        Next

        ' 构建每个子张量的形状
        Dim subShape = CType(t.Shape.Clone(), Integer())
        subShape(axis) = splitSize

        Dim results = New Tensor(numSplits - 1) {}
        Dim srcData = t.Data

        For s = 0 To numSplits - 1
            Dim subData = New Double(preAxisSize * splitSize * postAxisSize - 1) {}
            Dim subDstIdx = 0

            For block = 0 To preAxisSize - 1
                Dim srcBase = (block * axisSize + s * splitSize) * postAxisSize
                System.Array.Copy(srcData, srcBase, subData, subDstIdx, splitSize * postAxisSize)
                subDstIdx += splitSize * postAxisSize
            Next

            results(s) = New Tensor(subData, subShape)
        Next

        Return results
    End Function

#End Region

#Region "统计函数"

    ''' <summary>
    ''' 计算所有元素或沿指定轴的和 (np.sum)
    ''' </summary>
    Public Function sum(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Return Math.reduce_sum(t, axis:=axis)
    End Function

    ''' <summary>
    ''' 计算所有元素或沿指定轴的均值 (np.mean)
    ''' </summary>
    Public Function mean(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Return Math.reduce_mean(t, axis:=axis)
    End Function

    ''' <summary>
    ''' 计算所有元素或沿指定轴的最大值 (np.max)
    ''' </summary>
    Public Function max(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Return Math.reduce_max(t, axis:=axis)
    End Function

    ''' <summary>
    ''' 计算所有元素或沿指定轴的最小值 (np.min)
    ''' </summary>
    Public Function min(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Return Math.reduce_min(t, axis:=axis)
    End Function

    ''' <summary>
    ''' 找到最大值索引 (np.argmax)
    ''' </summary>
    Public Function argmax(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Return Math.argmax(t, axis:=axis)
    End Function

    ''' <summary>
    ''' 找到最小值索引 (np.argmin)
    ''' </summary>
    Public Function argmin(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Return Math.argmin(t, axis:=axis)
    End Function

    ''' <summary>
    ''' 计算标准差 (np.std)
    ''' </summary>
    Public Function std(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        If Not axis.HasValue Then
            Return Math.reduce_std(t)
        End If
        ' 沿轴的标准差需要更复杂的实现，暂时返回整体标准差
        Return Math.reduce_std(t)
    End Function

    ''' <summary>
    ''' 所有元素的乘积 (np.prod)
    ''' </summary>
    Public Function prod(t As Tensor, Optional axis As Integer? = Nothing) As Tensor
        Return Math.reduce_prod(t, axis:=axis)
    End Function

#End Region

#Region "数学函数"

    ''' <summary>
    ''' 逐元素指数 (np.exp)
    ''' </summary>
    Public Function exp(t As Tensor) As Tensor
        Return Math.exp(t)
    End Function

    ''' <summary>
    ''' 逐元素自然对数 (np.log)
    ''' </summary>
    Public Function log(t As Tensor) As Tensor
        Return Math.log(t)
    End Function

    ''' <summary>
    ''' 逐元素平方根 (np.sqrt)
    ''' </summary>
    Public Function sqrt(t As Tensor) As Tensor
        Return Math.sqrt(t)
    End Function

    ''' <summary>
    ''' 逐元素平方 (np.square)
    ''' </summary>
    Public Function square(t As Tensor) As Tensor
        Return Math.square(t)
    End Function

    ''' <summary>
    ''' 逐元素绝对值 (np.abs)
    ''' </summary>
    Public Function abs(t As Tensor) As Tensor
        Return Math.abs(t)
    End Function

    ''' <summary>
    ''' 逐元素幂运算 (np.power)
    ''' </summary>
    Public Function power(t As Tensor, exponent As Double) As Tensor
        Return Math.pow(t, exponent)
    End Function

    ''' <summary>
    ''' 逐元素裁剪 (np.clip)
    ''' </summary>
    Public Function clip(t As Tensor, minVal As Double, maxVal As Double) As Tensor
        Return Math.clip_by_value(t, minVal, maxVal)
    End Function

    ''' <summary>
    ''' 逐元素最大值 (np.maximum)
    ''' </summary>
    Public Function maximum(a As Tensor, b As Tensor) As Tensor
        Return Math.maximum(a, b)
    End Function

    ''' <summary>
    ''' 逐元素最小值 (np.minimum)
    ''' </summary>
    Public Function minimum(a As Tensor, b As Tensor) As Tensor
        Return Math.minimum(a, b)
    End Function

    ''' <summary>
    ''' 逐元素正弦 (np.sin)
    ''' </summary>
    Public Function sin(t As Tensor) As Tensor
        Return Math.sin(t)
    End Function

    ''' <summary>
    ''' 逐元素余弦 (np.cos)
    ''' </summary>
    Public Function cos(t As Tensor) As Tensor
        Return Math.cos(t)
    End Function

    ''' <summary>
    ''' 逐元素双曲正切 (np.tanh)
    ''' </summary>
    Public Function tanh(t As Tensor) As Tensor
        Return Math.tanh(t)
    End Function

#End Region

#Region "逻辑与条件"

    ''' <summary>
    ''' 条件选择 (np.where)
    ''' condition 中非零位置取 x 值，零位置取 y 值
    ''' </summary>
    Public Function [where](condition As Tensor, x As Tensor, y As Tensor) As Tensor
        If Not x.Shape.SequenceEqual(y.Shape) Then
            Throw New ArgumentException($"x 和 y 形状必须相同: [{String.Join(",", x.Shape)}] vs [{String.Join(",", y.Shape)}]")
        End If
        If Not condition.Shape.SequenceEqual(x.Shape) Then
            Throw New ArgumentException($"condition 和 x 形状必须相同: [{String.Join(",", condition.Shape)}] vs [{String.Join(",", x.Shape)}]")
        End If

        Dim result = New Tensor(x.Shape)
        Dim srcCond = condition.Data
        Dim srcX = x.Data
        Dim srcY = y.Data
        Dim dst = result.Data

        For i = 0 To dst.Length - 1
            dst(i) = If(srcCond(i) <> 0.0, srcX(i), srcY(i))
        Next

        Return result
    End Function

    ''' <summary>
    ''' 逐元素比较大于 (np.greater)
    ''' </summary>
    Public Function greater(a As Tensor, b As Tensor) As Tensor
        Return Math.greater(a, b)
    End Function

    ''' <summary>
    ''' 逐元素比较小于 (np.less)
    ''' </summary>
    Public Function less(a As Tensor, b As Tensor) As Tensor
        Return Math.less(a, b)
    End Function

    ''' <summary>
    ''' 逐元素比较相等 (np.equal)
    ''' </summary>
    Public Function equal(a As Tensor, b As Tensor) As Tensor
        Return Math.equal(a, b)
    End Function

#End Region

#Region "实用工具"

    ''' <summary>
    ''' 获取张量的形状 (np.shape)
    ''' </summary>
    Public Function shape(t As Tensor) As Integer()
        Return t.Shape
    End Function

    ''' <summary>
    ''' 获取张量的维度数 (np.ndim)
    ''' </summary>
    Public Function ndim(t As Tensor) As Integer
        Return t.Rank
    End Function

    ''' <summary>
    ''' 获取张量中元素的总数 (np.size)
    ''' </summary>
    Public Function size(t As Tensor) As Integer
        Return t.Length
    End Function

    ''' <summary>
    ''' 将张量转换为 Double 数组 (np.asarray)
    ''' </summary>
    Public Function as_double_array(t As Tensor) As Double()
        Return t.ToDoubleArray()
    End Function

#End Region

End Module

End Namespace
