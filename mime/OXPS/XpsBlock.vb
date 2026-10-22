' ============================================================================
' XpsBlock.vb - 内容块模型与文本度量
'
' XpsDocument 的写入 API 只做两件事：把内容与样式快照收集为 XpsBlock 队列、
' 把样式登记到 XpsTextStyle。真正的换行、分页与绘制发生在 XpsLayoutEngine。
' 样式必须在入队时 Clone，避免后续 XxxStyle() 调用回溯影响已写入的内容。
' ============================================================================

''' <summary>XPS 文档内容块类型。</summary>
Public Enum XpsBlockType
    ''' <summary>文档大标题（封面标题）。</summary>
    Title
    ''' <summary>分级标题（1-6）。</summary>
    Heading
    ''' <summary>正文段落。</summary>
    Paragraph
    ''' <summary>代码块（保留原始换行）。</summary>
    Code
    ''' <summary>引用块。</summary>
    Quote
    ''' <summary>有序 / 无序列表。</summary>
    List
    ''' <summary>任务列表（[x] / [ ]）。</summary>
    TaskList
    ''' <summary>定义列表（术语 + 定义）。</summary>
    DefList
    ''' <summary>水平分割线。</summary>
    Hr
    ''' <summary>分页符。</summary>
    PageBreak
    ''' <summary>静态目录占位。</summary>
    Toc
    ''' <summary>表格。</summary>
    Table
    ''' <summary>图片。</summary>
    Image
End Enum

