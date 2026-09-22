' ============================================================================
' RtfSerializer.vb - RTF 1.x 序列化器
'
' 把 RtfDocument 收集到的 RtfBlock 队列一次性渲染为符合 RTF 1.x 规范的文本：
'   - 文档头 \\rtf1\\ansi\\ansicpg936\\deff0\\deflang1033\\deflangfe2052\\uc1
'   - 字体表 fonttbl：表项序号与 \\fN 严格对齐；东亚字体使用 fcharset134
'   - 颜色表 colortbl：索引 0 保留给 \\cf0 自动色，自定义色自索引 1 起
'   - 生成器与 \\info 元数据（title / subject / author / operator / keywords / doccomm）
'   - 段落：\\pard \\qc/\\qr/\\qj/\\ql、\\li/\\fi/\\tx、\\sl/\\slmult1、\\sb/\\sa、\\cbpat
'   - 字符：\\plain \\fN\\afM \\fs<磅×2> \\b \\i \\ul \\cfN \\chcbpatN
'   - 表格：\\trowd\\trgaph\\trleft\\cellx ... \\intbl\\cell ... \\row（含 \\trhdr 表头行、
'     三线表边框控制、等宽 / 按窗口 / 按内容三种列宽分配）
'   - 图片：{\\pict\\pngblip|\\jpegblip\\picw\\pich\\picwgoal\\pichgoal <十六进制流>}
'
' 非 ASCII 字符一律以 \\uN?（有符号 16 位）输出，因此输出文件本身是纯 ASCII 文本，
' 在任何打开环境下中文都能正确显示；图片十六进制转换使用查表 + 预分配缓冲避免 O(n^2)。
' ============================================================================

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MIME.Office.WordDocument

