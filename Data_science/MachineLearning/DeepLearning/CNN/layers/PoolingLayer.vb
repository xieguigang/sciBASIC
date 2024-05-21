#Region "Microsoft.VisualBasic::b0939dbede742703c71be634a4c046f1, Data_science\MachineLearning\DeepLearning\CNN\Layers\PoolingLayer.vb"

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

    '   Total Lines: 219
    '    Code Lines: 145
    ' Comment Lines: 31
    '   Blank Lines: 43
    '     File Size: 8.18 KB


    '     Class PoolingLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward, initSwitchMaps
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
        ''' [ax,ay] map to [x,y]
        ''' </summary>
        <IgnoreDataMember>
        Dim switchMaps As New Dictionary(Of UInteger, Dictionary(Of String, Integer()))

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

            Call initSwitchMaps()
        End Sub

        Private Sub initSwitchMaps()
            ' store switches for x,y coordinates for where the max comes from, for each output neuron
            ' switchx = New Integer(out_sx * out_sy * out_depth - 1) {}
            ' switchy = New Integer(out_sx * out_sy * out_depth - 1) {}
            For d As Integer = 0 To out_depth - 1
                Call switchMaps.Add(d, New Dictionary(Of String, Integer()))
            Next
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim lA As New DataBlock(out_sx, out_sy, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db
            out_act = lA

            If Not training Then
                If switchMaps.IsNullOrEmpty Then
                    Call initSwitchMaps()
                End If
            End If

            ' clear all mapping
            ' a counter for switches
            For Each d As UInteger In switchMaps.Keys
                Call switchMaps(key:=d).Clear()
            Next

            Call New ForwardTask(Me, lA, db).Run()

            Return out_act
        End Function

        Private Class ForwardTask : Inherits VectorTask

            Dim layer As PoolingLayer
            Dim lA, db As DataBlock

            Public Sub New(layer As PoolingLayer, lA As DataBlock, db As DataBlock)
                MyBase.New(layer.out_depth)
                Me.db = db
                Me.lA = lA
                Me.layer = layer
                ' Me.sequenceMode = True
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                For d As Integer = start To ends
                    Dim x = -layer.padding
                    Dim ax = 0
                    Dim map As Dictionary(Of String, Integer()) = layer.switchMaps(key:=CUInt(d))

                    While ax < layer.out_sx
                        Dim y = -layer.padding
                        Dim ay = 0

                        While ay < layer.out_sy
                            ' convolve centered at this particular location
                            Dim a As Double = -99999 ' hopefully small enough ;\
                            Dim winx = -1
                            Dim winy = -1

                            For fx = 0 To layer.sx - 1
                                For fy = 0 To layer.sy - 1
                                    Dim oy = y + fy
                                    Dim ox = x + fx
                                    If oy >= 0 AndAlso oy < db.SY AndAlso ox >= 0 AndAlso ox < db.SX Then
                                        Dim v = db.getWeight(ox, oy, d)
                                        ' perform max pooling and store pointers to where
                                        ' the max came from. This will speed up backprop
                                        ' and can help make nice visualizations in future
                                        If v > a Then
                                            a = v
                                            winx = ox
                                            winy = oy
                                        End If
                                    End If
                                Next
                            Next

                            map.Add(ax & "," & ay, {winx, winy})

                            'switchx(n) = winx
                            'switchy(n) = winy
                            'n += 1
                            lA.setWeight(ax, ay, d, a)
                            y += layer.stride
                            ay += 1
                        End While

                        x += layer.stride
                        ax += 1
                    End While
                Next
            End Sub
        End Class

        Public Overridable Sub backward() Implements Layer.backward
            ' pooling layers have no parameters, so simply compute
            ' gradient wrt data here
            ' zero out gradient wrt data
            Call New BackwardTask(Me, v:=in_act.clearGradient()).Run()
        End Sub

        Private Class BackwardTask : Inherits VectorTask

            Dim layer As PoolingLayer
            Dim v As DataBlock

            Public Sub New(layer As PoolingLayer, v As DataBlock)
                MyBase.New(layer.out_depth)
                Me.v = v
                Me.layer = layer
                ' Me.sequenceMode = True
            End Sub

            Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
                For d As Integer = start To ends
                    Dim x = -layer.padding
                    Dim ax = 0
                    Dim map = layer.switchMaps(key:=CUInt(d))

                    While ax < layer.out_sx
                        Dim y = -layer.padding
                        Dim ay = 0

                        While ay < layer.out_sy
                            Dim chain_grad = layer.out_act.getGradient(ax, ay, d)
                            Dim key As String = ax & "," & ay

                            If map.ContainsKey(key) Then
                                Dim switch As Integer() = map(key)

                                ' V.addGradient(switchx(n), switchy(n), d, chain_grad)
                                v.addGradient(switch(0), switch(1), d, chain_grad)
                                y += layer.stride
                            End If

                            ay += 1
                        End While

                        x += layer.stride
                        ax += 1
                    End While
                Next
            End Sub
        End Class

        Public Overrides Function ToString() As String
            Return "pooling()"
        End Function
    End Class

End Namespace
