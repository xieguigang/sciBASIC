#Region "Microsoft.VisualBasic::9dc9fd9d9cf5d36f692a3977c2a36949, Data_science\MachineLearning\DeepLearning\CNN\Layers\FullyConnectedLayer.vb"

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

    '   Total Lines: 145
    '    Code Lines: 99 (68.28%)
    ' Comment Lines: 11 (7.59%)
    '    - Xml Docs: 63.64%
    ' 
    '   Blank Lines: 35 (24.14%)
    '     File Size: 5.08 KB


    '     Class FullyConnectedLayer
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

Namespace CNN.layers

    ''' <summary>
    ''' Neurons in a fully connected layer have full connections to all
    ''' activations in the previous layer, as seen in regular Neural Networks.
    ''' Their activations can hence be computed with a matrix multiplication
    ''' followed by a bias offset.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class FullyConnectedLayer : Inherits DataLink
        Implements Layer

        Private l1_decay_mul As Double = 0.0
        Private l2_decay_mul As Double = 1.0

        Private ReadOnly BIAS_PREF As Single = 0.1F

        Private out_depth, out_sx, out_sy As Integer
        Private num_inputs As Integer
        Private filters As DataBlock()
        Private biases As DataBlock

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                For i As Integer = 0 To out_depth - 1
                    Yield New BackPropResult(filters(i).Weights, filters(i).Gradients, l1_decay_mul, l2_decay_mul)
                Next

                Yield New BackPropResult(biases.Weights, biases.Gradients, 0.0, 0.0)
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.FullyConnected
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition, num_neurons As Integer)
            out_depth = num_neurons

            ' computed
            num_inputs = def.outX * def.outY * def.depth
            out_sx = 1
            out_sy = 1

            ' initializations
            filters = New DataBlock(out_depth - 1) {}

            For i As Integer = 0 To out_depth - 1
                filters(i) = New DataBlock(1, 1, num_inputs)
            Next

            biases = New DataBlock(1, 1, out_depth, BIAS_PREF)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(1, 1, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            Call New ForwardTask(Me, lA, db).Run()

            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            Dim v As DataBlock = in_act.clearGradient()

            ' 第一阶段: 权重与偏置的梯度。
            ' 按输出下标 i 并行, 每个线程独占 filters(i) 与 biases 的第 i 个分量。
            Call New BackwardParamTask(Me, v).Run()

            ' 第二阶段: 输入数据的梯度。
            ' 原先的实现是在上面那个按 i 并行的同一个循环里对 v 的**全体**下标做
            ' v.addGradient(d, ...), 所有线程写入的区域完全重叠, 与卷积层一样存在
            ' 非原子读-改-写造成的丢失更新。这里改为按输入下标 d 并行,
            ' 每个线程先把自己那段元素沿输出维度的贡献累加完毕再写入一次。
            Call New BackwardInputTask(Me, v).Run()
        End Sub

        Private Class ForwardTask : Inherits VectorTask

            Dim layer As FullyConnectedLayer
            Dim lA As DataBlock
            Dim db As DataBlock

            Public Sub New(layer As FullyConnectedLayer, lA As DataBlock, db As DataBlock)
                MyBase.New(layer.out_depth)
                Me.lA = lA
                Me.db = db
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim Vw As Double() = db.Weights

                For i As Integer = start To ends
                    Dim a = 0.0
                    Dim wi = layer.filters(i).Weights

                    For d As Integer = 0 To layer.num_inputs - 1
                        a += Vw(d) * wi(d) ' for efficiency use Vols directly for now
                    Next

                    a += layer.biases.getWeight(i)
                    lA.setWeight(i, a)
                Next
            End Sub
        End Class

        ''' <summary>
        ''' 反向传播第一阶段: 计算权重与偏置的梯度。
        ''' </summary>
        ''' <remarks>
        ''' 按输出下标并行; 每个线程只写 <see cref="filters"/>(i) 以及 <see cref="biases"/>
        ''' 之中下标为 i 的那个元素, 不存在跨线程的写入冲突。
        ''' </remarks>
        Private Class BackwardParamTask : Inherits VectorTask

            Dim v As DataBlock
            Dim layer As FullyConnectedLayer

            Public Sub New(layer As FullyConnectedLayer, v As DataBlock)
                MyBase.New(layer.out_depth)
                Me.v = v
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                ' compute gradient wrt weights
                For i As Integer = start To ends
                    Dim tfi = layer.filters(i)
                    Dim chain_grad = layer.out_act.Gradients(i)

                    For d As Integer = 0 To layer.num_inputs - 1
                        Call tfi.addGradient(d, v.getWeight(d) * chain_grad) ' grad wrt params
                    Next

                    Call layer.biases.addGradient(i, chain_grad)
                Next
            End Sub
        End Class

        ''' <summary>
        ''' 反向传播第二阶段: 计算输入数据的梯度。
        ''' </summary>
        ''' <remarks>
        ''' 按输入下标并行, 每个线程只写自己那段输入梯度元素, 因此不存在数据竞争。
        ''' </remarks>
        Private Class BackwardInputTask : Inherits VectorTask

            Dim v As DataBlock
            Dim layer As FullyConnectedLayer

            Public Sub New(layer As FullyConnectedLayer, v As DataBlock)
                MyBase.New(layer.num_inputs)
                Me.v = v
                Me.layer = layer
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                Dim grads As Double() = layer.out_act.Gradients

                For d As Integer = start To ends
                    Dim a = 0.0

                    For i As Integer = 0 To layer.out_depth - 1
                        a += layer.filters(i).getWeight(d) * grads(i)
                    Next

                    Call v.addGradient(d, a) ' grad wrt input data
                Next
            End Sub
        End Class

        Public Overrides Function ToString() As String
            Return $"full_connected({out_depth})"
        End Function
    End Class

End Namespace
