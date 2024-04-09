Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec

Namespace node2vec

    ''' <summary>
    ''' Created by freemso on 17-3-14.
    ''' </summary>
    Public Module Solver

        <Extension>
        Public Function CreateEmbedding(graph As Graph,
                                        Optional numWalks As Integer = 10,
                                        Optional walkLength As Integer = 80,
                                        Optional dimensions As Integer = 20,
                                        Optional windowSize As Integer = 10) As VectorModel

            Dim pathList As IList(Of IList) = graph.simulateWalks(numWalks, walkLength)

            Console.WriteLine("Learning Embedding...")

            ' convert path list to string
            Dim sentList = ""
            For Each path As IList(Of Graph.Node) In pathList
                Dim sent = ""
                For Each node In path
                    sent += node.Id.ToString() & " "
                Next
                sentList += sent & vbLf
            Next

            ' use word2vec to do word embedding
            Dim model As New Word2VecFactory
            model.setMethod(TrainMethod.Skip_Gram).setWindow(windowSize).setVectorSize(dimensions)
            Dim engine As Word2Vec = model.build()

            For Each path As IList(Of Graph.Node) In pathList
                engine.readTokens(path.Select(Function(v) v.Id.ToString).ToArray)
            Next

            engine.training()

            Dim vectors = engine.outputVector
            Return vectors
        End Function
    End Module

End Namespace
