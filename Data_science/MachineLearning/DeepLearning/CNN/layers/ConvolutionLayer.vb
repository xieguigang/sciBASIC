#Region "Microsoft.VisualBasic::4781baf8d1c0e4246eb5cd33fca020fc, Data_science\MachineLearning\DeepLearning\CNN\Layers\ConvolutionLayer.vb"

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

    '   Total Lines: 217
    '    Code Lines: 156 (71.89%)
    ' Comment Lines: 20 (9.22%)
    '    - Xml Docs: 30.00%
    ' 
    '   Blank Lines: 41 (18.89%)
    '     File Size: 8.76 KB


    '     Class ConvolutionLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward
    '         Class ForwardTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    '         Class BackwardTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' This layer uses different filters to find attributes of the data that
    ''' affects the result. As an example there could be a filter to find
    ''' horizontal edges in an image.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class ConvolutionLayer : Inherits DataLink
        Implements Layer

        Friend l1_decay_mul As Double = 0.0
        Friend l2_decay_mul As Double = 1.0
        Friend BIAS_PREF As Single = 0.1F
        Friend out_depth, out_sx, out_sy As Integer
        Friend in_depth, in_sx, in_sy As Integer
        Friend sx, sy As Integer
        Friend stride, padding As Integer
        Friend filters As DataBlock()
        Friend biases As DataBlock

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                For i As Integer = 0 To out_depth - 1
                    Yield New BackPropResult(filters(i).Weights, filters(i).Gradients, l2_decay_mul, l1_decay_mul)
                Next

                Yield New BackPropResult(biases.Weights, biases.Gradients, 0.0, 0.0)
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Convolution
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition, sx As Integer, filters As Integer,
                       Optional stride As Integer = 1,
                       Optional padding As Integer = 0)

            ' required
            Me.out_depth = filters
            Me.sx = sx ' filter size. Should be odd if possible, it's cleaner.
            in_depth = def.depth
            in_sx = def.outX
            in_sy = def.outY

            ' optional
            Me.sy = Me.sx
            Me.stride = stride
            Me.padding = padding

            ' computed
            ' note we are doing floor, so if the strided convolution of the filter doesnt fit into the input
            ' volume exactly, the output volume will be trimmed and not contain the (incomplete) computed
            ' final application.
            out_sx = CInt(std.Floor((in_sx + Me.padding * 2 - Me.sx) / Me.stride + 1))
            out_sy = CInt(std.Floor((in_sy + Me.padding * 2 - sy) / Me.stride + 1))

            ' initializations
            Me.filters = New DataBlock(out_depth - 1) {}

            For i As Integer = 0 To out_depth - 1
                Me.filters(i) = New DataBlock(Me.sx, sy, in_depth)
            Next

            biases = New DataBlock(1, 1, out_depth, BIAS_PREF)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(out_sx, out_sy, out_depth, 0.0) With {.trace = Me.ToString}
            in_act = db
            out_act = lA
            Call New ForwardTask(Me, lA).Run()
            Return lA
        End Function

        Private Class ForwardTask : Inherits VectorTask

            Dim layer As ConvolutionLayer
            Dim lA As DataBlock

            Public Sub New(layer As ConvolutionLayer, lA As DataBlock)
                MyBase.New(layer.out_depth)
                Me.lA = lA
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim V_sx = layer.in_sx
                Dim V_sy = layer.in_sy
                Dim xy_stride = layer.stride
                Dim db = layer.in_act

                For d As Integer = start To ends
                    Dim f = layer.filters(d)
                    Dim y = -layer.padding
                    Dim ay = 0

                    While ay < layer.out_sy
                        Dim x = -layer.padding
                        Dim ax = 0

                        While ax < layer.out_sx

                            ' convolve centered at this particular location
                            Dim a = 0.0
                            For fy = 0 To f.SY - 1
                                Dim oy = y + fy ' coordinates in the original input array coordinates
                                For fx = 0 To f.SX - 1
                                    Dim ox = x + fx
                                    If oy >= 0 AndAlso oy < V_sy AndAlso ox >= 0 AndAlso ox < V_sx Then
                                        For fd = 0 To f.Depth - 1
                                            ' avoid function call overhead (x2) for efficiency, compromise modularity :(
                                            a += f.getWeight(fx, fy, fd) * db.getWeight(ox, oy, fd)
                                        Next
                                    End If
                                Next
                            Next
                            a += layer.biases.getWeight(d)
                            lA.setWeight(ax, ay, d, a)
                            x += xy_stride
                            ax += 1 ' xy_stride
                        End While

                        y += xy_stride
                        ay += 1 ' xy_stride
                    End While
                Next
            End Sub
        End Class

        Public Overridable Sub backward() Implements Layer.backward
            ' zero out gradient wrt bottom data, we're about to fill it
            Dim db As DataBlock = in_act.clearGradient()

            ' 第一阶段: 滤波核与偏置项的梯度。
            ' 按输出通道 d 并行, 每个线程只写 filters(d) 与 biases 的第 d 个分量, 写入区域互不相交。
            Call New BackwardFilterTask(Me, db).Run()

            ' 第二阶段: 输入数据的梯度。
            ' 原先的实现是在上面那个按 d 并行的同一个循环里做 db.addGradient(ix1, ...),
            ' 但 ix1 的取值集合在所有线程之间是完全重叠的(不同输出通道都落在同一批滤波窗口上),
            ' 于是 dw(ix) += val 这种非原子的读-改-写在多线程并发时会出现丢失更新,
            ' 导致训练结果随线程调度而漂移(实测同一个随机种子多次运行 loss 从 0.37 漂到 0.75)。
            ' 这里改为按**输入位置**并行: 每个线程独占自己那段输入位置的梯度元素,
            ' 先对全部贡献求和再一次性写入, 既消除了数据竞争又保留了并行。
            Call New BackwardInputTask(Me, db).Run()
        End Sub

        ''' <summary>
        ''' 反向传播第一阶段: 计算各个滤波核以及偏置项的梯度。
        ''' </summary>
        ''' <remarks>
        ''' 按输出通道并行; 每个线程独占 <see cref="filters"/>(d) 以及 <see cref="biases"/>
        ''' 之中下标为 d 的那个元素, 因此不存在跨线程的写入冲突。
        ''' </remarks>
        Private Class BackwardFilterTask : Inherits VectorTask

            Dim layer As ConvolutionLayer
            Dim db As DataBlock

            Public Sub New(layer As ConvolutionLayer, db As DataBlock)
                MyBase.New(layer.out_depth)
                Me.db = db
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim V_sx = db.SX
                Dim V_sy = db.SY
                Dim xy_stride = layer.stride

                For d As Integer = start To ends
                    Dim f = layer.filters(d)
                    Dim y = -layer.padding
                    Dim ay = 0

                    While ay < layer.out_sy
                        Dim x = -layer.padding
                        Dim ax = 0

                        While ax < layer.out_sx
                            ' convolve centered at this particular location
                            ' gradient from above, from chain rule
                            Dim chain_grad = layer.out_act.getGradient(ax, ay, d)

                            For fy As Integer = 0 To f.SY - 1
                                Dim oy As Integer = y + fy ' coordinates in the original input array coordinates

                                For fx As Integer = 0 To f.SX - 1
                                    Dim ox = x + fx

                                    If oy >= 0 AndAlso oy < V_sy AndAlso ox >= 0 AndAlso ox < V_sx Then
                                        For fd As Integer = 0 To f.Depth - 1
                                            ' avoid function call overhead (x2) for efficiency, compromise modularity :(
                                            Dim ix1 = (V_sx * oy + ox) * db.Depth + fd
                                            Dim ix2 = (f.SY * fy + fx) * f.Depth + fd

                                            f.addGradient(ix2, db.getWeight(ix1) * chain_grad)
                                        Next
                                    End If
                                Next
                            Next

                            layer.biases.addGradient(d, chain_grad)
                            x += xy_stride
                            ax += 1 ' xy_stride
                        End While

                        y += xy_stride
                        ay += 1 ' xy_stride
                    End While
                Next
            End Sub
        End Class

        ''' <summary>
        ''' 反向传播第二阶段: 计算输入数据的梯度。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 按输入平面上的空间位置(展平后的 ox,oy)并行。
        ''' </para>
        ''' <para>
        ''' 前向传播之中, 输出位置与输入位置之间的关系是
        ''' ``ox = ax * stride - padding + fx`` (``oy`` 同理), 反向传播需要的就是这个映射的逆:
        ''' 对每一个输入位置反解出所有会对它产生贡献的输出位置与滤波核偏移量,
        ''' 把全部贡献一次累加完毕之后只写一次结果。
        ''' </para>
        ''' <para>
        ''' 由于每个空间位置只属于一个线程, 因此每个梯度元素都只被写入一次, 不存在数据竞争。
        ''' </para>
        ''' </remarks>
        Private Class BackwardInputTask : Inherits VectorTask

            Dim layer As ConvolutionLayer
            Dim db As DataBlock

            Public Sub New(layer As ConvolutionLayer, db As DataBlock)
                ' 并行粒度 = 输入平面上所有空间位置的个数
                MyBase.New(db.SX * db.SY)
                Me.db = db
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim V_sx = db.SX
                Dim V_sy = db.SY
                Dim V_depth = db.Depth
                Dim xy_stride = layer.stride
                Dim padding = layer.padding
                Dim out_sx = layer.out_sx
                Dim out_sy = layer.out_sy

                ' 当前输入位置在各输入通道上的部分和; 在位置循环之内复用, 避免反复分配
                Dim acc As Double() = New Double(V_depth - 1) {}

                For p As Integer = start To ends
                    Dim oy As Integer = p \ V_sx
                    Dim ox As Integer = p Mod V_sx

                    Call Array.Clear(acc, 0, acc.Length)

                    For d As Integer = 0 To layer.out_depth - 1
                        Dim f = layer.filters(d)

                        For fy As Integer = 0 To f.SY - 1
                            Dim ay = InversePosition(oy + padding - fy, xy_stride, out_sy)

                            If ay >= 0 Then
                                For fx As Integer = 0 To f.SX - 1
                                    Dim ax = InversePosition(ox + padding - fx, xy_stride, out_sx)

                                    If ax >= 0 Then
                                        Dim chain_grad = layer.out_act.getGradient(ax, ay, d)

                                        For fd As Integer = 0 To V_depth - 1
                                            acc(fd) += f.getWeight(fx, fy, fd) * chain_grad
                                        Next
                                    End If
                                Next
                            End If
                        Next
                    Next

                    For fd As Integer = 0 To V_depth - 1
                        db.addGradient((V_sx * oy + ox) * V_depth + fd, acc(fd))
                    Next
                Next
            End Sub

            ''' <summary>
            ''' 由 ``坐标差 = 输出索引 * stride`` 反解出输出索引; 无法整除或者越界时返回 -1。
            ''' </summary>
            ''' <param name="delta">``输入坐标 + padding - 滤波核偏移``</param>
            ''' <param name="stride">卷积步长</param>
            ''' <param name="outSize">该维度上输出的大小</param>
            Private Shared Function InversePosition(delta As Integer, stride As Integer, outSize As Integer) As Integer
                If delta < 0 Then
                    Return -1
                End If

                ' stride = 1 是最常见的配置, 免去除法与取模的开销
                If stride = 1 Then
                    If delta >= outSize Then
                        Return -1
                    Else
                        Return delta
                    End If
                End If

                If delta Mod stride <> 0 Then
                    Return -1
                End If

                Dim index As Integer = delta \ stride

                If index >= outSize Then
                    Return -1
                Else
                    Return index
                End If
            End Function
        End Class

        Public Overrides Function ToString() As String
            Return "conv()"
        End Function
    End Class

End Namespace
