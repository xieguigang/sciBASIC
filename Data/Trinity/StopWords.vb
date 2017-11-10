Public Class StopWords : Implements IEnumerable(Of String)
    Implements IReadOnlyCollection(Of String)
    Implements IReadOnlyList(Of String)

    ReadOnly stopwords$()

    Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of String).Count

    Default Public ReadOnly Property Item(index As Integer) As String Implements IReadOnlyList(Of String).Item
        Get
            Return stopwords(index)
        End Get
    End Property

    Sub New()
        Count = stopwords.Length
    End Sub

    Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
        For Each word$ In stopwords
            Yield word
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
