Namespace BM25

    ''' <summary>
    ''' 单条检索结果。
    ''' </summary>
    Public Class SearchResult

        ''' <summary>文档 ID。</summary>
        Public Property DocId As Integer

        ''' <summary>BM25 得分。</summary>
        Public Property Score As Double

        ''' <summary>各查询词的贡献明细（用于可解释性）。</summary>
        Public Property TermContributions As List(Of TermContribution)

        Public Overrides Function ToString() As String
            Return $"Doc#{DocId}  Score={Score:F6}"
        End Function

    End Class
End Namespace