#Region "Microsoft.VisualBasic::0576a074502f3b252f65cdf443189274, Data_science\MachineLearning\DeepLearning\CNN\Layers\MaxoutLayer.vb"

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

    '   Total Lines: 147
    '    Code Lines: 100 (68.03%)
    ' Comment Lines: 32 (21.77%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 15 (10.20%)
    '     File Size: 6.03 KB


    '     Class MaxoutLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' Implements Maxout nonlinearity that computes x to max(x)
    ''' where x is a vector of size group_size. Ideally of course,
    ''' the input size should be exactly divisible by group_size
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    <Serializable>
    Public Class MaxoutLayer : Inherits DataLink
        Implements Layer

        Private out_depth, out_sx, out_sy As Integer
        Private ReadOnly group_size As Integer = 2
        Private switches As Integer()

        ''' <summary>Gets the parameter and gradient blocks of this layer; the maxout layer has none.</summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Maxout"/>.</summary>
        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Maxout
            End Get
        End Property

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates a maxout layer; the input depth is divided by the group size.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the input shape.</param>
        Public Sub New(def As OutputDefinition)
            ' computed
            out_sx = def.outX
            out_sy = def.outY
            out_depth = CInt(std.Floor(def.depth / group_size))

            switches = New Integer(out_sx * out_sy * out_depth - 1) {} ' useful for backprop
            switches.fill(0)
        End Sub

        ''' <summary>
        ''' Computes the maximum inside every group and records which element won, so the gradient can be routed back.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">Ignored; the activation behaves the same in both modes.</param>
        ''' <returns>The grouped maxima.</returns>
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db
            Dim lN = out_depth
            Dim V2 As DataBlock = New DataBlock(out_sx, out_sy, out_depth, 0.0) With {.trace = Me.ToString}

            ' optimization branch. If we're operating on 1D arrays we dont have
            ' to worry about keeping track of x,y,d coordinates inside
            ' input volumes. In convnets we do :(
            If out_sx = 1 AndAlso out_sy = 1 Then
                For i = 0 To lN - 1
                    Dim ix = i * group_size ' base index offset
                    Dim a = db.getWeight(ix)
                    Dim ai = 0
                    For j = 1 To group_size - 1
                        Dim a2 = db.getWeight(ix + j)
                        If a2 > a Then
                            a = a2
                            ai = j
                        End If
                    Next
                    V2.setWeight(i, a)
                    switches(i) = ix + ai
                Next
            Else
                Dim n = 0 ' counter for switches
                For x = 0 To db.SX - 1
                    For y = 0 To db.SY - 1
                        For i = 0 To lN - 1
                            Dim ix = i * group_size
                            Dim a = db.getWeight(x, y, ix)
                            Dim ai = 0
                            For j = 1 To group_size - 1
                                Dim a2 = db.getWeight(x, y, ix + j)
                                If a2 > a Then
                                    a = a2
                                    ai = j
                                End If
                            Next
                            V2.setWeight(x, y, i, a)
                            switches(n) = ix + ai
                            n += 1
                        Next
                    Next
                Next

            End If
            out_act = V2
            Return out_act
        End Function

        ''' <summary>
        ''' Routes the upstream gradient back through the group members that produced the maxima.
        ''' </summary>
        Public Overridable Sub backward() Implements Layer.backward
            Dim V = in_act ' we need to set dw of this
            Dim V2 = out_act
            Dim lN = out_depth
            V.clearGradient() ' zero out gradient wrt data

            ' pass the gradient through the appropriate switch
            If out_sx = 1 AndAlso out_sy = 1 Then
                For i = 0 To lN - 1
                    Dim chain_grad = V2.getGradient(i)
                    V.setGradient(switches(i), chain_grad)
                Next
            Else
                ' bleh okay, lets do this the hard way
                Dim n = 0 ' counter for switches
                For x = 0 To V2.SX - 1
                    For y = 0 To V2.SY - 1
                        For i = 0 To lN - 1
                            Dim chain_grad = V2.getGradient(x, y, i)
                            V.setGradient(x, y, switches(n), chain_grad)
                            n += 1
                        Next
                    Next
                Next
            End If
        End Sub

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>The constant text <c>maxout()</c>.</returns>
        Public Overrides Function ToString() As String
            Return "maxout()"
        End Function
    End Class

End Namespace
