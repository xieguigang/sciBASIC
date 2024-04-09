Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec

Namespace node2vec

    ''' <summary>
    ''' Created by freemso on 17-3-14.
    ''' </summary>
    Public Module Solver

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="graph"></param>
        ''' <param name="numWalks"></param>
        ''' <param name="walkLength"></param>
        ''' <param name="dimensions"></param>
        ''' <param name="windowSize"></param>
        ''' <returns>node mapping to a vector</returns>
        <Extension>
        Public Function CreateEmbedding(graph As Graph,
                                        Optional numWalks As Integer = 10,
                                        Optional walkLength As Integer = 80,
                                        Optional dimensions As Integer = 20,
                                        Optional windowSize As Integer = 10) As VectorModel

            Dim pathList As IList(Of IList(Of Graph.Node)) = graph.simulateWalks(numWalks, walkLength)
            ' use word2vec to do word embedding
            Dim model As New Word2VecFactory
            model.setMethod(TrainMethod.Skip_Gram).setWindow(windowSize).setVectorSize(dimensions)
            Dim engine As Word2Vec = model.build()

            For Each path As IList(Of Graph.Node) In pathList
                ' convert path list to string
                engine.readTokens(path.Select(Function(v) v.Id.ToString).ToArray)
            Next

            Console.WriteLine("Learning Embedding...")
            engine.training()

            Dim vectors = engine.outputVector
            Return vectors
        End Function
    End Module

End Namespace
