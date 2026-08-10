#Region "Microsoft.VisualBasic::60718293a4b5c6d7e8f90123456789, mime\application%pdf\PdfWriter\PdfDocument.vb"

    ' Author:
    ' 
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2026 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

    ' /********************************************************************************/

    ' Class PdfDocument
    ' 
    '     Function: (与 WordDocument 一致的流式 API)
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' PdfDocument.vb - PDF 文档生成器主类
'
' 实现 IDocumentWriter，提供与 WordDocument 完全一致的流式 API 与同形构造
' 函数。调用时仅收集 PdfBlock 内容块与样式状态；Save 时由 PdfLayoutEngine
' 排版分页、PdfWriter 序列化为 PDF 文件。
'
' 页面尺寸以 twips 入参，内部转换为 pt（1 pt = 20 twips）。
' 图片文件缺失时走 [警告] 日志并跳过，不影响其余内容生成。
' ============================================================================

Imports System.IO
Imports Microsoft.VisualBasic.MIME.Office.WordDocument
Imports Microsoft.VisualBasic.MIME.text.markdown

''' <summary>
''' PDF 文档生成器。实现 <see cref="IDocumentWriter"/>，对外提供与
''' <c>WordDocument</c> 完全一致的编程接口，将文本、表格、图片写入并生成 PDF 文件。
''' </summary>
Public Class PdfDocument
    Implements IDocumentWriter

    ' 元数据
    Public Property Author As String = ""
    Public Property Title As String = ""
    Public Property Subject As String = ""
    Public Property Description As String = ""
    Public Property Tags As String() = Nothing
    Public Property ApplicationName As String = "VB.NET PdfDocument Generator"

    ' 页面尺寸（twips）
    Private _pageWidth As Integer = 11906
    Private _pageHeight As Integer = 16838
    Private _marginTop As Integer = 1440
    Private _marginRight As Integer = 1440
    Private _marginBottom As Integer = 1440
    Private _marginLeft As Integer = 1440

    ' 样式状态
    Private _headingStyles(5) As WordStyle
    Private _paragraphStyle As WordStyle
    Private _defaultStyle As WordStyle
    Private _codeStyle As WordStyle
    Private _blockquoteStyle As WordStyle
    Private _titleStyle As WordStyle
    Private _tableStyle As TableStyle

    ' 内容块队列
    Private blocks As New List(Of PdfBlock)()

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

    Public Function HeadingStyle(level As Integer, style As WordStyle) As PdfDocument
        If level >= 1 AndAlso level <= 6 Then _headingStyles(level - 1) = style
        Return Me
    End Function

    Public Function ParagraphStyle(style As WordStyle) As PdfDocument
        _paragraphStyle = style
        Return Me
    End Function

    Public Function DefaultStyle(style As WordStyle) As PdfDocument
        _defaultStyle = style
        Return Me
    End Function

    Public Function TableStyle(style As TableStyle) As PdfDocument
        _tableStyle = style
        Return Me
    End Function

    Public Function CodeStyle(style As WordStyle) As PdfDocument
        _codeStyle = style
        Return Me
    End Function

    Public Function BlockquoteStyle(style As WordStyle) As PdfDocument
        _blockquoteStyle = style
        Return Me
    End Function

    Public Function TitleStyle(style As WordStyle) As PdfDocument
        _titleStyle = style
        Return Me
    End Function

    Public Function HeadingStyle(level As Integer, style As WordStyle) As IDocumentWriter Implements IDocumentWriter.HeadingStyle
        Return HeadingStyle(level, style)
    End Function
    Public Function ParagraphStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.ParagraphStyle
        Return ParagraphStyle(style)
    End Function
    Public Function DefaultStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.DefaultStyle
        Return DefaultStyle(style)
    End Function
    Public Function TableStyle(style As TableStyle) As IDocumentWriter Implements IDocumentWriter.TableStyle
        Return TableStyle(style)
    End Function
    Public Function CodeStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.CodeStyle
        Return CodeStyle(style)
    End Function
    Public Function BlockquoteStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.BlockquoteStyle
        Return BlockquoteStyle(style)
    End Function
    Public Function TitleStyle(style As WordStyle) As IDocumentWriter Implements IDocumentWriter.TitleStyle
        Return TitleStyle(style)
    End Function

    ' ========================================================================
    ' 页面设置（twips）
    ' ========================================================================

    Public Function PageSetup(pageWidth As Integer, pageHeight As Integer,
                              marginTop As Integer, marginRight As Integer,
                              marginBottom As Integer, marginLeft As Integer) As PdfDocument
        _pageWidth = pageWidth
        _pageHeight = pageHeight
        _marginTop = marginTop
        _marginRight = marginRight
        _marginBottom = marginBottom
        _marginLeft = marginLeft
        Return Me
    End Function

    Public Function PageSetupA4() As PdfDocument
        Return PageSetup(11906, 16838, 1440, 1440, 1440, 1440)
    End Function

    Public Function PageSetupLetter() As PdfDocument
        Return PageSetup(12240, 15840, 1440, 1440, 1440, 1440)
    End Function

    Public Function PageSetup(pageWidth As Integer, pageHeight As Integer,
                              marginTop As Integer, marginRight As Integer,
                              marginBottom As Integer, marginLeft As Integer) As IDocumentWriter Implements IDocumentWriter.PageSetup
        Return PageSetup(pageWidth, pageHeight, marginTop, marginRight, marginBottom, marginLeft)
    End Function
    Public Function PageSetupA4() As IDocumentWriter Implements IDocumentWriter.PageSetupA4
        Return PageSetupA4()
    End Function
    Public Function PageSetupLetter() As IDocumentWriter Implements IDocumentWriter.PageSetupLetter
        Return PageSetupLetter()
    End Function

    ' ========================================================================
    ' 内容写入
    ' ========================================================================

    Public Function DocTitle(text As String) As PdfDocument
        Dim b As New PdfBlock() With {.Type = PdfBlockType.Title, .Text = text, .Style = _titleStyle.Clone()}
        blocks.Add(b)
        Return Me
    End Function

    Public Function H1(text As String) As PdfDocument
        Return Heading(1, text)
    End Function
    Public Function H2(text As String) As PdfDocument
        Return Heading(2, text)
    End Function
    Public Function H3(text As String) As PdfDocument
        Return Heading(3, text)
    End Function
    Public Function H4(text As String) As PdfDocument
        Return Heading(4, text)
    End Function
    Public Function H5(text As String) As PdfDocument
        Return Heading(5, text)
    End Function
    Public Function H6(text As String) As PdfDocument
        Return Heading(6, text)
    End Function

    Public Function Heading(level As Integer, text As String) As PdfDocument
        Dim idx = Math.Min(Math.Max(level, 1), 6) - 1
        Dim b As New PdfBlock() With {.Type = PdfBlockType.Heading, .Level = level, .Text = text, .Style = _headingStyles(idx).Clone()}
        blocks.Add(b)
        Return Me
    End Function

    Public Function Paragraph(text As String) As PdfDocument
        Return Paragraph(text, _paragraphStyle)
    End Function

    Public Function Paragraph(text As String, style As WordStyle) As PdfDocument
        Dim b As New PdfBlock() With {.Type = PdfBlockType.Paragraph, .Text = text, .Style = style.Clone()}
        blocks.Add(b)
        Return Me
    End Function

    Public Function CodeBlock(code As String, Optional language As String = "") As PdfDocument
        Dim b As New PdfBlock() With {.Type = PdfBlockType.Code, .Text = code, .Style = _codeStyle.Clone()}
        blocks.Add(b)
        Return Me
    End Function

    Public Function Blockquote(text As String) As PdfDocument
        Dim b As New PdfBlock() With {.Type = PdfBlockType.Quote, .Text = text, .Style = _blockquoteStyle.Clone()}
        blocks.Add(b)
        Return Me
    End Function

    Public Function List(items As String(), Optional ordered As Boolean = False) As PdfDocument
        If items IsNot Nothing Then
            For Each it In items
                Dim b As New PdfBlock() With {.Type = PdfBlockType.List, .Text = it, .Ordered = ordered, .Style = _paragraphStyle.Clone()}
                blocks.Add(b)
            Next
        End If
        Return Me
    End Function

    Public Function TaskList(items As String(), checked As Boolean()) As PdfDocument
        If items IsNot Nothing Then
            For i = 0 To items.Length - 1
                Dim b As New PdfBlock() With {
                    .Type = PdfBlockType.TaskList,
                    .Text = items(i),
                    .Checked = If(checked IsNot Nothing AndAlso i < checked.Length, checked(i), False),
                    .Style = _paragraphStyle.Clone()
                }
                blocks.Add(b)
            Next
        End If
        Return Me
    End Function

    Public Function DefinitionList(terms As String(), definitions As String()) As PdfDocument
        If terms IsNot Nothing Then
            For i = 0 To terms.Length - 1
                Dim b As New PdfBlock() With {
                    .Type = PdfBlockType.DefList,
                    .Term = terms(i),
                    .Text = If(definitions IsNot Nothing AndAlso i < definitions.Length, definitions(i), ""),
                    .Style = _paragraphStyle.Clone()
                }
                blocks.Add(b)
            Next
        End If
        Return Me
    End Function

    Public Function Hr() As PdfDocument
        blocks.Add(New PdfBlock() With {.Type = PdfBlockType.Hr})
        Return Me
    End Function

    Public Function PageBreak() As PdfDocument
        blocks.Add(New PdfBlock() With {.Type = PdfBlockType.PageBreak})
        Return Me
    End Function

    Public Function Toc(Optional maxLevel As Integer = 3) As PdfDocument
        blocks.Add(New PdfBlock() With {.Type = PdfBlockType.Toc, .Level = maxLevel})
        Return Me
    End Function

    Public Function DocTitle(text As String) As IDocumentWriter Implements IDocumentWriter.DocTitle
        Return DocTitle(text)
    End Function
    Public Function H1(text As String) As IDocumentWriter Implements IDocumentWriter.H1
        Return H1(text)
    End Function
    Public Function H2(text As String) As IDocumentWriter Implements IDocumentWriter.H2
        Return H2(text)
    End Function
    Public Function H3(text As String) As IDocumentWriter Implements IDocumentWriter.H3
        Return H3(text)
    End Function
    Public Function H4(text As String) As IDocumentWriter Implements IDocumentWriter.H4
        Return H4(text)
    End Function
    Public Function H5(text As String) As IDocumentWriter Implements IDocumentWriter.H5
        Return H5(text)
    End Function
    Public Function H6(text As String) As IDocumentWriter Implements IDocumentWriter.H6
        Return H6(text)
    End Function
    Public Function Heading(level As Integer, text As String) As IDocumentWriter Implements IDocumentWriter.Heading
        Return Heading(level, text)
    End Function
    Public Function Paragraph(text As String) As IDocumentWriter Implements IDocumentWriter.Paragraph
        Return Paragraph(text)
    End Function
    Public Function Paragraph(text As String, style As WordStyle) As IDocumentWriter Implements IDocumentWriter.Paragraph
        Return Paragraph(text, style)
    End Function
    Public Function CodeBlock(code As String, Optional language As String = "") As IDocumentWriter Implements IDocumentWriter.CodeBlock
        Return CodeBlock(code, language)
    End Function
    Public Function Blockquote(text As String) As IDocumentWriter Implements IDocumentWriter.Blockquote
        Return Blockquote(text)
    End Function
    Public Function List(items As String(), Optional ordered As Boolean = False) As IDocumentWriter Implements IDocumentWriter.List
        Return List(items, ordered)
    End Function
    Public Function TaskList(items As String(), checked As Boolean()) As IDocumentWriter Implements IDocumentWriter.TaskList
        Return TaskList(items, checked)
    End Function
    Public Function DefinitionList(terms As String(), definitions As String()) As IDocumentWriter Implements IDocumentWriter.DefinitionList
        Return DefinitionList(terms, definitions)
    End Function
    Public Function Hr() As IDocumentWriter Implements IDocumentWriter.Hr
        Return Hr()
    End Function
    Public Function PageBreak() As IDocumentWriter Implements IDocumentWriter.PageBreak
        Return PageBreak()
    End Function
    Public Function Toc(Optional maxLevel As Integer = 3) As IDocumentWriter Implements IDocumentWriter.Toc
        Return Toc(maxLevel)
    End Function

    ' ========================================================================
    ' 表格
    ' ========================================================================

    Public Function Table(headers As String(), data As String(,)) As PdfDocument
        Return Table(headers, data, Nothing)
    End Function

    Public Function Table(headers As String(), data As String(,), alignments As String()) As PdfDocument
        Dim rows As New List(Of String())()
        For i = 0 To data.GetLength(0) - 1
            Dim row(data.GetLength(1) - 1) As String
            For j = 0 To data.GetLength(1) - 1
                row(j) = data(i, j)
            Next
            rows.Add(row)
        Next
        Return Table(headers, rows.ToArray(), alignments)
    End Function

    Public Function Table(headers As String(), rows As String()(),
                          Optional alignments As String() = Nothing) As PdfDocument
        Dim b As New PdfBlock() With {
            .Type = PdfBlockType.Table,
            .TableHeaders = If(headers, New String() {}),
            .TableRows = If(rows, New String()() {}),
            .TableAlignments = alignments,
            .TableMode = "equal"
        }
        ' 拷贝一份，避免外部后续修改影响快照
        b.TableHeaders = CloneRows(b.TableHeaders)
        b.TableRows = CloneRows(b.TableRows)
        blocks.Add(b)
        Return Me
    End Function

    Public Function TableAutoFitWindow(headers As String(), rows As String()(),
                                       Optional alignments As String() = Nothing,
                                       Optional center As Boolean = False,
                                       Optional threeLine As Boolean = False) As PdfDocument
        Return AddAutoFitTable("window", headers, rows, alignments, center, threeLine)
    End Function

    Public Function TableAutoFitContents(headers As String(), rows As String()(),
                                         Optional alignments As String() = Nothing,
                                         Optional center As Boolean = False,
                                         Optional threeLine As Boolean = False) As PdfDocument
        Return AddAutoFitTable("contents", headers, rows, alignments, center, threeLine)
    End Function

    Public Function TableAutoFitWindow(headers As String(,), rows As String(,),
                                       Optional alignments As String() = Nothing,
                                       Optional center As Boolean = False,
                                       Optional threeLine As Boolean = False) As PdfDocument
        Return AddAutoFitTable("window", ToJagged(headers), ToJagged(rows), alignments, center, threeLine)
    End Function

    Public Function TableAutoFitContents(headers As String(,), rows As String(,),
                                          Optional alignments As String() = Nothing,
                                          Optional center As Boolean = False,
                                          Optional threeLine As Boolean = False) As PdfDocument
        Return AddAutoFitTable("contents", ToJagged(headers), ToJagged(rows), alignments, center, threeLine)
    End Function

    Private Function AddAutoFitTable(mode As String, headers As String(), rows As String()(),
                                     alignments As String(), center As Boolean, threeLine As Boolean) As PdfDocument
        Dim b As New PdfBlock() With {
            .Type = PdfBlockType.Table,
            .TableHeaders = CloneRows(headers),
            .TableRows = CloneRows(rows),
            .TableAlignments = alignments,
            .TableMode = mode,
            .TableCenter = center,
            .TableThreeLine = threeLine
        }
        blocks.Add(b)
        Return Me
    End Function

    Public Function Table(headers As String(), data As String(,)) As IDocumentWriter Implements IDocumentWriter.Table
        Return Table(headers, data)
    End Function
    Public Function Table(headers As String(), data As String(,), alignments As String()) As IDocumentWriter Implements IDocumentWriter.Table
        Return Table(headers, data, alignments)
    End Function
    Public Function Table(headers As String(), rows As String()(),
                          Optional alignments As String() = Nothing) As IDocumentWriter Implements IDocumentWriter.Table
        Return Table(headers, rows, alignments)
    End Function
    Public Function TableAutoFitWindow(headers As String(), rows As String()(),
                                       Optional alignments As String() = Nothing,
                                       Optional center As Boolean = False,
                                       Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitWindow
        Return TableAutoFitWindow(headers, rows, alignments, center, threeLine)
    End Function
    Public Function TableAutoFitContents(headers As String(), rows As String()(),
                                         Optional alignments As String() = Nothing,
                                         Optional center As Boolean = False,
                                         Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitContents
        Return TableAutoFitContents(headers, rows, alignments, center, threeLine)
    End Function
    Public Function TableAutoFitWindow(headers As String(,), rows As String(,),
                                       Optional alignments As String() = Nothing,
                                       Optional center As Boolean = False,
                                       Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitWindow
        Return TableAutoFitWindow(headers, rows, alignments, center, threeLine)
    End Function
    Public Function TableAutoFitContents(headers As String(,), rows As String(,),
                                         Optional alignments As String() = Nothing,
                                         Optional center As Boolean = False,
                                         Optional threeLine As Boolean = False) As IDocumentWriter Implements IDocumentWriter.TableAutoFitContents
        Return TableAutoFitContents(headers, rows, alignments, center, threeLine)
    End Function

    ' ========================================================================
    ' 图片与 Markdown 块
    ' ========================================================================

    Public Function Image(file As String,
                          Optional width As Double = 0,
                          Optional height As Double = 0,
                          Optional caption As String = "") As PdfDocument
        If Not File.Exists(file) Then
            Console.Error.WriteLine($"[警告] 图片文件不存在: {file}")
            Return Me
        End If
        Dim b As New PdfBlock() With {
            .Type = PdfBlockType.Image,
            .ImagePath = file,
            .ImageWidth = width,
            .ImageHeight = height,
            .ImageCaption = caption
        }
        blocks.Add(b)
        Return Me
    End Function

    Public Function Image(file As String,
                          Optional width As Double = 0,
                          Optional height As Double = 0,
                          Optional caption As String = "") As IDocumentWriter Implements IDocumentWriter.Image
        Return Image(file, width, height, caption)
    End Function

    Public Function WriteBlocks(blocksInput As IEnumerable(Of JSONSchema.Block)) As PdfDocument
        If blocksInput Is Nothing Then Return Me
        For Each blk In blocksInput
            If blk Is Nothing Then Continue For
            WriteBlock(blk)
        Next
        Return Me
    End Function

    Private Sub WriteBlock(blk As JSONSchema.Block)
        ' 将 Markdown 块转换为对应内容块（覆盖常见段落/标题/代码/引用/列表/表格）
        If blk.type Is Nothing Then
            If Not String.IsNullOrEmpty(blk.content) Then
                Paragraph(blk.content)
            End If
            Return
        End If
        Dim t = blk.type.ToLower()
        Select Case t
            Case "heading"
                Dim lvl = If(blk.level >= 1 AndAlso blk.level <= 6, blk.level, 1)
                Heading(lvl, blk.content)
            Case "code"
                CodeBlock(blk.content)
            Case "quote", "blockquote"
                Blockquote(blk.content)
            Case "list", "li", "bulletedlist", "orderedlist"
                If blk.items IsNot Nothing AndAlso blk.items.Length > 0 Then
                    List(blk.items, blk.ordered)
                End If
            Case "tasklist"
                If blk.items IsNot Nothing AndAlso blk.items.Length > 0 Then
                    TaskList(blk.items, blk.checked)
                End If
            Case "deflist"
                If blk.terms IsNot Nothing AndAlso blk.terms.Length > 0 Then
                    DefinitionList(blk.terms, blk.definitions)
                End If
            Case "table"
                If blk.rows IsNot Nothing AndAlso blk.rows.Length > 0 Then
                    Table(If(blk.headers, New String() {}), blk.rows, blk.alignments)
                End If
            Case "hr", "horizontal-rule"
                Hr()
            Case Else
                If Not String.IsNullOrEmpty(blk.content) Then
                    Paragraph(blk.content)
                End If
        End Select
    End Sub

    Public Function WriteBlocks(blocksInput As IEnumerable(Of JSONSchema.Block)) As IDocumentWriter Implements IDocumentWriter.WriteBlocks
        Return WriteBlocks(blocksInput)
    End Function

    ' ========================================================================
    ' 保存
    ' ========================================================================

    Public Sub Save(filePath As String) Implements IDocumentWriter.Save
        ' twips -> pt
        Dim pageWpt = _pageWidth / 20.0
        Dim pageHpt = _pageHeight / 20.0
        Dim mT = _marginTop / 20.0
        Dim mR = _marginRight / 20.0
        Dim mB = _marginBottom / 20.0
        Dim mL = _marginLeft / 20.0

        Dim fonts As New PdfFontResource()
        Dim engine As New PdfLayoutEngine(pageWpt, pageHpt, mT, mR, mB, mL, fonts,
                                          _headingStyles, _paragraphStyle, _codeStyle, _blockquoteStyle, _titleStyle, _tableStyle, blocks)
        Dim render = engine.Layout()

        Dim meta As New PdfWriteMeta() With {
            .Author = Author,
            .Title = Title,
            .Subject = Subject,
            .Keywords = If(Tags Is Nothing, "", String.Join(", ", Tags)),
            .Creator = ApplicationName
        }
        PdfWriter.Save(filePath, render, pageWpt, pageHpt, fonts, meta)
    End Sub

    ' ========================================================================
    ' 辅助
    ' ========================================================================

    Private Shared Function CloneRows(rows As String()()) As String()()
        If rows Is Nothing Then Return New String()() {}
        Dim out As New List(Of String())()
        For Each r In rows
            out.Add(If(r, New String() {}))
        Next
        Return out.ToArray()
    End Function

    Private Shared Function CloneRows(headers As String()) As String()
        If headers Is Nothing Then Return New String() {}
        Return CType(headers.Clone(), String())
    End Function

    Private Shared Function ToJagged(headers As String(,)) As String()
        If headers Is Nothing Then Return New String() {}
        Dim out(headers.GetLength(1) - 1) As String
        For j = 0 To headers.GetLength(1) - 1
            out(j) = headers(0, j)
        Next
        Return out
    End Function

    Private Shared Function ToJagged(rows As String(,)) As String()()
        If rows Is Nothing Then Return New String()() {}
        Dim out(rows.GetLength(0) - 1) As String()
        For i = 0 To rows.GetLength(0) - 1
            Dim row(rows.GetLength(1) - 1) As String
            For j = 0 To rows.GetLength(1) - 1
                row(j) = rows(i, j)
            Next
            out(i) = row
        Next
        Return out
    End Function

End Class
