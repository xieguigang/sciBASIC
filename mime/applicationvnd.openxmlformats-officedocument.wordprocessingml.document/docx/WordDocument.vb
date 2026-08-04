' ============================================================================
' WordDocument.vb - Word 文档生成器主类
'
' 流式 API 设计：
'   Dim docx As New WordDocument(author:="张三", title:="报告", tags:={"报告"})
'   docx.HeadingStyle(1, New WordStyle With {.ForeColor = "0000FF", .Size = 25})
'       .ParagraphStyle(New WordStyle With {.Size = 12})
'       .Title("文档标题")
'       .Toc()
'       .H1("第一章 概述")
'       .H2("1.1 背景")
'       .Paragraph("正文内容...")
'       .Table({"列1", "列2"}, {{"a", "b"}, {"c", "d"}})
'       .Image("chart.png", caption:="图1")
'       .PageBreak()
'       .Save("output.docx")
'
' 内部维护一个 StringBuilder 收集 body XML 片段，
' Save 时由 DocxPackager 组装完整的 .docx 包。
' ============================================================================

Imports System.Drawing
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports std = System.Math

''' <summary>
''' Word 文档生成器。
''' 支持通过流式 API 构建 docx 文档，包括标题、段落、表格、图片、
''' 目录(TOC)、分页符、代码块、引用、列表等。
''' </summary>
Public Class WordDocument

    ' === 元数据 ===
    Public Property Author As String = ""
    Public Property Title As String = ""
    Public Property Subject As String = ""
    Public Property Description As String = ""
    Public Property Tags As String() = {}
    Public Property ApplicationName As String = "VB.NET WordDocument Generator"

    ' === 页面设置 (twips) ===
    Private _pageWidth As Integer = 11906   ' A4 宽
    Private _pageHeight As Integer = 16838  ' A4 高
    Private _marginTop As Integer = 1440    ' 1 英寸
    Private _marginRight As Integer = 1440
    Private _marginBottom As Integer = 1440
    Private _marginLeft As Integer = 1440

    ' === 样式 ===
    Private _defaultStyle As New WordStyle()
    Private _headingStyles(5) As WordStyle  ' 索引 0-5 对应 H1-H6
    Private _paragraphStyle As WordStyle
    Private _tableStyle As New TableStyle()
    Private _codeStyle As WordStyle
    Private _blockquoteStyle As WordStyle
    Private _titleStyle As WordStyle

    ' === 内部状态 ===
    Private _body As New StringBuilder()
    Private _images As New List(Of ImageEntry)()
    Private _imageRelIdCounter As Integer = 2  ' rId1=styles, rId2=settings
    Private _imageIdCounter As Integer = 0

    ''' <summary>图像嵌入信息。</summary>
    Friend Class ImageEntry
        Public Property RelId As String
        Public Property Extension As String
        Public Property Data As Byte()
        Public Property WidthEmu As Integer
        Public Property HeightEmu As Integer
    End Class

    ' === 构造函数 ===

    Public Sub New(Optional author As String = "",
                   Optional title As String = "",
                   Optional tags As String() = Nothing,
                   Optional subject As String = "",
                   Optional description As String = "")
        Me.Author = author
        Me.Title = title
        If tags IsNot Nothing Then Me.Tags = tags
        Me.Subject = subject
        Me.Description = description

        ' 默认标题样式
        _headingStyles(0) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 24, .Bold = True, .ForeColor = WordColors.Heading1Color, .SpaceBefore = 12, .SpaceAfter = 6}
        _headingStyles(1) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 22, .Bold = True, .ForeColor = WordColors.Heading2Color, .SpaceBefore = 10, .SpaceAfter = 6}
        _headingStyles(2) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 20, .Bold = True, .ForeColor = WordColors.Heading3Color, .SpaceBefore = 10, .SpaceAfter = 4}
        _headingStyles(3) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 18, .Bold = True, .ForeColor = WordColors.Heading1Color, .SpaceBefore = 8, .SpaceAfter = 4}
        _headingStyles(4) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 16, .Bold = True, .ForeColor = WordColors.Heading2Color, .SpaceBefore = 6, .SpaceAfter = 4}
        _headingStyles(5) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 14, .Bold = True, .ForeColor = WordColors.Heading2Color, .SpaceBefore = 6, .SpaceAfter = 2}

        ' 默认正文样式
        _paragraphStyle = New WordStyle With {.FontName = "Calibri", .FontNameEastAsia = "Microsoft YaHei", .Size = 11, .LineSpacing = 1.5, .SpaceAfter = 6}

        ' 默认代码样式
        _codeStyle = New WordStyle With {.FontName = "Consolas", .FontNameEastAsia = "Consolas", .Size = 10, .ForeColor = WordColors.DarkGray, .BackColor = WordColors.CodeBg, .SpaceBefore = 6, .SpaceAfter = 6}

        ' 默认引用样式
        _blockquoteStyle = New WordStyle With {.FontName = "Calibri", .FontNameEastAsia = "Microsoft YaHei", .Size = 11, .Italic = True, .ForeColor = WordColors.DarkGray, .BackColor = WordColors.QuoteBg, .SpaceBefore = 6, .SpaceAfter = 6}

        ' 默认标题样式
        _titleStyle = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 36, .Bold = True, .ForeColor = WordColors.DarkBlue, .Alignment = "center", .SpaceAfter = 12}
    End Sub

    ' ========================================================================
    ' 样式设置 (流式 API)
    ' ========================================================================

    ''' <summary>设置指定级别的标题样式 (level 1-6)。</summary>
    Public Function HeadingStyle(level As Integer, style As WordStyle) As WordDocument
        If level >= 1 AndAlso level <= 6 Then
            _headingStyles(level - 1) = style
        End If
        Return Me
    End Function

    ''' <summary>设置正文段落样式。</summary>
    Public Function ParagraphStyle(style As WordStyle) As WordDocument
        _paragraphStyle = style
        Return Me
    End Function

    ''' <summary>设置文档默认样式。</summary>
    Public Function DefaultStyle(style As WordStyle) As WordDocument
        _defaultStyle = style
        Return Me
    End Function

    ''' <summary>设置表格样式。</summary>
    Public Function TableStyle(style As TableStyle) As WordDocument
        _tableStyle = style
        Return Me
    End Function

    ''' <summary>设置代码块样式。</summary>
    Public Function CodeStyle(style As WordStyle) As WordDocument
        _codeStyle = style
        Return Me
    End Function

    ''' <summary>设置引用块样式。</summary>
    Public Function BlockquoteStyle(style As WordStyle) As WordDocument
        _blockquoteStyle = style
        Return Me
    End Function

    ''' <summary>设置文档标题样式。</summary>
    Public Function TitleStyle(style As WordStyle) As WordDocument
        _titleStyle = style
        Return Me
    End Function

    ''' <summary>设置页面尺寸和边距 (twips)。</summary>
    Public Function PageSetup(pageWidth As Integer, pageHeight As Integer,
                              marginTop As Integer, marginRight As Integer,
                              marginBottom As Integer, marginLeft As Integer) As WordDocument
        _pageWidth = pageWidth
        _pageHeight = pageHeight
        _marginTop = marginTop
        _marginRight = marginRight
        _marginBottom = marginBottom
        _marginLeft = marginLeft
        Return Me
    End Function

    ''' <summary>A4 纸张，1 英寸边距。</summary>
    Public Function PageSetupA4() As WordDocument
        Return PageSetup(11906, 16838, 1440, 1440, 1440, 1440)
    End Function

    ''' <summary>Letter 纸张，1 英寸边距。</summary>
    Public Function PageSetupLetter() As WordDocument
        Return PageSetup(12240, 15840, 1440, 1440, 1440, 1440)
    End Function

    ' ========================================================================
    ' 内容写入方法 (流式 API，均返回 Me)
    ' ========================================================================

    ''' <summary>写入文档标题（居中大字号，非 heading 样式）。</summary>
    Public Function DocTitle(text As String) As WordDocument
        Dim s As WordStyle = _titleStyle
        _body.Append("<w:p><w:pPr>")
        _body.Append($"<w:spacing w:before=""{PtToTwip(s.SpaceBefore)}"" w:after=""{PtToTwip(s.SpaceAfter)}"" w:line=""{CInt(s.LineSpacing * 240)}"" w:lineRule=""auto""/>")
        _body.Append($"<w:jc w:val=""{s.Alignment}""/>")
        If s.BackColor <> "" Then _body.Append($"<w:shd w:val=""clear"" w:color=""auto"" w:fill=""{s.BackColor}""/>")
        _body.Append("</w:pPr><w:r><w:rPr>")
        _body.Append($"<w:rFonts w:ascii=""{s.FontName}"" w:eastAsia=""{s.FontNameEastAsia}"" w:hAnsi=""{s.FontName}""/>")
        If s.Bold Then _body.Append("<w:b/>")
        If s.Italic Then _body.Append("<w:i/>")
        If s.Underline Then _body.Append("<w:u w:val=""single""/>")
        _body.Append($"<w:color w:val=""{s.ForeColor}""/>")
        _body.Append($"<w:sz w:val=""{CInt(s.Size * 2)}""/>")
        _body.Append($"<w:szCs w:val=""{CInt(s.Size * 2)}""/>")
        _body.Append("</w:rPr>")
        _body.Append($"<w:t xml:space=""preserve"">{XEsc(text)}</w:t></w:r></w:p>")
        Return Me
    End Function

    ''' <summary>写入一级标题。</summary>
    Public Function H1(text As String) As WordDocument
        Return Heading(1, text)
    End Function

    ''' <summary>写入二级标题。</summary>
    Public Function H2(text As String) As WordDocument
        Return Heading(2, text)
    End Function

    ''' <summary>写入三级标题。</summary>
    Public Function H3(text As String) As WordDocument
        Return Heading(3, text)
    End Function

    ''' <summary>写入四级标题。</summary>
    Public Function H4(text As String) As WordDocument
        Return Heading(4, text)
    End Function

    ''' <summary>写入五级标题。</summary>
    Public Function H5(text As String) As WordDocument
        Return Heading(5, text)
    End Function

    ''' <summary>写入六级标题。</summary>
    Public Function H6(text As String) As WordDocument
        Return Heading(6, text)
    End Function

    ''' <summary>写入指定级别的标题 (level 1-6)。</summary>
    Public Function Heading(level As Integer, text As String) As WordDocument
        If level < 1 Then level = 1
        If level > 6 Then level = 6
        Dim s As WordStyle = _headingStyles(level - 1)

        _body.Append("<w:p><w:pPr>")
        _body.Append($"<w:pStyle w:val=""Heading{level}""/>")
        _body.Append($"<w:spacing w:before=""{PtToTwip(s.SpaceBefore)}"" w:after=""{PtToTwip(s.SpaceAfter)}"" w:line=""{CInt(s.LineSpacing * 240)}"" w:lineRule=""auto""/>")
        If s.Alignment <> "left" Then _body.Append($"<w:jc w:val=""{s.Alignment}""/>")
        If s.BackColor <> "" Then _body.Append($"<w:shd w:val=""clear"" w:color=""auto"" w:fill=""{s.BackColor}""/>")
        _body.Append("</w:pPr><w:r><w:rPr>")
        _body.Append($"<w:rFonts w:ascii=""{s.FontName}"" w:eastAsia=""{s.FontNameEastAsia}"" w:hAnsi=""{s.FontName}""/>")
        If s.Bold Then _body.Append("<w:b/>")
        If s.Italic Then _body.Append("<w:i/>")
        _body.Append($"<w:color w:val=""{s.ForeColor}""/>")
        _body.Append($"<w:sz w:val=""{CInt(s.Size * 2)}""/>")
        _body.Append($"<w:szCs w:val=""{CInt(s.Size * 2)}""/>")
        _body.Append("</w:rPr>")
        _body.Append($"<w:t xml:space=""preserve"">{XEsc(text)}</w:t></w:r></w:p>")
        Return Me
    End Function

    ''' <summary>写入正文段落。</summary>
    Public Function Paragraph(text As String) As WordDocument
        Return Paragraph(text, _paragraphStyle)
    End Function

    ''' <summary>写入正文段落（指定样式）。</summary>
    Public Function Paragraph(text As String, style As WordStyle) As WordDocument
        _body.Append("<w:p><w:pPr>")
        _body.Append($"<w:spacing w:before=""{PtToTwip(style.SpaceBefore)}"" w:after=""{PtToTwip(style.SpaceAfter)}"" w:line=""{CInt(style.LineSpacing * 240)}"" w:lineRule=""auto""/>")
        If style.Alignment <> "left" Then _body.Append($"<w:jc w:val=""{style.Alignment}""/>")
        If style.FirstLineIndent > 0 Then _body.Append($"<w:ind w:firstLine=""{PtToTwip(style.FirstLineIndent)}""/>")
        If style.BackColor <> "" Then _body.Append($"<w:shd w:val=""clear"" w:color=""auto"" w:fill=""{style.BackColor}""/>")
        _body.Append("</w:pPr>")

        ' 支持多行文本
        Dim lines As String() = text.Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
        For i As Integer = 0 To lines.Length - 1
            If i > 0 Then
                _body.Append("<w:r><w:br/></w:r>")
            End If
            _body.Append("<w:r><w:rPr>")
            _body.Append($"<w:rFonts w:ascii=""{style.FontName}"" w:eastAsia=""{style.FontNameEastAsia}"" w:hAnsi=""{style.FontName}""/>")
            If style.Bold Then _body.Append("<w:b/>")
            If style.Italic Then _body.Append("<w:i/>")
            If style.Underline Then _body.Append("<w:u w:val=""single""/>")
            _body.Append($"<w:color w:val=""{style.ForeColor}""/>")
            _body.Append($"<w:sz w:val=""{CInt(style.Size * 2)}""/>")
            _body.Append("</w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(lines(i))}</w:t></w:r>")
        Next

        _body.Append("</w:p>")
        Return Me
    End Function

    ''' <summary>写入代码块（等宽字体，灰色背景）。</summary>
    Public Function CodeBlock(code As String, Optional language As String = "") As WordDocument
        Dim s As WordStyle = _codeStyle
        _body.Append("<w:p><w:pPr>")
        _body.Append($"<w:spacing w:before=""{PtToTwip(s.SpaceBefore)}"" w:after=""{PtToTwip(s.SpaceAfter)}"" w:line=""240"" w:lineRule=""auto""/>")
        If s.BackColor <> "" Then _body.Append($"<w:shd w:val=""clear"" w:color=""auto"" w:fill=""{s.BackColor}""/>")
        _body.Append("<w:pBdr>")
        _body.Append("<w:top w:val=""single"" w:sz=""4"" w:space=""4"" w:color=""D0D0D0""/>")
        _body.Append("<w:bottom w:val=""single"" w:sz=""4"" w:space=""4"" w:color=""D0D0D0""/>")
        _body.Append("<w:left w:val=""single"" w:sz=""4"" w:space=""4"" w:color=""D0D0D0""/>")
        _body.Append("<w:right w:val=""single"" w:sz=""4"" w:space=""4"" w:color=""D0D0D0""/>")
        _body.Append("</w:pBdr>")
        _body.Append("</w:pPr>")

        ' 代码内容（每行一个 run，用 <w:br/> 换行）
        Dim lines As String() = code.Split({vbCrLf, vbLf}, StringSplitOptions.None)
        For i As Integer = 0 To lines.Length - 1
            If i > 0 Then
                _body.Append("<w:r><w:br/></w:r>")
            End If
            _body.Append("<w:r><w:rPr>")
            _body.Append($"<w:rFonts w:ascii=""{s.FontName}"" w:eastAsia=""{s.FontNameEastAsia}"" w:hAnsi=""{s.FontName}""/>")
            _body.Append($"<w:color w:val=""{s.ForeColor}""/>")
            _body.Append($"<w:sz w:val=""{CInt(s.Size * 2)}""/>")
            _body.Append("</w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(lines(i))}</w:t></w:r>")
        Next

        _body.Append("</w:p>")
        Return Me
    End Function

    ''' <summary>写入引用块。</summary>
    Public Function Blockquote(text As String) As WordDocument
        Dim s As WordStyle = _blockquoteStyle
        _body.Append("<w:p><w:pPr>")
        _body.Append($"<w:spacing w:before=""{PtToTwip(s.SpaceBefore)}"" w:after=""{PtToTwip(s.SpaceAfter)}"" w:line=""{CInt(s.LineSpacing * 240)}"" w:lineRule=""auto""/>")
        _body.Append("<w:ind w:left=""720""/>")  ' 左缩进 0.5 英寸
        If s.BackColor <> "" Then _body.Append($"<w:shd w:val=""clear"" w:color=""auto"" w:fill=""{s.BackColor}""/>")
        _body.Append("<w:pBdr>")
        _body.Append("<w:left w:val=""single"" w:sz=""24"" w:space=""8"" w:color=""4472C4""/>")
        _body.Append("</w:pBdr>")
        _body.Append("</w:pPr><w:r><w:rPr>")
        _body.Append($"<w:rFonts w:ascii=""{s.FontName}"" w:eastAsia=""{s.FontNameEastAsia}"" w:hAnsi=""{s.FontName}""/>")
        If s.Italic Then _body.Append("<w:i/>")
        _body.Append($"<w:color w:val=""{s.ForeColor}""/>")
        _body.Append($"<w:sz w:val=""{CInt(s.Size * 2)}""/>")
        _body.Append("</w:rPr>")

        Dim lines As String() = text.Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
        For i As Integer = 0 To lines.Length - 1
            If i > 0 Then _body.Append("<w:br/>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(lines(i))}</w:t>")
        Next
        _body.Append("</w:r></w:p>")
        Return Me
    End Function

    ''' <summary>写入列表（有序或无序）。</summary>
    Public Function List(items As String(), Optional ordered As Boolean = False) As WordDocument
        Dim numFmt As String = If(ordered, "decimal", "bullet")
        Dim numId As Integer = If(ordered, 1, 2)

        For Each item As String In items
            _body.Append("<w:p><w:pPr>")
            _body.Append($"<w:numPr><w:ilvl w:val=""0""/><w:numId w:val=""{numId}""/></w:numPr>")
            _body.Append($"<w:spacing w:after=""60""/>")
            _body.Append("</w:pPr><w:r><w:rPr>")
            _body.Append($"<w:rFonts w:ascii=""{_paragraphStyle.FontName}"" w:eastAsia=""{_paragraphStyle.FontNameEastAsia}"" w:hAnsi=""{_paragraphStyle.FontName}""/>")
            _body.Append($"<w:sz w:val=""{CInt(_paragraphStyle.Size * 2)}""/>")
            _body.Append("</w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(item)}</w:t></w:r></w:p>")
        Next
        Return Me
    End Function

    ''' <summary>写入任务列表。</summary>
    Public Function TaskList(items As String(), checked As Boolean()) As WordDocument
        For i As Integer = 0 To items.Length - 1
            Dim isChecked As Boolean = (checked IsNot Nothing AndAlso i < checked.Length AndAlso checked(i))
            Dim sym As String = If(isChecked, "☒", "☐")
            _body.Append("<w:p><w:pPr><w:spacing w:after=""60""/></w:pPr>")
            _body.Append("<w:r><w:rPr><w:rFonts w:ascii=""Segoe UI Symbol"" w:hAnsi=""Segoe UI Symbol""/>")
            _body.Append($"<w:sz w:val=""{CInt(_paragraphStyle.Size * 2)}""/></w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{sym} </w:t></w:r>")
            _body.Append("<w:r><w:rPr>")
            _body.Append($"<w:rFonts w:ascii=""{_paragraphStyle.FontName}"" w:eastAsia=""{_paragraphStyle.FontNameEastAsia}"" w:hAnsi=""{_paragraphStyle.FontName}""/>")
            _body.Append($"<w:sz w:val=""{CInt(_paragraphStyle.Size * 2)}""/></w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(items(i))}</w:t></w:r></w:p>")
        Next
        Return Me
    End Function

    ''' <summary>写入定义列表。</summary>
    Public Function DefinitionList(terms As String(), definitions As String()) As WordDocument
        Dim n As Integer = std.Min(If(terms?.Length, 0), If(definitions?.Length, 0))
        For i As Integer = 0 To n - 1
            ' 术语 (粗体)
            _body.Append("<w:p><w:pPr><w:spacing w:before=""60"" w:after=""20""/></w:pPr>")
            _body.Append("<w:r><w:rPr>")
            _body.Append($"<w:rFonts w:ascii=""{_paragraphStyle.FontName}"" w:eastAsia=""{_paragraphStyle.FontNameEastAsia}"" w:hAnsi=""{_paragraphStyle.FontName}""/>")
            _body.Append("<w:b/>")
            _body.Append($"<w:sz w:val=""{CInt(_paragraphStyle.Size * 2)}""/></w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(terms(i))}</w:t></w:r></w:p>")

            ' 定义 (缩进)
            _body.Append("<w:p><w:pPr><w:spacing w:after=""60""/><w:ind w:left=""480""/></w:pPr>")
            _body.Append("<w:r><w:rPr>")
            _body.Append($"<w:rFonts w:ascii=""{_paragraphStyle.FontName}"" w:eastAsia=""{_paragraphStyle.FontNameEastAsia}"" w:hAnsi=""{_paragraphStyle.FontName}""/>")
            _body.Append($"<w:sz w:val=""{CInt(_paragraphStyle.Size * 2)}""/></w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(definitions(i))}</w:t></w:r></w:p>")
        Next
        Return Me
    End Function

    ''' <summary>写入水平分割线。</summary>
    Public Function Hr() As WordDocument
        _body.Append("<w:p><w:pPr><w:pBdr>")
        _body.Append("<w:bottom w:val=""single"" w:sz=""6"" w:space=""1"" w:color=""BFBFBF""/>")
        _body.Append("</w:pBdr></w:pPr></w:p>")
        Return Me
    End Function

    ''' <summary>插入分页符。</summary>
    Public Function PageBreak() As WordDocument
        _body.Append("<w:p><w:r><w:br w:type=""page""/></w:r></w:p>")
        Return Me
    End Function

    ''' <summary>插入目录 (TOC)。Word 打开时会自动更新目录。</summary>
    Public Function Toc(Optional maxLevel As Integer = 3) As WordDocument
        _body.Append("<w:p><w:pPr><w:pStyle w:val=""TOCHeading""/></w:pPr>")
        _body.Append("<w:r><w:rPr><w:b/><w:sz w:val=""28""/></w:rPr>")
        _body.Append("<w:t>目录</w:t></w:r></w:p>")

        _body.Append("<w:p><w:r><w:fldChar w:fldCharType=""begin""/></w:r>")
        _body.Append("<w:r><w:instrText xml:space=""preserve""> TOC \o ""1-")
        _body.Append(maxLevel.ToString())
        _body.Append(""" \h \z \u </w:instrText></w:r>")
        _body.Append("<w:r><w:fldChar w:fldCharType=""separate""/></w:r>")
        _body.Append("<w:r><w:rPr><w:color w:val=""808080""/><w:i/></w:rPr>")
        _body.Append("<w:t>右键此处选择「更新域」以生成目录</w:t></w:r>")
        _body.Append("<w:r><w:fldChar w:fldCharType=""end""/></w:r></w:p>")
        Return Me
    End Function

    ''' <summary>写入表格（二维数组形式）。</summary>
    Public Function Table(headers As String(), data As String(,)) As WordDocument
        Return Table(headers, data, Nothing)
    End Function

    ''' <summary>写入表格（二维数组形式，支持对齐方式）。</summary>
    Public Function Table(headers As String(), data As String(,), alignments As String()) As WordDocument
        Dim rows As New List(Of String())
        For i As Integer = 0 To data.GetLength(0) - 1
            Dim row(data.GetLength(1) - 1) As String
            For j As Integer = 0 To data.GetLength(1) - 1
                row(j) = data(i, j)
            Next
            rows.Add(row)
        Next
        Return Table(headers, rows.ToArray(), alignments)
    End Function

    ''' <summary>写入表格（交错数组形式，支持对齐方式）。</summary>
    Public Function Table(headers As String(), rows As String()(),
                          Optional alignments As String() = Nothing) As WordDocument
        Dim nCols As Integer = If(headers?.Length, 0)
        If nCols = 0 AndAlso rows?.Length > 0 Then
            nCols = If(rows(0)?.Length, 0)
        End If
        If nCols = 0 Then Return Me

        ' 计算列宽 (平均分配页面内容宽度)
        Dim contentWidth As Integer = _pageWidth - _marginLeft - _marginRight
        Dim colWidth As Integer = contentWidth \ nCols
        Dim ts As TableStyle = _tableStyle

        _body.Append("<w:tbl><w:tblPr>")
        _body.Append($"<w:tblW w:w=""{contentWidth}"" w:type=""dxa""/>")
        _body.Append("<w:tblBorders>")
        _body.Append($"<w:top w:val=""single"" w:sz=""{ts.BorderSize}"" w:color=""{ts.BorderColor}""/>")
        _body.Append($"<w:left w:val=""single"" w:sz=""{ts.BorderSize}"" w:color=""{ts.BorderColor}""/>")
        _body.Append($"<w:bottom w:val=""single"" w:sz=""{ts.BorderSize}"" w:color=""{ts.BorderColor}""/>")
        _body.Append($"<w:right w:val=""single"" w:sz=""{ts.BorderSize}"" w:color=""{ts.BorderColor}""/>")
        _body.Append($"<w:insideH w:val=""single"" w:sz=""{ts.BorderSize}"" w:color=""{ts.BorderColor}""/>")
        _body.Append($"<w:insideV w:val=""single"" w:sz=""{ts.BorderSize}"" w:color=""{ts.BorderColor}""/>")
        _body.Append("</w:tblBorders>")
        _body.Append("</w:tblPr>")

        ' 列定义
        _body.Append("<w:tblGrid>")
        For c As Integer = 0 To nCols - 1
            _body.Append($"<w:gridCol w:w=""{colWidth}""/>")
        Next
        _body.Append("</w:tblGrid>")

        ' 表头行
        If headers IsNot Nothing AndAlso headers.Length > 0 Then
            _body.Append("<w:tr><w:trPr><w:tblHeader/></w:trPr>")
            For c As Integer = 0 To nCols - 1
                _body.Append("<w:tc><w:tcPr>")
                _body.Append($"<w:tcW w:w=""{colWidth}"" w:type=""dxa""/>")
                _body.Append($"<w:shd w:val=""clear"" w:color=""auto"" w:fill=""{ts.HeaderBackColor}""/>")
                Dim align As String = GetAlign(alignments, c)
                _body.Append($"<w:vAlign w:val=""center""/></w:tcPr>")
                _body.Append("<w:p><w:pPr>")
                If align <> "left" Then _body.Append($"<w:jc w:val=""{align}""/>")
                _body.Append("</w:pPr><w:r><w:rPr>")
                _body.Append($"<w:rFonts w:ascii=""{_paragraphStyle.FontName}"" w:eastAsia=""{_paragraphStyle.FontNameEastAsia}"" w:hAnsi=""{_paragraphStyle.FontName}""/>")
                If ts.HeaderBold Then _body.Append("<w:b/>")
                _body.Append($"<w:color w:val=""{ts.HeaderForeColor}""/>")
                _body.Append($"<w:sz w:val=""{CInt(_paragraphStyle.Size * 2)}""/></w:rPr>")
                _body.Append($"<w:t xml:space=""preserve"">{XEsc(If(c < headers.Length, headers(c), ""))}</w:t></w:r></w:p></w:tc>")
            Next
            _body.Append("</w:tr>")
        End If

        ' 数据行
        For rIdx As Integer = 0 To rows.Length - 1
            Dim row As String() = rows(rIdx)
            _body.Append("<w:tr>")
            ' 交替行背景
            Dim rowBg As String = If(rIdx Mod 2 = 1 AndAlso ts.AltRowBackColor <> "", ts.AltRowBackColor, "")
            For c As Integer = 0 To nCols - 1
                _body.Append("<w:tc><w:tcPr>")
                _body.Append($"<w:tcW w:w=""{colWidth}"" w:type=""dxa""/>")
                If rowBg <> "" Then _body.Append($"<w:shd w:val=""clear"" w:color=""auto"" w:fill=""{rowBg}""/>")
                _body.Append("<w:vAlign w:val=""center""/></w:tcPr>")
                _body.Append("<w:p><w:pPr>")
                Dim align As String = GetAlign(alignments, c)
                If align <> "left" Then _body.Append($"<w:jc w:val=""{align}""/>")
                _body.Append("</w:pPr><w:r><w:rPr>")
                _body.Append($"<w:rFonts w:ascii=""{_paragraphStyle.FontName}"" w:eastAsia=""{_paragraphStyle.FontNameEastAsia}"" w:hAnsi=""{_paragraphStyle.FontName}""/>")
                _body.Append($"<w:sz w:val=""{CInt(_paragraphStyle.Size * 2)}""/></w:rPr>")
                _body.Append($"<w:t xml:space=""preserve"">{XEsc(If(c < If(row?.Length, 0), row(c), ""))}</w:t></w:r></w:p></w:tc>")
            Next
            _body.Append("</w:tr>")
        Next

        _body.Append("</w:tbl>")
        ' 表格后需要一个空段落
        _body.Append("<w:p/>")
        Return Me
    End Function

    ''' <summary>插入图片。</summary>
    ''' <param name="file">图片文件路径。</param>
    ''' <param name="width">指定宽度（磅），0 表示自动。</param>
    ''' <param name="height">指定高度（磅），0 表示自动。</param>
    ''' <param name="caption">图片说明文字。</param>
    Public Function Image(file As String,
                          Optional width As Double = 0,
                          Optional height As Double = 0,
                          Optional caption As String = "") As WordDocument
        If Not file.FileExists Then
            Console.Error.WriteLine($"[警告] 图片文件不存在: {file}")
            Return Me
        End If

        Dim ext As String = Path.GetExtension(file).TrimStart("."c).ToLower()
        Dim imgBytes As Byte() = file.ReadBinary
        Dim dims As Size = ImageHelper.ReadImageDimensions(file)

        ' 转换为 EMU (1 pixel @96DPI = 9525 EMU, 1 pt = 12700 EMU)
        Dim widthEmu As Integer = If(width > 0, CInt(width * 12700), dims.Width * 9525)
        Dim heightEmu As Integer = If(height > 0, CInt(height * 12700), dims.Height * 9525)

        ' 缩放以适应页面宽度 (1 twip = 635 EMU)
        Dim maxWEmu As Integer = (_pageWidth - _marginLeft - _marginRight) * 635
        If widthEmu > maxWEmu Then
            Dim scale As Double = CDbl(maxWEmu) / widthEmu
            widthEmu = maxWEmu
            heightEmu = CInt(heightEmu * scale)
        End If

        ' 注册图片关系
        _imageRelIdCounter += 1
        Dim relId As String = "rId" & _imageRelIdCounter.ToString()
        _imageIdCounter += 1
        Dim imgId As Integer = _imageIdCounter

        _images.Add(New ImageEntry With {
            .RelId = relId,
            .Extension = ext,
            .Data = imgBytes,
            .WidthEmu = widthEmu,
            .HeightEmu = heightEmu
        })

        ' 图片 XML
        _body.Append("<w:p><w:pPr><w:jc w:val=""center""/></w:pPr><w:r><w:drawing>")
        _body.Append("<wp:inline distT=""0"" distB=""0"" distL=""0"" distR=""0"">")
        _body.Append($"<wp:extent cx=""{widthEmu}"" cy=""{heightEmu}""/>")
        _body.Append($"<wp:docPr id=""{imgId}"" name=""Picture {imgId}""/>")
        _body.Append("<a:graphic><a:graphicData uri=""http://schemas.openxmlformats.org/drawingml/2006/picture"">")
        _body.Append("<pic:pic><pic:blipFill><a:blip r:embed=""")
        _body.Append(relId & """/>")
        _body.Append("<a:stretch><a:fillRect/></a:stretch></pic:blipFill>")
        _body.Append("<pic:spPr><a:xfrm><a:off x=""0"" y=""0""/>")
        _body.Append($"<a:ext cx=""{widthEmu}"" cy=""{heightEmu}""/></a:xfrm>")
        _body.Append("<a:prstGeom prst=""rect""><a:avLst/></a:prstGeom></pic:spPr></pic:pic>")
        _body.Append("</a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>")

        ' 图注
        If caption <> "" Then
            _body.Append("<w:p><w:pPr><w:jc w:val=""center""/>")
            _body.Append("<w:spacing w:after=""120""/></w:pPr>")
            _body.Append("<w:r><w:rPr><w:rFonts w:eastAsia=""Microsoft YaHei""/>")
            _body.Append("<w:sz w:val=""18""/><w:i/><w:color w:val=""808080""/></w:rPr>")
            _body.Append($"<w:t xml:space=""preserve"">{XEsc(caption)}</w:t></w:r></w:p>")
        End If

        Return Me
    End Function

    ' ========================================================================
    ' Block 模型兼容
    ' ========================================================================

    ''' <summary>
    ''' 将 Block 列表写入文档（兼容用户现有 JSONSchema.Block 模型）。
    ''' </summary>
    Public Function WriteBlocks(blocks As IEnumerable(Of JSONSchema.Block)) As WordDocument
        If blocks Is Nothing Then Return Me
        For Each b As JSONSchema.Block In blocks
            If b Is Nothing Then Continue For
            WriteBlock(b)
        Next
        Return Me
    End Function

    Private Sub WriteBlock(b As JSONSchema.Block)
        Dim t As String = If(b.type, "").ToLower()

        Select Case t
            Case "heading", "h"
                Heading(If(b.level < 1, 1, If(b.level > 6, 6, b.level)), If(b.content, ""))
            Case "paragraph", "p"
                Paragraph(If(b.content, ""))
            Case "code"
                CodeBlock(If(b.content, ""), If(b.language, ""))
            Case "list", "li"
                List(If(b.items, New String() {}), b.ordered)
            Case "blockquote"
                Blockquote(If(b.content, ""))
            Case "table"
                Table(If(b.headers, New String() {}), If(b.rows, New String()() {}), b.alignments)
            Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                Hr()
            Case "image", "img"
                If Not String.IsNullOrEmpty(b.url) AndAlso File.Exists(b.url) Then
                    Image(b.url, caption:=If(b.alt, ""))
                Else
                    Paragraph($"[图片: {If(b.alt, b.url)}]")
                End If
            Case "html", "raw"
                Paragraph(If(b.content, ""))
            Case "math", "equation", "tex", "latex"
                CodeBlock(If(b.content, ""), "latex")
            Case "link", "a"
                Paragraph($"[{If(b.alt, "")}]({If(b.url, "")})")
            Case "tasklist", "tasks", "todo"
                TaskList(If(b.items, New String() {}), b.checked)
            Case "footnote", "note"
                Paragraph($"[{If(b.id, "")}] {If(b.content, "")}")
            Case "deflist", "definition", "dl"
                DefinitionList(If(b.terms, New String() {}), If(b.definitions, New String() {}))
            Case Else
                Paragraph(If(b.content, ""))
        End Select
    End Sub

    ' ========================================================================
    ' 保存
    ' ========================================================================

    ''' <summary>保存为 .docx 文件。</summary>
    Public Sub Save(filePath As String)
        Dim packager As New DocxPackager()
        packager.Save(Me, filePath)
    End Sub

    ' ========================================================================
    ' 内部访问器（供 DocxPackager 使用）
    ' ========================================================================

    Friend Function GetBodyXml() As String
        Return _body.ToString()
    End Function

    Friend Function GetImages() As List(Of ImageEntry)
        Return _images
    End Function

    Friend Function GetDefaultStyle() As WordStyle
        Return _defaultStyle
    End Function

    Friend Function GetHeadingStyles() As WordStyle()
        Return _headingStyles
    End Function

    Friend Function GetParagraphStyle() As WordStyle
        Return _paragraphStyle
    End Function

    Friend Function GetCodeStyle() As WordStyle
        Return _codeStyle
    End Function

    Friend Function GetBlockquoteStyle() As WordStyle
        Return _blockquoteStyle
    End Function

    Friend Function GetTitleStyle() As WordStyle
        Return _titleStyle
    End Function

    Friend Function GetTableStyle() As TableStyle
        Return _tableStyle
    End Function

    Friend Function GetPageWidth() As Integer
        Return _pageWidth
    End Function

    Friend Function GetPageHeight() As Integer
        Return _pageHeight
    End Function

    Friend Function GetMargins() As (Top As Integer, Right As Integer, Bottom As Integer, Left As Integer)
        Return (_marginTop, _marginRight, _marginBottom, _marginLeft)
    End Function

    ' ========================================================================
    ' 辅助函数
    ' ========================================================================

    Private Shared Function PtToTwip(pt As Double) As Integer
        Return CInt(pt * 20)
    End Function

    Private Shared Function XEsc(text As String) As String
        If String.IsNullOrEmpty(text) Then Return ""
        Return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
    End Function

    Private Shared Function GetAlign(alignments As String(), i As Integer) As String
        If alignments Is Nothing OrElse i >= alignments.Length Then Return "left"
        Select Case alignments(i).ToLower()
            Case "left" : Return "left"
            Case "center" : Return "center"
            Case "right" : Return "right"
            Case Else : Return "left"
        End Select
    End Function

End Class
