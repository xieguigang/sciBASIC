#Region "Microsoft.VisualBasic::a0f30889f24336a09f59e37a73e3d83a, mime\text%html\Render\CSS\CssLayoutEngine.vb"

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

    '   Total Lines: 517
    '    Code Lines: 243 (47.00%)
    ' Comment Lines: 159 (30.75%)
    '    - Xml Docs: 59.12%
    ' 
    '   Blank Lines: 115 (22.24%)
    '     File Size: 20.16 KB


    '     Class CssLayoutEngine
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetAscent, GetDescent, GetLineSpacing, WhiteSpace
    ' 
    '         Sub: ApplyAlignment, ApplyCellVerticalAlignment, ApplyCenterAlignment, ApplyJustifyAlignment, ApplyLeftAlignment
    '              ApplyRightAlignment, ApplyRightToLeft, ApplyVerticalAlignment, BubbleRectangles, CreateLineBoxes
    '              FlowBox
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math

Namespace Render.CSS

    ''' <summary>
    ''' Helps on CSS Layout
    ''' </summary>
    Friend NotInheritable Class CssLayoutEngine
        Private Sub New()
        End Sub
#Region "Fields"

        Private Shared _lastTreatedWord As CssBoxWord = Nothing

#End Region

#Region "Inline Boxes"

        ''' <summary>
        ''' Creates line boxes for the specified blockbox
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="blockBox"></param>
        Public Shared Sub CreateLineBoxes(g As Graphics, blockBox As CssBox)

            blockBox.LineBoxes.Clear()

            Dim maxRight As Single = blockBox.ActualRight - blockBox.ActualPaddingRight - blockBox.ActualBorderRightWidth

            'Get the start x and y of the blockBox
            Dim startx As Single = blockBox.Location.X + blockBox.ActualPaddingLeft - 0 + blockBox.ActualBorderLeftWidth
            'TODO: Check for floats
            Dim starty As Single = blockBox.Location.Y + blockBox.ActualPaddingTop - 0 + blockBox.ActualBorderTopWidth
            Dim curx As Single = startx + blockBox.ActualTextIndent
            Dim cury As Single = starty

            'Reminds the maximum bottom reached
            Dim maxBottom As Single = starty

            'Extra amount of spacing that should be applied to lines when breaking them.
            Dim lineSpacing As Single = 0F

            'First line box
            Dim line As New CssLineBox(blockBox)

            'Flow words and boxes
            FlowBox(g, blockBox, blockBox, maxRight, lineSpacing, startx,
            line, curx, cury, maxBottom)

            'Gets the rectangles foreach linebox
            For Each linebox As CssLineBox In blockBox.LineBoxes

                BubbleRectangles(blockBox, linebox)
                linebox.AssignRectanglesToBoxes()
                ApplyAlignment(g, linebox)
                If blockBox.Direction = CssConstants.Rtl Then
                    ApplyRightToLeft(linebox)

                    'linebox.DrawRectangles(g);
                End If
            Next

            blockBox.ActualBottom = maxBottom + blockBox.ActualPaddingBottom + blockBox.ActualBorderBottomWidth
        End Sub

        ''' <summary>
        ''' Recursively flows the content of the box using the inline model
        ''' </summary>
        ''' <param name="g">Device Info</param>
        ''' <param name="blockbox">Blockbox that contains the text flow</param>
        ''' <param name="box">Current box to flow its content</param>
        ''' <param name="maxright">Maximum reached right</param>
        ''' <param name="linespacing">Space to use between rows of text</param>
        ''' <param name="startx">x starting coordinate for when breaking lines of text</param>
        ''' <param name="line">Current linebox being used</param>
        ''' <param name="curx">Current x coordinate that will be the left of the next word</param>
        ''' <param name="cury">Current y coordinate that will be the top of the next word</param>
        ''' <param name="maxbottom">Maximum bottom reached so far</param>
        Private Shared Sub FlowBox(g As Graphics, blockbox As CssBox, box As CssBox, maxright As Single, linespacing As Single, startx As Single,
        ByRef line As CssLineBox, ByRef curx As Single, ByRef cury As Single, ByRef maxbottom As Single)
            box.FirstHostingLineBox = line

            For Each b As CssBox In box.Boxes

                Dim leftspacing As Single = b.ActualMarginLeft + b.ActualBorderLeftWidth + b.ActualPaddingLeft
                Dim rightspacing As Single = b.ActualMarginRight + b.ActualBorderRightWidth + b.ActualPaddingRight
                Dim topspacing As Single = b.ActualBorderTopWidth + b.ActualPaddingTop
                Dim bottomspacing As Single = b.ActualBorderBottomWidth + b.ActualPaddingTop

                b.RectanglesReset()
                b.MeasureWordsSize(g)

                curx += leftspacing

                If b.Words.Count > 0 Then
                    '#Region "Flow words"

                    For Each word As CssBoxWord In b.Words
                        'curx += word.SpacesBeforeWidth;

                        If (b.WhiteSpace <> CssConstants.Nowrap AndAlso curx + word.Width + rightspacing > maxright) OrElse word.IsLineBreak Then
                            '#Region "Break line"

                            curx = startx
                            cury = maxbottom + linespacing

                            line = New CssLineBox(blockbox)

                            If word.IsImage OrElse word.Equals(b.FirstWord) Then
                                curx += leftspacing

                                '#End Region
                            End If
                        End If

                        line.ReportExistanceOf(word)

                        word.Left = curx
                        ' -word.LastMeasureOffset.X + 1;
                        word.Top = cury
                        ' - word.LastMeasureOffset.Y;
                        curx = word.Right
                        ' +word.SpacesAfterWidth;
                        maxbottom = Max(maxbottom, word.Bottom)
                        '+ (word.IsImage ? topspacing + bottomspacing : 0));
                        _lastTreatedWord = word

                        '#End Region
                    Next
                Else
                    FlowBox(g, blockbox, b, maxright, linespacing, startx,
                    line, curx, cury, maxbottom)
                End If

                curx += rightspacing
            Next

            box.LastHostingLineBox = line
        End Sub

        ''' <summary>
        ''' Recursively creates the rectangles of the blockBox, by bubbling from deep to outside of the boxes 
        ''' in the rectangle structure
        ''' </summary>
        Private Shared Sub BubbleRectangles(box As CssBox, line As CssLineBox)
            If box.Words.Count > 0 Then
                Dim x As Single = Single.MaxValue, y As Single = Single.MaxValue, r As Single = Single.MinValue, b As Single = Single.MinValue
                Dim words As List(Of CssBoxWord) = line.WordsOf(box)

                If words.Count > 0 Then
                    For Each word As CssBoxWord In words
                        x = Min(x, word.Left)
                        ' - word.SpacesBeforeWidth);
                        r = Max(r, word.Right)
                        ' + word.SpacesAfterWidth);
                        y = Min(y, word.Top)
                        b = Max(b, word.Bottom)
                    Next
                    line.UpdateRectangle(box, x, y, r, b)
                End If
            Else
                For Each b As CssBox In box.Boxes
                    BubbleRectangles(b, line)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Gets the white space width of the specified box
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Function WhiteSpace(g As Graphics, b As CssBox) As Single
            Dim space As String = " ."
            Dim w As Single = 0F
            Dim onError As Single = 5.0F

            Dim sf As New StringFormat()
            sf.SetMeasurableCharacterRanges(New CharacterRange() {New CharacterRange(0, 1)})
            Dim regs As Region() = g.MeasureCharacterRanges(space, b.ActualFont, New RectangleF(0, 0, Single.MaxValue, Single.MaxValue), sf)

            If regs Is Nothing OrElse regs.Length = 0 Then
                Return onError
            End If

            w = regs(0).GetBounds(g).Width

            If Not (String.IsNullOrEmpty(b.WordSpacing) OrElse b.WordSpacing = CssConstants.Normal) Then
                w += CssValue.ParseLength(b.WordSpacing, 0, b)
            End If
            Return w
        End Function

        ''' <summary>
        ''' Applies vertical and horizontal alignment to words in lineboxes
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="lineBox"></param>
        Private Shared Sub ApplyAlignment(g As Graphics, lineBox As CssLineBox)

            '#Region "Horizontal alignment"

            Select Case lineBox.OwnerBox.TextAlign
                Case CssConstants.Right
                    ApplyRightAlignment(g, lineBox)

                Case CssConstants.Center
                    ApplyCenterAlignment(g, lineBox)

                Case CssConstants.Justify
                    ApplyJustifyAlignment(g, lineBox)

                Case Else
                    ApplyLeftAlignment(g, lineBox)

            End Select

            '#End Region

            ApplyVerticalAlignment(g, lineBox)
        End Sub

        ''' <summary>
        ''' Applies right to left direction to words
        ''' </summary>
        ''' <param name="line"></param>
        Private Shared Sub ApplyRightToLeft(line As CssLineBox)
            Dim left As Single = line.OwnerBox.ClientLeft
            Dim right As Single = line.OwnerBox.ClientRight

            For Each word As CssBoxWord In line.Words
                Dim diff As Single = word.Left - left
                Dim wright As Single = right - diff
                word.Left = wright - word.Width
            Next
        End Sub

        ''' <summary>
        ''' Gets the ascent of the font
        ''' </summary>
        ''' <param name="f"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Font metrics from http://msdn.microsoft.com/en-us/library/xwf9s90b(VS.71).aspx
        ''' </remarks>
        Public Shared Function GetAscent(f As Font) As Single
            Dim mainAscent As Single = f.Size * f.FontFamily.GetCellAscent(f.Style) / f.FontFamily.GetEmHeight(f.Style)
            Return mainAscent
        End Function

        ''' <summary>
        ''' Gets the descent of the font
        ''' </summary>
        ''' <param name="f"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Font metrics from http://msdn.microsoft.com/en-us/library/xwf9s90b(VS.71).aspx
        ''' </remarks>
        Public Shared Function GetDescent(f As Font) As Single
            Dim mainDescent As Single = f.Size * f.FontFamily.GetCellDescent(f.Style) / f.FontFamily.GetEmHeight(f.Style)
            Return mainDescent
        End Function

        ''' <summary>
        ''' Gets the line spacing of the font
        ''' </summary>
        ''' <param name="f"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Font metrics from http://msdn.microsoft.com/en-us/library/xwf9s90b(VS.71).aspx
        ''' </remarks>
        Public Shared Function GetLineSpacing(f As Font) As Single
            Dim s As Single = f.Size * f.FontFamily.GetLineSpacing(f.Style) / f.FontFamily.GetEmHeight(f.Style)
            Return s
        End Function

        ''' <summary>
        ''' Applies vertical alignment to the linebox
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="lineBox"></param>
        Private Shared Sub ApplyVerticalAlignment(g As Graphics, lineBox As CssLineBox)

            Dim isTableCell As Boolean = lineBox.OwnerBox.Display = CssConstants.TableCell
            Dim baseline As Single = lineBox.GetMaxWordBottom() - GetDescent(lineBox.OwnerBox.ActualFont) - 2
            Dim boxes As New List(Of CssBox)(lineBox.Rectangles.Keys)

            For Each b As CssBox In boxes
                Dim ascent As Single = GetAscent(b.ActualFont)
                Dim descent As Single = GetDescent(b.ActualFont)

                'Important notes on http://www.w3.org/TR/CSS21/tables.html#height-layout
                Select Case b.VerticalAlign
                    Case CssConstants.[Sub]
                        lineBox.SetBaseLine(g, b, baseline + lineBox.Rectangles(b).Height * 0.2F)

                    Case CssConstants.Super
                        lineBox.SetBaseLine(g, b, baseline - lineBox.Rectangles(b).Height * 0.2F)

                    Case CssConstants.TextTop


                    Case CssConstants.TextBottom


                    Case CssConstants.Top


                    Case CssConstants.Bottom


                    Case CssConstants.Middle


                    Case Else
                        'case: baseline
                        lineBox.SetBaseLine(g, b, baseline)


                        'Graphic cues
                        'g.FillRectangle(Brushes.Aqua, r.Left, r.Top, r.Width, ascent);
                        'g.FillRectangle(Brushes.Yellow, r.Left, r.Top + ascent, r.Width, descent);
                        'g.DrawLine(Pens.Fuchsia, r.Left, baseline, r.Right, baseline);

                End Select
            Next


        End Sub

        ''' <summary>
        ''' Applies special vertical alignment for table-cells
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="cell"></param>
        Public Shared Sub ApplyCellVerticalAlignment(g As Graphics, cell As CssBox)
            If cell.VerticalAlign = CssConstants.Top OrElse cell.VerticalAlign = CssConstants.Baseline Then
                Return
            End If

            Dim celltop As Single = cell.ClientTop
            Dim cellbot As Single = cell.ClientBottom
            Dim bottom As Single = cell.GetMaximumBottom(cell, 0F)
            Dim dist As Single = 0F

            If cell.VerticalAlign = CssConstants.Bottom Then
                dist = cellbot - bottom
            ElseIf cell.VerticalAlign = CssConstants.Middle Then
                dist = (cellbot - bottom) / 2
            End If

            For Each b As CssBox In cell.Boxes
                b.OffsetTop(dist)
            Next

            'float top = cell.ClientTop;
            'float bottom = cell.ClientBottom;
            'bool middle = cell.VerticalAlign == CssConstants.Middle;

            'foreach (LineBox line in cell.LineBoxes)
            '{
            '    for (int i = 0; i < line.RelatedBoxes.Count; i++)
            '    {

            '        float diff = bottom - line.RelatedBoxes[i].Rectangles[line].Bottom;
            '        if (middle) diff /= 2f;
            '        RectangleF r = line.RelatedBoxes[i].Rectangles[line];
            '        line.RelatedBoxes[i].Rectangles[line] = new RectangleF(r.X, r.Y + diff, r.Width, r.Height);

            '    }

            '    foreach (BoxWord word in line.Words)
            '    {
            '        float gap = word.Top - top;
            '        word.Top = bottom - gap - word.Height;
            '    }
            '}
        End Sub

        ''' <summary>
        ''' Applies centered alignment to the text on the linebox
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="lineBox"></param>
        Private Shared Sub ApplyJustifyAlignment(g As Graphics, lineBox As CssLineBox)
            If lineBox.Equals(lineBox.OwnerBox.LineBoxes(lineBox.OwnerBox.LineBoxes.Count - 1)) Then
                Return
            End If

            Dim indent As Single = If(lineBox.Equals(lineBox.OwnerBox.LineBoxes(0)), lineBox.OwnerBox.ActualTextIndent, 0F)
            Dim textSum As Single = 0F
            Dim words As Single = 0F
            Dim availWidth As Single = lineBox.OwnerBox.ClientRectangle.Width - indent

            '#Region "Gather text sum"
            For Each w As CssBoxWord In lineBox.Words
                textSum += w.Width
                words += 1.0F
            Next
            '#End Region

            If words <= 0F Then
                Return
            End If
            'Avoid Zero division
            Dim spacing As Single = (availWidth - textSum) / words
            'Spacing that will be used
            Dim curx As Single = lineBox.OwnerBox.ClientLeft + indent

            For Each word As CssBoxWord In lineBox.Words
                word.Left = curx
                curx = word.Right + spacing

                If word Is lineBox.Words(lineBox.Words.Count - 1) Then
                    word.Left = lineBox.OwnerBox.ClientRight - word.Width

                    'TODO: Background rectangles are being deactivated when justifying text.
                End If
            Next



        End Sub

        ''' <summary>
        ''' Applies centered alignment to the text on the linebox
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="line"></param>
        Private Shared Sub ApplyCenterAlignment(g As Graphics, line As CssLineBox)
            If line.Words.Count = 0 Then
                Return
            End If

            Dim lastWord As CssBoxWord = line.Words(line.Words.Count - 1)
            Dim right As Single = line.OwnerBox.ActualRight - line.OwnerBox.ActualPaddingRight - line.OwnerBox.ActualBorderRightWidth
            Dim diff As Single = right - lastWord.Right - lastWord.LastMeasureOffset.X - lastWord.OwnerBox.ActualBorderRightWidth - lastWord.OwnerBox.ActualPaddingRight
            diff /= 2

            If diff <= 0 Then
                Return
            End If

            For Each word As CssBoxWord In line.Words
                word.Left += diff
            Next

            For Each b As CssBox In line.Rectangles.Keys
                Dim r As RectangleF = b.Rectangles(line)
                b.Rectangles(line) = New RectangleF(r.X + diff, r.Y, r.Width, r.Height)
            Next
        End Sub

        ''' <summary>
        ''' Applies right alignment to the text on the linebox
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="line"></param>
        Private Shared Sub ApplyRightAlignment(g As Graphics, line As CssLineBox)
            If line.Words.Count = 0 Then
                Return
            End If


            Dim lastWord As CssBoxWord = line.Words(line.Words.Count - 1)
            Dim right As Single = line.OwnerBox.ActualRight - line.OwnerBox.ActualPaddingRight - line.OwnerBox.ActualBorderRightWidth
            Dim diff As Single = right - lastWord.Right - lastWord.LastMeasureOffset.X - lastWord.OwnerBox.ActualBorderRightWidth - lastWord.OwnerBox.ActualPaddingRight


            If diff <= 0 Then
                Return
            End If

            'if (line.OwnerBox.Direction == CssConstants.Rtl)
            '{

            '}

            For Each word As CssBoxWord In line.Words
                word.Left += diff
            Next

            For Each b As CssBox In line.Rectangles.Keys
                Dim r As RectangleF = b.Rectangles(line)
                b.Rectangles(line) = New RectangleF(r.X + diff, r.Y, r.Width, r.Height)
            Next
        End Sub

        ''' <summary>
        ''' Simplest alignment, just arrange words.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="line"></param>
        Private Shared Sub ApplyLeftAlignment(g As Graphics, line As CssLineBox)
            'No alignment needed.

            'foreach (LineBoxRectangle r in line.Rectangles)
            '{
            '    float curx = r.Left + (r.Index == 0 ? r.OwnerBox.ActualPaddingLeft + r.OwnerBox.ActualBorderLeftWidth / 2 : 0);

            '    if (r.SpaceBefore) curx += r.OwnerBox.ActualWordSpacing;

            '    foreach (BoxWord word in r.Words)
            '    {
            '        word.Left = curx;
            '        word.Top = r.Top;// +r.OwnerBox.ActualPaddingTop + r.OwnerBox.ActualBorderTopWidth / 2;

            '        curx = word.Right + r.OwnerBox.ActualWordSpacing;
            '    }
            '}
        End Sub

#End Region
    End Class
End Namespace
