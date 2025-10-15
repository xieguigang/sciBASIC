#Region "Microsoft.VisualBasic::2a5786482568a028326dcac0339b4f74, Data_science\MachineLearning\DeepLearning\Transformer\EncoderLayer.vb"

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

    '   Total Lines: 58
    '    Code Lines: 41 (70.69%)
    ' Comment Lines: 2 (3.45%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (25.86%)
    '     File Size: 2.47 KB


    '     Class EncoderLayer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Encode
    ' 
    '         Sub: MakeTrainingStep, SetDropoutNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Transformer
    Public Class EncoderLayer
        Private embeddingSize As Integer

        Private mha As MultiHeadAttention
        Private ff As FeedForwardNetwork

        Private dropoutMask1, dropoutMask2 As Boolean()
        Private dropoutRate As Double = 0

        Public Sub New(embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.embeddingSize = embeddingSize

            mha = New MultiHeadAttention(dk, dv, h, embeddingSize, False)
            ff = New FeedForwardNetwork(dff, embeddingSize)

            dropoutMask1 = New Boolean(embeddingSize - 1) {}
            dropoutMask2 = New Boolean(embeddingSize - 1) {}
        End Sub

        Public Function Encode(encoderInput As Tensor, isTraining As Boolean) As Tensor
            ' Multi headed attention
            Dim attentionFilteredData = mha.Update(encoderInput)
            If isTraining AndAlso dropoutRate > 0 Then attentionFilteredData = attentionFilteredData.Dropout(dropoutMask1, dropoutRate)
            attentionFilteredData = Tensor.AddNorm(encoderInput, attentionFilteredData)

            ' Feed forward neural network
            Dim feedForwardOutput = ff.FeedForward(attentionFilteredData)
            If isTraining AndAlso dropoutRate > 0 Then feedForwardOutput = feedForwardOutput.Dropout(dropoutMask2, dropoutRate)
            feedForwardOutput = Tensor.AddNorm(attentionFilteredData, feedForwardOutput)

            Return feedForwardOutput
        End Function

        Public Sub SetDropoutNodes(dropoutRate As Double)
            If dropoutRate < 0 OrElse dropoutRate >= 1 Then Throw New ArgumentException("Error: dropout rate must be >= 0 and < 1")

            Me.dropoutRate = dropoutRate

            For i = 0 To embeddingSize - 1
                dropoutMask1(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask1(i) = True

                dropoutMask2(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask2(i) = True
            Next
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            mha.MakeTrainingStep(learningRate, [step])
            ff.MakeTrainingStep(learningRate, [step])
        End Sub

    End Class
End Namespace
