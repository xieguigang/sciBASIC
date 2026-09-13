#Region "Microsoft.VisualBasic::8a4961ba3cd85a31d7f7560e7ed1ae02, Data_science\MachineLearning\DeepLearning\CNN\Layers\PoolingLayer.vb"

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

    '   Total Lines: 238
    '    Code Lines: 158 (66.39%)
    ' Comment Lines: 31 (13.03%)
    '    - Xml Docs: 29.03%
    ' 
    '   Blank Lines: 49 (20.59%)
    '     File Size: 8.67 KB


    '     Class PoolingLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward, initSwitchMaps
    '         Class SwitchMap
    ' 
    '             Constructor: (+2 Overloads) Sub New
    '             Function: ToString
    ' 
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

Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' This layer will reduce the dataset by creating a smaller zoomed out
    ''' version. In essence you take a cluster of pixels take the sum of them
    ''' and put the result in the reduced position of the new image.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class PoolingLayer : Inherits DataLink
        Implements Layer

        Private in_depth, in_sx, in_sy As Integer
        Private out_depth, out_sx, out_sy As Integer
        Private sx, sy, stride, padding As Integer

        ''' <summary>
        ''' 最近一次前向传播产出的 argMax 张量: 每个输出位置对应的**输入张量扁平下标**。
        ''' </summary>
        ''' <remarks>
        ''' 由后端 <c>MaxPool2D</c> 算子在同一次调用里与池化结果一并算出, 反向传播直接按它做
        ''' 散射累加。这取代了原先那套 ``Dictionary(Of UInteger, Dictionary(Of String, SwitchMap))``
        ''' (每个输出位置用 "ax,ay" 这样的字符串作为键) 的索引映射——字符串字典既无法映射到 GPU,
        ''' 也需要在每个线程上各持一份。
        ''' </remarks>
        <IgnoreDataMember>
        Dim argMaxIndex As Tensor

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Pool
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition, sx As Integer, stride As Integer, padding As Integer)
            Me.sx = sx
            Me.stride = stride

            in_depth = def.depth
            in_sx = def.outX
            in_sy = def.outY

            ' optional
            sy = Me.sx
            Me.padding = padding

            ' computed
            out_depth = in_depth
            out_sx = CInt(std.Floor((in_sx + Me.padding * 2 - Me.sx) / Me.stride + 1))
            out_sy = CInt(std.Floor((in_sy + Me.padding * 2 - sy) / Me.stride + 1))

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth

        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(out_sx, out_sy, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            ' 最大池化的前向由张量后端的 MaxPool2D 算子一次完成, 并且在同一次调用里产出 argMax
            ' (每个输出位置对应的输入张量扁平下标), 供反向传播做散射累加使用。
            '
            ' 后端张量的布局约定是 (N, H, W, C), 而 DataBlock 的 (SY, SX, Depth) 与它在内存里
            ' 是同一个顺序, 所以只需要一次零拷贝的形状重解释, 算完之后把结果整块拷回即可。
            Dim x4 As Tensor = db.Value4D
            Dim argMax As Tensor = Nothing
            Dim pooled = Tensor.computeKernel.MaxPool2D(x4, sx, stride, padding, argMax)

            Call lA.SetValues(pooled.Data)

            argMaxIndex = argMax
            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            ' pooling layers have no parameters, so simply compute gradient wrt data here
            Dim v As DataBlock = in_act.clearGradient()

            ' 反向严格按前向记录下来的 argMax 做散射累加: 每个输出位置唯一对应一个输入位置,
            ' 因此既不需要原子操作, 也不存在数据竞争
            Dim gradOut As Tensor = out_act.Grad4D
            Dim dx = Tensor.computeKernel.MaxPool2DBackward(gradOut, argMaxIndex, v.TensorShape4D)

            Call v.SetGradients(dx.Data)
        End Sub

        Public Overrides Function ToString() As String
            Return "pooling()"
        End Function
    End Class

End Namespace
