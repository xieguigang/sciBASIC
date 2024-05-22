#Region "Microsoft.VisualBasic::4781baf8d1c0e4246eb5cd33fca020fc, Data_science\MachineLearning\DeepLearning\CNN\layers\ConvolutionLayer.vb"

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
            Call New BackwardTask(Me, in_act.clearGradient()).Run()
        End Sub

        Private Class BackwardTask : Inherits VectorTask

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
                                            db.addGradient(ix1, f.getWeight(ix2) * chain_grad)
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

        Public Overrides Function ToString() As String
            Return "conv()"
        End Function
    End Class

End Namespace
