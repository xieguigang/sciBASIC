#Region "Microsoft.VisualBasic::26ac7721553b15818f6675535239fa7d, mime\application%pdf\PdfWriter\PdfLayoutEngine.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 691
    '    Code Lines: 546 (79.02%)
    ' Comment Lines: 69 (9.99%)
    '    - Xml Docs: 8.70%
    ' 
    '   Blank Lines: 76 (11.00%)
    '     File Size: 30.78 KB


    ' Class PdfRenderResult
    ' 
    ' 
    ' 
    ' Class PdfLayoutEngine
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: DrawWrapped, EscapeLatin, FormatNum, GetAlign, Layout
    '               WrapText
    ' 
    '     Sub: BuildPages, ComputeColumnWidths, DrawCenteredLine, DrawHLine, DrawLine
    '          DrawRect, DrawTableRow, DrawVLine, EmitRun, NewPage
    '          RenderCode, RenderDefList, RenderHeading, RenderHr, RenderImage
    '          RenderList, RenderListItems, RenderParagraph, RenderQuote, RenderTable
    '          RenderTaskList, RenderTitle, RenderToc
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' PdfLayoutEngine.vb - 排版与分页核心
'
' 负责：文本换行测量（中英文混排）、行高与段前段后距推进、首行缩进、
' 对齐、自动分页、表格列宽计算与跨页重绘表头、三线表边框分支、
' 图片尺寸解算与居中、目录两遍布局回填页码。
'
' 内部采用“距页顶距离 y”向下推进的坐标模型，输出时统一转换为
' PDF 左下角原点坐标（pageHeight - y）。
' ============================================================================

Imports System.Text
Imports Microsoft.VisualBasic.MIME.Office.WordDocument

