Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Transformer
    ''' <summary>
    ''' Produce a flat array with the same dimension as the number of words in the dictionary
    ''' </summary>
    Public Class OutputLayer
        Public Wo As Tensor

        Private WoOptimizer As Optimizer

        Public Sub New(sequenceLength As Integer, embeddingSize As Integer, dictionarySize As Integer)
            Wo = New Tensor(embeddingSize * sequenceLength, dictionarySize)
            Wo.GenerateNormalRandomValues()

            WoOptimizer = New Optimizer(Wo)
        End Sub

        Public Function Output(input As Tensor) As Tensor
            Dim flatInput = input.Flatten()
            Dim filteredOutput = Tensor.MatMul(flatInput, Wo)
            Dim softmaxOutput = filteredOutput.Softmax()

            Return softmaxOutput
        End Function

        Public Sub MakeTrainingStep(learningRate As Double, [step] As Integer)
            WoOptimizer.MakeTrainingStep(learningRate, [step], Wo)
        End Sub

    End Class
End Namespace
