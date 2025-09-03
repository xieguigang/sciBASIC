Imports Microsoft.VisualBasic.MachineLearning.Transformer
Imports Microsoft.VisualBasic.Parallel

Module testTransformer

    Sub New()
        VectorTask.n_threads = 16
    End Sub

    Public Sub run()
        ' Load data
        Dim nrSentences = 1000
        Dim allEnglishSentences As List(Of List(Of String)) = Nothing, allSpanishSentences As List(Of List(Of String)) = Nothing
        TextProcessing.Load("\GCModeller\src\runtime\sciBASIC#\Data_science\MachineLearning\DeepLearning\Transformer\TrainingData\english-spanish.txt", nrSentences, allEnglishSentences, allSpanishSentences)

        ' Transformer setup
        Dim batchSize = 100
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

    Sub seqGenerate()

        Dim from As New List(Of List(Of String)) From {
            New List(Of String) From {"1.1.2.3", "K0001", "COG001"},
            New List(Of String) From {"1.22.2.3", "K0002", "COG011"},
            New List(Of String) From {"1.1.32.3", "K0003", "COG003"},
            New List(Of String) From {"1.1.32.3", "K0004", "COG007"},
            New List(Of String) From {"2.1.42.1", "K0005", "COG101"}
        }
        Dim [to] As New List(Of List(Of String)) From {
            New List(Of String) From {"AAA", "AAG", "AGT", "GTA", "TAC"},
            New List(Of String) From {"CAA", "AAG", "AGT", "GTA", "TAA"},
            New List(Of String) From {"AAA", "AAG", "AGT", "GTA", "TAT"},
            New List(Of String) From {"AAA", "AAG", "ACT", "CTA", "TAA"},
            New List(Of String) From {"TTC", "TCG", "CGT", "GTA", "TAT"}
        }

        ' Transformer setup
        Dim batchSize = 2
        Dim embeddingSize = 8
        Dim dk = 4
        Dim dv = 4
        Dim h = 2
        Dim dff = 16
        Dim Nx = 2
        Dim dropout = 0.0
        Dim transformer As TransformerModel = New TransformerModel(Nx, embeddingSize, dk, dv, h, dff, batchSize, dropout, from, [to])

        ' Training
        Dim nrEpochs = 10
        Dim nrTrainingSteps = 10
        Dim learningRate = 0.01
        Try
            transformer.Train(nrEpochs, nrTrainingSteps, learningRate, batchSize, from, [to])
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
