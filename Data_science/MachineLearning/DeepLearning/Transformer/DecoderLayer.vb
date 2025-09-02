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
