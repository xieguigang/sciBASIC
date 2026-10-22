' ============================================================================
' XpsLayoutEngine.vb - 固定页排版引擎
'
' XPS 是固定版式格式：没有流式重排，所有文字都必须给出绝对坐标。本引擎把
' XpsBlock 队列排成一串 XpsPage，每个页面由三类绘制原语组成：
'
'   - XpsGlyphs     一段同一字号的文本（OriginX / OriginY 为基线起点）
'   - XpsLineSegment / XpsRectangle  分割线、表格网格线、引用块竖线、代码底纹
'   - XpsImagePlacement  图片（ImageBrush 引用的资源 Uri）
'
' 坐标单位为磅（pt），原点在页面左上角。文本宽度用 XpsTextMetrics 近似度量，
' 因此折行与分页在序列化之前就已完全确定。
' ============================================================================

''' <summary>页面上的一段文本（对应一个 &lt;Glyphs&gt; 元素）。</summary>
Public Class XpsGlyphs

    ''' <summary>基线起点 X（磅）。</summary>
    Public Property OriginX As Double
    ''' <summary>基线 Y（磅，自页面顶部向下）。</summary>
    Public Property OriginY As Double
    ''' <summary>字号（磅）。</summary>
    Public Property FontSize As Double
    ''' <summary>文本内容（未转义）。</summary>
    Public Property Text As String
    ''' <summary>是否加粗。</summary>
    Public Property Bold As Boolean
    ''' <summary>是否斜体。</summary>
    Public Property Italic As Boolean
    ''' <summary>文字颜色（#RRGGBB）。</summary>
    Public Property Fill As String = "#000000"

End Class

''' <summary>页面上的直线段（分割线、表格网格线、引用块竖线）。</summary>
Public Class XpsLineSegment

    ''' <summary>起点 X。</summary>
    Public Property X1 As Double
    ''' <summary>起点 Y。</summary>
    Public Property Y1 As Double
    ''' <summary>终点 X。</summary>
    Public Property X2 As Double
    ''' <summary>终点 Y。</summary>
    Public Property Y2 As Double
    ''' <summary>线宽（磅）。</summary>
    Public Property Thickness As Double = 0.5
    ''' <summary>颜色（#RRGGBB）。</summary>
    Public Property Stroke As String = "#000000"

End Class

''' <summary>页面上的实心矩形（代码块底纹）。</summary>
Public Class XpsRectangle

    ''' <summary>左上角 X。</summary>
    Public Property X As Double
    ''' <summary>左上角 Y。</summary>
    Public Property Y As Double
    ''' <summary>宽度。</summary>
    Public Property Width As Double
    ''' <summary>高度。</summary>
    Public Property Height As Double
    ''' <summary>填充色（#RRGGBB）。</summary>
    Public Property Fill As String = "#F2F2F2"

End Class

''' <summary>页面上的图片放置（ImageBrush 资源引用）。</summary>
Public Class XpsImagePlacement

    ''' <summary>左上角 X。</summary>
    Public Property X As Double
    ''' <summary>左上角 Y。</summary>
    Public Property Y As Double
    ''' <summary>显示宽度（磅）。</summary>
    Public Property Width As Double
    ''' <summary>显示高度（磅）。</summary>
    Public Property Height As Double
    ''' <summary>图片资源 Uri（``/Resources/Images/1.png``）。</summary>
    Public Property ResourceUri As String
    ''' <summary>图片原始像素宽度（ImageBrush 的 Viewbox 使用）。</summary>
    Public Property PixelWidth As Integer
    ''' <summary>图片原始像素高度（ImageBrush 的 Viewbox 使用）。</summary>
    Public Property PixelHeight As Integer

End Class

''' <summary>一个固定页面：页面尺寸 + 该页的全部绘制原语。</summary>
Public Class XpsPage

    ''' <summary>页面宽度（磅）。</summary>
    Public Property Width As Double
    ''' <summary>页面高度（磅）。</summary>
    Public Property Height As Double
    ''' <summary>文本段。</summary>
    Public Property Glyphs As New List(Of XpsGlyphs)()
    ''' <summary>直线段。</summary>
    Public Property Lines As New List(Of XpsLineSegment)()
    ''' <summary>实心矩形。</summary>
    Public Property Rectangles As New List(Of XpsRectangle)()
    ''' <summary>图片放置。</summary>
    Public Property Images As New List(Of XpsImagePlacement)()

End Class

