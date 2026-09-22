#Region "Microsoft.VisualBasic::3c5404700da9db1bd8155c335dd2f854, Data_science\MachineLearning\DeepLearning\Transformer\TensorOps.vb"

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

    '   Total Lines: 778
    '    Code Lines: 494 (63.50%)
    ' Comment Lines: 112 (14.40%)
    '    - Xml Docs: 84.82%
    ' 
    '   Blank Lines: 172 (22.11%)
    '     File Size: 30.27 KB


    '     Module TensorOps
    ' 
    '         Properties: Seed
    ' 
    '         Function: AddNormBackward, AddNormForward, ArgMaxLastDim, BatchedMatMul, CloneTensor
    '                   ConcatLastDim, DropoutMask, DropoutMaskBackward, ElementwiseMultiply, FlattenLastTwo
    '                   Heaviside, HeNormalInit, Scale, SoftmaxBackward, SoftmaxLastDim
    '                   SplitLastDim, TransposeLastTwo, UnflattenLastTwo, VecAdd, VecAddBackward
    '                   ZerosLike
    ' 
    '         Sub: Accumulate, BatchedMatMulBackward, MaskUpperTriangular, ScaleInPlace, ZeroInPlace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' TensorOps —— Transformer 迁移辅助算子层
'
' 旧的 Transformer 实现依赖 AutomaticDifferentiation 命名空间下的 AD 张量，
' 那个张量在最后一维 / 最后两维上提供了大量「N 维语义」的算子，并且自带反向传播。
' 迁移到 TensorFlow\Tensor.vb（纯 Double 数值张量，无自动微分）之后，需要：
'
'   1. 补齐新 Tensor 缺失的 N 维算子（批量矩阵乘、最后一维 concat、上三角掩码等）；
'   2. 为每一个算子显式写出反向传播公式（手写 backprop）。
'
' 这些算子都是 Transformer 专属语义，因此放在 DeepLearning 工程内部而不是去污染
' TensorFlow 通用张量库。所有梯度写回均为原地 `+=` 累加，避免重复分配。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

Namespace Transformer

    ''' <summary>
    ''' Transformer 迁移辅助算子集合：补齐 <see cref="Tensor"/> 相对旧 AD 张量缺失的
    ''' N 维算子，并为每个算子提供显式反向传播实现。
    ''' </summary>
    Public Module TensorOps

        ''' <summary>AddNorm（残差 + LayerNorm）使用的数值稳定项，与旧实现保持一致。</summary>
        Public Const AddNormEps As Double = 0.001

        ''' <summary>参数初始化使用的随机数发生器（全局共享，模型构建阶段单线程使用）。</summary>
        Private _random As Random = New Random()

        Private _seed As Integer? = Nothing

        ''' <summary>
        ''' 参数初始化的随机种子。设定后所有 <see cref="HeNormalInit"/> 都从同一个
        ''' 确定性随机序列取值，从而让整次训练可复现。
        ''' </summary>
        Public Property Seed As Integer?
            Get
                Return _seed
            End Get
            Set(value As Integer?)
                _seed = value
                _random = If(value.HasValue, New Random(value.Value), New Random())
            End Set
        End Property

