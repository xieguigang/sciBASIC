#Region "Microsoft.VisualBasic::aa310086233f718b006971cb9e68e0ac, Data_science\MachineLearning\DeepLearning\CNN\Layers\losslayers\RegressionLayer.vb"

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

    '   Total Lines: 66
    '    Code Lines: 41 (62.12%)
    ' Comment Lines: 10 (15.15%)
    '    - Xml Docs: 60.00%
    ' 
    '   Blank Lines: 15 (22.73%)
    '     File Size: 2.18 KB


    '     Class RegressionLayer
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

Namespace CNN.losslayers

    ''' <summary>
    ''' Regression layer is used when your output is an area of data.
    ''' When you don't have a single class that is the correct activation
    ''' but you try to find a result set near to your training area.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class RegressionLayer : Inherits LossLayer

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Regression"/>.</summary>
        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.Regression
            End Get
        End Property

        ''' <summary>
        ''' Creates the regression loss layer.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the last layer shape.</param>
        Public Sub New(def As OutputDefinition)
            MyBase.New(def)
        End Sub

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Passes the raw scores through unchanged; the regression layer applies no activation.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">Ignored; the loss layer behaves the same in both modes.</param>
        ''' <returns>The raw scores.</returns>
        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            in_act = db
            out_act = db ' nothing to do, output raw scores
            Return db
        End Function

        ''' <summary>
        ''' Computes the squared error loss and its gradient for a continuous target vector.
        ''' </summary>
        ''' <param name="y">The target output vector.</param>
        ''' <returns>The per element loss vector.</returns>
        Public Overrides Function backward(y As Double()) As Double()
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act.clearGradient() ' zero out the gradient of input Vol
            Dim loss As Double() = New Double(y.Length - 1) {}

            For i As Integer = 0 To out_depth - 1
                Dim dy = x.getWeight(i) - y(i)

                x.setGradient(i, dy)
                loss(i) = 0.5 * dy * dy
            Next

            Return loss
        End Function

        ''' <summary>
        ''' Computes the squared error loss and its gradient for a single regressed value.
        ''' </summary>
        ''' <param name="y">The target value.</param>
        ''' <returns>The loss value.</returns>
        Public Overrides Function backward(y As Integer) As Double
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act.clearGradient() ' zero out the gradient of input Vol
            Dim loss = 0.0
            ' lets hope that only one number is being regressed
            Dim dy = x.getWeight(0) - y

            x.setGradient(0, dy)
            loss += 0.5 * dy * dy

            Return loss
        End Function

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>The constant text <c>regression()</c>.</returns>
        Public Overrides Function ToString() As String
            Return "regression()"
        End Function
    End Class

End Namespace
