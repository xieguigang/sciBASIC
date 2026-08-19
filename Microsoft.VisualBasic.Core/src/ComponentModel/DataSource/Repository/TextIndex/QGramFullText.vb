Namespace ComponentModel.DataSourceModel.Repository

    ''' <summary>
    ''' 基于Qgram的全文索引
    ''' </summary>
    Public Class QGramFullText

        ReadOnly docs As Dictionary(Of String, String)
        ReadOnly word2Docs As Dictionary(Of String, HashSet(Of String))
        ReadOnly wordIndex As QGramIndex
        ReadOnly tokenlicer As Func(Of String, IEnumerable(Of String))

        Sub New(Optional q As Integer = 3, Optional tokenlicer As Func(Of String, IEnumerable(Of String)) = Nothing)
            Me.wordIndex = New QGramIndex(q)
            Me.tokenlicer = If()
        End Sub


    End Class
End Namespace