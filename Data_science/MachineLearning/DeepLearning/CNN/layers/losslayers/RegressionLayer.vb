#Region "Microsoft.VisualBasic::ef5e5b267f492786ecccc255f652464b, Data_science\MachineLearning\DeepLearning\CNN\Layers\losslayers\RegressionLayer.vb"

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

    '   Total Lines: 67
    '    Code Lines: 42
    ' Comment Lines: 10
    '   Blank Lines: 15
    '     File Size: 2.24 KB


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
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.losslayers

    ''' <summary>
    ''' Regression layer is used when your output is an area of data.
    ''' When you don't have a single class that is the correct activation
    ''' but you try to find a result set near to your training area.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class RegressionLayer : Inherits LossLayer

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.Regression
            End Get
        End Property

        Public Sub New(def As OutputDefinition)
            MyBase.New(def)
        End Sub

        Sub New()
        End Sub

        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            in_act = db
            out_act = db ' nothing to do, output raw scores
            Return db
        End Function

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

        Public Overrides Function ToString() As String
            Return "regression()"
        End Function
    End Class

End Namespace
