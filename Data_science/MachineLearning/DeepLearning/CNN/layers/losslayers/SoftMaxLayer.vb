#Region "Microsoft.VisualBasic::02926f09f5c5e5b75b17ddf95b529385, Data_science\MachineLearning\DeepLearning\CNN\Layers\losslayers\SoftMaxLayer.vb"

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

    '   Total Lines: 109
    '    Code Lines: 70 (64.22%)
    ' Comment Lines: 17 (15.60%)
    '    - Xml Docs: 58.82%
    ' 
    '   Blank Lines: 22 (20.18%)
    '     File Size: 3.45 KB


    '     Class SoftMaxLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) backward, forward, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports std = System.Math
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace CNN.losslayers


    ''' <summary>
    ''' This layer will squash the result of the activations in the fully
    ''' connected layer and give you a value of 0 to 1 for all output activations.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class SoftMaxLayer : Inherits LossLayer

        Dim es As Double()

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.SoftMax
            End Get
        End Property

        Public Sub New(def As OutputDefinition)
            MyBase.New(def)

            ' computed
            num_inputs = def.outY * def.outX * def.depth
            out_depth = num_inputs
            out_sx = 1
            out_sy = 1

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Sub New()
        End Sub

        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            Dim A As New DataBlock(1, 1, out_depth, 0.0) With {.trace = Me.ToString}

            in_act = db

            ' compute max activation
            Dim [as] = db.Weights
            Dim amax = db.getWeight(0)
            For i As Integer = 1 To out_depth - 1
                If [as](i) > amax Then
                    amax = [as](i)
                End If
            Next

            ' compute exponentials (carefully to not blow up)
            Dim es = New Double(out_depth - 1) {}
            Dim esum = 0.0

            For i As Integer = 0 To out_depth - 1
                Dim e = std.Exp([as](i) - amax)
                esum += e
                es(i) = e
            Next

            ' normalize and output to sum to one
            For i As Integer = 0 To out_depth - 1
                es(i) /= esum
                A.setWeight(i, es(i))
            Next

            Me.es = es ' save these for backprop
            out_act = A
            Return out_act
        End Function

        ''' <summary>
        ''' compute and accumulate gradient wrt weights and bias of this layer
        ''' </summary>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Overrides Function backward(y As Integer) As Double
            Dim x As DataBlock = in_act.clearGradient() ' zero out the gradient of input Vol

            For i = 0 To out_depth - 1
                Dim indicator = If(i = y, 1.0, 0.0)
                Dim mul = -(indicator - es(i))

                x.setGradient(i, mul)
            Next

            ' loss is the class negative log likelihood
            Return -std.Log(es(y))
        End Function

        Public Overrides Function backward(y() As Double) As Double()
            Dim x As DataBlock = in_act.clearGradient
            ' -(y-es) = es - y
            Dim mul = SIMD.Subtract.f64_op_subtract_f64(es, y)
            x.setGradient(mul)
            Return New Vector(es).Log * -1
        End Function

        Public Overrides Function ToString() As String
            Return "softmax()"
        End Function
    End Class
End Namespace