#Region "通用工具"

        ''' <summary>创建与 <paramref name="tensor"/> 同形状的全零张量。</summary>
        Public Function ZerosLike(tensor As Tensor) As Tensor
            Return New Tensor(tensor.Shape)
        End Function

        ''' <summary>
        ''' 深拷贝张量。用于「同一个梯度同时属于两个分支」的场景
        ''' （例如 AddNorm 反向对 A 与 B 给出同一份 dx，必须各自持有独立副本）。
        ''' </summary>
        Public Function CloneTensor(tensor As Tensor) As Tensor
            Return CType(tensor.Clone(), Tensor)
        End Function

        ''' <summary>把张量的全部元素置零（原地）。</summary>
        Public Sub ZeroInPlace(tensor As Tensor)
            Array.Clear(tensor.Data, 0, tensor.Length)
            Call tensor.MarkHostModified()
        End Sub

        ''' <summary>原地累加：<c>target += grad</c>（要求形状一致）。</summary>
        Public Sub Accumulate(target As Tensor, grad As Tensor)
            If grad Is Nothing Then Return

            Dim dst = target.Data
            Dim src = grad.Data

            For idx As Integer = 0 To dst.Length - 1
                dst(idx) += src(idx)
            Next

            Call target.MarkHostModified()
        End Sub

        ''' <summary>原地缩放：<c>tensor *= scalar</c>。</summary>
        Public Sub ScaleInPlace(tensor As Tensor, scalar As Double)
            Dim data = tensor.Data

            For idx As Integer = 0 To data.Length - 1
                data(idx) *= scalar
            Next

            Call tensor.MarkHostModified()
        End Sub

        ''' <summary>按标量缩放（返回新张量）。</summary>
        Public Function Scale(tensor As Tensor, scalar As Double) As Tensor
            Return Tensor.computeKernel.MultiplyScalar(tensor, scalar)
        End Function

        ''' <summary>逐元素乘法（Hadamard 积）。</summary>
        Public Function ElementwiseMultiply(a As Tensor, b As Tensor) As Tensor
            Return Tensor.computeKernel.Multiply(a, b)
        End Function

        ''' <summary>ReLU 反向所需的阶跃掩码：元素 &gt; 0 取 1，否则取 0。</summary>
        Public Function Heaviside(tensor As Tensor) As Tensor
            Return Tensor.computeKernel.Heaviside(tensor)
        End Function

        ''' <summary>
        ''' 旧 AD 张量 <c>GenerateNormalRandomValues()</c> 的等价实现：
        ''' 以倒数第二维作为 fan-in 做 He 缩放的正态初始化。
        ''' </summary>
        ''' <remarks>
        ''' 初始化使用本模块内部的随机数发生器；通过 <see cref="Seed"/> 设定种子即可让
        ''' 整个模型的初始化（进而整次训练）完全可复现。
        ''' </remarks>
        Public Function HeNormalInit(shape As Integer()) As Tensor
            Dim fanIn As Integer = shape(shape.Length - 2)
            Dim stdDev = std.Sqrt(2.0 / fanIn)
            Dim result = New Tensor(shape)
            Dim data = result.Data

            For idx As Integer = 0 To data.Length - 1
                ' Box-Muller 变换，与 Tensor.RandomNormal 保持一致的分布
                Dim u1 As Double = 1.0 - _random.NextDouble()
                Dim u2 As Double = 1.0 - _random.NextDouble()
                Dim randStdNormal = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
                data(idx) = stdDev * randStdNormal
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' 对最后一维做 argmax，返回每个 slice 的最大值位置
        ''' （等价于旧 AD 张量的 <c>GetMaxIndex()</c>，用于 <c>[batch, 1, dict]</c>）。
        ''' </summary>
        Public Function ArgMaxLastDim(tensor As Tensor) As Integer()
            Dim n As Integer = tensor.Shape(tensor.Rank - 1)
            Dim blocks As Integer = tensor.Length \ n
            Dim data = tensor.Data
            Dim result(blocks - 1) As Integer

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n
                Dim maxVal As Double = Double.NegativeInfinity
                Dim maxIdx As Integer = 0

                For bj As Integer = 0 To n - 1
                    If data(offset + bj) > maxVal Then
                        maxVal = data(offset + bj)
                        maxIdx = bj
                    End If
                Next

                result(blk) = maxIdx
            Next

            Return result
        End Function

#End Region

#Region "批量矩阵乘"

        ''' <summary>
        ''' 对最后两维做矩阵乘：<c>C[..., i, j] = Σ_k A[..., i, k] * B[..., k, j]</c>。
        ''' </summary>
        ''' <remarks>
        ''' 当 <paramref name="b"/> 为二维张量时，它在所有 block 上广播（共享一份权重），
        ''' 与旧 <c>SIMDMatMul.Solve</c> 语义一致。B 为二维时走「reshape + 2D SIMD MatMul」
        ''' 的快路径，其它情况按 block 逐块计算。
        ''' </remarks>
        Public Function BatchedMatMul(a As Tensor, b As Tensor) As Tensor
            Dim aShape = a.Shape
            Dim bShape = b.Shape

            If aShape.Length < 2 OrElse bShape.Length < 2 Then
                Throw New ArgumentException("张量必须具有 >= 2 个维度")
            End If

            Dim rank As Integer = aShape.Length
            Dim imax As Integer = aShape(rank - 2)
            Dim kmax As Integer = aShape(rank - 1)
            Dim bKmax As Integer = bShape(bShape.Length - 2)
            Dim jmax As Integer = bShape(bShape.Length - 1)

            If kmax <> bKmax Then
                Throw New ArgumentException($"矩阵维度不匹配: {kmax} != {bKmax}")
            End If

            Dim outShape = CType(aShape.Clone(), Integer())
            outShape(rank - 2) = imax
            outShape(rank - 1) = jmax

            Dim nrBlocks As Integer = 1

            For ld As Integer = 0 To rank - 3
                nrBlocks *= aShape(ld)
            Next

            If bShape.Length = 2 Then
                ' 快路径：[rows, K] × [K, J]，直接复用 2D SIMD 后端
                Dim rows As Integer = a.Length \ kmax
                Dim a2 = Tensor.Wrap(a.Data, rows, kmax)
                Dim c2 = a2.MatMul(b)

                Return Tensor.Wrap(c2.Data, outShape)
            End If

            If b.Length \ (kmax * jmax) <> nrBlocks Then
                Throw New ArgumentException("批量矩阵乘的 block 数量不一致")
            End If

            Dim result = New Tensor(outShape)
            Dim aData = a.Data, bData = b.Data, cData = result.Data
            Dim bsA As Integer = imax * kmax
            Dim bsB As Integer = kmax * jmax
            Dim bsC As Integer = imax * jmax

            For blk As Integer = 0 To nrBlocks - 1
                Dim offsetA = blk * bsA, offsetB = blk * bsB, offsetC = blk * bsC

                For bi As Integer = 0 To imax - 1
                    Dim rowA = offsetA + bi * kmax
                    Dim rowC = offsetC + bi * jmax

                    For bk As Integer = 0 To kmax - 1
                        Dim av = aData(rowA + bk)

                        If av <> 0.0 Then
                            Dim rowB = offsetB + bk * jmax

                            For bj As Integer = 0 To jmax - 1
                                cData(rowC + bj) += av * bData(rowB + bj)
                            Next
                        End If
                    Next
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' <see cref="BatchedMatMul"/> 的反向传播。
        ''' </summary>
        ''' <remarks>
        ''' <c>dA = dC · Bᵀ</c>；<c>dB = Σ_block Aᵀ · dC</c>（B 为二维时对全部 block 求和，
        ''' 因为前向阶段它是被共享使用的）。
        ''' </remarks>
        Public Sub BatchedMatMulBackward(dC As Tensor, a As Tensor, b As Tensor,
                                         ByRef dA As Tensor, ByRef dB As Tensor)
            Dim aShape = a.Shape
            Dim bShape = b.Shape
            Dim rank As Integer = aShape.Length
            Dim imax As Integer = aShape(rank - 2)
            Dim kmax As Integer = aShape(rank - 1)
            Dim jmax As Integer = bShape(bShape.Length - 1)

            Dim nrBlocks As Integer = 1

            For ld As Integer = 0 To rank - 3
                nrBlocks *= aShape(ld)
            Next

            dA = New Tensor(aShape)

            If bShape.Length = 2 Then
                Dim rows As Integer = a.Length \ kmax
                Dim a2 = Tensor.Wrap(a.Data, rows, kmax)
                Dim dc2 = Tensor.Wrap(dC.Data, rows, jmax)

                ' dB = Aᵀ · dC（对全部 block 求和）
                dB = a2.Transpose().MatMul(dc2)

                ' dA = dC · Bᵀ
                Dim da2 = dc2.MatMul(b.Transpose())
                Array.Copy(da2.Data, dA.Data, dA.Length)
                Call dA.MarkHostModified()

                Return
            End If

            dB = New Tensor(bShape)

            Dim aData = a.Data, bData = b.Data, cData = dC.Data
            Dim daData = dA.Data, dbData = dB.Data
            Dim bsA As Integer = imax * kmax
            Dim bsB As Integer = kmax * jmax
            Dim bsC As Integer = imax * jmax

            For blk As Integer = 0 To nrBlocks - 1
                Dim offsetA = blk * bsA, offsetB = blk * bsB, offsetC = blk * bsC

                For bi As Integer = 0 To imax - 1
                    For bj As Integer = 0 To jmax - 1
                        Dim gval = cData(offsetC + bi * jmax + bj)

                        If gval <> 0.0 Then
                            For bk As Integer = 0 To kmax - 1
                                daData(offsetA + bi * kmax + bk) += gval * bData(offsetB + bk * jmax + bj)
                                dbData(offsetB + bk * jmax + bj) += aData(offsetA + bi * kmax + bk) * gval
                            Next
                        End If
                    Next
                Next
            Next

            Call dA.MarkHostModified()
            Call dB.MarkHostModified()
        End Sub

