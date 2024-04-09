Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.SequenceGraphTransform
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Graph2Vec

    ReadOnly vocabulary As New Dictionary(Of String, Char)
    ReadOnly getVocabulary As Func(Of Vertex, String)
    ReadOnly other As String

    Const offset As Integer = 33

    Dim sgt As SequenceGraphTransform

    Sub New(getVocabulary As Func(Of Vertex, String), Optional other As String = "*")
        Me.other = other
        Me.getVocabulary = getVocabulary
    End Sub

    Public Function Setup(terms As IEnumerable(Of String)) As Graph2Vec
        For Each term As String In terms.JoinIterates({other})
            Call vocabulary.Add(term, Chr(offset + vocabulary.Count))
        Next

        If vocabulary.Count >= (127 - offset) Then
            Call VBDebugger.EchoLine("alphabet set too much chars, the embedding vector will be very very long!")
        End If

        sgt = New SequenceGraphTransform(mode:=Modes.Fast)
        sgt.set_alphabets(vocabulary.Values.Select(Function(c) c.ToString).ToArray)

        Return Me
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
            .Select(Function(key)
                        If vocabulary.ContainsKey(key) Then
                            Return vocabulary(key)
                        Else
                            Return vocabulary(other)
                        End If
                    End Function) _
            .ToArray
        Dim vector As Double() = sgt.fitVector(New String(chars))

        Return vector
    End Function

End Class
