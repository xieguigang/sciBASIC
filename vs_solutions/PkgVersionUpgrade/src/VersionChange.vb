
''' <summary>
''' 单个版本元素的变更记录，用于 dry-run 预览以及最终的统计汇总
''' </summary>
Public Class VersionChange

    ''' <summary>版本元素的名称，例如 Version / AssemblyVersion / FileVersion</summary>
    Public Property Name As String
    ''' <summary>写入之前的值，元素原本不存在时为空字符串</summary>
    Public Property OldValue As String
    ''' <summary>本次计算出的新值</summary>
    Public Property NewValue As String
    ''' <summary>该元素是否是本次新建出来的</summary>
    Public Property Inserted As Boolean

    Public ReadOnly Property Changed As Boolean
        Get
            Return Inserted OrElse Not String.Equals(OldValue, NewValue, StringComparison.Ordinal)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Dim from As String = If(Inserted, "<none>", If(String.IsNullOrEmpty(OldValue), "<empty>", OldValue))
        Return $"{Name}: {from} -> {NewValue}"
    End Function

End Class