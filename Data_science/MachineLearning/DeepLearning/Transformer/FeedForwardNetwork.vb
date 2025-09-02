Imports Microsoft.VisualBasic.MachineLearning.Transformer.Utils

Namespace Transformer
    Public Class FeedForwardNetwork
        Private W1, W2 As Tensor
        Private b1, b2 As Tensor

        Private W1Optimizer, W2Optimizer, b1Optimizer, b2Optimizer As Optimizer

        Public Sub New(dff As Integer, embeddingSize As Integer)
            W1 = New Tensor(embeddingSize, dff)
            W2 = New Tensor(dff, embeddingSize)
            b1 = New Tensor(dff)
            b2 = New Tensor(embeddingSize)
            GenerateRandomLayers()

            W1Optimizer = New Optimizer(W1)
            W2Optimizer = New Optimizer(W2)
            b1Optimizer = New Optimizer(b1)
            b2Optimizer = New Optimizer(b2)
        End Sub

        Public Function FeedForward(G As Tensor) As Tensor
            ' First layer
            Dim FFN1 = Tensor.MatMul(G, W1).VecAdd(b1)
            FFN1.ReLU()

            ' Second layer
            Dim FFN2 = Tensor.MatMul(FFN1, W2).VecAdd(b2)

            Return FFN2
        End Function

        Private Sub GenerateRandomLayers()
            W1.GenerateNormalRandomValues()
            W2.GenerateNormalRandomValues()
        End Sub

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            W1Optimizer.MakeTrainingStep(learningRate, [step], W1)
            W2Optimizer.MakeTrainingStep(learningRate, [step], W2)
            b1Optimizer.MakeTrainingStep(learningRate, [step], b1)
            b2Optimizer.MakeTrainingStep(learningRate, [step], b2)
        End Sub

    End Class
End Namespace
