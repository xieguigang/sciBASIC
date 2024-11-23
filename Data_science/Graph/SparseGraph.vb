Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Public Class SparseGraph

    <XmlElement("edges")>
    Public Property Edges As Edge()
        Get
            Return graph.ToArray
        End Get
        Set(value As Edge())
            graph = value.ToArray
            index_u = value _
                .GroupBy(Function(a) a.u) _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return a.ToArray
                              End Function)
        End Set
    End Property

    Public Class Edge

        <XmlAttribute> Public Property u As String
        <XmlAttribute> Public Property v As String

        Public Overrides Function ToString() As String
            Return $"[{u}, {v}]"
        End Function

    End Class

    Dim index_u As Dictionary(Of String, Edge())
    Dim graph As Edge()

    Public Function CreateMatrix(keys As String()) As NumericMatrix
        Dim rows As New List(Of Double())

        For Each u As String In keys
            Call rows.Add(Links(u, keys).ToArray)
        Next

        Return New NumericMatrix(rows)
    End Function

    Private Iterator Function Links(u As String, keys As String()) As IEnumerable(Of Double)
        If Not index_u.ContainsKey(u) Then
            For i As Integer = 0 To keys.Length - 1
                Yield 0.0
            Next
        Else
            Dim vlist As Edge() = index_u.TryGetValue(u)

            For Each v As String In keys
                Yield Aggregate e As Edge
                      In vlist
                      Where e.v = v
                      Into Count
            Next
        End If
    End Function

End Class
