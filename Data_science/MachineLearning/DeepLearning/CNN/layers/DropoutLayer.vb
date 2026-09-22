#Region "Microsoft.VisualBasic::bf8c0a64a3a8c5282f07adc1bef1e731, Data_science\MachineLearning\DeepLearning\CNN\Layers\DropoutLayer.vb"

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

    '   Total Lines: 115
    '    Code Lines: 63 (54.78%)
    ' Comment Lines: 33 (28.70%)
    '    - Xml Docs: 75.76%
    ' 
    '   Blank Lines: 19 (16.52%)
    '     File Size: 4.31 KB


    '     Class DropoutLayer
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

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace CNN.layers

    ''' <summary>
    ''' This layer will remove some random activations in order to
    ''' defeat over-fitting.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class DropoutLayer : Inherits DataLink
        Implements Layer

        Private out_depth, out_sx, out_sy As Integer
        Private ReadOnly drop_prob As Double = 0.5
        Private dropped As Boolean()

        ''' <summary>Gets the parameter and gradient blocks of this layer; the dropout layer has none.</summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Dropout"/>.</summary>
        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Dropout
            End Get
        End Property

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates a dropout layer.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the layer shape.</param>
        ''' <param name="drop_prob">Probability of dropping an activation during training.</param>
        Public Sub New(def As OutputDefinition, drop_prob As Double)
            ' computed
            out_sx = def.outX
            out_sy = def.outY
            out_depth = def.depth

            Me.dropped = New Boolean(out_sx * out_sy * out_depth - 1) {}
            Me.drop_prob = drop_prob
        End Sub

        ''' <summary>
        ''' Applies dropout during training by zeroing a random subset of activations; during inference the activations
        ''' are rescaled instead.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">When <c>True</c> the random dropout mask is applied.</param>
        ''' <returns>The masked (or rescaled) activations.</returns>
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim V2 As DataBlock = db.clone()
            Dim N = db.Weights.Length

            in_act = db

            If training Then
                ' do dropout
                For i As Integer = 0 To N - 1
                    If randf.NextDouble() < drop_prob Then
                        V2.setWeight(i, 0)
                        dropped(i) = True
                    Else
                        ' drop!
                        dropped(i) = False
                    End If
                Next
            Else
                ' scale the activations during prediction
                V2.mulGradient(drop_prob)
            End If

            out_act = V2

            ' dummy identity function for now
            Return out_act
        End Function

        ''' <summary>
        ''' Passes the upstream gradients back only through the activations that were kept during the forward pass.
        ''' </summary>
        Public Overridable Sub backward() Implements Layer.backward
            Dim V = in_act ' we need to set dw of this
            Dim chain_grad = out_act
            Dim N = V.Weights.Length

            V.clearGradient() ' zero out gradient wrt data

            Dim V_dw = V.Gradients
            Dim chain_dw = chain_grad.Gradients

            For i As Integer = 0 To N - 1
                If Not dropped(i) Then
                    ' copy over the gradient
                    V_dw(i) = chain_dw(i)
                End If
            Next
        End Sub

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>The constant text <c>dropout()</c>.</returns>
        Public Overrides Function ToString() As String
            Return "dropout()"
        End Function
    End Class

End Namespace
