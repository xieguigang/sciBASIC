
' -----------------------------------------------------------------------
' 差异块：连续相同类型差异项的分组，用于统一差异格式输出
' -----------------------------------------------------------------------
''' <summary>
''' 表示一个差异块，即连续相同类型差异项的分组。
''' </summary>
Public Class DiffBlock
    ''' <summary>该块中旧序列的起始行号（从 1 开始，用于显示）。</summary>
    Public Property OldStart As Integer

    ''' <summary>该块中旧序列的行数。</summary>
    Public Property OldCount As Integer

    ''' <summary>该块中新序列的起始行号（从 1 开始，用于显示）。</summary>
    Public Property NewStart As Integer

    ''' <summary>该块中新序列的行数。</summary>
    Public Property NewCount As Integer

    ''' <summary>该块包含的差异项列表。</summary>
    Public Property Items As New List(Of DiffItem)()

    Public Sub New(oldStart As Integer, oldCount As Integer,
                   newStart As Integer, newCount As Integer)
        Me.OldStart = oldStart
        Me.OldCount = oldCount
        Me.NewStart = newStart
        Me.NewCount = newCount
    End Sub
End Class