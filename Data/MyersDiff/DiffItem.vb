

' -----------------------------------------------------------------------
' 差异项：记录单个编辑操作
' -----------------------------------------------------------------------
''' <summary>
''' 表示一个差异项，包含编辑类型、旧序列索引和新序列索引。
''' </summary>
Public Class DiffItem
    ''' <summary>编辑操作类型。</summary>
    Public Property Type As EditType

    ''' <summary>旧序列中的索引（从 0 开始）；对于 Insert 操作为 -1。</summary>
    Public Property OldIndex As Integer

    ''' <summary>新序列中的索引（从 0 开始）；对于 Delete 操作为 -1。</summary>
    Public Property NewIndex As Integer

    ''' <summary>涉及的元素值（行文本或字符）。</summary>
    Public Property Value As String

    Public Sub New(type As EditType, oldIndex As Integer, newIndex As Integer, value As String)
        Me.Type = type
        Me.OldIndex = oldIndex
        Me.NewIndex = newIndex
        Me.Value = value
    End Sub

    Public Overrides Function ToString() As String
        Select Case Type
            Case EditType.Equal
                Return String.Format("  {0}", Value)
            Case EditType.Delete
                Return String.Format("- {0} [old:{1}]", Value, OldIndex)
            Case EditType.Insert
                Return String.Format("+ {0} [new:{1}]", Value, NewIndex)
            Case Else
                Return Value
        End Select
    End Function
End Class