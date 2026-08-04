' ============================================================================
' Block.vb - Markdown Block 模型（兼容用户现有 JSONSchema.Block）
'
' 这是用户现有 Block 类的简化版本，去除了对外部库的依赖。
' 属性与 JSONSchema.Block 完全一致，可无缝替换。
' ============================================================================

Namespace JSONSchema

    ''' <summary>
    ''' 针对markdown格式有限的支持。表示一个 block 级元素。
    ''' </summary>
    Public Class Block

        ''' <summary>
        ''' 块级语法类型。支持: heading(h) / paragraph(p) / code / list(li) /
        ''' blockquote / table / hr / image(img) / html(raw) / math /
        ''' link / tasklist / footnote / deflist
        ''' </summary>
        Public Property type As String

        ''' <summary>heading level if type = heading (1-6)。</summary>
        Public Property level As Integer

        ''' <summary>heading/paragraph/code/blockquote 的文本内容。</summary>
        Public Property content As String

        ''' <summary>code block 的语言标识: bash/r/vbnet/python 等。</summary>
        Public Property language As String

        ''' <summary>list 是否有序。</summary>
        Public Property ordered As Boolean

        ''' <summary>list 的条目数组。</summary>
        Public Property items As String()

        ''' <summary>table 的表头。</summary>
        Public Property headers As String()

        ''' <summary>table 表头对齐: left|right|center。</summary>
        Public Property alignments As String()

        ''' <summary>table 的数据行，每行是单元格数组。</summary>
        Public Property rows As String()()

        ''' <summary>image 的源 URL 或文件路径。</summary>
        Public Property url As String

        ''' <summary>image 的替代文本。</summary>
        Public Property alt As String

        ''' <summary>image/link 的标题（悬停提示）。</summary>
        Public Property title As String

        ''' <summary>tasklist: 每项是否勾选。Nothing = 全部未勾选。</summary>
        Public Property checked As Boolean()

        ''' <summary>footnote 的唯一标识。</summary>
        Public Property id As String

        ''' <summary>deflist 的术语列表。</summary>
        Public Property terms As String()

        ''' <summary>deflist 的定义列表，与 terms 平行。</summary>
        Public Property definitions As String()

    End Class

End Namespace
