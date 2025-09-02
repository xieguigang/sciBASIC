Imports Microsoft.VisualBasic.MachineLearning.Transformer

Module testTransformer

    Public Sub run()
        ' Load data
        Dim nrSentences = 100
        Dim allEnglishSentences As List(Of List(Of String)) = Nothing, allSpanishSentences As List(Of List(Of String)) = Nothing
        TextProcessing.Load("F:\Transformer-master\Transformer\TrainingData\english-spanish.txt", nrSentences, allEnglishSentences, allSpanishSentences)

        ' Transformer setup
        Dim batchSize = 10
        Dim embeddingSize = 8
        Dim dk = 4
        Dim dv = 4
        Dim h = 2
        Dim dff = 16
        Dim Nx = 2
        Dim dropout = 0.0
        Dim transformer As TransformerModel = New TransformerModel(Nx, embeddingSize, dk, dv, h, dff, batchSize, dropout, allEnglishSentences, allSpanishSentences)

        ' Training
        Dim nrEpochs = 2
        Dim nrTrainingSteps = 10
        Dim learningRate = 0.01
        Try
            transformer.Train(nrEpochs, nrTrainingSteps, learningRate, batchSize, allEnglishSentences, allSpanishSentences)
        Catch ex As Exception
            Console.WriteLine(ex.ToString())
        End Try

        ' Testing
        Try
            transformer.Infer()
        Catch ex As Exception
            Console.WriteLine(ex.ToString())
        End Try

        Console.WriteLine("")
    End Sub
End Module
