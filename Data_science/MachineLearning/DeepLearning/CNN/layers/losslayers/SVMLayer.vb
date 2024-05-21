#Region "Microsoft.VisualBasic::df0e9367fbccb0fbd14b9dbb0d53c17b, Data_science\MachineLearning\DeepLearning\CNN\Layers\losslayers\SVMLayer.vb"

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

    '   Total Lines: 70
    '    Code Lines: 45
    ' Comment Lines: 11
    '   Blank Lines: 14
    '     File Size: 2.27 KB


    '     Class SVMLayer
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
    ''' This layer uses the input area trying to find a line to
    ''' separate the correct activation from the incorrect ones.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class SVMLayer : Inherits LossLayer

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.SVM
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

        Public Overrides Function backward(y As Integer) As Double
            ' compute and accumulate gradient wrt weights and bias of this layer
            Dim x = in_act.clearGradient()
            ' we're using structured loss here, which means that the score
            ' of the ground truth should be higher than the score of any other
            ' class, by a margin
            Dim yscore = x.getWeight(y) ' score of ground truth
            Dim margin = 1.0
            Dim loss = 0.0

            For i As Integer = 0 To out_depth - 1
                If y = i Then
                    Continue For
                End If

                Dim ydiff = -yscore + x.getWeight(i) + margin

                If ydiff > 0 Then
                    ' violating dimension, apply loss
                    x.addGradient(i, 1)
                    x.subGradient(y, 1)

                    loss += ydiff
                End If
            Next
            Return loss
        End Function

        Public Overrides Function ToString() As String
            Return "svm()"
        End Function

        Public Overrides Function backward(y() As Double) As Double()
            Throw New NotSupportedException("svm not supported")
        End Function
    End Class

End Namespace
