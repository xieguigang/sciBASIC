Imports Microsoft.VisualBasic.MachineLearning.Transformer.Utils

Namespace Transformer
    Public Class DecoderStack
        Private Nx As Integer
        Private decoderLayers As List(Of DecoderLayer) = New List(Of DecoderLayer)()

        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.Nx = Nx

            For i = 0 To Nx - 1
                decoderLayers.Add(New DecoderLayer(embeddingSize, dk, dv, h, dff))
            Next
        End Sub

        Public Function Decode(encoderOutput As Tensor, word_embeddings As Tensor, isTraining As Boolean) As Tensor
            Dim decoderOutput = decoderLayers(0).Decode(encoderOutput, word_embeddings, isTraining)
            For i = 1 To Nx - 1
                decoderOutput = decoderLayers(i).Decode(encoderOutput, decoderOutput, isTraining)
            Next

            Return decoderOutput
        End Function

        Public Sub SetDropoutNodes(dropout As Double)
            For i = 0 To Nx - 1
                decoderLayers(i).SetDropoutNodes(dropout)
            Next
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For i = 0 To Nx - 1
                decoderLayers(i).MakeTrainingStep(learningRate, [step])
            Next
        End Sub

    End Class
End Namespace
