Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Public Structure ArticleInfo
    Dim Title As String
    Dim Authors As String()
    Dim Abstract As String
    Dim PubDate As Date
    Dim Journal As String
    Dim DOI As String
    ''' <summary>
    ''' 按照年计数的被引用量
    ''' </summary>
    Dim CitesCount As NamedValue(Of Integer)()
    Dim Pages As String
    ''' <summary>
    ''' 卷号
    ''' </summary>
    Dim Volume As String
    ''' <summary>
    ''' 期号
    ''' </summary>
    Dim Issue As String
    ''' <summary>
    ''' 有效的原文来源地址url
    ''' </summary>
    Dim source As String()
    ''' <summary>
    ''' 文献引文的bing学术搜索的url列表
    ''' </summary>
    Dim References As String()

    Public Overrides Function ToString() As String
        Return Title
    End Function
End Structure
