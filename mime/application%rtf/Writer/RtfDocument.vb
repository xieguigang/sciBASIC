' ============================================================================
' RtfDocument.vb - RTF 文档生成器主类
'
' 实现 IDocumentWriter，提供与 WordDocument(docx) / PdfDocument(pdf) 完全一致的
' 流式 API 与同形构造函数。调用时仅收集 RtfBlock 内容块与样式状态；
' Save 时由 RtfSerializer 序列化为符合 RTF 1.x 规范的 .rtf 文件。
'
' 所有写入方法返回 RtfDocument 以支持链式调用；接口成员另以 IDW_ 前缀的
' 私有函数显式实现并转发，这是 docx / pdf 两个既有实现共同遵循的仓库惯例。
'
' 页面尺寸以 twips 入参（1 pt = 20 twips），默认 A4 纸张、1 英寸边距。
' 图片文件缺失时走 [警告] 日志并跳过，不影响其余内容生成。
' ============================================================================

Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.text.markdown

''' <summary>
''' RTF 文档生成器。实现 <see cref="IDocumentWriter"/>，对外提供与
''' <c>WordDocument</c> / <c>PdfDocument</c> 完全一致的编程接口，
''' 将文本、表格、图片写入并生成 .rtf 文件。
''' </summary>
''' <remarks>
''' 该类不改动既有的 <see cref="Rtf"/> 文档对象模型，两者相互独立：
''' <see cref="Rtf"/> 面向"逐步追加带样式的文本区域"，
''' <see cref="RtfDocument"/> 面向"结构化文档写入接口"。
''' </remarks>
Public Class RtfDocument : Implements IDocumentWriter

    ' ==== 元数据（与 WordDocument / PdfDocument 同形同义） ====

    ''' <summary>文档作者。</summary>
    Public Property Author As String = "" Implements IDocumentWriter.Author
    ''' <summary>文档标题。</summary>
    Public Property Title As String = "" Implements IDocumentWriter.Title
    ''' <summary>文档主题。</summary>
    Public Property Subject As String = "" Implements IDocumentWriter.Subject
    ''' <summary>文档描述。</summary>
    Public Property Description As String = "" Implements IDocumentWriter.Description
    ''' <summary>文档标签。</summary>
    Public Property Tags As String() = Nothing Implements IDocumentWriter.Tags
    ''' <summary>生成该文档的应用程序名称。</summary>
    Public Property ApplicationName As String = "VB.NET RtfDocument Generator" Implements IDocumentWriter.ApplicationName

    ' ==== 页面尺寸（twips） ====

    Private _pageWidth As Integer = 11906
    Private _pageHeight As Integer = 16838
    Private _marginTop As Integer = 1440
    Private _marginRight As Integer = 1440
    Private _marginBottom As Integer = 1440
    Private _marginLeft As Integer = 1440

    ' ==== 样式状态 ====

    Private _headingStyles(5) As WordStyle
    Private _paragraphStyle As WordStyle
    Private _defaultStyle As WordStyle
    Private _codeStyle As WordStyle
    Private _blockquoteStyle As WordStyle
    Private _titleStyle As WordStyle
    Private _tableStyle As TableStyle

    ' ==== 内容块队列 ====

    Private blocks As New List(Of RtfBlock)()

    ''' <summary>
    ''' 构造 RTF 文档生成器。默认样式与 <c>WordDocument</c> / <c>PdfDocument</c> 保持一致。
    ''' </summary>
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

        ' 与 WordDocument 默认样式保持一致
        _headingStyles(0) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 24, .Bold = True, .ForeColor = WordColors.Heading1Color, .SpaceBefore = 12, .SpaceAfter = 6}
        _headingStyles(1) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 22, .Bold = True, .ForeColor = WordColors.Heading2Color, .SpaceBefore = 10, .SpaceAfter = 6}
        _headingStyles(2) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 20, .Bold = True, .ForeColor = WordColors.Heading3Color, .SpaceBefore = 10, .SpaceAfter = 4}
        _headingStyles(3) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 18, .Bold = True, .ForeColor = WordColors.Heading1Color, .SpaceBefore = 8, .SpaceAfter = 4}
        _headingStyles(4) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 16, .Bold = True, .ForeColor = WordColors.Heading2Color, .SpaceBefore = 6, .SpaceAfter = 4}
        _headingStyles(5) = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 14, .Bold = True, .ForeColor = WordColors.Heading2Color, .SpaceBefore = 6, .SpaceAfter = 2}

        _paragraphStyle = New WordStyle With {.FontName = "Calibri", .FontNameEastAsia = "Microsoft YaHei", .Size = 11, .LineSpacing = 1.5, .SpaceAfter = 6}
        _defaultStyle = New WordStyle With {.FontName = "Calibri", .FontNameEastAsia = "Microsoft YaHei", .Size = 11, .LineSpacing = 1.5}
        _codeStyle = New WordStyle With {.FontName = "Consolas", .FontNameEastAsia = "Consolas", .Size = 10, .ForeColor = WordColors.DarkGray, .BackColor = WordColors.CodeBg, .SpaceBefore = 6, .SpaceAfter = 6}
        _blockquoteStyle = New WordStyle With {.FontName = "Calibri", .FontNameEastAsia = "Microsoft YaHei", .Size = 11, .Italic = True, .ForeColor = WordColors.DarkGray, .BackColor = WordColors.QuoteBg, .SpaceBefore = 6, .SpaceAfter = 6}
        _titleStyle = New WordStyle With {.FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .Size = 36, .Bold = True, .ForeColor = WordColors.DarkBlue, .Alignment = "center", .SpaceAfter = 12}

        _tableStyle = New TableStyle()
    End Sub

    ' ========================================================================
    ' 样式设置（流式 API，返回 Me 以保持链式调用）
    ' ========================================================================

    ''' <summary>设置指定级别的标题样式 (level 1-6)。</summary>
    Public Function HeadingStyle(level As Integer, style As WordStyle) As RtfDocument
        If level >= 1 AndAlso level <= 6 Then _headingStyles(level - 1) = style
        Return Me
    End Function

    ''' <summary>设置正文段落样式。</summary>
    Public Function ParagraphStyle(style As WordStyle) As RtfDocument
        _paragraphStyle = style
        Return Me
    End Function

    ''' <summary>设置文档默认样式。</summary>
    Public Function DefaultStyle(style As WordStyle) As RtfDocument
        _defaultStyle = style
        Return Me
    End Function

    ''' <summary>设置表格样式。</summary>
    Public Function TableStyle(style As TableStyle) As RtfDocument
        _tableStyle = style
        Return Me
    End Function

    ''' <summary>设置代码块样式。</summary>
    Public Function CodeStyle(style As WordStyle) As RtfDocument
        _codeStyle = style
        Return Me
    End Function

    ''' <summary>设置引用块样式。</summary>
    Public Function BlockquoteStyle(style As WordStyle) As RtfDocument
        _blockquoteStyle = style
        Return Me
    End Function

    ''' <summary>设置文档标题样式。</summary>
    Public Function TitleStyle(style As WordStyle) As RtfDocument
        _titleStyle = style
        Return Me
    End Function

    ' ========================================================================
    ' 页面设置（twips）
    ' ========================================================================

    ''' <summary>设置页面尺寸和边距 (twips)。</summary>
    Public Function PageSetup(pageWidth As Integer, pageHeight As Integer,
                              marginTop As Integer, marginRight As Integer,
                              marginBottom As Integer, marginLeft As Integer) As RtfDocument
        _pageWidth = pageWidth
        _pageHeight = pageHeight
        _marginTop = marginTop
        _marginRight = marginRight
        _marginBottom = marginBottom
        _marginLeft = marginLeft
        Return Me
    End Function

    ''' <summary>A4 纸张，1 英寸边距。</summary>
    Public Function PageSetupA4() As RtfDocument
        Return PageSetup(11906, 16838, 1440, 1440, 1440, 1440)
    End Function

    ''' <summary>Letter 纸张，1 英寸边距。</summary>
    Public Function PageSetupLetter() As RtfDocument
        Return PageSetup(12240, 15840, 1440, 1440, 1440, 1440)
    End Function

    ' ========================================================================
    ' 内容写入
    ' ========================================================================

    ''' <summary>写入文档标题（居中大字号，非 heading 样式）。</summary>
    Public Function DocTitle(text As String) As RtfDocument
        Return AddBlock(New RtfBlock() With {.Type = RtfBlockType.Title, .Text = text, .Style = _titleStyle.Clone()})
    End Function

    ''' <summary>写入一级标题。</summary>
    Public Function H1(text As String) As RtfDocument
        Return Heading(1, text)
    End Function

    ''' <summary>写入二级标题。</summary>
    Public Function H2(text As String) As RtfDocument
        Return Heading(2, text)
    End Function

    ''' <summary>写入三级标题。</summary>
    Public Function H3(text As String) As RtfDocument
        Return Heading(3, text)
    End Function

    ''' <summary>写入四级标题。</summary>
    Public Function H4(text As String) As RtfDocument
        Return Heading(4, text)
    End Function

    ''' <summary>写入五级标题。</summary>
    Public Function H5(text As String) As RtfDocument
        Return Heading(5, text)
    End Function

    ''' <summary>写入六级标题。</summary>
    Public Function H6(text As String) As RtfDocument
        Return Heading(6, text)
    End Function

    ''' <summary>写入指定级别的标题 (level 1-6)。</summary>
    Public Function Heading(level As Integer, text As String) As RtfDocument
        Dim idx As Integer = System.Math.Min(System.Math.Max(level, 1), 6) - 1
        Return AddBlock(New RtfBlock() With {
            .Type = RtfBlockType.Heading,
            .Level = idx + 1,
            .Text = text,
            .Style = _headingStyles(idx).Clone()
        })
    End Function

    ''' <summary>写入正文段落。</summary>
    Public Function Paragraph(text As String) As RtfDocument
        Return Paragraph(text, _paragraphStyle)
    End Function

    ''' <summary>写入正文段落（指定样式）。</summary>
    Public Function Paragraph(text As String, style As WordStyle) As RtfDocument
        Return AddBlock(New RtfBlock() With {.Type = RtfBlockType.Paragraph, .Text = text, .Style = style.Clone()})
    End Function

    ''' <summary>写入代码块（等宽字体，灰色底纹）。</summary>
    Public Function CodeBlock(code As String, Optional language As String = "") As RtfDocument
        Return AddBlock(New RtfBlock() With {.Type = RtfBlockType.Code, .Text = code, .Style = _codeStyle.Clone()})
    End Function

    ''' <summary>写入引用块。</summary>
    Public Function Blockquote(text As String) As RtfDocument
        Return AddBlock(New RtfBlock() With {.Type = RtfBlockType.Quote, .Text = text, .Style = _blockquoteStyle.Clone()})
    End Function

    ''' <summary>写入列表（有序或无序）。</summary>
    Public Function List(items As String(), Optional ordered As Boolean = False) As RtfDocument
        If items IsNot Nothing Then
            For Each it As String In items
                Call AddBlock(New RtfBlock() With {
                    .Type = RtfBlockType.List,
                    .Text = it,
                    .Ordered = ordered,
                    .Style = _paragraphStyle.Clone()
                })
            Next
        End If
        Return Me
    End Function

    ''' <summary>写入任务列表。</summary>
    Public Function TaskList(items As String(), checked As Boolean()) As RtfDocument
        If items IsNot Nothing Then
            For i As Integer = 0 To items.Length - 1
                Call AddBlock(New RtfBlock() With {
                    .Type = RtfBlockType.TaskList,
                    .Text = items(i),
                    .Checked = checked IsNot Nothing AndAlso i < checked.Length AndAlso checked(i),
                    .Style = _paragraphStyle.Clone()
                })
            Next
        End If
        Return Me
    End Function

    ''' <summary>写入定义列表。</summary>
    Public Function DefinitionList(terms As String(), definitions As String()) As RtfDocument
        If terms IsNot Nothing Then
            For i As Integer = 0 To terms.Length - 1
                Call AddBlock(New RtfBlock() With {
                    .Type = RtfBlockType.DefList,
                    .Term = terms(i),
                    .Text = If(definitions IsNot Nothing AndAlso i < definitions.Length, definitions(i), ""),
                    .Style = _paragraphStyle.Clone()
                })
            Next
        End If
        Return Me
    End Function

    ''' <summary>写入水平分割线。</summary>
    Public Function Hr() As RtfDocument
        Return AddBlock(New RtfBlock() With {.Type = RtfBlockType.Hr})
    End Function

    ''' <summary>插入分页符。</summary>
    Public Function PageBreak() As RtfDocument
        Return AddBlock(New RtfBlock() With {.Type = RtfBlockType.PageBreak})
    End Function

    ''' <summary>插入目录 (TOC)。</summary>
    Public Function Toc(Optional maxLevel As Integer = 3) As RtfDocument
        Return AddBlock(New RtfBlock() With {.Type = RtfBlockType.Toc, .Level = maxLevel})
    End Function

    ' ========================================================================
    ' 表格
    ' ========================================================================

    ''' <summary>写入等宽表格（二维数组形式）。</summary>
    Public Function Table(headers As String(), data As String(,)) As RtfDocument
        Return Table(headers, data, Nothing)
    End Function

    ''' <summary>写入表格（二维数组形式，支持对齐方式）。</summary>
    Public Function Table(headers As String(), data As String(,), alignments As String()) As RtfDocument
        Return Table(headers, ToJaggedRows(data), alignments)
    End Function

    ''' <summary>写入等宽表格（交错数组形式）。</summary>
    Public Function Table(headers As String(), rows As String()(),
                          Optional alignments As String() = Nothing) As RtfDocument
        Return AddTableBlock("equal", headers, rows, alignments, False, False)
    End Function

    ''' <summary>写入表格，按窗口宽度自适应。</summary>
    Public Function TableAutoFitWindow(headers As String(), rows As String()(),
                                       Optional alignments As String() = Nothing,
                                       Optional center As Boolean = False,
                                       Optional threeLine As Boolean = False) As RtfDocument
        Return AddTableBlock("window", headers, rows, alignments, center, threeLine)
    End Function

    ''' <summary>写入表格，按内容宽度自适应。</summary>
    Public Function TableAutoFitContents(headers As String(), rows As String()(),
                                         Optional alignments As String() = Nothing,
                                         Optional center As Boolean = False,
                                         Optional threeLine As Boolean = False) As RtfDocument
        Return AddTableBlock("contents", headers, rows, alignments, center, threeLine)
    End Function

    ''' <summary>写入表格，按窗口宽度自适应（二维数组形式）。</summary>
    Public Function TableAutoFitWindow(headers As String(,), rows As String(,),
                                       Optional alignments As String() = Nothing,
                                       Optional center As Boolean = False,
                                       Optional threeLine As Boolean = False) As RtfDocument
        Return AddTableBlock("window", ToJaggedHeaders(headers), ToJaggedRows(rows), alignments, center, threeLine)
    End Function

    ''' <summary>写入表格，按内容宽度自适应（二维数组形式）。</summary>
    Public Function TableAutoFitContents(headers As String(,), rows As String(,),
                                         Optional alignments As String() = Nothing,
                                         Optional center As Boolean = False,
                                         Optional threeLine As Boolean = False) As RtfDocument
        Return AddTableBlock("contents", ToJaggedHeaders(headers), ToJaggedRows(rows), alignments, center, threeLine)
    End Function

    Private Function AddTableBlock(mode As String, headers As String(), rows As String()(),
                                   alignments As String(), center As Boolean, threeLine As Boolean) As RtfDocument
        Return AddBlock(New RtfBlock() With {
            .Type = RtfBlockType.Table,
            .TableHeaders = CloneRow(headers),
            .TableRows = CloneRows(rows),
            .TableAlignments = alignments,
            .TableMode = mode,
            .TableCenter = center,
            .TableThreeLine = threeLine
        })
    End Function

    ' ========================================================================
    ' 图片与 Markdown 块
    ' ========================================================================

    ''' <summary>
    ''' 插入图片。图片二进制将以十六进制内嵌进 RTF（png / jpg / jpeg）。
    ''' </summary>
    Public Function Image(file As String,
                          Optional width As Double = 0,
                          Optional height As Double = 0,
                          Optional caption As String = "") As RtfDocument
        If String.IsNullOrEmpty(file) OrElse Not System.IO.File.Exists(file) Then
            Console.Error.WriteLine($"[警告] 图片文件不存在: {file}")
            Return Me
        End If

        Dim ext As String = System.IO.Path.GetExtension(file).TrimStart("."c).ToLower()
        If ext <> "png" AndAlso ext <> "jpg" AndAlso ext <> "jpeg" Then
            Console.Error.WriteLine($"[警告] RTF 暂不支持该图片格式，已跳过: {file}")
            Return Me
        End If

        Dim size = ImageHelper.ReadImageDimensions(file)

        Return AddBlock(New RtfBlock() With {
            .Type = RtfBlockType.Image,
            .ImagePath = file,
            .ImageWidth = width,
            .ImageHeight = height,
            .ImageCaption = caption,
            .ImagePixelWidth = size.Width,
            .ImagePixelHeight = size.Height
        })
    End Function

    ''' <summary>将一组 Markdown 内容块写入文档。</summary>
    Public Function WriteBlocks(blocksInput As IEnumerable(Of JSONSchema.Block)) As RtfDocument
        If blocksInput Is Nothing Then Return Me

        For Each blk As JSONSchema.Block In blocksInput
            If blk Is Nothing Then Continue For
            Call WriteBlock(blk)
        Next

        Return Me
    End Function

    ''' <summary>
    ''' 将单个 Markdown 内容块映射为等价的写入调用。映射语义与
    ''' <c>WordDocument.WriteBlock</c> / <c>PdfDocument.WriteBlock</c> 保持一致。
    ''' </summary>
    Private Sub WriteBlock(blk As JSONSchema.Block)
        If blk.type Is Nothing Then
            If Not String.IsNullOrEmpty(blk.content) Then
                Call Paragraph(blk.content)
            End If
            Return
        End If

        Select Case blk.type.ToLower()
            Case "heading", "h"
                Call Heading(If(blk.level >= 1 AndAlso blk.level <= 6, blk.level, 1), blk.content)
            Case "paragraph", "p"
                Call Paragraph(blk.content)
            Case "code"
                Call CodeBlock(blk.content, blk.language)
            Case "quote", "blockquote"
                Call Blockquote(blk.content)
            Case "list", "li", "bulletedlist", "orderedlist"
                If blk.items IsNot Nothing AndAlso blk.items.Length > 0 Then
                    Call List(blk.items, blk.ordered)
                End If
            Case "tasklist", "tasks", "todo"
                If blk.items IsNot Nothing AndAlso blk.items.Length > 0 Then
                    Call TaskList(blk.items, blk.checked)
                End If
            Case "deflist", "definition", "dl"
                If blk.terms IsNot Nothing AndAlso blk.terms.Length > 0 Then
                    Call DefinitionList(blk.terms, blk.definitions)
                End If
            Case "table"
                If blk.rows IsNot Nothing AndAlso blk.rows.Length > 0 Then
                    Call Table(If(blk.headers, New String() {}), blk.rows, blk.alignments)
                End If
            Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                Call Hr()
            Case "image", "img"
                If Not String.IsNullOrEmpty(blk.url) AndAlso System.IO.File.Exists(blk.url) Then
                    Call Image(blk.url, caption:=blk.alt)
                Else
                    ' 与 docx 侧一致：图片缺失时降级为文本占位
                    Call Paragraph($"[图片: {If(String.IsNullOrEmpty(blk.alt), blk.url, blk.alt)}]")
                End If
            Case "math", "equation", "tex", "latex"
                Call CodeBlock(blk.content, "latex")
            Case "link", "a"
                Call Paragraph($"[{blk.alt}]({blk.url})")
            Case "footnote", "note"
                Call Paragraph($"[{blk.id}] {blk.content}")
            Case "html", "raw"
                Call Paragraph(blk.content)
            Case Else
                If Not String.IsNullOrEmpty(blk.content) Then
                    Call Paragraph(blk.content)
                End If
        End Select
    End Sub

    ' ========================================================================
    ' 保存
    ' ========================================================================

    ''' <summary>将文档保存到指定文件路径。</summary>
    Public Sub Save(filePath As String) Implements IDocumentWriter.Save
        Dim meta As New RtfWriteMeta() With {
            .Author = Author,
            .Title = Title,
            .Subject = Subject,
            .Description = Description,
            .Keywords = If(Tags Is Nothing, "", String.Join(", ", Tags)),
            .Creator = ApplicationName
        }

        Call RtfSerializer.Save(filePath, blocks,
                                _pageWidth, _pageHeight,
                                _marginTop, _marginRight, _marginBottom, _marginLeft,
                                _headingStyles, _paragraphStyle, _defaultStyle,
                                _codeStyle, _blockquoteStyle, _titleStyle, _tableStyle,
                                meta)
    End Sub

    ' ========================================================================
    ' IDocumentWriter 显式接口实现（与公开方法同形，返回接口自身）
    ' ========================================================================

    Private Function IDW_HeadingStyle(level As Integer, style As WordStyle) As IDocumentWriter Implements IDocumentWriter.HeadingStyle
        Return HeadingStyle(level, style)
    End Function

    Private Function IDW_ParagraphStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.ParagraphStyle
        Return ParagraphStyle(style)
    End Function

    Private Function IDW_DefaultStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.DefaultStyle
        Return DefaultStyle(style)
    End Function

    Private Function IDW_TableStyle(style As TableStyle) As IDocumentWriter Implements IDocumentWriter.TableStyle
        Return TableStyle(style)
    End Function

    Private Function IDW_CodeStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.CodeStyle
        Return CodeStyle(style)
    End Function

    Private Function IDW_BlockquoteStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.BlockquoteStyle
        Return BlockquoteStyle(style)
    End Function

    Private Function IDW_TitleStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.TitleStyle
        Return TitleStyle(style)
    End Function

    Private Function IDW_PageSetup(pageWidth As Integer, pageHeight As Integer,
                                   marginTop As Integer, marginRight As Integer,
                                   marginBottom As Integer, marginLeft As Integer) As IDocumentWriter Implements IDocumentWriter.PageSetup
        Return PageSetup(pageWidth, pageHeight, marginTop, marginRight, marginBottom, marginLeft)
    End Function

    Private Function IDW_PageSetupA4() As IDocumentWriter Implements IDocumentWriter.PageSetupA4
        Return PageSetupA4()
    End Function

    Private Function IDW_PageSetupLetter() As IDocumentWriter Implements IDocumentWriter.PageSetupLetter
        Return PageSetupLetter()
    End Function

    Private Function IDW_DocTitle(text As String) As IDocumentWriter Implements IDocumentWriter.DocTitle
        Return DocTitle(text)
    End Function

    Private Function IDW_H1(text As String) As IDocumentWriter Implements IDocumentWriter.H1
        Return H1(text)
    End Function

    Private Function IDW_H2(text As String) As IDocumentWriter Implements IDocumentWriter.H2
        Return H2(text)
    End Function

    Private Function IDW_H3(text As String) As IDocumentWriter Implements IDocumentWriter.H3
        Return H3(text)
    End Function

    Private Function IDW_H4(text As String) As IDocumentWriter Implements IDocumentWriter.H4
        Return H4(text)
    End Function

    Private Function IDW_H5(text As String) As IDocumentWriter Implements IDocumentWriter.H5
        Return H5(text)
    End Function

    Private Function IDW_H6(text As String) As IDocumentWriter Implements IDocumentWriter.H6
        Return H6(text)
    End Function

    Private Function IDW_Heading(level As Integer, text As String) As IDocumentWriter Implements IDocumentWriter.Heading
        Return Heading(level, text)
    End Function

    Private Function IDW_Paragraph1(text As String) As IDocumentWriter Implements IDocumentWriter.Paragraph
        Return Paragraph(text)
    End Function

    Private Function IDW_Paragraph2(text As String, style As WordStyle) As IDocumentWriter Implements IDocumentWriter.Paragraph
        Return Paragraph(text, style)
    End Function

    Private Function IDW_CodeBlock(code As String, Optional language As String = "") As IDocumentWriter Implements IDocumentWriter.CodeBlock
        Return CodeBlock(code, language)
    End Function

    Private Function IDW_Blockquote(text As String) As IDocumentWriter Implements IDocumentWriter.Blockquote
        Return Blockquote(text)
    End Function

    Private Function IDW_List(items As String(), Optional ordered As Boolean = False) As IDocumentWriter Implements IDocumentWriter.List
        Return List(items, ordered)
    End Function

    Private Function IDW_TaskList(items As String(), checked As Boolean()) As IDocumentWriter Implements IDocumentWriter.TaskList
        Return TaskList(items, checked)
    End Function

    Private Function IDW_DefinitionList(terms As String(), definitions As String()) As IDocumentWriter Implements IDocumentWriter.DefinitionList
        Return DefinitionList(terms, definitions)
    End Function

    Private Function IDW_Hr() As IDocumentWriter Implements IDocumentWriter.Hr
        Return Hr()
    End Function

    Private Function IDW_PageBreak() As IDocumentWriter Implements IDocumentWriter.PageBreak
        Return PageBreak()
    End Function

    Private Function IDW_Toc(Optional maxLevel As Integer = 3) As IDocumentWriter Implements IDocumentWriter.Toc
        Return Toc(maxLevel)
    End Function

    Private Function IDW_Table1(headers As String(), data As String(,)) As IDocumentWriter Implements IDocumentWriter.Table
        Return Table(headers, data)
    End Function

    Private Function IDW_Table2(headers As String(), data As String(,), alignments As String()) As IDocumentWriter Implements IDocumentWriter.Table
        Return Table(headers, data, alignments)
    End Function

    Private Function IDW_Table3(headers As String(), rows As String()(),
                                Optional alignments As String() = Nothing) As IDocumentWriter Implements IDocumentWriter.Table
        Return Table(headers, rows, alignments)
    End Function

    Private Function IDW_TableAutoFitWindow1(headers As String(), rows As String()(),
                                             Optional alignments As String() = Nothing,
                                             Optional center As Boolean = False,
                                             Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitWindow
        Return TableAutoFitWindow(headers, rows, alignments, center, threeLine)
    End Function

    Private Function IDW_TableAutoFitContents1(headers As String(), rows As String()(),
                                               Optional alignments As String() = Nothing,
                                               Optional center As Boolean = False,
                                               Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitContents
        Return TableAutoFitContents(headers, rows, alignments, center, threeLine)
    End Function

    Private Function IDW_TableAutoFitWindow2(headers As String(,), rows As String(,),
                                             Optional alignments As String() = Nothing,
                                             Optional center As Boolean = False,
                                             Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitWindow
        Return TableAutoFitWindow(headers, rows, alignments, center, threeLine)
    End Function

    Private Function IDW_TableAutoFitContents2(headers As String(,), rows As String(,),
                                               Optional alignments As String() = Nothing,
                                               Optional center As Boolean = False,
                                               Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitContents
        Return TableAutoFitContents(headers, rows, alignments, center, threeLine)
    End Function

    Private Function IDW_Image(file As String,
                               Optional width As Double = 0,
                               Optional height As Double = 0,
                               Optional caption As String = "") As IDocumentWriter Implements IDocumentWriter.Image
        Return Image(file, width, height, caption)
    End Function

    Private Function IDW_WriteBlocks(blocksInput As IEnumerable(Of JSONSchema.Block)) As IDocumentWriter Implements IDocumentWriter.WriteBlocks
        Return WriteBlocks(blocksInput)
    End Function

    ' ========================================================================
    ' 辅助
    ' ========================================================================

    Private Function AddBlock(block As RtfBlock) As RtfDocument
        blocks.Add(block)
        Return Me
    End Function

    Private Shared Function CloneRow(row As String()) As String()
        If row Is Nothing Then Return New String() {}
        Return CType(row.Clone(), String())
    End Function

    Private Shared Function CloneRows(rows As String()()) As String()()
        If rows Is Nothing Then Return New String()() {}

        Dim out As New List(Of String())()
        For Each r As String() In rows
            out.Add(If(r, New String() {}))
        Next

        Return out.ToArray()
    End Function

    Private Shared Function ToJaggedHeaders(headers As String(,)) As String()
        If headers Is Nothing OrElse headers.Length = 0 Then Return New String() {}

        Dim out(headers.GetLength(1) - 1) As String
        For j As Integer = 0 To headers.GetLength(1) - 1
            out(j) = headers(0, j)
        Next

        Return out
    End Function

    Private Shared Function ToJaggedRows(rows As String(,)) As String()()
        If rows Is Nothing OrElse rows.Length = 0 Then Return New String()() {}

        Dim out(rows.GetLength(0) - 1)() As String
        For i As Integer = 0 To rows.GetLength(0) - 1
            Dim row(rows.GetLength(1) - 1) As String
            For j As Integer = 0 To rows.GetLength(1) - 1
                row(j) = rows(i, j)
            Next
            out(i) = row
        Next

        Return out
    End Function

End Class

''' <summary>写入 RTF 时携带的文档元数据。</summary>
Public Class RtfWriteMeta
    ''' <summary>文档标题（写入 \info 的 title）。</summary>
    Public Property Title As String = ""
    ''' <summary>文档作者（写入 \info 的 author）。</summary>
    Public Property Author As String = ""
    ''' <summary>文档主题。</summary>
    Public Property Subject As String = ""
    ''' <summary>文档描述。</summary>
    Public Property Description As String = ""
    ''' <summary>关键字（逗号分隔）。</summary>
    Public Property Keywords As String = ""
    ''' <summary>生成程序名称。</summary>
    Public Property Creator As String = ""
End Class