''' <summary>
''' RTF 1.x 序列化器：把 <see cref="RtfBlock"/> 队列渲染为 RTF 文本并落盘。
''' </summary>
''' <remarks>
''' 一般不需要直接使用本类，调用 <see cref="RtfDocument.Save"/> 即可。
''' </remarks>
Public Class RtfSerializer

    ''' <summary>1 磅 = 20 twips。</summary>
    Private Const TwipsPerPoint As Double = 20.0

    ''' <summary>1 像素 = 15 twips（按 96 dpi 换算）。</summary>
    Private Const TwipsPerPixel As Double = 15.0

    ''' <summary>列表缩进（twips）。</summary>
    Private Const ListIndent As Integer = 360

    ''' <summary>单列最小宽度（twips）。</summary>
    Private Const MinColumnWidth As Integer = 600

    Private Shared ReadOnly HexDigits As Char() = "0123456789abcdef".ToCharArray()

    Private ReadOnly blocks As List(Of RtfBlock)
    Private ReadOnly pageWidth As Integer
    Private ReadOnly marginLeft As Integer
    Private ReadOnly marginRight As Integer

    Private ReadOnly headingStyles As WordStyle()
    Private ReadOnly paragraphStyle As WordStyle
    Private ReadOnly codeStyle As WordStyle
    Private ReadOnly blockquoteStyle As WordStyle
    Private ReadOnly titleStyle As WordStyle
    Private ReadOnly tableStyle As TableStyle
    Private ReadOnly meta As RtfWriteMeta

    ' 渲染期状态
    Private ReadOnly fonts As New FontTable()
    Private ReadOnly colors As New ColorTable()
    Private ReadOnly content As New StringBuilder(8192)
    Private ReadOnly headings As New List(Of RtfBlock)()
    Private orderedIndex As Integer = 0

    Private Sub New(blocks As List(Of RtfBlock),
                    pageWidth As Integer, pageHeight As Integer,
                    marginTop As Integer, marginRight As Integer,
                    marginBottom As Integer, marginLeft As Integer,
                    headingStyles As WordStyle(), paragraphStyle As WordStyle, defaultStyle As WordStyle,
                    codeStyle As WordStyle, blockquoteStyle As WordStyle, titleStyle As WordStyle,
                    tableStyle As TableStyle, meta As RtfWriteMeta)

        Me.blocks = blocks
        Me.pageWidth = pageWidth
        Me.marginLeft = marginLeft
        Me.marginRight = marginRight
        Me.headingStyles = headingStyles
        Me.paragraphStyle = If(paragraphStyle, New WordStyle())
        Me.codeStyle = If(codeStyle, Me.paragraphStyle)
        Me.blockquoteStyle = If(blockquoteStyle, Me.paragraphStyle)
        Me.titleStyle = If(titleStyle, Me.paragraphStyle)
        Me.tableStyle = If(tableStyle, New TableStyle())
        Me.meta = meta

        ' 预登记默认字体，保证 \deff0 指向有效的字体表项
        Call fonts.Lookup(If(defaultStyle Is Nothing, Me.paragraphStyle.FontName, defaultStyle.FontName))
    End Sub

    ''' <summary>
    ''' 将内容块队列序列化为 .rtf 文件。
    ''' </summary>
    ''' <param name="filePath">输出的 .rtf 文件路径（目录不存在时会自动创建）。</param>
    Public Shared Sub Save(filePath As String, blocks As List(Of RtfBlock),
                           pageWidth As Integer, pageHeight As Integer,
                           marginTop As Integer, marginRight As Integer,
                           marginBottom As Integer, marginLeft As Integer,
                           headingStyles As WordStyle(), paragraphStyle As WordStyle, defaultStyle As WordStyle,
                           codeStyle As WordStyle, blockquoteStyle As WordStyle, titleStyle As WordStyle,
                           tableStyle As TableStyle, meta As RtfWriteMeta)

        Dim serializer As New RtfSerializer(blocks, pageWidth, pageHeight,
                                            marginTop, marginRight, marginBottom, marginLeft,
                                            headingStyles, paragraphStyle, defaultStyle,
                                            codeStyle, blockquoteStyle, titleStyle, tableStyle, meta)
        Call serializer.Render()

        Dim dir As String = Path.GetDirectoryName(filePath)
        If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
            Call Directory.CreateDirectory(dir)
        End If

        ' 输出文本已全部转义为 ASCII，使用无 BOM 的 UTF-8 保证任何残留非 ASCII 也不会丢失
        Call File.WriteAllText(filePath, serializer.BuildDocument(), New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))
    End Sub

    ' ========================================================================
    ' 渲染
    ' ========================================================================

    Private Sub Render()
        Call CollectHeadings()

        For Each block As RtfBlock In blocks
            Call RenderBlock(block)
        Next
    End Sub

    ''' <summary>收集所有标题块，供 \Toc 展开为静态目录。</summary>
    Private Sub CollectHeadings()
        For Each block As RtfBlock In blocks
            If block.Type = RtfBlockType.Heading Then
                headings.Add(block)
            End If
        Next
    End Sub

    Private Sub RenderBlock(block As RtfBlock)
        If block.Type <> RtfBlockType.List Then orderedIndex = 0

        Select Case block.Type
            Case RtfBlockType.Title
                Call WriteParagraphCore(block.Style, block.Text, 0, 0, 0)
            Case RtfBlockType.Heading
                Call WriteParagraphCore(block.Style, block.Text, 0, 0, 0)
            Case RtfBlockType.Paragraph
                Call WriteParagraphCore(block.Style, block.Text, 0, 0, 0)
            Case RtfBlockType.Code
                Call WriteParagraphCore(block.Style, block.Text, 0, 0, 0)
            Case RtfBlockType.Quote
                Call WriteParagraphCore(block.Style, block.Text, 360, 0, 0)
            Case RtfBlockType.List
                Call WriteListItem(block)
            Case RtfBlockType.TaskList
                Call WriteTaskListItem(block)
            Case RtfBlockType.DefList
                Call WriteDefinitionListItem(block)
            Case RtfBlockType.Hr
                Call WriteHorizontalRule()
            Case RtfBlockType.PageBreak
                Call WritePageBreak()
            Case RtfBlockType.Toc
                Call WriteToc(block.Level)
            Case RtfBlockType.Table
                Call WriteTable(block)
            Case RtfBlockType.Image
                Call WriteImage(block)
        End Select
    End Sub

    ' ========================================================================
    ' 段落 / 字符属性
    ' ========================================================================

    Private Shared Function ToTwips(point As Double) As Integer
        If point <= 0 Then Return 0
        Return CInt(System.Math.Round(point * TwipsPerPoint))
    End Function

    ''' <summary>生成段落属性（\pard 及对齐、缩进、行距、段间距、底纹）。</summary>
    Private Function ParagraphProps(style As WordStyle, indent As Integer, hanging As Integer, tabStop As Integer) As String
        Dim sb As New StringBuilder("\pard")

        Select Case Strings.LCase(If(style Is Nothing, "left", If(style.Alignment, "left"))).Trim()
            Case "center"
                Call sb.Append("\qc")
            Case "right"
                Call sb.Append("\qr")
            Case "justify", "both"
                Call sb.Append("\qj")
            Case Else
                Call sb.Append("\ql")
        End Select

        If indent > 0 Then Call sb.Append("\li").Append(indent)
        If tabStop > 0 Then Call sb.Append("\tx").Append(tabStop)
        If hanging > 0 Then Call sb.Append("\fi-").Append(hanging)

        If style IsNot Nothing Then
            If style.LineSpacing > 0 AndAlso System.Math.Abs(style.LineSpacing - 1.0) > 0.001 Then
                ' \sl 以 1/240 行表示，\slmult1 表示按倍数解释
                Call sb.Append("\sl").Append(CInt(System.Math.Round(style.LineSpacing * 240))).Append("\slmult1")
            End If

            Call sb.Append("\sb").Append(ToTwips(style.SpaceBefore))
            Call sb.Append("\sa").Append(ToTwips(style.SpaceAfter))

            If hanging <= 0 AndAlso style.FirstLineIndent > 0 Then
                Call sb.Append("\fi").Append(ToTwips(style.FirstLineIndent))
            End If

            Dim bg As Integer = colors.Lookup(style.BackColor)
            If bg > 0 Then Call sb.Append("\cbpat").Append(bg).Append("\chshdng0")
        End If

        Return sb.ToString()
    End Function

    ''' <summary>生成字符属性（\plain + 字体/字号/粗斜下划线/前景色）。</summary>
    Private Function CharProps(style As WordStyle,
                               Optional foreColor As String = Nothing,
                               Optional bold As Boolean? = Nothing,
                               Optional size As Double? = Nothing) As String
        Dim sb As New StringBuilder("\plain")

        Dim name As String = If(style Is Nothing OrElse String.IsNullOrEmpty(style.FontName), "Calibri", style.FontName)
        Dim eastAsia As String = If(style Is Nothing, name, If(String.IsNullOrEmpty(style.FontNameEastAsia), name, style.FontNameEastAsia))

        Call sb.Append("\f").Append(fonts.Lookup(name))
        If Not String.Equals(name, eastAsia, StringComparison.OrdinalIgnoreCase) Then
            ' \afN 让东亚字符使用关联字体，避免中文被西文字体替代
            Call sb.Append("\af").Append(fonts.Lookup(eastAsia, asEastAsia:=True))
        End If

        Dim fontSize As Double = If(size.HasValue, size.Value, If(style Is Nothing, 11, style.Size))
        Call sb.Append("\fs").Append(CInt(System.Math.Round(fontSize * 2)))

        If If(bold.HasValue, bold.Value, style IsNot Nothing AndAlso style.Bold) Then Call sb.Append("\b")
        If style IsNot Nothing AndAlso style.Italic Then Call sb.Append("\i")
        If style IsNot Nothing AndAlso style.Underline Then Call sb.Append("\ul")

        Dim hex As String = If(foreColor, If(style Is Nothing, WordColors.Black, style.ForeColor))
        Dim cf As Integer = colors.Lookup(hex)
        If cf > 0 Then Call sb.Append("\cf").Append(cf)

        ' 控制字以空格作为分隔符（该空格会被 RTF 读取器吞掉）。
        ' 若省略，末尾的数字参数会与紧随其后的文本粘连：
        ' "\cf2" + "1" 会被解析成 \cf21，从而吞掉单元格文本的首个数字。
        Call sb.Append(" ")

        Return sb.ToString()
    End Function

    Private Sub WriteParagraphCore(style As WordStyle, text As String,
                                   indent As Integer, hanging As Integer, tabStop As Integer)
        Call content.Append(ParagraphProps(style, indent, hanging, tabStop))
        Call content.Append(CharProps(style))
        Call content.Append(EscapeText(text))
        Call content.Append("\par").Append(vbCrLf)
    End Sub

    ' ========================================================================
    ' 列表 / 分割线 / 分页 / 目录
    ' ========================================================================

    Private Sub WriteListItem(block As RtfBlock)
        Dim marker As String

        If block.Ordered Then
            orderedIndex += 1
            marker = orderedIndex.ToString() & "."
        Else
            orderedIndex = 0
            marker = "•"
        End If

        Call WriteParagraphCore(block.Style, marker & vbTab & block.Text, ListIndent, ListIndent, ListIndent)
    End Sub

    Private Sub WriteTaskListItem(block As RtfBlock)
        Dim marker As String = If(block.Checked, "☑", "☐")
        Call WriteParagraphCore(block.Style, marker & vbTab & block.Text, ListIndent, ListIndent, ListIndent)
    End Sub

    Private Sub WriteDefinitionListItem(block As RtfBlock)
        If Not String.IsNullOrEmpty(block.Term) Then
            Call content.Append(ParagraphProps(block.Style, 0, 0, 0))
            Call content.Append(CharProps(block.Style, bold:=True))
            Call content.Append(EscapeText(block.Term))
            Call content.Append("\par").Append(vbCrLf)
        End If

        Call content.Append(ParagraphProps(block.Style, ListIndent, 0, 0))
        Call content.Append(CharProps(block.Style))
        Call content.Append(EscapeText(block.Text))
        Call content.Append("\par").Append(vbCrLf)
    End Sub

    Private Sub WriteHorizontalRule()
        Dim cf As Integer = colors.Lookup(If(tableStyle Is Nothing, WordColors.Gray, tableStyle.BorderColor))

        Call content.Append("\pard\brdrb\brdrs\brdrw10\brsp20")
        If cf > 0 Then Call content.Append("\brdrcf").Append(cf)
        Call content.Append("\par").Append(vbCrLf)
    End Sub

    Private Sub WritePageBreak()
        Call content.Append("\pard\page").Append(vbCrLf)
    End Sub

    ''' <summary>
    ''' 输出静态目录：不依赖 Word 域（域在非 Word 打开器中不可靠），
    ''' 直接按已写入的标题层级生成缩进目录，结果确定且可被解析回读。
    ''' </summary>
    Private Sub WriteToc(maxLevel As Integer)
        Dim level As Integer = If(maxLevel < 1, 1, maxLevel)

        For Each heading As RtfBlock In headings
            If heading.Level > level Then Continue For

            Dim indent As Integer = System.Math.Max(0, heading.Level - 1) * ListIndent

            Call content.Append("\pard")
            If indent > 0 Then Call content.Append("\li").Append(indent)
            Call content.Append(CharProps(paragraphStyle))
            Call content.Append(EscapeText(heading.Text))
            Call content.Append("\par").Append(vbCrLf)
        Next
    End Sub

    ' ========================================================================
    ' 表格
    ' ========================================================================

    Private Sub WriteTable(block As RtfBlock)
        Dim headers As String() = block.TableHeaders
        Dim rows As String()() = block.TableRows

        Dim ncols As Integer = If(headers Is Nothing, 0, headers.Length)
        If rows IsNot Nothing Then
            For Each row As String() In rows
                If row IsNot Nothing AndAlso row.Length > ncols Then ncols = row.Length
            Next
        End If
        If ncols = 0 Then Return

        Dim usable As Integer = System.Math.Max(1200, pageWidth - marginLeft - marginRight)
        Dim widths As Integer() = ComputeColumnWidths(block, ncols, usable)

        Dim total As Integer = 0
        For Each w As Integer In widths
            total += w
        Next

        Dim leftOffset As Integer = 0
        If block.TableCenter AndAlso total < usable Then
            leftOffset = CInt((usable - total) / 2)
        End If

        Dim lastDataRow As Integer = If(rows Is Nothing, -1, rows.Length - 1)

        If headers IsNot Nothing AndAlso headers.Length > 0 Then
            Call WriteTableRow(headers, widths, block.TableAlignments, leftOffset,
                               isHeader:=True, threeLine:=block.TableThreeLine, altRow:=False, isLastRow:=(lastDataRow < 0))
        End If

        If rows IsNot Nothing Then
            For i As Integer = 0 To rows.Length - 1
                Call WriteTableRow(rows(i), widths, block.TableAlignments, leftOffset,
                                   isHeader:=False, threeLine:=block.TableThreeLine, altRow:=(i Mod 2 = 1), isLastRow:=(i = lastDataRow))
            Next
        End If
    End Sub

    ''' <summary>
    ''' 计算各列宽度（twips）：等宽 / 按窗口铺满 / 按内容文本长度占比分配。
    ''' </summary>
    Private Shared Function ComputeColumnWidths(block As RtfBlock, ncols As Integer, usable As Integer) As Integer()
        Dim widths(ncols - 1) As Integer

        If String.Equals(block.TableMode, "contents", StringComparison.OrdinalIgnoreCase) Then
            Dim weights(ncols - 1) As Integer
            Dim totalWeight As Integer = 0

            For j As Integer = 0 To ncols - 1
                Dim w As Integer = 1

                If block.TableHeaders IsNot Nothing AndAlso j < block.TableHeaders.Length Then
                    w = System.Math.Max(w, Measure(block.TableHeaders(j)))
                End If

                If block.TableRows IsNot Nothing Then
                    For Each row As String() In block.TableRows
                        If row IsNot Nothing AndAlso j < row.Length Then
                            w = System.Math.Max(w, Measure(row(j)))
                        End If
                    Next
                End If

                weights(j) = w
                totalWeight += w
            Next

            Dim used As Integer = 0
            For j As Integer = 0 To ncols - 1
                widths(j) = System.Math.Max(MinColumnWidth, CInt(usable * weights(j) / totalWeight))
                used += widths(j)
            Next

            ' 修正累计取整误差，保证不超出可用宽度
            Dim excess As Integer = used - usable
            Dim k As Integer = ncols - 1
            Do While excess > 0 AndAlso k >= 0
                Dim cut As Integer = System.Math.Min(excess, System.Math.Max(0, widths(k) - MinColumnWidth))
                widths(k) -= cut
                excess -= cut
                k -= 1
            Loop
        Else
            Dim eachWidth As Integer = System.Math.Max(MinColumnWidth, usable \ ncols)
            For j As Integer = 0 To ncols - 1
                widths(j) = eachWidth
            Next
        End If

        Return widths
    End Function

    Private Shared Function Measure(text As String) As Integer
        If String.IsNullOrEmpty(text) Then Return 1
        Return System.Math.Min(60, text.Length)
    End Function

    Private Sub WriteTableRow(cells As String(), widths As Integer(), alignments As String(), leftOffset As Integer,
                              isHeader As Boolean, threeLine As Boolean, altRow As Boolean, isLastRow As Boolean)
        Dim borderWidth As Integer = System.Math.Max(5, CInt(tableStyle.BorderSize * 2.5))
        Dim borderColor As Integer = colors.Lookup(tableStyle.BorderColor)
        Dim headerBack As Integer = colors.Lookup(tableStyle.HeaderBackColor)
        Dim altBack As Integer = colors.Lookup(tableStyle.AltRowBackColor)
        Dim cellBack As Integer = If(isHeader, headerBack, If(altRow, altBack, 0))

        ' 三线表：仅在首行顶部、表头行底部与末行底部绘制横线
        Dim showTop As Boolean = Not threeLine OrElse isHeader
        Dim showBottom As Boolean = Not threeLine OrElse isHeader OrElse isLastRow
        Dim showSide As Boolean = Not threeLine

        Call content.Append("\trowd")
        Call content.Append("\trgaph").Append(System.Math.Max(0, tableStyle.CellPadding))
        Call content.Append("\trleft").Append(leftOffset)
        Call content.Append("\trrh0")
        If isHeader Then Call content.Append("\trhdr")

        ' 单元格边界定义
        Dim boundary As Integer = 0
        For j As Integer = 0 To widths.Length - 1
            boundary += widths(j)

            If showTop Then Call AppendBorder(content, "\clbrdrt", borderWidth, borderColor)
            If showSide Then Call AppendBorder(content, "\clbrdrl", borderWidth, borderColor)
            If showBottom Then Call AppendBorder(content, "\clbrdrb", borderWidth, borderColor)
            If showSide Then Call AppendBorder(content, "\clbrdrr", borderWidth, borderColor)

            If cellBack > 0 Then Call content.Append("\clcbpat").Append(cellBack)
            Call content.Append("\cellx").Append(boundary)
        Next

        ' 单元格内容
        Dim headerProps As String = CharProps(paragraphStyle, tableStyle.HeaderForeColor, tableStyle.HeaderBold)
        Dim normalProps As String = CharProps(paragraphStyle)

        For j As Integer = 0 To widths.Length - 1
            Dim text As String = If(cells IsNot Nothing AndAlso j < cells.Length AndAlso cells(j) IsNot Nothing, cells(j), "")

            Call content.Append("\pard\intbl")
            Call content.Append(AlignmentToken(alignments, j))
            If cellBack > 0 Then Call content.Append("\cbpat").Append(cellBack)
            Call content.Append(If(isHeader, headerProps, normalProps))
            Call content.Append(EscapeText(text))
            Call content.Append("\cell")
        Next

        Call content.Append("\row").Append(vbCrLf)
    End Sub

    Private Shared Sub AppendBorder(sb As StringBuilder, border As String, width As Integer, colorIndex As Integer)
        Call sb.Append(border).Append("\brdrs\brdrw").Append(width)
        If colorIndex > 0 Then Call sb.Append("\brdrcf").Append(colorIndex)
    End Sub

    Private Shared Function AlignmentToken(alignments As String(), index As Integer) As String
        If alignments Is Nothing OrElse index >= alignments.Length Then Return "\ql"

        Select Case Strings.LCase(If(alignments(index), "left")).Trim()
            Case "center" : Return "\qc"
            Case "right" : Return "\qr"
            Case "justify", "both" : Return "\qj"
            Case Else : Return "\ql"
        End Select
    End Function

    ' ========================================================================
    ' 图片
    ' ========================================================================

    Private Sub WriteImage(block As RtfBlock)
        Dim data As Byte()

        Try
            data = File.ReadAllBytes(block.ImagePath)
        Catch ex As Exception
            Call Console.Error.WriteLine($"[警告] 读取图片失败: {block.ImagePath} - {ex.Message}")
            Return
        End Try

        Dim blip As String

        Select Case Path.GetExtension(block.ImagePath).TrimStart("."c).ToLower()
            Case "png"
                blip = "\pngblip"
            Case "jpg", "jpeg"
                blip = "\jpegblip"
            Case Else
                Call Console.Error.WriteLine($"[警告] RTF 暂不支持该图片格式，已跳过: {block.ImagePath}")
                Return
        End Select

        Dim pixelWidth As Integer = If(block.ImagePixelWidth > 0, block.ImagePixelWidth, 96)
        Dim pixelHeight As Integer = If(block.ImagePixelHeight > 0, block.ImagePixelHeight, 96)
        Dim usable As Integer = System.Math.Max(1200, pageWidth - marginLeft - marginRight)
        Dim goalWidth As Integer
        Dim goalHeight As Integer

        If block.ImageWidth > 0 Then
            goalWidth = CInt(System.Math.Round(block.ImageWidth * TwipsPerPoint))
            goalHeight = If(block.ImageHeight > 0,
                            CInt(System.Math.Round(block.ImageHeight * TwipsPerPoint)),
                            CInt(System.Math.Round(goalWidth * pixelHeight / pixelWidth)))
        ElseIf block.ImageHeight > 0 Then
            goalHeight = CInt(System.Math.Round(block.ImageHeight * TwipsPerPoint))
            goalWidth = CInt(System.Math.Round(goalHeight * pixelWidth / pixelHeight))
        Else
            goalWidth = CInt(System.Math.Round(pixelWidth * TwipsPerPixel))
            goalHeight = CInt(System.Math.Round(pixelHeight * TwipsPerPixel))
        End If

        ' 超出可用正文宽度时等比缩放
        If goalWidth > usable Then
            goalHeight = CInt(System.Math.Round(goalHeight * usable / goalWidth))
            goalWidth = usable
        End If

        Call content.Append("\pard\qc")
        Call content.Append("{\pict").Append(blip)
        Call content.Append("\picw").Append(pixelWidth).Append("\pich").Append(pixelHeight)
        Call content.Append("\picwgoal").Append(goalWidth).Append("\pichgoal").Append(goalHeight).Append(vbCrLf)
        Call AppendHex(content, data)
        Call content.Append("}")
        Call content.Append("\par").Append(vbCrLf)

        If Not String.IsNullOrEmpty(block.ImageCaption) Then
            Call content.Append("\pard\qc")
            Call content.Append(CharProps(paragraphStyle, WordColors.DarkGray, False, 9))
            Call content.Append(EscapeText(block.ImageCaption))
            Call content.Append("\par").Append(vbCrLf)
        End If
    End Sub

    ''' <summary>把二进制数据以十六进制流追加到 RTF（每行 128 个十六进制字符）。</summary>
    Private Shared Sub AppendHex(sb As StringBuilder, data As Byte())
        Const perLine As Integer = 64

        Call sb.EnsureCapacity(sb.Length + data.Length * 2 + data.Length \ perLine + 2)

        Dim count As Integer = 0
        For Each b As Byte In data
            Call sb.Append(HexDigits(CInt(b) >> 4)).Append(HexDigits(CInt(b) And &HF))
            count += 1

            If count Mod perLine = 0 Then Call sb.Append(vbCrLf)
        Next

        Call sb.Append(vbCrLf)
    End Sub

    ' ========================================================================
    ' 文档装配
    ' ========================================================================

    Private Function BuildDocument() As String
        Dim doc As New StringBuilder(content.Length + 1024)

        Call doc.Append("{\rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052\uc1").Append(vbCrLf)
        Call doc.Append(fonts.ToRtf())
        Call doc.Append(colors.ToRtf())
        Call doc.Append("{\*\generator ")
        Call doc.Append(RtfCodePage.EncodeText(If(meta Is Nothing OrElse String.IsNullOrEmpty(meta.Creator), "sciBASIC# RtfDocument", meta.Creator)))
        Call doc.Append(";}").Append(vbCrLf)
        Call doc.Append(InfoGroup())
        Call doc.Append("\viewkind4").Append(vbCrLf)
        Call doc.Append(content)
        Call doc.Append("}").Append(vbCrLf)

        Return doc.ToString()
    End Function

    Private Function InfoGroup() As String
        If meta Is Nothing Then Return ""

        Dim sb As New StringBuilder("{\info")

        Call AppendInfoField(sb, "title", meta.Title)
        Call AppendInfoField(sb, "subject", meta.Subject)
        Call AppendInfoField(sb, "author", meta.Author)
        Call AppendInfoField(sb, "operator", meta.Creator)
        Call AppendInfoField(sb, "keywords", meta.Keywords)
        Call AppendInfoField(sb, "doccomm", meta.Description)

        Call sb.Append("}").Append(vbCrLf)

        Return sb.ToString()
    End Function

    Private Shared Sub AppendInfoField(sb As StringBuilder, name As String, value As String)
        If String.IsNullOrEmpty(value) Then Return

        Call sb.Append("{\").Append(name).Append(" ")
        Call sb.Append(EscapeText(value))
        Call sb.Append("}")
    End Sub

    ''' <summary>
    ''' RTF 文本转义：``\`` ``{`` ``}`` 转义，制表符与换行转控制字，
    ''' 其余非 ASCII 字符统一输出为 ``\uN?``（有符号 16 位，含代理对）。
    ''' </summary>
    Private Shared Function EscapeText(text As String) As String
        If String.IsNullOrEmpty(text) Then Return ""

        Dim sb As New StringBuilder(text.Length + 16)

        For Each c As Char In text
            Select Case c
                Case "\"c
                    Call sb.Append("\\")
                Case "{"c
                    Call sb.Append("\{")
                Case "}"c
                    Call sb.Append("\}")
                Case vbTab
                    Call sb.Append("\tab ")
                Case vbCr
                    ' 换行交由 \line 处理，避免 CRLF 被重复展开
                Case vbLf
                    Call sb.Append("\line ")
                Case Else
                    Dim code As Integer = AscW(c)

                    If code < &H20 Then
                        ' 其余控制字符直接丢弃
                    ElseIf code < &H7F Then
                        Call sb.Append(c)
                    Else
                        Call sb.Append("\u").Append(Signed16(code)).Append("?")
                    End If
            End Select
        Next

        Return sb.ToString()
    End Function

    Private Shared Function Signed16(code As Integer) As Integer
        If code > &H7FFF Then Return code - &H10000
        Return code
    End Function

    ' ========================================================================
    ' 字体表 / 颜色表
    ' ========================================================================

    ''' <summary>RTF 字体表：名称去重，序号即 \fN 的 N。</summary>
    Private Class FontTable

        Private ReadOnly index As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly order As New List(Of String)()
        Private ReadOnly eastAsia As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Public Function Lookup(name As String, Optional asEastAsia As Boolean = False) As Integer
            Dim fontName As String = If(String.IsNullOrEmpty(name), "Calibri", name.Trim())

            If asEastAsia Then Call eastAsia.Add(fontName)

            Dim i As Integer
            If index.TryGetValue(fontName, i) Then Return i

            i = order.Count
            index(fontName) = i
            order.Add(fontName)

            Return i
        End Function

        Public Function ToRtf() As String
            Dim sb As New StringBuilder("{\fonttbl")

            For i As Integer = 0 To order.Count - 1
                Dim name As String = order(i)
                Dim charset As Integer = If(eastAsia.Contains(name) OrElse HasNonAscii(name), 134, 0)

                Call sb.Append("{\f").Append(i).Append("\fnil\fcharset").Append(charset).Append(" ")
                Call sb.Append(RtfCodePage.EncodeText(name))
                Call sb.Append(";}")
            Next

            Call sb.Append("}").Append(vbCrLf)

            Return sb.ToString()
        End Function

        Private Shared Function HasNonAscii(text As String) As Boolean
            For Each c As Char In text
                If AscW(c) > &H7E Then Return True
            Next
            Return False
        End Function
    End Class

    ''' <summary>RTF 颜色表：索引 0 保留给 \cf0（自动色），自定义色自 1 起。</summary>
    Private Class ColorTable

        Private ReadOnly index As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly order As New List(Of String)()

        Public Function Lookup(hex As String) As Integer
            Dim rgb As Integer() = ParseHex(hex)
            If rgb Is Nothing Then Return 0

            Dim key As String = hex.Trim().TrimStart("#"c)

            Dim i As Integer
            If index.TryGetValue(key, i) Then Return i

            i = order.Count + 1
            index(key) = i
            order.Add(key)

            Return i
        End Function

        Public Function ToRtf() As String
            Dim sb As New StringBuilder("{\colortbl ;")

            For Each hex As String In order
                Dim rgb As Integer() = ParseHex(hex)

                Call sb.Append("\red").Append(rgb(0))
                Call sb.Append("\green").Append(rgb(1))
                Call sb.Append("\blue").Append(rgb(2)).Append(";")
            Next

            Call sb.Append("}").Append(vbCrLf)

            Return sb.ToString()
        End Function

        ''' <summary>解析 6 位十六进制 RGB（可带 # 前缀）；非法值返回 Nothing 表示使用自动色。</summary>
        Private Shared Function ParseHex(hex As String) As Integer()
            If String.IsNullOrEmpty(hex) Then Return Nothing

            Dim value As String = hex.Trim().TrimStart("#"c)
            If value.Length <> 6 Then Return Nothing

            Dim rgb(2) As Integer

            For i As Integer = 0 To 2
                Dim part As String = value.Substring(i * 2, 2)
                Dim b As Integer

                If Not Integer.TryParse(part, Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.InvariantCulture, b) Then
                    Return Nothing
                End If

                rgb(i) = b
            Next

            Return rgb
        End Function
    End Class

End Class