#End Region

#Region "转置 / 拼接 / 掩码"

        ''' <summary>交换最后两维（任意 rank >= 2）。</summary>
        Public Function TransposeLastTwo(tensor As Tensor) As Tensor
            Dim shape = tensor.Shape
            Dim rank As Integer = shape.Length

            If rank < 2 Then
                Throw New ArgumentException("张量必须具有 >= 2 个维度")
            End If

            Dim imax As Integer = shape(rank - 2)
            Dim jmax As Integer = shape(rank - 1)
            Dim outShape = CType(shape.Clone(), Integer())
            outShape(rank - 2) = jmax
            outShape(rank - 1) = imax

            Dim result = New Tensor(outShape)
            Dim src = tensor.Data, dst = result.Data
            Dim blocks As Integer = tensor.Length \ (imax * jmax)
            Dim blockSize As Integer = imax * jmax

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * blockSize

                For bi As Integer = 0 To imax - 1
                    For bj As Integer = 0 To jmax - 1
                        dst(offset + bj * imax + bi) = src(offset + bi * jmax + bj)
                    Next
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' 沿最后一维拼接若干张量（其余维度必须一致），等价于旧 AD 张量的 <c>Concat</c>。
        ''' </summary>
        Public Function ConcatLastDim(parts As Tensor()) As Tensor
            If parts Is Nothing OrElse parts.Length = 0 Then
                Throw New ArgumentException("至少需要一个待拼接的张量")
            End If

            Dim rank As Integer = parts(0).Rank
            Dim outShape = CType(parts(0).Shape.Clone(), Integer())
            Dim totalLast As Integer = 0

            For pt As Integer = 0 To parts.Length - 1
                If parts(pt).Rank <> rank Then
                    Throw New ArgumentException("所有张量的维度数必须相同")
                End If

                For ld As Integer = 0 To rank - 2
                    If parts(pt).Shape(ld) <> parts(0).Shape(ld) Then
                        Throw New ArgumentException("除最后一维外，所有张量的形状必须一致")
                    End If
                Next

                totalLast += parts(pt).Shape(rank - 1)
            Next

            outShape(rank - 1) = totalLast

            Dim result = New Tensor(outShape)
            Dim dst = result.Data
            Dim blocks As Integer = parts(0).Length \ parts(0).Shape(rank - 1)

            For blk As Integer = 0 To blocks - 1
                Dim outBase = blk * totalLast
                Dim pos As Integer = 0

                For pt As Integer = 0 To parts.Length - 1
                    Dim srcLast As Integer = parts(pt).Shape(rank - 1)
                    Array.Copy(parts(pt).Data, blk * srcLast, dst, outBase + pos, srcLast)
                    pos += srcLast
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary><see cref="ConcatLastDim"/> 的反向：沿最后一维等分切回各分量。</summary>
        Public Function SplitLastDim(tensor As Tensor, nrParts As Integer) As Tensor()
            Dim rank As Integer = tensor.Rank
            Dim totalLast As Integer = tensor.Shape(rank - 1)

            If totalLast Mod nrParts <> 0 Then
                Throw New ArgumentException("最后一维无法被等分")
            End If

            Dim partLast As Integer = totalLast \ nrParts
            Dim blocks As Integer = tensor.Length \ totalLast
            Dim src = tensor.Data
            Dim result(nrParts - 1) As Tensor
            Dim partShape = CType(tensor.Shape.Clone(), Integer())
            partShape(rank - 1) = partLast

            For pt As Integer = 0 To nrParts - 1
                Dim part = New Tensor(partShape)
                Dim dst = part.Data

                For blk As Integer = 0 To blocks - 1
                    Array.Copy(src, blk * totalLast + pt * partLast, dst, blk * partLast, partLast)
                Next

                Call part.MarkHostModified()
                result(pt) = part
            Next

            Return result
        End Function

        ''' <summary>
        ''' 把最后两维的上三角（<c>j &gt; i</c>）原地置为负无穷，用于因果掩码。
        ''' </summary>
        ''' <remarks>
        ''' 掩码后的位置经过 softmax 后输出恒为 0，其上游梯度也自然为 0
        ''' （<c>dx_i = y_i·(dy_i − Σ dy·y)</c>，而 <c>y_i = 0</c>），因此本算子不需要单独的反向实现。
        ''' </remarks>
        Public Sub MaskUpperTriangular(tensor As Tensor)
            Dim shape = tensor.Shape
            Dim rank As Integer = shape.Length

            If rank < 2 Then
                Throw New ArgumentException("张量必须具有 >= 2 个维度")
            End If

            Dim imax As Integer = shape(rank - 2)
            Dim jmax As Integer = shape(rank - 1)
            Dim data = tensor.Data
            Dim blocks As Integer = tensor.Length \ (imax * jmax)
            Dim blockSize As Integer = imax * jmax

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * blockSize

                For bi As Integer = 0 To imax - 1
                    For bj As Integer = bi + 1 To jmax - 1
                        data(offset + bi * jmax + bj) = Double.NegativeInfinity
                    Next
                Next
            Next

            Call tensor.MarkHostModified()
        End Sub

#End Region

#Region "Softmax"

        ''' <summary>沿最后一维做数值稳定的 softmax。</summary>
        Public Function SoftmaxLastDim(tensor As Tensor) As Tensor
            Dim n As Integer = tensor.Shape(tensor.Rank - 1)
            Dim blocks As Integer = tensor.Length \ n
            Dim result = New Tensor(tensor.Shape)
            Dim src = tensor.Data, dst = result.Data

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n
                Dim maxVal As Double = Double.NegativeInfinity

                For bi As Integer = 0 To n - 1
                    If src(offset + bi) > maxVal Then maxVal = src(offset + bi)
                Next

                Dim sumExp As Double = 0.0

                For bi As Integer = 0 To n - 1
                    Dim e = std.Exp(src(offset + bi) - maxVal)
                    dst(offset + bi) = e
                    sumExp += e
                Next

                If sumExp > 0 Then
                    For bi As Integer = 0 To n - 1
                        dst(offset + bi) /= sumExp
                    Next
                End If
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' softmax 的反向传播：<c>dx_i = y_i · (dy_i − Σ_j dy_j·y_j)</c>。
        ''' </summary>
        ''' <param name="y">前向阶段 softmax 的输出</param>
        ''' <param name="dy">上游梯度</param>
        Public Function SoftmaxBackward(y As Tensor, dy As Tensor) As Tensor
            Dim n As Integer = y.Shape(y.Rank - 1)
            Dim blocks As Integer = y.Length \ n
            Dim dx = New Tensor(y.Shape)
            Dim yData = y.Data, dyData = dy.Data, dxData = dx.Data

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n
                Dim dot As Double = 0.0

                For bi As Integer = 0 To n - 1
                    dot += dyData(offset + bi) * yData(offset + bi)
                Next

                For bi As Integer = 0 To n - 1
                    dxData(offset + bi) = yData(offset + bi) * (dyData(offset + bi) - dot)
                Next
            Next

            Call dx.MarkHostModified()

            Return dx
        End Function

#End Region

#Region "AddNorm（残差 + LayerNorm）"

        ''' <summary>
        ''' 残差相加后沿最后一维做 LayerNorm：<c>C = (A + B − mean) / sqrt(var + eps)</c>。
        ''' </summary>
        ''' <remarks>
        ''' 与旧 AD 张量的 <c>AddNorm</c> 完全一致：<c>eps = 0.001</c>，且没有可学习的 γ / β。
        ''' 均值与逆标准差按最后一个维度逐 slice 缓存，供反向传播使用。
        ''' </remarks>
        Public Function AddNormForward(a As Tensor, b As Tensor,
                                       ByRef mean As Double(), ByRef invStd As Double()) As Tensor
            If Not a.Shape.SequenceEqual(b.Shape) Then
                Throw New ArgumentException("AddNorm 要求两个张量形状一致")
            End If

            Dim n As Integer = a.Shape(a.Rank - 1)
            Dim blocks As Integer = a.Length \ n
            Dim result = New Tensor(a.Shape)
            Dim aData = a.Data, bData = b.Data, cData = result.Data

            ReDim mean(blocks - 1)
            ReDim invStd(blocks - 1)

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n
                Dim mu As Double = 0.0

                For bi As Integer = 0 To n - 1
                    mu += aData(offset + bi) + bData(offset + bi)
                Next

                mu /= n

                Dim vari As Double = 0.0

                For bi As Integer = 0 To n - 1
                    Dim diff = aData(offset + bi) + bData(offset + bi) - mu
                    vari += diff * diff
                Next

                vari /= n

                Dim isd = 1.0 / std.Sqrt(vari + AddNormEps)
                mean(blk) = mu
                invStd(blk) = isd

                For bi As Integer = 0 To n - 1
                    cData(offset + bi) = (aData(offset + bi) + bData(offset + bi) - mu) * isd
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary>
        ''' <see cref="AddNormForward"/> 的反向传播。
        ''' </summary>
        ''' <returns>
        ''' 对 <c>x = A + B</c> 的梯度；由于 A 与 B 在加法处是对称的，
        ''' <c>dA = dB = 返回值</c>。
        ''' </returns>
        ''' <remarks>
        ''' <c>x̂ = (x − μ)·invStd</c>，精确反向公式为：
        ''' <c>dx = invStd · (dy − mean(dy) − x̂ · mean(dy ⊙ x̂))</c>。
        ''' </remarks>
        Public Function AddNormBackward(dOut As Tensor, a As Tensor, b As Tensor,
                                        mean As Double(), invStd As Double()) As Tensor
            Dim n As Integer = a.Shape(a.Rank - 1)
            Dim blocks As Integer = a.Length \ n
            Dim dx = New Tensor(a.Shape)
            Dim aData = a.Data, bData = b.Data, dOutData = dOut.Data, dxData = dx.Data

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n
                Dim mu = mean(blk)
                Dim isd = invStd(blk)
                Dim sumDy As Double = 0.0
                Dim sumDyX As Double = 0.0

                For bi As Integer = 0 To n - 1
                    Dim xh = (aData(offset + bi) + bData(offset + bi) - mu) * isd
                    Dim g = dOutData(offset + bi)
                    sumDy += g
                    sumDyX += g * xh
                Next

                Dim meanDy = sumDy / n
                Dim meanDyX = sumDyX / n

                For bi As Integer = 0 To n - 1
                    Dim xh = (aData(offset + bi) + bData(offset + bi) - mu) * isd
                    dxData(offset + bi) = isd * (dOutData(offset + bi) - meanDy - xh * meanDyX)
                Next
            Next

            Call dx.MarkHostModified()

            Return dx
        End Function

