Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer

    Public Class EncoderStack

        Private Nx As Integer
        Private encoderLayers As New List(Of EncoderLayer)()

        Public Sub New(Nx As Integer, embeddingSize As Integer, dk As Integer, dv As Integer, h As Integer, dff As Integer)
            Me.Nx = Nx

            For i = 0 To Nx - 1
                encoderLayers.Add(New EncoderLayer(embeddingSize, dk, dv, h, dff))
            Next
        End Sub

        Public Function Encode(word_embeddings As Tensor, isTraining As Boolean) As Tensor
            Dim encoderOutput = encoderLayers(0).Encode(word_embeddings, isTraining)
            For i = 1 To Nx - 1
                encoderOutput = encoderLayers(i).Encode(encoderOutput, isTraining)
            Next

            Return encoderOutput
        End Function

        Public Sub SetDropoutNodes(dropout As Double)
            For i = 0 To Nx - 1
                encoderLayers(i).SetDropoutNodes(dropout)
            Next
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            For i = 0 To Nx - 1
                encoderLayers(i).MakeTrainingStep(learningRate, [step])
            Next
        End Sub

    End Class
End Namespace
