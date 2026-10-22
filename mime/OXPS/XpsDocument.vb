' ============================================================================
' XpsDocument.vb - XPS / OXPS 文档生成器
'
' 与 WordDocument / RtfDocument / PdfDocument 同族的流式写入 API：调用时只把
' 内容与样式快照收集为 XpsBlock 队列，Save / ToBytes 时才做排版与打包。
'
' 输出方言由 Flavor 决定（Oxps / Xps），两者共用同一套版式与内容能力。
' ============================================================================

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Report

''' <summary>
''' XPS / OXPS 文档生成器。收集内容与样式，最终由 <see cref="XpsLayoutEngine"/>
''' 排版、<see cref="XpsPackager"/> 打包为 .oxps / .xps 文件。
''' </summary>
''' <remarks>
''' 页面尺寸与边距入参单位为 twips（1 pt = 20 twips），内部换算为磅后排版。
''' 图片文件缺失或格式不支持时降级为文本占位，不影响其余内容。
''' </remarks>
Public Class XpsDocument

    ' ==== 元数据 ====

    ''' <summary>文档作者。</summary>
    Public Property Author As String = ""
    ''' <summary>文档标题。</summary>
    Public Property Title As String = ""
    ''' <summary>文档主题。</summary>
    Public Property Subject As String = ""
    ''' <summary>文档描述。</summary>
    Public Property Description As String = ""
    ''' <summary>文档关键词。</summary>
    Public Property Tags As String() = Nothing
    ''' <summary>生成该文档的应用程序名称。</summary>
    Public Property ApplicationName As String = "VB.NET XpsDocument Generator"

    ' ==== 输出格式与字体 ====

    ''' <summary>输出方言（OXPS / XPS），默认 OpenXPS。</summary>
    Public Property Flavor As XpsFlavor = XpsFlavor.Oxps

    ''' <summary>可选的字体文件路径（.ttf）；为空时自动查找系统字体。</summary>
    Public Property FontFile As String = Nothing

    ''' <summary>是否嵌入字体；False 时输出不嵌字体的轻量文档。</summary>
    Public Property EmbedFont As Boolean = True

    ' ==== 页面尺寸（twips） ====

    Private _pageWidth As Integer = XpsSpec.DefaultPageWidth
    Private _pageHeight As Integer = XpsSpec.DefaultPageHeight
    Private _marginTop As Integer = XpsSpec.DefaultMargin
    Private _marginRight As Integer = XpsSpec.DefaultMargin
    Private _marginBottom As Integer = XpsSpec.DefaultMargin
    Private _marginLeft As Integer = XpsSpec.DefaultMargin

    ' ==== 样式 ====

    Private ReadOnly _headingStyles(5) As XpsTextStyle
    Private _paragraphStyle As XpsTextStyle
    Private _codeStyle As XpsTextStyle
    Private _quoteStyle As XpsTextStyle
    Private _titleStyle As XpsTextStyle

    ' ==== 内容 ====

    Private ReadOnly _blocks As New List(Of XpsBlock)()
    Private ReadOnly _images As New List(Of XpsImageResource)()

    ''' <summary>构造 XPS 文档生成器（默认 A4、1 英寸边距、OXPS 输出）。</summary>
    ''' <param name="author">文档作者。</param>
    ''' <param name="title">文档标题。</param>
    ''' <param name="tags">文档关键词。</param>
    ''' <param name="subject">文档主题。</param>
    ''' <param name="description">文档描述。</param>
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

        ' 文档大标题
        _titleStyle = New XpsTextStyle With {
            .Size = 18,
            .Bold = True,
            .Alignment = "center",
            .SpaceAfter = 12,
            .LineSpacing = 1.5
        }

        ' 1-6 级标题：字号逐级递减
        _headingStyles(0) = New XpsTextStyle With {.Size = 16, .Bold = True, .SpaceBefore = 12, .SpaceAfter = 6, .LineSpacing = 1.5}
        _headingStyles(1) = New XpsTextStyle With {.Size = 14, .Bold = True, .SpaceBefore = 10, .SpaceAfter = 5, .LineSpacing = 1.5}
        _headingStyles(2) = New XpsTextStyle With {.Size = 12, .Bold = True, .SpaceBefore = 8, .SpaceAfter = 4, .LineSpacing = 1.5}
        _headingStyles(3) = New XpsTextStyle With {.Size = 11, .Bold = True, .SpaceBefore = 6, .SpaceAfter = 4, .LineSpacing = 1.5}
        _headingStyles(4) = New XpsTextStyle With {.Size = 10.5, .Bold = True, .SpaceBefore = 6, .SpaceAfter = 3, .LineSpacing = 1.4}
        _headingStyles(5) = New XpsTextStyle With {.Size = 10.5, .Bold = True, .SpaceBefore = 6, .SpaceAfter = 3, .LineSpacing = 1.4}

        _paragraphStyle = New XpsTextStyle With {.Size = 10.5, .LineSpacing = 1.6, .SpaceAfter = 3}
        _codeStyle = New XpsTextStyle With {.Size = 9, .LineSpacing = 1.3, .SpaceBefore = 4, .SpaceAfter = 4}
        _quoteStyle = New XpsTextStyle With {.Size = 10.5, .LineSpacing = 1.5, .Indent = 12, .SpaceBefore = 4, .SpaceAfter = 4}
    End Sub

    ' ========================================================================
    ' 样式与页面
    ' ========================================================================

    ''' <summary>设置指定级别的标题样式（level 1-6）。</summary>
    ''' <param name="level">标题级别。</param>
    ''' <param name="style">样式。</param>
    ''' <returns>自身，便于链式调用。</returns>
    Public Function HeadingStyle(level As Integer, style As XpsTextStyle) As XpsDocument
        If level >= 1 AndAlso level <= 6 Then
            _headingStyles(level - 1) = style
        End If

        Return Me
    End Function

    ''' <summary>设置正文段落样式。</summary>
    ''' <param name="style"></param>
    ''' <returns></returns>
    Public Function ParagraphStyle(style As XpsTextStyle) As XpsDocument
        _paragraphStyle = style
        Return Me
    End Function

    ''' <summary>设置代码块样式。</summary>
    ''' <param name="style"></param>
    ''' <returns></returns>
    Public Function CodeStyle(style As XpsTextStyle) As XpsDocument
        _codeStyle = style
        Return Me
    End Function

    ''' <summary>设置引用块样式。</summary>
    ''' <param name="style"></param>
    ''' <returns></returns>
    Public Function BlockquoteStyle(style As XpsTextStyle) As XpsDocument
        _quoteStyle = style
        Return Me
    End Function

    ''' <summary>设置文档大标题样式。</summary>
    ''' <param name="style"></param>
    ''' <returns></returns>
    Public Function TitleStyle(style As XpsTextStyle) As XpsDocument
        _titleStyle = style
        Return Me
    End Function

    ''' <summary>设置页面尺寸与四边页边距（twips）。</summary>
    ''' <param name="pageWidth">页面宽度。</param>
    ''' <param name="pageHeight">页面高度。</param>
    ''' <param name="marginTop">上边距。</param>
    ''' <param name="marginRight">右边距。</param>
    ''' <param name="marginBottom">下边距。</param>
    ''' <param name="marginLeft">左边距。</param>
    ''' <returns>自身，便于链式调用。</returns>
    Public Function PageSetup(pageWidth As Integer, pageHeight As Integer,
                              marginTop As Integer, marginRight As Integer,
                              marginBottom As Integer, marginLeft As Integer) As XpsDocument
        _pageWidth = pageWidth
        _pageHeight = pageHeight
        _marginTop = marginTop
        _marginRight = marginRight
        _marginBottom = marginBottom
        _marginLeft = marginLeft

        Return Me
    End Function

    ''' <summary>A4 纸张，1 英寸边距。</summary>
    ''' <returns></returns>
    Public Function PageSetupA4() As XpsDocument
        Return PageSetup(11906, 16838, 1440, 1440, 1440, 1440)
    End Function

    ''' <summary>Letter 纸张，1 英寸边距。</summary>
    ''' <returns></returns>
    Public Function PageSetupLetter() As XpsDocument
        Return PageSetup(12240, 15840, 1440, 1440, 1440, 1440)
    End Function

    ' ========================================================================
    ' 内容写入
    ' ========================================================================

    Private Function AddBlock(block As XpsBlock) As XpsDocument
        Call _blocks.Add(block)
        Return Me
    End Function

    ''' <summary>写入文档大标题（居中大字）。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function DocTitle(text As String) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Title,
            .Text = text,
            .Style = _titleStyle.Clone()
        })
    End Function

    ''' <summary>写入指定级别的标题（level 1-6）。</summary>
    ''' <param name="level"></param>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function Heading(level As Integer, text As String) As XpsDocument
        Dim style As XpsTextStyle = _headingStyles(If(level < 1, 0, If(level > 6, 5, level - 1)))

        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Heading,
            .Level = If(level < 1, 1, If(level > 6, 6, level)),
            .Text = text,
            .Style = style.Clone()
        })
    End Function

    ''' <summary>写入一级标题。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function H1(text As String) As XpsDocument
        Return Heading(1, text)
    End Function

    ''' <summary>写入二级标题。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function H2(text As String) As XpsDocument
        Return Heading(2, text)
    End Function

    ''' <summary>写入三级标题。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function H3(text As String) As XpsDocument
        Return Heading(3, text)
    End Function

    ''' <summary>写入四级标题。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function H4(text As String) As XpsDocument
        Return Heading(4, text)
    End Function

    ''' <summary>写入五级标题。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function H5(text As String) As XpsDocument
        Return Heading(5, text)
    End Function

    ''' <summary>写入六级标题。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function H6(text As String) As XpsDocument
        Return Heading(6, text)
    End Function

    ''' <summary>写入正文段落。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function Paragraph(text As String) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Paragraph,
            .Text = text,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>写入代码块（保留原始换行）。</summary>
    ''' <param name="code">代码正文。</param>
    ''' <param name="language">语言标识（仅作元信息保留）。</param>
    ''' <returns></returns>
    Public Function CodeBlock(code As String, Optional language As String = "") As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Code,
            .Text = code,
            .Style = _codeStyle.Clone()
        })
    End Function

    ''' <summary>写入引用块。</summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function Blockquote(text As String) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Quote,
            .Text = text,
            .Style = _quoteStyle.Clone()
        })
    End Function

    ''' <summary>写入列表。</summary>
    ''' <param name="items">列表项。</param>
    ''' <param name="ordered">是否有序列表。</param>
    ''' <returns></returns>
    Public Function List(items As String(), Optional ordered As Boolean = False) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.List,
            .Items = items,
            .Ordered = ordered,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>写入任务列表。</summary>
    ''' <param name="items">任务项。</param>
    ''' <param name="checked">勾选状态（可为 Nothing）。</param>
    ''' <returns></returns>
    Public Function TaskList(items As String(), checked As Boolean()) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.TaskList,
            .Items = items,
            .Checked = checked,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>写入定义列表。</summary>
    ''' <param name="terms">术语。</param>
    ''' <param name="definitions">定义。</param>
    ''' <returns></returns>
    Public Function DefinitionList(terms As String(), definitions As String()) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.DefList,
            .Terms = terms,
            .Definitions = definitions,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>写入水平分割线。</summary>
    ''' <returns></returns>
    Public Function Hr() As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Hr,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>写入分页符。</summary>
    ''' <returns></returns>
    Public Function PageBreak() As XpsDocument
        Return AddBlock(New XpsBlock() With {.Type = XpsBlockType.PageBreak})
    End Function

    ''' <summary>插入静态目录（按已写入的标题生成）。</summary>
    ''' <param name="maxLevel">收集的最大标题层级（1-6）。</param>
    ''' <returns></returns>
    Public Function Toc(Optional maxLevel As Integer = 3) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Toc,
            .Level = maxLevel,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>写入表格。</summary>
    ''' <param name="headers">表头。</param>
    ''' <param name="rows">数据行（交错数组）。</param>
    ''' <param name="alignments">逐列对齐方式（left / center / right）。</param>
    ''' <returns></returns>
    Public Function Table(headers As String(), rows As String()(),
                          Optional alignments As String() = Nothing) As XpsDocument
        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Table,
            .TableHeaders = headers,
            .TableRows = rows,
            .TableAlignments = alignments,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>写入图片；文件缺失或格式不支持时降级为文本占位。</summary>
    ''' <param name="imageFile">png / jpg / jpeg 文件路径。</param>
    ''' <param name="width">显示宽度（磅），0 表示按原始像素尺寸换算。</param>
    ''' <param name="height">显示高度（磅），0 表示按原始像素尺寸换算。</param>
    ''' <param name="caption">图注。</param>
    ''' <returns></returns>
    Public Function Image(imageFile As String,
                          Optional width As Double = 0,
                          Optional height As Double = 0,
                          Optional caption As String = "") As XpsDocument
        Dim resource As XpsImageResource = XpsImageResource.Create(imageFile, _images.Count + 1)

        If resource Is Nothing Then
            Return Paragraph($"[图片: {If(String.IsNullOrEmpty(caption), imageFile, caption)}]")
        End If

        Call _images.Add(resource)

        Return AddBlock(New XpsBlock() With {
            .Type = XpsBlockType.Image,
            .ImagePath = imageFile,
            .ImageResource = resource,
            .ImageWidth = width,
            .ImageHeight = height,
            .ImageCaption = caption,
            .Style = _paragraphStyle.Clone()
        })
    End Function

    ''' <summary>
    ''' 把 markdown 内容块（<see cref="Block"/>）写入文档，是统一报告接口的落点。
    ''' </summary>
    ''' <param name="blocksInput">内容块序列。</param>
    ''' <returns></returns>
    Public Function WriteBlocks(blocksInput As IEnumerable(Of Block)) As XpsDocument
        If blocksInput Is Nothing Then
            Return Me
        End If

        For Each block As Block In blocksInput
            If block IsNot Nothing Then
                Call WriteBlock(block)
            End If
        Next

        Return Me
    End Function

    ''' <summary>
    ''' 将单个 markdown 内容块映射为等价的写入调用。映射语义与
    ''' <c>WordDocument.WriteBlock</c> / <c>RtfDocument.WriteBlock</c> 保持一致。
    ''' </summary>
    Private Sub WriteBlock(block As Block)
        If block.type Is Nothing Then
            If Not String.IsNullOrEmpty(block.content) Then
                Call Paragraph(block.content)
            End If
            Return
        End If

        Select Case LCase(block.type)
            Case "heading", "h"
                Call Heading(If(block.level >= 1 AndAlso block.level <= 6, block.level, 1), If(block.content, ""))
            Case "paragraph", "p"
                Call Paragraph(If(block.content, ""))
            Case "code"
                Call CodeBlock(If(block.content, ""), If(block.language, ""))
            Case "blockquote", "quote"
                Call Blockquote(If(block.content, ""))
            Case "list", "li", "bulletedlist", "orderedlist"
                If block.items IsNot Nothing AndAlso block.items.Length > 0 Then
                    Call List(block.items, block.ordered)
                End If
            Case "tasklist", "tasks", "todo"
                If block.items IsNot Nothing AndAlso block.items.Length > 0 Then
                    Call TaskList(block.items, block.checked)
                End If
            Case "deflist", "definition", "dl"
                If block.terms IsNot Nothing AndAlso block.terms.Length > 0 Then
                    Call DefinitionList(block.terms, If(block.definitions, New String() {}))
                End If
            Case "table"
                If block.rows IsNot Nothing AndAlso block.rows.Length > 0 Then
                    Call Table(If(block.headers, New String() {}), block.rows, block.alignments)
                End If
            Case "hr", "horizontal-rule", "horizontalrule", "thematic-break"
                Call Hr()
            Case "image", "img"
                Call Image(If(block.url, ""), caption:=If(block.alt, ""))
            Case "math", "equation", "mathml", "tex", "latex"
                ' XPS 无原生数学排版，按 LaTeX 源码代码块输出
                Call CodeBlock(If(block.content, "").Trim(), "latex")
            Case "link", "a"
                Call Paragraph($"[{If(block.alt, "")}]({If(block.url, "")})")
            Case "footnote", "note"
                Call Paragraph($"[{If(block.id, "")}] {If(block.content, "")}")
            Case "html", "raw"
                Call Paragraph(If(block.content, ""))
            Case Else
                If Not String.IsNullOrEmpty(block.content) Then
                    Call Paragraph(block.content)
                End If
        End Select
    End Sub

    ' ========================================================================
    ' 输出
    ' ========================================================================

    ''' <summary>
    ''' 排版并打包为 XPS/OXPS 字节流（不落盘）。
    ''' </summary>
    ''' <returns>完整的 .oxps / .xps 包字节。</returns>
    Public Function ToBytes() As Byte()
        Dim font As XpsFontResource = XpsFontResource.Create(FontFile, EmbedFont)
        Dim engine As New XpsLayoutEngine(_pageWidth / XpsSpec.TwipsPerPoint,
                                          _pageHeight / XpsSpec.TwipsPerPoint,
                                          _marginTop / XpsSpec.TwipsPerPoint,
                                          _marginRight / XpsSpec.TwipsPerPoint,
                                          _marginBottom / XpsSpec.TwipsPerPoint,
                                          _marginLeft / XpsSpec.TwipsPerPoint,
                                          _blocks, _headingStyles, _paragraphStyle,
                                          _codeStyle, _quoteStyle, _titleStyle)
        Dim pages As List(Of XpsPage) = engine.Layout()
        Dim meta As New XpsWriteMeta() With {
            .Author = Author,
            .Title = Title,
            .Subject = Subject,
            .Description = Description,
            .Keywords = If(Tags Is Nothing, "", String.Join(", ", Tags)),
            .Creator = ApplicationName
        }

        Return XpsPackager.Build(pages, _images, font, meta, Flavor)
    End Function

    ''' <summary>
    ''' 保存为 .oxps / .xps 文件（目录不存在时自动创建）。
    ''' </summary>
    ''' <param name="filePath">输出文件路径。</param>
    Public Sub Save(filePath As String)
        Dim data As Byte() = ToBytes()
        Dim dir As String = Path.GetDirectoryName(filePath)

        If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
            Call Directory.CreateDirectory(dir)
        End If

        Call File.WriteAllBytes(filePath, data)
    End Sub

End Class
