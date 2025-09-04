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
