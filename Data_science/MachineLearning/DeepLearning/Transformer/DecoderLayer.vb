#Region "Microsoft.VisualBasic::504982ffc64e1c2f9385478bb8b60b27, Data_science\MachineLearning\DeepLearning\Transformer\DecoderLayer.vb"

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
    '    Code Lines: 50 (71.43%)
    ' Comment Lines: 3 (4.29%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 17 (24.29%)
    '     File Size: 3.31 KB


    '     Class DecoderLayer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Decode
    ' 
    '         Sub: MakeTrainingStep, SetDropoutNodes
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Transformer
    Public Class DecoderLayer
        Private embeddingSize As Integer

        Private mha As MultiHeadAttention
        Private mha_masked As MultiHeadAttention
        Public ff As FeedForwardNetwork

        Private dropoutMask1, dropoutMask2, dropoutMask3 As Boolean()
        Private dropoutRate As Double = 0

        Public Sub New(embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.embeddingSize = embeddingSize

            mha = New MultiHeadAttention(dk, dv, h, embeddingSize, False)
            mha_masked = New MultiHeadAttention(dk, dv, h, embeddingSize, True)
            ff = New FeedForwardNetwork(dff, embeddingSize)

            dropoutMask1 = New Boolean(embeddingSize - 1) {}
            dropoutMask2 = New Boolean(embeddingSize - 1) {}
            dropoutMask3 = New Boolean(embeddingSize - 1) {}
        End Sub

        Public Function Decode(encoderOutput As Tensor, decoderInput As Tensor, isTraining As Boolean) As Tensor
            ' Masked multi headed attention
            Dim maskedAttentionFilteredData = mha_masked.Update(decoderInput)
            If isTraining AndAlso dropoutRate > 0 Then maskedAttentionFilteredData = maskedAttentionFilteredData.Dropout(dropoutMask1, dropoutRate)
            maskedAttentionFilteredData = Tensor.AddNorm(decoderInput, maskedAttentionFilteredData)

            ' Multi headed attention
            Dim attentionFilteredData = mha.Update(encoderOutput, maskedAttentionFilteredData)
            If isTraining AndAlso dropoutRate > 0 Then attentionFilteredData = attentionFilteredData.Dropout(dropoutMask2, dropoutRate)
            attentionFilteredData = Tensor.AddNorm(maskedAttentionFilteredData, attentionFilteredData)

            ' Feed forward neural network
            Dim feedForwardOutput = ff.FeedForward(attentionFilteredData)
            If isTraining AndAlso dropoutRate > 0 Then feedForwardOutput = feedForwardOutput.Dropout(dropoutMask3, dropoutRate)
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

                dropoutMask3(i) = False
                If randf.NextDouble < dropoutRate Then dropoutMask3(i) = True
            Next
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            mha_masked.MakeTrainingStep(learningRate, [step])
            mha.MakeTrainingStep(learningRate, [step])
            ff.MakeTrainingStep(learningRate, [step])
        End Sub

    End Class
End Namespace
