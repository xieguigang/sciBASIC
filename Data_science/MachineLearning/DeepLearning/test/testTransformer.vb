#Region "Microsoft.VisualBasic::d4e93e23a1c8c3a4886bfdba3dba9c6b, Data_science\MachineLearning\DeepLearning\test\testTransformer.vb"

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

    '   Total Lines: 94
    '    Code Lines: 74 (78.72%)
    ' Comment Lines: 7 (7.45%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (13.83%)
    '     File Size: 3.32 KB


    ' Module testTransformer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: run, seqGenerate
    ' 
    ' /********************************************************************************/

#End Region

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
