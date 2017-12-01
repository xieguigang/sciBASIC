Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Academic

    ''' <summary>
    ''' 文献的一些摘要信息
    ''' </summary>
    Public Class ArticleProfile

        Public Property Title As String
        Public Property Authors As NamedValue(Of String)()
        Public Property Abstract As String
        Public Property PubDate As Date
        Public Property Journal As String
        Public Property DOI As String
        ''' <summary>
        ''' 按照年计数的被引用量
        ''' </summary>
        Public Property CitesCount As NamedValue(Of Integer)()
        Public Property Pages As String
        ''' <summary>
        ''' 卷号
        ''' </summary>
        Public Property Volume As String
        ''' <summary>
        ''' 期号
        ''' </summary>
        Public Property Issue As String
        ''' <summary>
        ''' 有效的原文来源地址url
        ''' </summary>
        Public Property source As String()
        ''' <summary>
        ''' 文献引文的bing学术搜索的url列表
        ''' </summary>
        Public Property References As String()

        Public Overrides Function ToString() As String
            Return Title
        End Function
    End Class
End Namespace