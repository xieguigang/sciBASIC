Public Class TFIDF

    ReadOnly vecs As New Dictionary(Of String, Dictionary(Of String, Integer))

    ''' <summary>
    ''' get N sequence
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property N As Integer
        Get
            Return vecs.Count
        End Get
    End Property

    Sub New()
    End Sub

    Public Sub Add(id As String, seq As IEnumerable(Of String))
        Dim counter As Dictionary(Of String, Integer) = seq _
            .GroupBy(Function(a) a) _
            .ToDictionary(Function(a) a.Key,
                          Function(a)
                              Return a.Count
                          End Function)

        Call vecs.Add(id, seq)
    End Sub



End Class