''' <summary>布局结果：每页内容流文本 + 引用的图片对象。</summary>
Public Class PdfRenderResult
    Public Pages As New List(Of String)()
    Public Images As New List(Of PdfImageObject)()
End Class

''' <summary>PDF 内容排版与分页引擎。</summary>
Public Class PdfLayoutEngine

    Private pageW, pageH As Double
    Private mLeft, mRight, mTop, mBottom As Double
    Private contentW As Double

    Private fonts As PdfFontResource
    Private headingStyles() As WordStyle
    Private paraStyle, codeStyle, quoteStyle, titleStyle As WordStyle
    Private tableStyle As TableStyle
    Private blocks As List(Of PdfBlock)

    Private images As New List(Of PdfImageObject)()
    Private imgCounter As Integer = 0

    ' 当前页状态
    Private sb As StringBuilder
    Private y As Double
    Private result As PdfRenderResult

    ' 标题追踪（用于目录）
    Private orderedHeadings As New List(Of (level As Integer, text As String))
    Private headingPages As New List(Of Integer)
    Private tocMaxLevel As Integer = 3
    Private hasToc As Boolean = False

    Public Sub New(pageWidthPt As Double, pageHeightPt As Double,
                   marginTop As Double, marginRight As Double, marginBottom As Double, marginLeft As Double,
                   fontRes As PdfFontResource,
                   headingStyles() As WordStyle, paraStyle As WordStyle, codeStyle As WordStyle,
                   quoteStyle As WordStyle, titleStyle As WordStyle, tableStyle As TableStyle,
                   blocks As List(Of PdfBlock))
        pageW = pageWidthPt
        pageH = pageHeightPt
        mTop = marginTop : mRight = marginRight : mBottom = marginBottom : mLeft = marginLeft
        contentW = pageW - mLeft - mRight
        fonts = fontRes
        Me.headingStyles = headingStyles
        Me.paraStyle = paraStyle
        Me.codeStyle = codeStyle
        Me.quoteStyle = quoteStyle
        Me.titleStyle = titleStyle
        Me.tableStyle = tableStyle
        Me.blocks = blocks

        ' 预扫描标题顺序与目录设置
        For Each b In blocks
            If b.Type = PdfBlockType.Heading Then
                orderedHeadings.Add((b.Level, b.Text))
            ElseIf b.Type = PdfBlockType.Toc Then
                hasToc = True
                tocMaxLevel = If(b.Level > 0, b.Level, 3)
            End If
        Next
    End Sub

    ''' <summary>执行排版：先占位一遍定位标题页码，再正式生成页面内容。</summary>
    Public Function Layout() As PdfRenderResult
        ' 第一遍：确定各标题所在页码（目录也要渲染占位，保证两遍流式一致）
        headingPages.Clear()
        BuildPages(recordHeadingPages:=True, useRealPages:=False)

        ' 第二遍：正式生成页面，目录回填真实页码
        result = New PdfRenderResult()
        BuildPages(recordHeadingPages:=False, useRealPages:=True)
        result.Images = images
        Return result
    End Function

    ' ------------------------------------------------------------------
    ' 页面构建
    ' ------------------------------------------------------------------

    Private Sub BuildPages(recordHeadingPages As Boolean, useRealPages As Boolean)
        sb = New StringBuilder()
        y = mTop
        ' 维护一个临时结果容器
        Dim localResult As New PdfRenderResult()

        For Each b In blocks
            Select Case b.Type
                Case PdfBlockType.Title : RenderTitle(b)
                Case PdfBlockType.Heading : RenderHeading(b, recordHeadingPages, useRealPages, localResult.Pages.Count + 1)
                Case PdfBlockType.Paragraph : RenderParagraph(b)
                Case PdfBlockType.Code : RenderCode(b)
                Case PdfBlockType.Quote : RenderQuote(b)
                Case PdfBlockType.List : RenderList(b)
                Case PdfBlockType.TaskList : RenderTaskList(b)
                Case PdfBlockType.DefList : RenderDefList(b)
                Case PdfBlockType.Hr : RenderHr(b)
                Case PdfBlockType.PageBreak : NewPage(localResult)
                Case PdfBlockType.Toc : RenderToc(b, useRealPages)
                Case PdfBlockType.Table : RenderTable(b, localResult)
                Case PdfBlockType.Image : RenderImage(b)
            End Select
        Next

        localResult.Pages.Add(sb.ToString())
        If useRealPages Then
            result.Pages = localResult.Pages
        End If
    End Sub

    Private Sub NewPage(localResult As PdfRenderResult)
        localResult.Pages.Add(sb.ToString())
        sb = New StringBuilder()
        y = mTop
    End Sub

    ' ------------------------------------------------------------------
    ' 文本工具
    ' ------------------------------------------------------------------

    ''' <summary>把字符串按最大宽度换行（中英文混排，CJK 可任意断行，西文按空格断词）。</summary>
    Private Shared Function WrapText(text As String, maxWidth As Double, size As Double) As List(Of String)
        Dim lines As New List(Of String)()
        If String.IsNullOrEmpty(text) Then
            lines.Add("")
            Return lines
        End If
        Dim paras = text.Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
        For Each para In paras
            If para.Length = 0 Then
                lines.Add("")
                Continue For
            End If
            Dim line As New StringBuilder()
            Dim lineW As Double = 0
            Dim lastSpace As Integer = -1
            For i = 0 To para.Length - 1
                Dim c = para(i)
                Dim cw = PdfFontResource.MeasureText(c.ToString(), size)
                If lineW + cw > maxWidth AndAlso line.Length > 0 Then
                    ' 尝试在西文空格处断行
                    If lastSpace >= 0 AndAlso Not PdfFontResource.IsCJK(c) Then
                        lines.Add(line.ToString().Substring(0, lastSpace).TrimEnd())
                        line = New StringBuilder(line.ToString().Substring(lastSpace + 1))
                        lineW = PdfFontResource.MeasureText(line.ToString(), size)
                        lastSpace = -1
                    Else
                        lines.Add(line.ToString())
                        line = New StringBuilder()
                        lineW = 0
                        lastSpace = -1
                    End If
                End If
                line.Append(c)
                lineW += cw
                If c = " "c Then lastSpace = line.Length - 1
            Next
            lines.Add(line.ToString())
        Next
        Return lines
    End Function

    ''' <summary>绘制一段文本（自动换行）。返回占用的垂直高度。</summary>
    Private Function DrawWrapped(text As String, x As Double, style As WordStyle,
                                 Optional firstIndent As Double = 0,
                                 Optional maxW As Double = -1) As Double
        If maxW < 0 Then maxW = contentW
        Dim size = If(style.Size > 0, style.Size, 11)
        Dim lineH = size * (If(style.LineSpacing > 0, style.LineSpacing, 1.2))
        Dim lines = WrapText(text, maxW - firstIndent, size)
        Dim colorHex = If(String.IsNullOrEmpty(style.ForeColor), "", style.ForeColor)
        Dim fill = PdfColor.FromHex(colorHex).ToFill()

        For i = 0 To lines.Count - 1
            Dim indent = If(i = 0, firstIndent, 0)
            Dim lx = x + indent
            DrawLine(lines(i), lx, y + size, style, fill, size)
            y += lineH
        Next
        Return lines.Count * lineH
    End Function

    ''' <summary>绘制单行文本（含 CJK 字体切换）。</summary>
    Private Sub DrawLine(text As String, x As Double, baselineY As Double, style As WordStyle, fill As String, size As Double)
        If String.IsNullOrEmpty(text) Then Return
        Dim py = pageH - baselineY
        Dim bold = style.Bold
        Dim italic = style.Italic
        ' 拆成 CJK / 西文 run
        Dim i = 0
        Dim runStart = 0
        Dim curIsCJK = PdfFontResource.IsCJK(text(0))
        Do While i < text.Length
            Dim isC = PdfFontResource.IsCJK(text(i))
            If isC <> curIsCJK Then
                EmitRun(text.Substring(runStart, i - runStart), x, py, curIsCJK, bold, italic, fill, size)
                ' 推进 x
                x += PdfFontResource.MeasureText(text.Substring(runStart, i - runStart), size)
                runStart = i
                curIsCJK = isC
            End If
            i += 1
        Loop
        If runStart < text.Length Then
            EmitRun(text.Substring(runStart), x, py, curIsCJK, bold, italic, fill, size)
        End If
    End Sub

    Private Sub EmitRun(runText As String, x As Double, py As Double, isCJK As Boolean, bold As Boolean, italic As Boolean, fill As String, size As Double)
        If String.IsNullOrEmpty(runText) Then Return
        If isCJK Then
            Dim f = fonts.CJKFontName()
            sb.Append("BT ").Append(f).Append(" ").Append(FormatNum(size)).Append(" Tf ")
            sb.Append(fill).Append(" ")
            sb.Append("1 0 0 1 ").Append(FormatNum(x)).Append(" ").Append(FormatNum(py)).Append(" Tm <")
            sb.Append(PdfFontResource.EncodeCJKHex(runText))
            sb.Append("> Tj ET").Append(vbCrLf)
        Else
            Dim f = fonts.LatinFont(bold, italic, False)
            sb.Append("BT ").Append(f).Append(" ").Append(FormatNum(size)).Append(" Tf ")
            sb.Append(fill).Append(" ")
            sb.Append("1 0 0 1 ").Append(FormatNum(x)).Append(" ").Append(FormatNum(py)).Append(" Tm (")
            sb.Append(EscapeLatin(runText))
            sb.Append(") Tj ET").Append(vbCrLf)
        End If
    End Sub

    Private Shared Function EscapeLatin(s As String) As String
        Return s.Replace("\", "\\").Replace("(", "\(").Replace(")", "\)")
    End Function

    Private Shared Function FormatNum(v As Double) As String
        If v = 0 Then Return "0"
        If System.Math.Abs(v) < 0.001 Then Return "0"
        Return v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
    End Function

    ' ------------------------------------------------------------------
    ' 各块渲染
    ' ------------------------------------------------------------------

    Private Sub RenderTitle(b As PdfBlock)
        Dim s = titleStyle
        y += If(s.SpaceBefore > 0, s.SpaceBefore, 0)
        Dim size = If(s.Size > 0, s.Size, 36)
        Dim lineH = size * (If(s.LineSpacing > 0, s.LineSpacing, 1.2))
        Dim cx = mLeft + contentW / 2
        Dim colorHex = If(String.IsNullOrEmpty(s.ForeColor), "000000", s.ForeColor)
        Dim fill = PdfColor.FromHex(colorHex).ToFill()
        DrawCenteredLine(b.Text, cx, y + size, s, fill, size)
        y += lineH + If(s.SpaceAfter > 0, s.SpaceAfter, 12)
    End Sub

    Private Sub DrawCenteredLine(text As String, centerX As Double, baselineY As Double, style As WordStyle, fill As String, size As Double)
        Dim w = PdfFontResource.MeasureText(text, size)
        Dim x = centerX - w / 2
        DrawLine(text, x, baselineY, style, fill, size)
    End Sub

    Private Sub RenderHeading(b As PdfBlock, recordHeadingPages As Boolean, useRealPages As Boolean, pageNo As Integer)
        If recordHeadingPages Then
            headingPages.Add(pageNo)
        End If
        Dim s = headingStyles(System.Math.Min(System.Math.Max(b.Level, 1), 6) - 1)
        y += If(s.SpaceBefore > 0, s.SpaceBefore, 6)
        Dim size = If(s.Size > 0, s.Size, 14)
        Dim lineH = size * (If(s.LineSpacing > 0, s.LineSpacing, 1.2))
        Dim colorHex = If(String.IsNullOrEmpty(s.ForeColor), "000000", s.ForeColor)
        Dim fill = PdfColor.FromHex(colorHex).ToFill()
        DrawLine(b.Text, mLeft, y + size, s, fill, size)
        y += lineH + If(s.SpaceAfter > 0, s.SpaceAfter, 4)
    End Sub

    Private Sub RenderParagraph(b As PdfBlock)
        Dim s = b.Style
        y += If(s.SpaceBefore > 0, s.SpaceBefore, 0)
        Dim indent = If(s.FirstLineIndent > 0, s.FirstLineIndent, 0)
        DrawWrapped(b.Text, mLeft, s, indent)
        y += If(s.SpaceAfter > 0, s.SpaceAfter, 6)
    End Sub

    Private Sub RenderCode(b As PdfBlock)
        Dim s = codeStyle
        y += If(s.SpaceBefore > 0, s.SpaceBefore, 6)
        Dim size = If(s.Size > 0, s.Size, 10)
        Dim lineH = size * 1.2
        Dim lines = b.Text.Split({vbCrLf, vbLf}, StringSplitOptions.None)
        Dim codeW = contentW - 8
        Dim blockH = lines.Length * lineH + 8

        ' 背景矩形
        Dim bg = PdfColor.FromHex(If(String.IsNullOrEmpty(s.BackColor), "F5F5F5", s.BackColor))
        DrawRect(mLeft, y, contentW, blockH, bg.ToFill())

        Dim colorHex = If(String.IsNullOrEmpty(s.ForeColor), "000000", s.ForeColor)
        Dim fill = PdfColor.FromHex(colorHex).ToFill()
        Dim x = mLeft + 4
        For i = 0 To lines.Length - 1
            DrawLine(lines(i), x, y + size + i * lineH, s, fill, size)
        Next
        y += blockH + If(s.SpaceAfter > 0, s.SpaceAfter, 6)
    End Sub

    Private Sub RenderQuote(b As PdfBlock)
        Dim s = quoteStyle
        y += If(s.SpaceBefore > 0, s.SpaceBefore, 6)
        Dim size = If(s.Size > 0, s.Size, 11)
        Dim indent = 18 ' 左缩进 0.25 英寸
        ' 左侧竖线
        DrawVLine(mLeft + 2, y, PdfColor.FromHex("4472C4").ToStroke())
        ' 背景
        Dim bg = PdfColor.FromHex(If(String.IsNullOrEmpty(s.BackColor), "FFF8E1", s.BackColor))
        Dim text = b.Text
        Dim lines = WrapText(text, contentW - indent - 8, size)
        Dim blockH = lines.Count * (size * (If(s.LineSpacing > 0, s.LineSpacing, 1.2))) + 6
        DrawRect(mLeft, y, contentW, blockH, bg.ToFill())
        Dim colorHex = If(String.IsNullOrEmpty(s.ForeColor), "000000", s.ForeColor)
        Dim fill = PdfColor.FromHex(colorHex).ToFill()
        Dim lh = size * (If(s.LineSpacing > 0, s.LineSpacing, 1.2))
        For i = 0 To lines.Count - 1
            DrawLine(lines(i), mLeft + indent, y + size + i * lh, s, fill, size)
        Next
        y += blockH + If(s.SpaceAfter > 0, s.SpaceAfter, 6)
    End Sub

    Private Sub RenderList(b As PdfBlock)
        RenderListItems(New String() {b.Text}, b.Ordered, Nothing, False)
    End Sub

    Private Sub RenderTaskList(b As PdfBlock)
        RenderListItems(New String() {b.Text}, False, New Boolean() {b.Checked}, True)
    End Sub

    Private Sub RenderListItems(items As String(), ordered As Boolean, checkedStates As Boolean(), task As Boolean)
        Dim s = paraStyle
        y += If(s.SpaceBefore > 0, s.SpaceBefore, 0)
        Dim size = If(s.Size > 0, s.Size, 11)
        Dim lh = size * (If(s.LineSpacing > 0, s.LineSpacing, 1.2))
        Dim indent = 18
        Dim markerW = 18
        For i = 0 To items.Length - 1
            Dim prefix As String
            If task Then
                prefix = If(checkedStates(i), "[x] ", "[ ] ")
            ElseIf ordered Then
                prefix = (i + 1).ToString() & ". "
            Else
                prefix = "• "
            End If
            Dim lines = WrapText(items(i), contentW - indent - markerW, size)
            ' 标记
            DrawLine(prefix, mLeft + indent, y + size, s, PdfColor.FromHex("000000").ToFill(), size)
            For j = 0 To lines.Count - 1
                Dim lx = mLeft + indent + markerW
                DrawLine(lines(j), lx, y + size + j * lh, s, PdfColor.FromHex(If(String.IsNullOrEmpty(s.ForeColor), "000000", s.ForeColor)).ToFill(), size)
            Next
            y += lines.Count * lh
        Next
        y += If(s.SpaceAfter > 0, s.SpaceAfter, 6)
    End Sub

    Private Sub RenderDefList(b As PdfBlock)
        Dim s = paraStyle
        y += If(s.SpaceBefore > 0, s.SpaceBefore, 0)
        Dim size = If(s.Size > 0, s.Size, 11)
        Dim lh = size * (If(s.LineSpacing > 0, s.LineSpacing, 1.2))
        Dim boldTerm = New WordStyle With {.Bold = True, .Size = size, .ForeColor = s.ForeColor, .FontName = s.FontName, .FontNameEastAsia = s.FontNameEastAsia}
        DrawLine(b.Term, mLeft + 12, y + size, boldTerm, PdfColor.FromHex(If(String.IsNullOrEmpty(s.ForeColor), "000000", s.ForeColor)).ToFill(), size)
        y += lh
        Dim lines = WrapText(b.Text, contentW - 24, size)
        For j = 0 To lines.Count - 1
            DrawLine(lines(j), mLeft + 24, y + size + j * lh, s, PdfColor.FromHex(If(String.IsNullOrEmpty(s.ForeColor), "000000", s.ForeColor)).ToFill(), size)
        Next
        y += lines.Count * lh + If(s.SpaceAfter > 0, s.SpaceAfter, 6)
    End Sub

    Private Sub RenderHr(b As PdfBlock)
        y += 4
        DrawHLine(mLeft, y, contentW, PdfColor.FromHex("BFBFBF").ToStroke())
        y += 8
    End Sub

    Private Sub RenderToc(b As PdfBlock, useRealPages As Boolean)
        Dim size = 12.0
        Dim lh = size * 1.4
        y += 6
        DrawLine("目录", mLeft, y + size, New WordStyle With {.Bold = True, .Size = size, .FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei"}, PdfColor.FromHex("000000").ToFill(), size)
        y += lh + 4

        ' 过滤出符合 maxLevel 的标题（按预扫描顺序）
        Dim idx = 0
        For hi = 0 To orderedHeadings.Count - 1
            If orderedHeadings(hi).level > tocMaxLevel Then Continue For
            Dim entryText = orderedHeadings(hi).text
            Dim pageNum As Integer = 0
            If useRealPages AndAlso idx < headingPages.Count Then
                pageNum = headingPages(idx)
            End If
            Dim lead = New String(" "c, orderedHeadings(hi).level - 1)
            Dim label = lead & entryText
            ' 标题 + 前导点 + 页码
            DrawLine(label, mLeft + 8, y + size, New WordStyle With {.Size = size, .FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei"}, PdfColor.FromHex("000000").ToFill(), size)
            If pageNum > 0 Then
                Dim pg = pageNum.ToString()
                Dim pw = PdfFontResource.MeasureText(pg, size)
                DrawLine(pg, mLeft + contentW - pw, y + size, New WordStyle With {.Size = size, .FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei"}, PdfColor.FromHex("000000").ToFill(), size)
            End If
            y += lh
            idx += 1
        Next
        y += 8
    End Sub

    ' ------------------------------------------------------------------
    ' 表格
    ' ------------------------------------------------------------------

    Private Sub RenderTable(b As PdfBlock, localResult As PdfRenderResult)
        y += 6
        Dim headers = b.TableHeaders
        Dim rows = b.TableRows
        Dim n = headers.Length
        Dim align = b.TableAlignments

        ' 计算列宽
        Dim colW(n - 1) As Double
        ComputeColumnWidths(headers, rows, b.TableMode, colW)
        Dim totalW = 0.0
        For i = 0 To n - 1 : totalW += colW(i) : Next

        Dim startX = mLeft
        If b.TableCenter Then startX = mLeft + (contentW - totalW) / 2

        Dim pad = 4.0
        Dim headerSize = 10.0
        Dim cellSize = 10.0
        Dim lineH = cellSize * 1.2

        ' 表头
        DrawTableRow(headers, startX, colW, pad, lineH, cellSize, align,
                     headerBack:=PdfColor.FromHex(If(String.IsNullOrEmpty(tableStyle.HeaderBackColor), "4472C4", tableStyle.HeaderBackColor)),
                     headerFore:=PdfColor.FromHex(If(String.IsNullOrEmpty(tableStyle.HeaderForeColor), "FFFFFF", tableStyle.HeaderForeColor)),
                     isHeader:=True, threeLine:=b.TableThreeLine, bold:=tableStyle.HeaderBold)

        ' 数据行
        For r = 0 To rows.Length - 1
            Dim row = rows(r)
            ' 该行的行高
            Dim maxLines = 1
            For c = 0 To n - 1
                Dim cellText = If(c < row.Length, If(row(c) Is Nothing, "", row(c)), "")
                Dim lines = WrapText(cellText, colW(c) - 2 * pad, cellSize)
                If lines.Count > maxLines Then maxLines = lines.Count
            Next
            Dim rowH = maxLines * lineH + 2 * pad

            If y + rowH > pageH - mBottom Then
                ' 换页并重复表头
                NewPage(localResult)
                DrawTableRow(headers, startX, colW, pad, lineH, cellSize, align,
                             headerBack:=PdfColor.FromHex(If(String.IsNullOrEmpty(tableStyle.HeaderBackColor), "4472C4", tableStyle.HeaderBackColor)),
                             headerFore:=PdfColor.FromHex(If(String.IsNullOrEmpty(tableStyle.HeaderForeColor), "FFFFFF", tableStyle.HeaderForeColor)),
                             isHeader:=True, threeLine:=b.TableThreeLine, bold:=tableStyle.HeaderBold)
            End If

            ' 隔行底色
            If tableStyle.AltRowBackColor <> "" AndAlso (r Mod 2 = 1) Then
                DrawRect(startX, y, totalW, rowH, PdfColor.FromHex(tableStyle.AltRowBackColor).ToFill())
            End If

            ' 单元格文本
            Dim cx = startX
            For c = 0 To n - 1
                Dim cellText = If(c < row.Length, If(row(c) Is Nothing, "", row(c)), "")
                Dim lines = WrapText(cellText, colW(c) - 2 * pad, cellSize)
                Dim al = GetAlign(align, c)
                Dim tx = cx + pad
                If al = "right" Then
                    tx = cx + colW(c) - pad - PdfFontResource.MeasureText(cellText, cellSize)
                ElseIf al = "center" Then
                    tx = cx + (colW(c) - PdfFontResource.MeasureText(cellText, cellSize)) / 2
                End If
                Dim fill = PdfColor.FromHex("000000").ToFill()
                For li = 0 To lines.Count - 1
                    DrawLine(lines(li), tx, y + cellSize + pad + li * lineH, New WordStyle With {.Size = cellSize, .FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei"}, fill, cellSize)
                Next
                cx += colW(c)
            Next

            y += rowH
        Next

        ' 表格外边框 / 三线表底线
        DrawHLine(startX, y, totalW, PdfColor.FromHex(If(String.IsNullOrEmpty(tableStyle.BorderColor), "BFBFBF", tableStyle.BorderColor)).ToStroke())
        y += 8
    End Sub

    Private Sub DrawTableRow(headers As String(), x As Double, colW() As Double, pad As Double, lineH As Double, size As Double, align As String(),
                             headerBack As PdfColor, headerFore As PdfColor, isHeader As Boolean, threeLine As Boolean, bold As Boolean)
        Dim maxLines = 1
        For c = 0 To headers.Length - 1
            Dim lines = WrapText(headers(c), colW(c) - 2 * pad, size)
            If lines.Count > maxLines Then maxLines = lines.Count
        Next
        Dim rowH = maxLines * lineH + 2 * pad

        If Not threeLine Then
            ' 表头底色
            DrawRect(x, y, colW.Sum(), rowH, headerBack.ToFill())
        End If

        Dim cx = x
        For c = 0 To headers.Length - 1
            Dim al = GetAlign(align, c)
            Dim tx = cx + pad
            Dim w = PdfFontResource.MeasureText(headers(c), size)
            If al = "right" Then tx = cx + colW(c) - pad - w
            If al = "center" Then tx = cx + (colW(c) - w) / 2
            Dim fill = If(threeLine, PdfColor.FromHex("000000").ToFill(), headerFore.ToFill())
            DrawLine(headers(c), tx, y + size + pad, New WordStyle With {.Bold = bold, .Size = size, .FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei"}, fill, size)
            cx += colW(c)
        Next
        y += rowH

        If threeLine Then
            ' 表头下沿线
            DrawHLine(x, y, colW.Sum(), PdfColor.FromHex("000000").ToStroke())
        End If
    End Sub

    Private Sub ComputeColumnWidths(headers As String(), rows As String()(), mode As String, ByRef colW() As Double)
        Dim n = headers.Length
        For i = 0 To n - 1 : colW(i) = 0 : Next

        If mode = "equal" OrElse mode = "window" Then
            Dim w = contentW / n
            For i = 0 To n - 1 : colW(i) = w : Next
            Return
        End If

        ' contents 模式：按内容宽度分配
        For i = 0 To n - 1
            Dim maxW = PdfFontResource.MeasureText(headers(i), 10)
            If rows IsNot Nothing Then
                For r = 0 To rows.Length - 1
                    If r < rows.Length AndAlso i < rows(r).Length AndAlso rows(r)(i) IsNot Nothing Then
                        Dim cw = PdfFontResource.MeasureText(rows(r)(i), 10)
                        If cw > maxW Then maxW = cw
                    End If
                Next
            End If
            colW(i) = maxW + 8 ' 加内边距
        Next
        Dim total = 0.0
        For i = 0 To n - 1 : total += colW(i) : Next
        If total > contentW Then
            Dim scale = contentW / total
            For i = 0 To n - 1 : colW(i) *= scale : Next
        End If
    End Sub

    Private Shared Function GetAlign(align As String(), i As Integer) As String
        If align Is Nothing OrElse i >= align.Length Then Return "left"
        Select Case align(i).ToLower()
            Case "left" : Return "left"
            Case "center" : Return "center"
            Case "right" : Return "right"
            Case Else : Return "left"
        End Select
    End Function

    ' ------------------------------------------------------------------
    ' 图片
    ' ------------------------------------------------------------------

    Private Sub RenderImage(b As PdfBlock)
        Dim img = PdfImageXObject.GetOrCreate(b.ImagePath)
        If img Is Nothing Then Return ' 已告警

        ' 分配资源名
        Dim name = ""
        For Each ex In images
            If ex Is img Then name = ex.Name
        Next
        If name = "" Then
            imgCounter += 1
            img.Name = "Img" & imgCounter
            name = img.Name
            images.Add(img)
        End If

        ' 解算显示尺寸（pt）
        Dim nativeWpt, nativeHpt As Double
        If img.Width > 0 AndAlso img.Height > 0 Then
            nativeWpt = img.Width * 0.75
            nativeHpt = img.Height * 0.75
        Else
            nativeWpt = contentW * 0.75
            nativeHpt = nativeWpt * 0.75 ' 4:3 兜底
        End If

        Dim w = b.ImageWidth
        Dim h = b.ImageHeight
        If w <= 0 AndAlso h <= 0 Then
            w = nativeWpt : h = nativeHpt
        ElseIf w <= 0 Then
            w = h * (nativeWpt / nativeHpt)
        ElseIf h <= 0 Then
            h = w * (nativeHpt / nativeWpt)
        End If

        ' 限制到可打印区域
        If w > contentW Then
            h = h * (contentW / w)
            w = contentW
        End If
        If h > (pageH - mTop - mBottom) Then
            w = w * ((pageH - mTop - mBottom) / h)
            h = pageH - mTop - mBottom
        End If

        ' 居中
        Dim x = mLeft + (contentW - w) / 2
        Dim drawY = y
        ' 图片绘制（左下角）
        Dim pdfBottom = pageH - (drawY + h)
        sb.Append("q ").Append(FormatNum(w)).Append(" 0 0 ").Append(FormatNum(h)).Append(" ")
        sb.Append(FormatNum(x)).Append(" ").Append(FormatNum(pdfBottom)).Append(" cm /")
        sb.Append(name).Append(" Do Q").Append(vbCrLf)

        y += h + 2

        ' 图注
        If Not String.IsNullOrEmpty(b.ImageCaption) Then
            Dim capStyle = New WordStyle With {.Size = 9, .Italic = True, .FontName = "Microsoft YaHei", .FontNameEastAsia = "Microsoft YaHei", .ForeColor = "595959"}
            Dim capLines = WrapText(b.ImageCaption, contentW, 9)
            Dim capH = capLines.Count * (9 * 1.2)
            For i = 0 To capLines.Count - 1
                DrawCenteredLine(capLines(i), mLeft + contentW / 2, y + 9 + i * (9 * 1.2), capStyle, PdfColor.FromHex("595959").ToFill(), 9)
            Next
            y += capH + 4
        End If
        y += 6
    End Sub

    ' ------------------------------------------------------------------
    ' 几何绘制
    ' ------------------------------------------------------------------

    Private Sub DrawRect(x As Double, yTop As Double, w As Double, h As Double, fill As String)
        If h <= 0 OrElse w <= 0 Then Return
        Dim bottom = pageH - (yTop + h)
        sb.Append(fill).Append(" ")
        sb.Append(FormatNum(x)).Append(" ").Append(FormatNum(bottom)).Append(" ")
        sb.Append(FormatNum(w)).Append(" ").Append(FormatNum(h)).Append(" re f").Append(vbCrLf)
    End Sub

    Private Sub DrawHLine(x As Double, yPos As Double, w As Double, stroke As String)
        Dim py = pageH - yPos
        sb.Append(stroke).Append(" 0.5 w ")
        sb.Append(FormatNum(x)).Append(" ").Append(FormatNum(py)).Append(" m ")
        sb.Append(FormatNum(x + w)).Append(" ").Append(FormatNum(py)).Append(" l S").Append(vbCrLf)
    End Sub

    Private Sub DrawVLine(x As Double, yTop As Double, stroke As String)
        Dim py1 = pageH - yTop
        Dim py2 = pageH - (yTop + 20)
        sb.Append(stroke).Append(" 1 w ")
        sb.Append(FormatNum(x)).Append(" ").Append(FormatNum(py1)).Append(" m ")
        sb.Append(FormatNum(x)).Append(" ").Append(FormatNum(py2)).Append(" l S").Append(vbCrLf)
    End Sub

    ' ------------------------------------------------------------------
    ' 页码
    ' ------------------------------------------------------------------

End Class
