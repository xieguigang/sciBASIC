#Region "Microsoft.VisualBasic::39f304b96e1eae7e3019477ff8875cab, mime\text%html\HTML\CSS\Render\CssTable.vb"

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

    '     Class CssTable
    ' 
    '         Properties: AllRows, BodyRows, Caption, ColumnCount, ColumnMinWidths
    '                     Columns, ColumnWidths, FooterBox, HeaderBox, HorizontalSpacing
    '                     RowCount, TableBox, VerticalSpacing, WidthSpecified
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) CanReduceWidth, CreateColumn, GetAvailableCellWidth, GetAvailableWidth, GetCellRealColumnIndex
    '                   GetCellWidth, GetColSpan, GetReductableColumns, GetRowSpan, GetSpan
    '                   GetSpannedMinWidth, GetWidthSum
    ' 
    '         Sub: Analyze, Measure, MeasureWords
    '         Class SpacingBox
    ' 
    '             Properties: EndRow, StartRow
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Math
Imports Microsoft.VisualBasic.MIME.Markup.HTML.Render

Namespace HTML.CSS.Render

    Friend Class CssTable
#Region "Subclasses"

        ''' <summary>
        ''' Used to make space on vertical cell combination
        ''' </summary>
        Public Class SpacingBox
            Inherits CssBox
            Public ReadOnly ExtendedBox As CssBox

            Public Sub New(tableBox As CssBox, ByRef extendedBox__1 As CssBox, startRow As Integer)
                MyBase.New(tableBox, New HtmlTag("<none colspan=" & extendedBox__1.GetAttribute("colspan", "1") & ">"))
                ExtendedBox = extendedBox__1
                Display = CssConstants.None

                _startRow = startRow
                _endRow = startRow + Integer.Parse(extendedBox__1.GetAttribute("rowspan", "1")) - 1
            End Sub

#Region "Props"

            Private _startRow As Integer
            ''' <summary>
            ''' Gets the index of the row where box starts
            ''' </summary>
            Public ReadOnly Property StartRow() As Integer
                Get
                    Return _startRow
                End Get
            End Property

            Private _endRow As Integer

            ''' <summary>
            ''' Gets the index of the row where box ends
            ''' </summary>
            Public ReadOnly Property EndRow() As Integer
                Get
                    Return _endRow
                End Get
            End Property


#End Region
        End Class

#End Region

#Region "Fields"

        Private _tableBox As CssBox
        Private _rowCount As Integer
        Private _columnCount As Integer
        Private _bodyrows As List(Of CssBox)
        Private _caption As CssBox
        Private _columns As List(Of CssBox)
        Private _headerBox As CssBox
        Private _footerBox As CssBox
        Private _allRows As List(Of CssBox)
        Private _columnWidths As Single()
        Private _widthSpecified As Boolean
        Private _columnMinWidths As Single()

#End Region

#Region "Ctor"

        Private Sub New()
            _bodyrows = New List(Of CssBox)()
            _columns = New List(Of CssBox)()
            _allRows = New List(Of CssBox)()
        End Sub

        Public Sub New(tableBox As CssBox, g As Graphics)
            Me.New()
            If Not (tableBox.Display = CssConstants.Table OrElse tableBox.Display = CssConstants.InlineTable) Then
                Throw New ArgumentException("Box is not a table", "tableBox")
            End If

            _tableBox = tableBox

            MeasureWords(tableBox, g)

            Analyze(g)
        End Sub

#End Region

#Region "Props"

        ''' <summary>
        ''' Gets if the user specified a width for the table
        ''' </summary>
        Public ReadOnly Property WidthSpecified() As Boolean
            Get
                Return _widthSpecified
            End Get
        End Property

        ''' <summary>
        ''' Hosts a list of all rows in the table, including those on the TFOOT, THEAD and TBODY
        ''' </summary>
        Public ReadOnly Property AllRows() As List(Of CssBox)
            Get
                Return _allRows
            End Get
        End Property

        ''' <summary>
        ''' Gets the box that represents the caption of this table, if any.
        ''' WARNING: May be null
        ''' </summary>
        Public ReadOnly Property Caption() As CssBox
            Get
                Return _caption
            End Get
        End Property

        ''' <summary>
        ''' Gets the column count of this table
        ''' </summary>
        Public ReadOnly Property ColumnCount() As Integer
            Get
                Return _columnCount
            End Get
        End Property

        ''' <summary>
        ''' Gets the minimum width of each column
        ''' </summary>
        Public ReadOnly Property ColumnMinWidths() As Single()
            Get
                If _columnMinWidths Is Nothing Then
                    _columnMinWidths = New Single(ColumnWidths.Length - 1) {}

                    For Each row As CssBox In AllRows
                        For Each cell As CssBox In row.Boxes
                            Dim colspan As Integer = GetColSpan(cell)
                            Dim col As Integer = GetCellRealColumnIndex(row, cell)
                            Dim affectcol As Integer = col + colspan - 1
                            Dim spannedwidth As Single = GetSpannedMinWidth(row, cell, col, colspan) + (colspan - 1) * HorizontalSpacing


                            _columnMinWidths(affectcol) = Max(_columnMinWidths(affectcol), cell.GetMinimumWidth() - spannedwidth)
                        Next

                    Next
                End If

                Return _columnMinWidths
            End Get
        End Property

        ''' <summary>
        ''' Gets the declared Columns on the TABLE tag
        ''' </summary>
        Public ReadOnly Property Columns() As List(Of CssBox)
            Get
                Return _columns
            End Get
        End Property

        ''' <summary>
        ''' Gets an array indicating the withs of each column.
        ''' This must have the same count than <see cref="Columns"/>
        ''' </summary>
        Public ReadOnly Property ColumnWidths() As Single()
            Get
                Return _columnWidths
            End Get
        End Property

        ''' <summary>
        ''' Gets the boxes that represents the table-row Boxes of the table, 
        ''' including those inside of the TBODY tags
        ''' </summary>
        Public ReadOnly Property BodyRows() As List(Of CssBox)
            Get
                Return _bodyrows
            End Get
        End Property

        ''' <summary>
        ''' Gets the table-footer-group Box
        ''' WARNING: May be null
        ''' </summary>
        Public ReadOnly Property FooterBox() As CssBox
            Get
                Return _footerBox
            End Get
        End Property

        ''' <summary>
        ''' Gets the table-header-group Box
        ''' WARNING: May be null
        ''' </summary>
        Public ReadOnly Property HeaderBox() As CssBox
            Get
                Return _headerBox
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual horizontal spacing of the table
        ''' </summary>
        Public ReadOnly Property HorizontalSpacing() As Single
            Get
                If TableBox.BorderCollapse = CssConstants.Collapse Then
                    Return -1.0F
                End If

                Return TableBox.ActualBorderSpacingHorizontal
            End Get
        End Property

        ''' <summary>
        ''' Gets the actual vertical spacing of the table
        ''' </summary>
        Public ReadOnly Property VerticalSpacing() As Single
            Get
                If TableBox.BorderCollapse = CssConstants.Collapse Then
                    Return -1.0F
                End If

                Return TableBox.ActualBorderSpacingVertical
            End Get
        End Property

        ''' <summary>
        ''' Gets the row count of this table, including the rows inside the table-row-group,
        ''' table-row-heaer and table-row-footer Boxes
        ''' </summary>
        Public ReadOnly Property RowCount() As Integer
            Get
                Return _rowCount
            End Get
        End Property

        ''' <summary>
        ''' Gets the original table box
        ''' </summary>
        Public ReadOnly Property TableBox() As CssBox
            Get
                Return _tableBox
            End Get
        End Property

#End Region

#Region "Methods"

        ''' <summary>
        ''' Analyzes the Table and assigns values to this CssTable object.
        ''' To be called from the constructor
        ''' </summary>
        Private Sub Analyze(g As Graphics)
            Dim availSpace As Single = GetAvailableWidth()
            Dim availCellSpace As Single = Single.NaN
            'Will be set later
            '#Region "Assign box kinds"
            For Each b As CssBox In TableBox.Boxes
                b.RemoveAnonymousSpaces()
                Select Case b.Display
                    Case CssConstants.TableCaption
                        _caption = b

                    Case CssConstants.TableColumn
                        For i As Integer = 0 To GetSpan(b) - 1
                            Columns.Add(CreateColumn(b))
                        Next

                    Case CssConstants.TableColumnGroup
                        If b.Boxes.Count = 0 Then
                            Dim gspan As Integer = GetSpan(b)
                            For i As Integer = 0 To gspan - 1
                                Columns.Add(CreateColumn(b))
                            Next
                        Else
                            For Each bb As CssBox In b.Boxes
                                Dim bbspan As Integer = GetSpan(bb)
                                For i As Integer = 0 To bbspan - 1
                                    Columns.Add(CreateColumn(bb))
                                Next
                            Next
                        End If

                    Case CssConstants.TableFooterGroup
                        If FooterBox IsNot Nothing Then
                            BodyRows.Add(b)
                        Else
                            _footerBox = b
                        End If

                    Case CssConstants.TableHeaderGroup
                        If HeaderBox IsNot Nothing Then
                            BodyRows.Add(b)
                        Else
                            _headerBox = b
                        End If

                    Case CssConstants.TableRow
                        BodyRows.Add(b)

                    Case CssConstants.TableRowGroup
                        For Each bb As CssBox In b.Boxes
                            If b.Display = CssConstants.TableRow Then
                                BodyRows.Add(b)
                            End If
                        Next

                    Case Else

                End Select
            Next
            '#End Region

            '#Region "Gather AllRows"

            If HeaderBox IsNot Nothing Then
                _allRows.AddRange(HeaderBox.Boxes)
            End If
            _allRows.AddRange(BodyRows)
            If FooterBox IsNot Nothing Then
                _allRows.AddRange(FooterBox.Boxes)
            End If

            '#End Region

            '#Region "Insert EmptyBoxes for vertical cell spanning"

            If Not TableBox.TableFixed Then
                Dim currow As Integer = 0
                Dim curcol__1 As Integer = 0
                Dim rows As List(Of CssBox) = BodyRows

                For Each row As CssBox In rows
                    row.RemoveAnonymousSpaces()
                    curcol__1 = 0
                    For k As Integer = 0 To row.Boxes.Count - 1

                        Dim cell As CssBox = row.Boxes(k)
                        Dim rowspan As Integer = GetRowSpan(cell)
                        Dim realcol As Integer = GetCellRealColumnIndex(row, cell)
                        'Real column of the cell
                        For i As Integer = currow + 1 To currow + (rowspan - 1)
                            Dim colcount As Integer = 0
                            For j As Integer = 0 To rows(i).Boxes.Count
                                If colcount = realcol Then
                                    rows(i).Boxes.Insert(colcount, New SpacingBox(TableBox, cell, currow))
                                    Exit For
                                End If
                                colcount += 1
                                realcol -= GetColSpan(rows(i).Boxes(j)) - 1

                            Next
                        Next
                        ' End for (int i = currow + 1; i < currow + rowspan; i++)
                        curcol__1 += 1
                    Next
                    ' End foreach (Box cell in row.Boxes)
                    currow += 1
                Next
                ' End foreach (Box row in rows)

                TableBox.TableFixed = True
            End If
            ' End if (!TableBox.TableFixed)
            '#End Region

            '#Region "Determine Row and Column Count, and ColumnWidths"

            'Rows
            _rowCount = BodyRows.Count + (If(HeaderBox IsNot Nothing, HeaderBox.Boxes.Count, 0)) + (If(FooterBox IsNot Nothing, FooterBox.Boxes.Count, 0))

            'Columns
            If Columns.Count > 0 Then
                _columnCount = Columns.Count
            Else
                For Each b As CssBox In AllRows
                    'Check trhough rows
                    _columnCount = Max(_columnCount, b.Boxes.Count)
                Next
            End If

            'Initialize column widths array
            _columnWidths = New Single(_columnCount - 1) {}

            'Fill them with NaNs
            For i As Integer = 0 To _columnWidths.Length - 1
                _columnWidths(i) = Single.NaN
            Next

            availCellSpace = GetAvailableCellWidth()

            If Columns.Count > 0 Then
                '#Region "Fill ColumnWidths array by scanning column widths"

                For i As Integer = 0 To Columns.Count - 1
                    Dim len As New CssLength(Columns(i).Width)
                    'Get specified width
                    If len.Number > 0 Then
                        'If some width specified
                        If len.IsPercentage Then
                            'Get width as a percentage
                            ColumnWidths(i) = CssValue.ParseNumber(Columns(i).Width, availCellSpace)
                        ElseIf len.Unit = CssLength.CssUnit.Pixels OrElse len.Unit = CssLength.CssUnit.None Then
                            'Get width as an absolute-pixel value
                            ColumnWidths(i) = len.Number
                        End If
                    End If

                    '#End Region
                Next
            Else
                '#Region "Fill ColumnWidths array by scanning width in table-cell definitions"
                For Each row As CssBox In AllRows
                    'Check for column width in table-cell definitions
                    For i As Integer = 0 To _columnCount - 1
                        'Check if no width specified for column
                        'And there's a box to check
                        If Single.IsNaN(ColumnWidths(i)) AndAlso i < row.Boxes.Count AndAlso row.Boxes(i).Display = CssConstants.TableCell Then
                            'And the box is a table-cell
                            Dim len As New CssLength(row.Boxes(i).Width)
                            'Get specified width
                            If len.Number > 0 Then
                                'If some width specified
                                Dim colspan As Integer = GetColSpan(row.Boxes(i))
                                Dim flen As Single = 0F
                                If len.IsPercentage Then
                                    'Get width as a percentage
                                    flen = CssValue.ParseNumber(row.Boxes(i).Width, availCellSpace)
                                ElseIf len.Unit = CssLength.CssUnit.Pixels OrElse len.Unit = CssLength.CssUnit.None Then
                                    'Get width as an absolute-pixel value
                                    flen = len.Number
                                End If
                                flen /= Convert.ToSingle(colspan)

                                For j As Integer = i To i + (colspan - 1)
                                    ColumnWidths(j) = flen
                                Next
                            End If
                        End If
                    Next
                    '#End Region
                Next
            End If

            '#End Region

            '#Region "Determine missing Column widths"

            If WidthSpecified Then
                'If a width was specified,
                'Assign NaNs equally with space left after gathering not-NaNs
                Dim numberOfNans As Integer = 0
                Dim occupedSpace As Single = 0F

                'Calculate number of NaNs and occuped space
                For i As Integer = 0 To ColumnWidths.Length - 1
                    If Single.IsNaN(ColumnWidths(i)) Then
                        numberOfNans += 1
                    Else
                        occupedSpace += ColumnWidths(i)
                    End If
                Next

                'Determine width that will be assigned to un asigned widths
                Dim nanWidth As Single = (availCellSpace - occupedSpace) / Convert.ToSingle(numberOfNans)

                For i As Integer = 0 To ColumnWidths.Length - 1
                    If Single.IsNaN(ColumnWidths(i)) Then
                        ColumnWidths(i) = nanWidth
                    End If
                Next
            Else
                'Assign NaNs using full width
                Dim _maxFullWidths As Single() = New Single(ColumnWidths.Length - 1) {}

                'Get the maximum full length of NaN boxes
                For Each row As CssBox In AllRows
                    For i As Integer = 0 To row.Boxes.Count - 1
                        Dim col As Integer = GetCellRealColumnIndex(row, row.Boxes(i))

                        If Single.IsNaN(ColumnWidths(col)) AndAlso i < row.Boxes.Count AndAlso GetColSpan(row.Boxes(i)) = 1 Then
                            _maxFullWidths(col) = Max(_maxFullWidths(col), row.Boxes(i).GetFullWidth(g))
                        End If
                    Next
                Next

                For i As Integer = 0 To ColumnWidths.Length - 1
                    If Single.IsNaN(ColumnWidths(i)) Then
                        ColumnWidths(i) = _maxFullWidths(i)
                    End If
                Next
            End If

            '#End Region

            '#Region "Reduce widths if necessary"

            Dim curCol__2 As Integer = 0
            Dim reduceAmount As Single = 1.0F

            'While table width is larger than it should, and width is reductable
            While GetWidthSum() > GetAvailableWidth() AndAlso CanReduceWidth()
                While Not CanReduceWidth(curCol__2)
                    curCol__2 += 1
                End While

                ColumnWidths(curCol__2) -= reduceAmount

                curCol__2 += 1

                If curCol__2 >= ColumnWidths.Length Then
                    curCol__2 = 0
                End If
            End While

            '#End Region

            '#Region "Check for minimum sizes (increment widths if necessary)"

            For Each row As CssBox In AllRows
                For Each cell As CssBox In row.Boxes
                    Dim colspan As Integer = GetColSpan(cell)
                    Dim col As Integer = GetCellRealColumnIndex(row, cell)
                    Dim affectcol As Integer = col + colspan - 1

                    If ColumnWidths(col) < ColumnMinWidths(col) Then
                        Dim diff As Single = ColumnMinWidths(col) - ColumnWidths(col)
                        ColumnWidths(affectcol) = ColumnMinWidths(affectcol)

                        If col < ColumnWidths.Length - 1 Then
                            ColumnWidths(col + 1) -= diff
                        End If
                    End If
                Next
            Next

            '#End Region

            '#Region "Set table padding"

            TableBox.Padding = "0"
            'Ensure there's no padding
            '#End Region

            '#Region "Layout cells"

            'Actually layout cells!
            Dim startx As Single = TableBox.ClientLeft + HorizontalSpacing
            Dim starty As Single = TableBox.ClientTop + VerticalSpacing
            Dim curx As Single = startx
            Dim cury As Single = starty
            Dim maxRight As Single = startx
            Dim maxBottom As Single = 0F
            Dim currentrow As Integer = 0

            For Each row As CssBox In AllRows
                If TypeOf row Is CssAnonymousSpaceBlockBox OrElse TypeOf row Is CssAnonymousSpaceBox Then
                    Continue For
                End If

                curx = startx
                curCol__2 = 0

                For Each cell As CssBox In row.Boxes
                    If curCol__2 >= ColumnWidths.Length Then
                        Exit For
                    End If

                    Dim rowspan As Integer = GetRowSpan(cell)
                    Dim width As Single = GetCellWidth(GetCellRealColumnIndex(row, cell), cell)

                    cell.Location = New PointF(curx, cury)
                    cell.Size = New SizeF(width, 0F)
                    cell.MeasureBounds(g)
                    'That will automatically set the bottom of the cell
                    'Alter max bottom only if row is cell's row + cell's rowspan - 1
                    Dim sb As SpacingBox = TryCast(cell, SpacingBox)
                    If sb IsNot Nothing Then
                        If sb.EndRow = currentrow Then
                            maxBottom = Max(maxBottom, sb.ExtendedBox.ActualBottom)
                        End If
                    ElseIf rowspan = 1 Then
                        maxBottom = Max(maxBottom, cell.ActualBottom)
                    End If
                    maxRight = Max(maxRight, cell.ActualRight)
                    curCol__2 += 1
                    curx = cell.ActualRight + HorizontalSpacing
                Next

                For Each cell As CssBox In row.Boxes
                    Dim spacer As SpacingBox = TryCast(cell, SpacingBox)

                    If spacer Is Nothing AndAlso GetRowSpan(cell) = 1 Then
                        cell.ActualBottom = maxBottom
                        CssLayoutEngine.ApplyCellVerticalAlignment(g, cell)
                    ElseIf spacer IsNot Nothing AndAlso spacer.EndRow = currentrow Then
                        spacer.ExtendedBox.ActualBottom = maxBottom
                        CssLayoutEngine.ApplyCellVerticalAlignment(g, spacer.ExtendedBox)
                    End If
                Next

                cury = maxBottom + VerticalSpacing
                currentrow += 1
            Next

            TableBox.ActualRight = maxRight + HorizontalSpacing + TableBox.ActualBorderRightWidth
            TableBox.ActualBottom = maxBottom + VerticalSpacing + TableBox.ActualBorderBottomWidth

            '#End Region
        End Sub

        ''' <summary>
        ''' Gets the spanned width of a cell
        ''' (With of all columns it spans minus one)
        ''' </summary>
        ''' <param name="row"></param>
        ''' <param name="cell"></param>
        ''' <param name="realcolindex"></param>
        ''' <param name="colspan"></param>
        ''' <returns></returns>
        Private Function GetSpannedMinWidth(row As CssBox, cell As CssBox, realcolindex As Integer, colspan As Integer) As Single
            Dim w As Single = 0F

            Dim i As Integer = realcolindex
            While i < row.Boxes.Count OrElse i < realcolindex + colspan - 1
                w += ColumnMinWidths(i)
                i += 1
            End While

            Return w
        End Function

        ''' <summary>
        ''' Gets the cell column index checking its position and other cells colspans
        ''' </summary>
        ''' <param name="row"></param>
        ''' <param name="cell"></param>
        ''' <returns></returns>
        Private Function GetCellRealColumnIndex(row As CssBox, cell As CssBox) As Integer
            Dim i As Integer = 0

            For Each b As CssBox In row.Boxes
                If b.Equals(cell) Then
                    Exit For
                End If
                i += GetColSpan(b)
            Next

            Return i
        End Function

        ''' <summary>
        ''' Gets the cells width, taking colspan and being in the specified column
        ''' </summary>
        ''' <param name="column"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Private Function GetCellWidth(column As Integer, b As CssBox) As Single
            Dim colspan As Single = Convert.ToSingle(GetColSpan(b))
            Dim sum As Single = 0F

            For i As Integer = column To column + (colspan - 1)
                If column >= ColumnWidths.Length Then
                    Exit For
                End If
                If ColumnWidths.Length <= i Then
                    Exit For
                End If
                sum += ColumnWidths(i)
            Next

            sum += (colspan - 1) * HorizontalSpacing

            Return sum


            ' -b.ActualBorderLeftWidth - b.ActualBorderRightWidth - b.ActualPaddingRight - b.ActualPaddingLeft;
        End Function

        ''' <summary>
        ''' Gets the colspan of the specified box
        ''' </summary>
        ''' <param name="b"></param>
        Private Function GetColSpan(b As CssBox) As Integer
            Dim att As String = b.GetAttribute("colspan", "1")
            Dim colspan As Integer

            If Not Integer.TryParse(att, colspan) Then
                Return 1
            End If

            Return colspan
        End Function

        ''' <summary>
        ''' Gets the rowspan of the specified box
        ''' </summary>
        ''' <param name="b"></param>
        Private Function GetRowSpan(b As CssBox) As Integer
            Dim att As String = b.GetAttribute("rowspan", "1")
            Dim rowspan As Integer

            If Not Integer.TryParse(att, rowspan) Then
                Return 1
            End If

            Return rowspan
        End Function

        ''' <summary>
        ''' Recursively measures the specified box
        ''' </summary>
        ''' <param name="b"></param>
        ''' <param name="g"></param>
        Private Sub Measure(b As CssBox, g As Graphics)
            If b Is Nothing Then
                Return
            End If

            For Each bb As CssBox In b.Boxes
                bb.MeasureBounds(g)
                Measure(bb, g)
            Next
        End Sub

        ''' <summary>
        ''' Recursively measures words inside the box
        ''' </summary>
        ''' <param name="b"></param>
        ''' <param name="g"></param>
        Private Sub MeasureWords(b As CssBox, g As Graphics)
            If b Is Nothing Then
                Return
            End If

            For Each bb As CssBox In b.Boxes
                bb.MeasureWordsSize(g)
                MeasureWords(bb, g)
            Next
        End Sub

        ''' <summary>
        ''' Gets the number of reductable columns
        ''' </summary>
        ''' <returns></returns>
        Private Function GetReductableColumns() As Integer
            Dim response As Integer = 0

            For i As Integer = 0 To ColumnWidths.Length - 1
                If CanReduceWidth(i) Then
                    response += 1
                End If
            Next

            Return response
        End Function

        ''' <summary>
        ''' Tells if the columns widths can be reduced,
        ''' by checking the minimum widths of all cells
        ''' </summary>
        ''' <returns></returns>
        Private Function CanReduceWidth() As Boolean
            For i As Integer = 0 To ColumnWidths.Length - 1
                If CanReduceWidth(i) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' Tells if the specified column can be reduced,
        ''' by checking its minimum width
        ''' </summary>
        ''' <param name="columnIndex"></param>
        ''' <returns></returns>
        Private Function CanReduceWidth(columnIndex As Integer) As Boolean
            If ColumnWidths.Length >= columnIndex OrElse ColumnMinWidths.Length >= columnIndex Then
                Return False
            End If
            Return ColumnWidths(columnIndex) > ColumnMinWidths(columnIndex)
        End Function

        ''' <summary>
        ''' Gets the available width for the whole table.
        ''' It also sets the value of <see cref="WidthSpecified"/>
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' The table's width can be larger than the result of this method, because of the minimum 
        ''' size that individual boxes.
        ''' </remarks>
        Private Function GetAvailableWidth() As Single
            Dim tblen As New CssLength(TableBox.Width)

            If tblen.Number > 0 Then
                _widthSpecified = True

                If tblen.IsPercentage Then
                    Return CssValue.ParseNumber(tblen.Length, TableBox.ParentBox.AvailableWidth)
                Else
                    Return tblen.Number
                End If
            Else
                Return TableBox.ParentBox.AvailableWidth
            End If
        End Function

        ''' <summary>
        ''' Gets the width available for cells
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' It takes away the cell-spacing from <see cref="GetAvailableWidth()"/>
        ''' </remarks>
        Private Function GetAvailableCellWidth() As Single
            Return GetAvailableWidth() - HorizontalSpacing * (ColumnCount + 1) - TableBox.ActualBorderLeftWidth - TableBox.ActualBorderRightWidth
        End Function

        ''' <summary>
        ''' Gets the current sum of column widths
        ''' </summary>
        ''' <returns></returns>
        Private Function GetWidthSum() As Single
            Dim f As Single = 0F

            For i As Integer = 0 To ColumnWidths.Length - 1
                If Single.IsNaN(ColumnWidths(i)) Then
                    Throw New Exception("CssTable Algorithm error: There's a NaN in column widths")
                Else
                    f += ColumnWidths(i)
                End If
            Next

            'Take cell-spacing
            f += HorizontalSpacing * (ColumnWidths.Length + 1)

            'Take table borders
            f += TableBox.ActualBorderLeftWidth + TableBox.ActualBorderRightWidth

            Return f
        End Function

        ''' <summary>
        ''' Gets the span attribute of the tag of the specified box
        ''' </summary>
        ''' <param name="b"></param>
        Private Function GetSpan(b As CssBox) As Integer
            Dim f As Single = CssValue.ParseNumber(b.GetAttribute("span"), 1)
            Return Max(1, Convert.ToInt32(f))
        End Function

        ''' <summary>
        ''' Creates the column with the specified width
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateColumn(modelBox As CssBox) As CssBox
            Return modelBox
            'Box b = new Box(null, new HtmlTag(string.Format("<COL style=\"width:{0}\" >", width)));
            'b.Width = width;
            'return b;
        End Function

#End Region
    End Class
End Namespace
