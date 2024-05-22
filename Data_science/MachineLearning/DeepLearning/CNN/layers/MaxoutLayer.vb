#Region "Microsoft.VisualBasic::488a1c77c02c4f87f8c635a0d2f9afb2, Data_science\MachineLearning\DeepLearning\CNN\layers\MaxoutLayer.vb"

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

    '   Total Lines: 130
    '    Code Lines: 101 (77.69%)
    ' Comment Lines: 14 (10.77%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 15 (11.54%)
    '     File Size: 4.90 KB


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
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
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

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Maxout
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition)
            ' computed
            out_sx = def.outX
            out_sy = def.outY
            out_depth = CInt(std.Floor(def.depth / group_size))

            switches = New Integer(out_sx * out_sy * out_depth - 1) {} ' useful for backprop
            switches.fill(0)
        End Sub

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

        Public Overrides Function ToString() As String
            Return "maxout()"
        End Function
    End Class

End Namespace
