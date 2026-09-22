#Region "Microsoft.VisualBasic::8d58f5b64c21a71858d4b1f3afe1e34c, Data_science\MachineLearning\DeepLearning\CNN\Layers\losslayers\SVMLayer.vb"

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

    '   Total Lines: 94
    '    Code Lines: 44 (46.81%)
    ' Comment Lines: 36 (38.30%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 14 (14.89%)
    '     File Size: 3.68 KB


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

Namespace CNN.losslayers

    ''' <summary>
    ''' This layer uses the input area trying to find a line to
    ''' separate the correct activation from the incorrect ones.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class SVMLayer : Inherits LossLayer

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.SVM"/>.</summary>
        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.SVM
            End Get
        End Property

        ''' <summary>
        ''' Creates the SVM (structured hinge loss) layer.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the last layer shape.</param>
        Public Sub New(def As OutputDefinition)
            MyBase.New(def)
        End Sub

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Passes the raw scores through unchanged; the SVM layer applies no activation.
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
        ''' Computes the structured hinge loss and its gradient for a classification target.
        ''' </summary>
        ''' <param name="y">Index of the target class.</param>
        ''' <returns>The loss value.</returns>
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

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>The constant text <c>svm()</c>.</returns>
        Public Overrides Function ToString() As String
            Return "svm()"
        End Function

        ''' <summary>
        ''' Not supported; the SVM loss only accepts a single class index.
        ''' </summary>
        ''' <param name="y">The target distribution, which is ignored.</param>
        ''' <returns>This method never returns.</returns>
        ''' <exception cref="NotSupportedException">Always thrown.</exception>
        Public Overrides Function backward(y() As Double) As Double()
            Throw New NotSupportedException("svm not supported")
        End Function
    End Class

End Namespace