''' <summary>
''' 文本样式快照。所有尺寸单位为磅（pt），<see cref="Indent"/> 为左缩进。
''' </summary>
Public Class XpsTextStyle

    ''' <summary>字体族名（仅用于元信息与回退渲染，包内实际字体由 XpsFontResource 决定）。</summary>
    Public Property FontFamily As String = "SimHei"

    ''' <summary>字号（磅）。</summary>
    Public Property Size As Double = 10.5

    ''' <summary>是否加粗。</summary>
    Public Property Bold As Boolean = False

    ''' <summary>是否斜体。</summary>
    Public Property Italic As Boolean = False

    ''' <summary>文字颜色（#RRGGBB）。</summary>
    Public Property ForeColor As String = "#000000"

    ''' <summary>段前间距（磅）。</summary>
    Public Property SpaceBefore As Double = 0

    ''' <summary>段后间距（磅）。</summary>
    Public Property SpaceAfter As Double = 0

    ''' <summary>行距倍数。</summary>
    Public Property LineSpacing As Double = 1.4

    ''' <summary>左缩进（磅）。</summary>
    Public Property Indent As Double = 0

    ''' <summary>对齐方式：left / center / right / justify。</summary>
    Public Property Alignment As String = "left"

    ''' <summary>行高（磅）：字号 × 行距。</summary>
    Public ReadOnly Property LineHeight As Double
        Get
            Return Size * If(LineSpacing <= 0, 1.2, LineSpacing)
        End Get
    End Property

    ''' <summary>复制一份样式快照。</summary>
    Public Function Clone() As XpsTextStyle
        Return New XpsTextStyle With {
            .FontFamily = FontFamily,
            .Size = Size,
            .Bold = Bold,
            .Italic = Italic,
            .ForeColor = ForeColor,
            .SpaceBefore = SpaceBefore,
            .SpaceAfter = SpaceAfter,
            .LineSpacing = LineSpacing,
            .Indent = Indent,
            .Alignment = Alignment
        }
    End Function

End Class

''' <summary>单个文档内容块：承载文本、级别、表格/图片数据与样式快照。</summary>
Public Class XpsBlock

    ''' <summary>块类型。</summary>
    Public Property Type As XpsBlockType

    ''' <summary>主文本（标题/段落/代码/引用/图注等）。</summary>
    Public Property Text As String

    ''' <summary>标题级别（1-6），其它块忽略。</summary>
    Public Property Level As Integer = 1

    ''' <summary>列表是否有序。</summary>
    Public Property Ordered As Boolean = False

    ''' <summary>任务列表勾选状态（与 <see cref="Items"/> 平行）。</summary>
    Public Property Checked As Boolean()

    ''' <summary>列表项 / 任务列表项文本。</summary>
    Public Property Items As String()

    ''' <summary>定义列表术语。</summary>
    Public Property Terms As String()

    ''' <summary>定义列表定义。</summary>
    Public Property Definitions As String()

    ''' <summary>表格表头。</summary>
    Public Property TableHeaders As String()

    ''' <summary>表格数据行。</summary>
    Public Property TableRows As String()()

    ''' <summary>表格逐列对齐方式。</summary>
    Public Property TableAlignments As String()

    ''' <summary>图片文件路径。</summary>
    Public Property ImagePath As String

    ''' <summary>已解析的图片资源（由 XpsDocument 在入队时准备好，排版阶段直接使用）。</summary>
    Public Property ImageResource As XpsImageResource

    ''' <summary>图片显示宽度（磅），0 表示按原始像素尺寸换算。</summary>
    Public Property ImageWidth As Double = 0

    ''' <summary>图片显示高度（磅），0 表示按原始像素尺寸换算。</summary>
    Public Property ImageHeight As Double = 0

    ''' <summary>图片图注。</summary>
    Public Property ImageCaption As String

    ''' <summary>样式快照（入队时 Clone）。</summary>
    Public Property Style As XpsTextStyle

End Class

''' <summary>
''' XPS 文本度量：XPS 的 Glyphs 需要显式坐标，因此换行必须在布局前用近似
''' 字宽计算完成。CJK / 全角字符按 1 em，ASCII 按 0.5 em（Helvetica 平均宽度）。
''' </summary>
Public Module XpsTextMetrics

    ''' <summary>制表符按 4 个空格（0.5 em × 4）计算。</summary>
    Public Const TabWidth As Double = 2.0

    ''' <summary>行距倍数下限（防止样式被设为 0 时行与行重叠）。</summary>
    Public Const MinLineSpacing As Double = 1.0

    ''' <summary>
    ''' 判断字符是否按全角宽度排版：CJK 统一表意文字、全角标点、平假名/片假名、
    ''' 谚文与全角字母数字。
    ''' </summary>
    ''' <param name="c"></param>
    ''' <returns></returns>
    Public Function IsWide(ByVal c As Char) As Boolean
        Dim code As Integer = AscW(c)

        If code < 0 Then code += &H10000

        Return (code >= &H1100 AndAlso code <= &H115F) OrElse
               (code >= &H2E80 AndAlso code <= &HA4CF) OrElse
               (code >= &HAC00 AndAlso code <= &HD7A3) OrElse
               (code >= &HF900 AndAlso code <= &HFAFF) OrElse
               (code >= &HFE30 AndAlso code <= &HFE4F) OrElse
               (code >= &HFF00 AndAlso code <= &HFF60) OrElse
               (code >= &HFFE0 AndAlso code <= &HFFE6)
    End Function

    ''' <summary>
    ''' 单个字符的宽度（em 单位）。
    ''' </summary>
    ''' <param name="c"></param>
    ''' <returns></returns>
    Public Function CharWidth(ByVal c As Char) As Double
        If c = ControlChars.Tab Then
            Return TabWidth
        End If
        If IsWide(c) Then
            Return 1.0
        End If

        Return 0.5
    End Function

    ''' <summary>
    ''' 测量一段文本在指定字号下的宽度（磅）。
    ''' </summary>
    ''' <param name="text"></param>
    ''' <param name="size"></param>
    ''' <returns></returns>
    Public Function MeasureText(text As String, size As Double) As Double
        If String.IsNullOrEmpty(text) Then
            Return 0
        End If

        Dim total As Double = 0

        For Each c As Char In text
            total += CharWidth(c)
        Next

        Return total * size
    End Function

    ''' <summary>
    ''' 按最大宽度对文本做贪心折行。文本中已有的换行符（CR/LF/CRLF）会被保留为
    ''' 强制换行；空行会被保留（返回空字符串元素）。
    ''' </summary>
    ''' <param name="text">待折行的文本。</param>
    ''' <param name="size">字号（磅）。</param>
    ''' <param name="maxWidth">版心宽度（磅），小于等于 0 时不折行。</param>
    ''' <returns>折行后的文本行。</returns>
    Public Function WrapText(text As String, size As Double, maxWidth As Double) As String()
        Return WrapText(text, size, maxWidth, 0, 0)
    End Function

    ''' <summary>
    ''' 按最大宽度折行，支持悬挂缩进：首行可用宽度为 width - firstIndent，
    ''' 其余行为 width - restIndent（用于列表项的项目符号悬挂对齐）。
    ''' </summary>
    ''' <param name="text">待折行的文本。</param>
    ''' <param name="size">字号（磅）。</param>
    ''' <param name="width">可用总宽度（磅）。</param>
    ''' <param name="firstIndent">首行缩进（磅）。</param>
    ''' <param name="restIndent">续行缩进（磅）。</param>
    ''' <returns>折行后的文本行。</returns>
    Public Function WrapText(text As String, size As Double, width As Double,
                             firstIndent As Double, restIndent As Double) As String()
        If text Is Nothing Then
            Return New String() {}
        End If

        Dim lines As New List(Of String)()
        Dim paragraphs As String() = text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)

        For Each paragraph As String In paragraphs
            If width <= 0 Then
                Call lines.Add(paragraph)
                Continue For
            End If

            Dim current As New System.Text.StringBuilder()
            Dim used As Double = 0
            Dim isFirst As Boolean = True

            For Each c As Char In paragraph
                Dim limit As Double = width - If(isFirst, firstIndent, restIndent)
                Dim w As Double = CharWidth(c) * size

                If used + w > limit AndAlso current.Length > 0 Then
                    Call lines.Add(current.ToString())
                    Call current.Clear()
                    used = 0
                    isFirst = False
                End If

                Call current.Append(c)
                used += w
            Next

            Call lines.Add(current.ToString())
        Next

        Return lines.ToArray()
    End Function

End Module
