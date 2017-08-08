''' <summary>
''' 文章正文数据模型
''' </summary>
Public Structure Article

    ''' <summary>
    ''' 文章标题
    ''' </summary>
    Public Property Title As String

    ''' <summary>
    ''' 正文文本
    ''' </summary>
    Public Property Content As String

    ''' <summary>
    ''' 带标签正文
    ''' </summary>
    Public Property ContentWithTags As String

    ''' <summary>
    ''' 文章发布时间
    ''' </summary>
    Public Property PublishDate As DateTime

    Public Overrides Function ToString() As String
        Return Title
    End Function

End Structure