''' <summary>
''' 固定页排版引擎：把内容块队列排成页面（自动折行、自动分页、表格网格、
''' 目录展开与图片版面）。
''' </summary>
Public Class XpsLayoutEngine

    ''' <summary>单元格内边距（磅）。</summary>
    Public Const CellPadding As Double = 3.0

    ''' <summary>最小列宽（磅）。</summary>
    Public Const MinColumnWidth As Double = 36.0

    ''' <summary>基线相对行顶的偏移比例（字号 × 该值）。</summary>
    Public Const BaselineRatio As Double = 0.86

    Private ReadOnly pageWidth As Double
    Private ReadOnly pageHeight As Double
    Private ReadOnly marginTop As Double
    Private ReadOnly marginRight As Double
    Private ReadOnly marginBottom As Double
    Private ReadOnly marginLeft As Double

    Private ReadOnly blocks As List(Of XpsBlock)
    Private ReadOnly headingStyles As XpsTextStyle()
    Private ReadOnly paragraphStyle As XpsTextStyle
    Private ReadOnly codeStyle As XpsTextStyle
    Private ReadOnly quoteStyle As XpsTextStyle
    Private ReadOnly titleStyle As XpsTextStyle

    Private ReadOnly pages As New List(Of XpsPage)()
    Private ReadOnly tocEntries As New List(Of Tuple(Of Integer, String))()

    Private page As XpsPage
    Private cursor As Double

    ''' <summary>可用版心宽度（磅）。</summary>
    Public ReadOnly Property ContentWidth As Double
        Get
            Return pageWidth - marginLeft - marginRight
        End Get
    End Property

    ''' <summary>
    ''' 创建排版引擎。
    ''' </summary>
    ''' <param name="pageWidth">页面宽度（磅）。</param>
    ''' <param name="pageHeight">页面高度（磅）。</param>
    ''' <param name="marginTop">上边距（磅）。</param>
    ''' <param name="marginRight">右边距（磅）。</param>
    ''' <param name="marginBottom">下边距（磅）。</param>
    ''' <param name="marginLeft">左边距（磅）。</param>
    ''' <param name="blocks">待排版的内容块。</param>
    ''' <param name="headingStyles">1-6 级标题样式。</param>
    ''' <param name="paragraphStyle">正文样式。</param>
    ''' <param name="codeStyle">代码块样式。</param>
    ''' <param name="quoteStyle">引用块样式。</param>
    ''' <param name="titleStyle">文档大标题样式。</param>
    Public Sub New(pageWidth As Double, pageHeight As Double,
                   marginTop As Double, marginRight As Double, marginBottom As Double, marginLeft As Double,
                   blocks As List(Of XpsBlock),
                   headingStyles As XpsTextStyle(), paragraphStyle As XpsTextStyle,
                   codeStyle As XpsTextStyle, quoteStyle As XpsTextStyle, titleStyle As XpsTextStyle)

        Me.pageWidth = pageWidth
        Me.pageHeight = pageHeight
        Me.marginTop = marginTop
        Me.marginRight = marginRight
        Me.marginBottom = marginBottom
        Me.marginLeft = marginLeft
        Me.blocks = If(blocks, New List(Of XpsBlock)())
        Me.headingStyles = If(headingStyles, New XpsTextStyle() {})
        Me.paragraphStyle = If(paragraphStyle, New XpsTextStyle())
        Me.codeStyle = If(codeStyle, Me.paragraphStyle)
        Me.quoteStyle = If(quoteStyle, Me.paragraphStyle)
        Me.titleStyle = If(titleStyle, Me.paragraphStyle)
    End Sub

    ''' <summary>
    ''' 执行排版，返回按页序排列的页面列表（至少一页）。
    ''' </summary>
    ''' <returns></returns>
    Public Function Layout() As List(Of XpsPage)
        Call CollectTocEntries()
        Call NewPage()

        For Each block As XpsBlock In blocks
            If block Is Nothing Then
                Continue For
            End If

            Call RenderBlock(block)
        Next

        Return pages
    End Function

    ' ========================================================================
    ' 页面管理
    ' ========================================================================

    Private Function NewPage() As XpsPage
        Dim created As New XpsPage() With {
            .Width = pageWidth,
            .Height = pageHeight
        }

        Call pages.Add(created)

        page = created
        cursor = marginTop

        Return created
    End Function

    ''' <summary>确保当前页还能放下 <paramref name="height"/> 磅的内容，放不下则翻页。</summary>
    Private Sub EnsureSpace(height As Double)
        If page Is Nothing Then
            Call NewPage()
            Return
        End If

        If cursor + height > pageHeight - marginBottom AndAlso
           (page.Glyphs.Count > 0 OrElse page.Images.Count > 0 OrElse page.Lines.Count > 0) Then

            Call NewPage()
        End If
    End Sub

    Private Sub AddLine(x1 As Double, y1 As Double, x2 As Double, y2 As Double,
                        Optional stroke As String = "#000000",
                        Optional thickness As Double = 0.5)

        Call page.Lines.Add(New XpsLineSegment() With {
            .X1 = x1,
            .Y1 = y1,
            .X2 = x2,
            .Y2 = y2,
            .Stroke = stroke,
            .Thickness = thickness
        })
    End Sub

    ' ========================================================================
    ' 文本排版
    ' ========================================================================

    ''' <summary>在指定 X 处输出一行文本（baseline 为基线 Y）。</summary>
    Private Sub AddGlyphs(text As String, style As XpsTextStyle, x As Double, baseline As Double)
        If String.IsNullOrEmpty(text) Then
            Return
        End If

        Call page.Glyphs.Add(New XpsGlyphs() With {
            .OriginX = x,
            .OriginY = baseline,
            .FontSize = style.Size,
            .Text = text,
            .Bold = style.Bold,
            .Italic = style.Italic,
            .Fill = style.ForeColor
        })
    End Sub

    ''' <summary>按样式对齐方式计算该行文本的起点 X。</summary>
    Private Function AlignX(text As String, style As XpsTextStyle, left As Double) As Double
        Dim width As Double = XpsTextMetrics.MeasureText(text, style.Size)
        Dim right As Double = pageWidth - marginRight

        Select Case LCase(If(style.Alignment, "left"))
            Case "center"
                Return left + (right - left - width) / 2
            Case "right"
                Return right - width
            Case Else
                Return left
        End Select
    End Function

    ''' <summary>
    ''' 输出一段可折行的文本（段落 / 列表项 / 引用 / 定义）。
    ''' </summary>
    ''' <param name="text">文本内容。</param>
    ''' <param name="style">样式（<see cref="XpsTextStyle.Indent"/> 参与版心计算）。</param>
    ''' <param name="restIndent">续行缩进（磅），用于列表项悬挂对齐。</param>
    ''' <param name="background">非空时先绘制一条底纹矩形（代码块）。</param>
    Private Sub WriteFlowText(text As String, style As XpsTextStyle,
                              Optional restIndent As Double = 0,
                              Optional background As String = Nothing)

        Dim left As Double = marginLeft + style.Indent
        Dim width As Double = ContentWidth - style.Indent

        If Not String.IsNullOrEmpty(background) Then
            ' 代码块：保留原始换行，按行数估算底纹高度
            Dim rawLines As String() = If(text, "").Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)
            Dim blockHeight As Double = rawLines.Length * style.LineHeight + CellPadding * 2

            Call EnsureSpace(blockHeight)
            Call page.Rectangles.Add(New XpsRectangle() With {
                .X = left - CellPadding,
                .Y = cursor,
                .Width = width + CellPadding * 2,
                .Height = blockHeight,
                .Fill = background
            })

            Dim top As Double = cursor

            cursor += CellPadding

            For Each rawLine As String In rawLines
                Call WriteTextLine(rawLine, style, left)
            Next

            If cursor < top + blockHeight Then
                cursor = top + blockHeight
            End If

            Return
        End If

        Dim lines As String() = XpsTextMetrics.WrapText(text, style.Size, width, 0, restIndent)

        For Each line As String In lines
            Call WriteTextLine(line, style, left)
        Next
    End Sub

    ''' <summary>输出一行（不折行），并推进光标。</summary>
    Private Sub WriteTextLine(text As String, style As XpsTextStyle, left As Double)
        Call EnsureSpace(style.LineHeight)

        Dim baseline As Double = cursor + style.Size * BaselineRatio
        Dim x As Double = AlignX(text, style, left)

        Call AddGlyphs(text, style, x, baseline)

        cursor += style.LineHeight
    End Sub

    ' ========================================================================
    ' 内容块分派
    ' ========================================================================

    Private Sub RenderBlock(block As XpsBlock)
        Dim style As XpsTextStyle = If(block.Style, paragraphStyle)

        Select Case block.Type
            Case XpsBlockType.Title
                cursor += titleStyle.SpaceBefore
                Call WriteFlowText(If(block.Text, ""), titleStyle)
                cursor += titleStyle.SpaceAfter
            Case XpsBlockType.Heading
                Dim level As Integer = If(block.Level >= 1 AndAlso block.Level <= 6, block.Level, 1)
                Dim headingStyle As XpsTextStyle = headingStyles(level - 1)

                cursor += headingStyle.SpaceBefore
                Call WriteFlowText(If(block.Text, ""), headingStyle)
                cursor += headingStyle.SpaceAfter
            Case XpsBlockType.Paragraph
                cursor += style.SpaceBefore
                Call WriteFlowText(If(block.Text, ""), style)
                cursor += style.SpaceAfter
            Case XpsBlockType.Code
                cursor += style.SpaceBefore
                Call WriteFlowText(If(block.Text, ""), codeStyle, background:="#F2F2F2")
                cursor += style.SpaceAfter
            Case XpsBlockType.Quote
                Call RenderQuote(block, style)
            Case XpsBlockType.List
                Call RenderList(block, style)
            Case XpsBlockType.TaskList
                Call RenderTaskList(block, style)
            Case XpsBlockType.DefList
                Call RenderDefList(block, style)
            Case XpsBlockType.Hr
                Call RenderHorizontalRule(style)
            Case XpsBlockType.PageBreak
                Call NewPage()
            Case XpsBlockType.Toc
                Call RenderToc(style, If(block.Level <= 0, 6, block.Level))
            Case XpsBlockType.Table
                Call RenderTable(block, style)
            Case XpsBlockType.Image
                Call RenderImage(block, style)
            Case Else
                Call WriteFlowText(If(block.Text, ""), style)
        End Select
    End Sub

    Private Sub RenderQuote(block As XpsBlock, style As XpsTextStyle)
        Dim quote As XpsTextStyle = If(quoteStyle, style).Clone()

        quote.Indent = style.Indent + 12
        quote.ForeColor = "#404040"

        cursor += style.SpaceBefore

        Dim startY As Double = cursor

        Call WriteFlowText(If(block.Text, ""), quote)

        Call page.Lines.Add(New XpsLineSegment() With {
            .X1 = marginLeft + style.Indent + 5,
            .Y1 = startY,
            .X2 = marginLeft + style.Indent + 5,
            .Y2 = System.Math.Max(startY + 1, cursor - quote.LineHeight * 0.2),
            .Thickness = 2,
            .Stroke = "#C0C0C0"
        })

        cursor += style.SpaceAfter
    End Sub

    Private Sub RenderList(block As XpsBlock, style As XpsTextStyle)
        Dim index As Integer = 1
        Dim hang As Double = XpsTextMetrics.MeasureText("0000. ", style.Size)
        Dim itemStyle As XpsTextStyle = style.Clone()

        itemStyle.Indent = style.Indent + 12

        For Each item As String In If(block.Items, New String() {})
            Dim marker As String = If(block.Ordered, index & ". ", "• ")

            Call WriteFlowText(marker & If(item, ""), itemStyle, restIndent:=hang)

            index += 1
        Next

        cursor += style.SpaceAfter
    End Sub

    Private Sub RenderTaskList(block As XpsBlock, style As XpsTextStyle)
        Dim pos As Integer = 0
        Dim itemStyle As XpsTextStyle = style.Clone()

        itemStyle.Indent = style.Indent + 12

        For Each item As String In If(block.Items, New String() {})
            Dim isChecked As Boolean = Not block.Checked Is Nothing AndAlso pos < block.Checked.Length AndAlso block.Checked(pos)
            Dim marker As String = If(isChecked, "[x] ", "[ ] ")

            Call WriteFlowText(marker & If(item, ""), itemStyle, restIndent:=XpsTextMetrics.MeasureText(marker, style.Size))

            pos += 1
        Next

        cursor += style.SpaceAfter
    End Sub

    Private Sub RenderDefList(block As XpsBlock, style As XpsTextStyle)
        Dim terms As String() = If(block.Terms, New String() {})
        Dim definitions As String() = If(block.Definitions, New String() {})
        Dim count As Integer = System.Math.Min(terms.Length, definitions.Length)
        Dim termStyle As XpsTextStyle = style.Clone()
        Dim defStyle As XpsTextStyle = style.Clone()

        termStyle.Indent = style.Indent + 12
        termStyle.Bold = True
        defStyle.Indent = style.Indent + 24

        For i As Integer = 0 To count - 1
            Call WriteFlowText(If(terms(i), ""), termStyle)
            Call WriteFlowText(If(definitions(i), ""), defStyle)
        Next

        cursor += style.SpaceAfter
    End Sub

    Private Sub RenderHorizontalRule(style As XpsTextStyle)
        Dim height As Double = If(style.LineHeight < 8, 8, style.LineHeight)

        Call EnsureSpace(height)

        Dim y As Double = cursor + height / 2

        Call AddLine(marginLeft, y, pageWidth - marginRight, y, "#808080", 0.75)

        cursor += height
    End Sub

    Private Sub RenderToc(style As XpsTextStyle, maxLevel As Integer)
        If tocEntries.Count = 0 Then
            Return
        End If

        cursor += style.SpaceBefore

        For Each entry As Tuple(Of Integer, String) In tocEntries
            If entry.Item1 > maxLevel Then
                Continue For
            End If

            Dim tocStyle As XpsTextStyle = style.Clone()

            tocStyle.Indent = style.Indent + (entry.Item1 - 1) * 12

            Call WriteFlowText(entry.Item2, tocStyle)
        Next

        cursor += style.SpaceAfter
    End Sub

    ''' <summary>预扫描标题，供目录块展开为静态目录。</summary>
    Private Sub CollectTocEntries()
        For Each block As XpsBlock In blocks
            If block Is Nothing Then
                Continue For
            End If
            If block.Type = XpsBlockType.Heading Then
                Dim level As Integer = If(block.Level < 1, 1, If(block.Level > 6, 6, block.Level))

                Call tocEntries.Add(New Tuple(Of Integer, String)(level, If(block.Text, "")))
            End If
        Next
    End Sub

    ' ========================================================================
    ' 表格
    ' ========================================================================

    Private Sub RenderTable(block As XpsBlock, style As XpsTextStyle)
        Dim headers As String() = If(block.TableHeaders, New String() {})
        Dim rows As String()() = If(block.TableRows, New String()() {})
        Dim columns As Integer = headers.Length

        For i As Integer = 0 To rows.Length - 1
            If rows(i) IsNot Nothing AndAlso rows(i).Length > columns Then
                columns = rows(i).Length
            End If
        Next

        If columns <= 0 Then
            Return
        End If

        Dim widths As Double() = ComputeColumnWidths(headers, rows, columns, style)
        Dim aligns As String() = If(block.TableAlignments, New String() {})
        Dim headerStyle As XpsTextStyle = style.Clone()

        headerStyle.Bold = True

        cursor += style.SpaceBefore

        Call DrawTableHeader(headers, widths, headerStyle, aligns)

        For Each row As String() In rows
            Dim cells As String() = If(row, New String() {})
            Dim height As Double = RowHeight(cells, widths, style)

            If cursor + height > pageHeight - marginBottom Then
                Call NewPage()
                Call DrawTableHeader(headers, widths, headerStyle, aligns)
            End If

            Dim top As Double = cursor

            Call WriteTableCells(cells, widths, style, top, height, aligns)

            cursor = top + height

            Call AddLine(marginLeft, cursor, pageWidth - marginRight, cursor, "#000000", 0.5)
        Next

        cursor += style.SpaceAfter
    End Sub

    ''' <summary>绘制表头（顶线 + 单元格 + 表头下分隔线）；跨页时重复调用。</summary>
    Private Sub DrawTableHeader(headers As String(), widths As Double(),
                                headerStyle As XpsTextStyle, aligns As String())
        If headers.Length = 0 Then
            Return
        End If

        Dim height As Double = RowHeight(headers, widths, headerStyle)

        Call EnsureSpace(height + headerStyle.LineHeight)

        Dim top As Double = cursor

        Call AddLine(marginLeft, top, pageWidth - marginRight, top, "#000000", 0.75)
        Call WriteTableCells(headers, widths, headerStyle, top, height, aligns)

        cursor = top + height

        Call AddLine(marginLeft, cursor, pageWidth - marginRight, cursor, "#000000", 0.75)
    End Sub

    ''' <summary>
    ''' 计算列宽：先按内容度量，超出可用宽度时按比例压缩，最小列宽 <see cref="MinColumnWidth"/>。
    ''' </summary>
    Private Function ComputeColumnWidths(headers As String(), rows As String()(),
                                         columns As Integer, style As XpsTextStyle) As Double()
        Dim widths(columns - 1) As Double

        For i As Integer = 0 To columns - 1
            Dim maxWidth As Double = 0

            If headers IsNot Nothing AndAlso i < headers.Length Then
                maxWidth = XpsTextMetrics.MeasureText(headers(i), style.Size)
            End If

            For Each row As String() In rows
                If row IsNot Nothing AndAlso i < row.Length Then
                    maxWidth = System.Math.Max(maxWidth, XpsTextMetrics.MeasureText(row(i), style.Size))
                End If
            Next

            widths(i) = System.Math.Max(MinColumnWidth, maxWidth + CellPadding * 2 + 4)
        Next

        Dim total As Double = 0

        For Each w As Double In widths
            total += w
        Next

        If total > ContentWidth Then
            Dim scale As Double = ContentWidth / total

            For i As Integer = 0 To columns - 1
                widths(i) = System.Math.Max(MinColumnWidth, widths(i) * scale)
            Next
        End If

        Return widths
    End Function

    Private Function RowHeight(cells As String(), widths As Double(), style As XpsTextStyle) As Double
        Dim lines As Integer = 1

        For i As Integer = 0 To cells.Length - 1
            If i >= widths.Length Then
                Exit For
            End If

            Dim wrapped As String() = XpsTextMetrics.WrapText(If(cells(i), ""), style.Size, widths(i) - CellPadding * 2)

            lines = System.Math.Max(lines, wrapped.Length)
        Next

        Return lines * style.LineHeight + CellPadding * 2
    End Function

    ''' <summary>在 [top, top+height) 区域内按列输出单元格文本与竖线。</summary>
    Private Sub WriteTableCells(cells As String(), widths As Double(), style As XpsTextStyle,
                                top As Double, height As Double, aligns As String())
        Dim x As Double = marginLeft

        For i As Integer = 0 To widths.Length - 1
            Dim cellText As String = If(i < cells.Length, If(cells(i), ""), "")
            Dim alignment As String = "left"

            If aligns IsNot Nothing AndAlso i < aligns.Length AndAlso Not String.IsNullOrEmpty(aligns(i)) Then
                alignment = LCase(aligns(i))
            End If

            ' 单元格左边框
            Call AddLine(x, top, x, top + height, "#000000", 0.5)

            Dim lines As String() = XpsTextMetrics.WrapText(cellText, style.Size, widths(i) - CellPadding * 2)
            Dim y As Double = top + CellPadding

            For Each line As String In lines
                Dim lineWidth As Double = XpsTextMetrics.MeasureText(line, style.Size)
                Dim left As Double = x + CellPadding

                Select Case alignment
                    Case "center"
                        left = x + (widths(i) - lineWidth) / 2
                    Case "right"
                        left = x + widths(i) - CellPadding - lineWidth
                End Select

                Call AddGlyphs(line, style, left, y + style.Size * BaselineRatio)

                y += style.LineHeight
            Next

            x += widths(i)
        Next

        ' 最后一列右边框
        Call AddLine(x, top, x, top + height, "#000000", 0.5)
    End Sub

    ' ========================================================================
    ' 图片
    ' ========================================================================

    Private Sub RenderImage(block As XpsBlock, style As XpsTextStyle)
        Dim resource As XpsImageResource = block.ImageResource

        If resource Is Nothing Then
            Call WriteFlowText($"[图片: {If(String.IsNullOrEmpty(block.ImageCaption), block.ImagePath, block.ImageCaption)}]", style)
            Return
        End If

        Dim width As Double = If(block.ImageWidth > 0, block.ImageWidth, resource.DisplayWidth)
        Dim height As Double = If(block.ImageHeight > 0, block.ImageHeight, resource.DisplayHeight)

        If width <= 0 OrElse height <= 0 Then
            Return
        End If

        If width > ContentWidth Then
            height = height * (ContentWidth / width)
            width = ContentWidth
        End If

        Dim captionStyle As XpsTextStyle = style.Clone()
        Dim captionHeight As Double = If(String.IsNullOrEmpty(block.ImageCaption), 0, captionStyle.LineHeight)

        cursor += style.SpaceBefore
        Call EnsureSpace(height + captionHeight)

        Dim x As Double = marginLeft + (ContentWidth - width) / 2

        Call page.Images.Add(New XpsImagePlacement() With {
            .X = x,
            .Y = cursor,
            .Width = width,
            .Height = height,
            .ResourceUri = resource.Uri,
            .PixelWidth = resource.PixelWidth,
            .PixelHeight = resource.PixelHeight
        })

        cursor += height

        If Not String.IsNullOrEmpty(block.ImageCaption) Then
            captionStyle.Alignment = "center"

            Call WriteTextLine(block.ImageCaption, captionStyle, marginLeft)
        End If

        cursor += style.SpaceAfter
    End Sub

End Class