#End Region

#Region "向量广播加 / Flatten / Dropout"

        ''' <summary>把一维向量加到张量最后一维：<c>C[..., j] = T[..., j] + v[j]</c>。</summary>
        Public Function VecAdd(tensor As Tensor, v As Tensor) As Tensor
            If v.Rank <> 1 Then
                Throw New ArgumentException("向量必须是一维张量")
            End If

            Dim n As Integer = tensor.Shape(tensor.Rank - 1)

            If v.Shape(0) <> n Then
                Throw New ArgumentException("向量长度与最后一维不一致")
            End If

            Dim result = New Tensor(tensor.Shape)
            Dim tData = tensor.Data, vData = v.Data, cData = result.Data
            Dim blocks As Integer = tensor.Length \ n

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n

                For bi As Integer = 0 To n - 1
                    cData(offset + bi) = tData(offset + bi) + vData(bi)
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary><see cref="VecAdd"/> 对偏单向量的反向：沿其余维度求和。</summary>
        Public Function VecAddBackward(dOut As Tensor) As Tensor
            Dim n As Integer = dOut.Shape(dOut.Rank - 1)
            Dim blocks As Integer = dOut.Length \ n
            Dim dv = New Tensor(New Integer() {n})
            Dim dOutData = dOut.Data, dvData = dv.Data

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n

                For bi As Integer = 0 To n - 1
                    dvData(bi) += dOutData(offset + bi)
                Next
            Next

            Call dv.MarkHostModified()

            Return dv
        End Function

        ''' <summary>把最后两维压平为 <c>[..., 1, I*J]</c>（行优先顺序不变）。</summary>
        Public Function FlattenLastTwo(tensor As Tensor) As Tensor
            Dim rank As Integer = tensor.Rank

            If rank < 2 Then
                Throw New ArgumentException("张量必须具有 >= 2 个维度")
            End If

            Dim shape = tensor.Shape
            Dim imax As Integer = shape(rank - 2)
            Dim jmax As Integer = shape(rank - 1)
            Dim outShape = CType(shape.Clone(), Integer())
            outShape(rank - 2) = 1
            outShape(rank - 1) = imax * jmax

            Return Tensor.Wrap(CType(tensor.Data.Clone(), Double()), outShape)
        End Function

        ''' <summary><see cref="FlattenLastTwo"/> 的反向：还原为原始形状。</summary>
        Public Function UnflattenLastTwo(dOut As Tensor, originalShape As Integer()) As Tensor
            Return Tensor.Wrap(CType(dOut.Data.Clone(), Double()), originalShape)
        End Function

        ''' <summary>
        ''' 按固定掩码做 dropout：先整体放大 <c>1/(1-rate)</c>，再把掩码位置清零。
        ''' </summary>
        ''' <param name="tensor">输入张量</param>
        ''' <param name="mask">长度为最后一维的布尔掩码，True 表示丢弃</param>
        ''' <param name="rate">丢弃概率</param>
        Public Function DropoutMask(tensor As Tensor, mask As Boolean(), rate As Double) As Tensor
            Dim n As Integer = tensor.Shape(tensor.Rank - 1)

            If mask Is Nothing OrElse mask.Length <> n Then
                Throw New ArgumentException("dropout 掩码长度与最后一维不一致")
            End If

            Dim scaleFactor As Double = 1.0 / (1.0 - rate)
            Dim result = New Tensor(tensor.Shape)
            Dim src = tensor.Data, dst = result.Data
            Dim blocks As Integer = tensor.Length \ n

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n

                For bi As Integer = 0 To n - 1
                    dst(offset + bi) = If(mask(bi), 0.0, src(offset + bi) * scaleFactor)
                Next
            Next

            Call result.MarkHostModified()

            Return result
        End Function

        ''' <summary><see cref="DropoutMask"/> 的反向：同一掩码位置梯度置零并做同样的缩放。</summary>
        Public Function DropoutMaskBackward(dOut As Tensor, mask As Boolean(), rate As Double) As Tensor
            Dim n As Integer = dOut.Shape(dOut.Rank - 1)

            If mask Is Nothing OrElse mask.Length <> n Then
                Throw New ArgumentException("dropout 掩码长度与最后一维不一致")
            End If

            Dim scaleFactor As Double = 1.0 / (1.0 - rate)
            Dim dx = New Tensor(dOut.Shape)
            Dim src = dOut.Data, dst = dx.Data
            Dim blocks As Integer = dOut.Length \ n

            For blk As Integer = 0 To blocks - 1
                Dim offset = blk * n

                For bi As Integer = 0 To n - 1
                    dst(offset + bi) = If(mask(bi), 0.0, src(offset + bi) * scaleFactor)
                Next
            Next

            Call dx.MarkHostModified()

            Return dx
        End Function

#End Region

    End Module
End Namespace
