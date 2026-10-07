Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>单个工程的输出路径修正结果</summary>
Public Class OutputPathResult

    ''' <summary>被改写或者补写了 OutputPath 的条件组数量</summary>
    Public Property Updated As Integer
    ''' <summary>新建的 nuget_release|x64 条件组数量（0 或 1）</summary>
    Public Property Created As Integer
    ''' <summary>补齐的 Configurations / Platforms 声明条数</summary>
    Public Property DeclarationsAdded As Integer
    ''' <summary>本次计算出的、指向输出文件夹的相对路径，用于日志展示</summary>
    Public Property OutputPath As String

    Public ReadOnly Property Changed As Boolean
        Get
            Return Updated > 0 OrElse Created > 0 OrElse DeclarationsAdded > 0
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

End Class