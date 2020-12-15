''' <summary>
''' 分隔符对象在分块之中的位置
''' </summary>
Public Enum DelimiterLocation As Integer
    ''' <summary>
    ''' 上一个分块的最末尾
    ''' </summary>
    PreviousLast
    ''' <summary>
    ''' 不会再任何分块之中包含有分隔符
    ''' </summary>
    NotIncludes
    ''' <summary>
    ''' 包含在下一个分块之中的最开始的位置
    ''' </summary>
    NextFirst
End Enum