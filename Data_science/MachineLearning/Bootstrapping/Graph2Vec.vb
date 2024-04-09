Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Graph2Vec

    ReadOnly vocabulary As New Dictionary(Of String, Char)
    ReadOnly getVocabulary As Func(Of Vertex, String)

    Dim sgt As SequenceGraphTransform

    Public Function Setup(terms As IEnumerable(Of String)) As Graph2Vec

    End Function

    Public Function GraphVector(g As Graph) As Double()
        Dim graph As New node2vec.Graph(g)
        Dim nodes As VectorModel = node2vec.CreateEmbedding(graph, dimensions:=3)
        Dim sort As NamedCollection(Of Single)() = nodes _
            .AsEnumerable _
            .OrderBy(Function(v) (New Vector(v) ^ 2).Sum) _
            .ToArray
        Dim vertexSet = g.vertex.ToDictionary(Function(v) v.label)
        Dim chars As Char() = sort _
            .Select(Function(lb) getVocabulary(vertexSet(lb.name))) _
            .Select(Function(key) vocabulary(key)) _
            .ToArray


    End Function

End Class
