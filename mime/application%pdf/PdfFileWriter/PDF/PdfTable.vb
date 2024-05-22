#Region "Microsoft.VisualBasic::7d97f28e179b7ce9c721402da6987ff8, mime\application%pdf\PdfFileWriter\PDF\PdfTable.vb"

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

    '   Total Lines: 1014
    '    Code Lines: 391 (38.56%)
    ' Comment Lines: 478 (47.14%)
    '    - Xml Docs: 73.01%
    ' 
    '   Blank Lines: 145 (14.30%)
    '     File Size: 34.91 KB


    ' Delegate Sub
    ' 
    ' 
    ' Delegate Sub
    ' 
    ' 
    ' Delegate Function
    ' 
    ' 
    ' Class PdfTable
    ' 
    '     Properties: Active, Borders, Cell, CellStyle, ColumnPosition
    '                 Columns, ColumnWidth, CommitGCCollectFreq, CommitToPdfFile, Contents
    '                 DefaultCellStyle, DefaultHeaderStyle, Document, Header, HeaderHeight
    '                 HeaderOnEachPage, HeaderStyle, MinHeaderHeight, MinRowHeight, Page
    '                 RowBottomPosition, RowHeight, RowNumber, RowPosition, RowTopPosition
    '                 TableArea, TablePageNumber
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: CalculateHeaderHeight, CalculateRowHeight, Close, CreateNewPage, DrawBorders
    '          DrawHeader, DrawOneRow, DrawRow, PdfTableInitialization, SetColumnWidth
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
'
'	PdfFileWriter
'	PDF File Write C# Class Library.
'
'	PdfTable
'	Data table support.
'
'	Uzi Granot
'	Version: 1.0
'	Date: April 1, 2013
'	Copyright (C) 2013-2019 Uzi Granot. All Rights Reserved
'
'	PdfFileWriter C# class library and TestPdfFileWriter test/demo
'  application are free software.
'	They is distributed under the Code Project Open License (CPOL).
'	The document PdfFileWriterReadmeAndLicense.pdf contained within
'	the distribution specify the license agreement and other
'	conditions and notes. You must read this document and agree
'	with the conditions specified in order to use this software.
'
'	For version history please refer to PdfDocument.cs
'
'

Imports System
Imports System.Collections.Generic
Imports System.Drawing


''' <summary>
''' PDF table start event handler delegate
''' </summary>
''' <param name="Table">Parent object.</param>
''' <param name="TableStartPos">Table top's position on the page.</param>
''' <remarks>
''' The purpose of the table start event handler is to allow custom 
''' drawing on the page before the header and the first row are drawn. 
''' 	For example, drawing a title above the table.
''' 	</remarks>
Public Delegate Sub PdfTableStart(Table As PdfTable, TableStartPos As Double)

''' <summary>
''' PDF table end event handler delegate
''' </summary>
''' <param name="Table">Parent object.</param>
''' <param name="TableEndPos">Table bottom's position on the page.</param>
''' <remarks>
''' The purpose of the table end event handler is to allow custom 
''' drawing on the page after the last row was drawn. 
''' 	For example, drawing comment below the table.
''' 	</remarks>
Public Delegate Sub PdfTableEnd(Table As PdfTable, TableEndPos As Double)

''' <summary>
''' PDF table custom draw cell event handler delegate
''' </summary>
''' <param name="Table">The current table object.</param>
''' <param name="Cell">The current's cell object.</param>
''' <returns>Action taken (see remarks).</returns>
''' <remarks>
''' <para>True if the event handler drew the cell.</para>
''' <para>False if the event handler did not draw the cell.</para>
''' </remarks>
Public Delegate Function PdfTableCustomDrawCell(Table As PdfTable, Cell As PdfTableCell) As Boolean

''' <summary>
''' PDF data table drawing class
''' </summary>
''' <remarks>
''' <para>
''' The main class for drawing a data table within a PDF document.
''' </para>
''' <para>
''' For more information go to <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DataTableSupport">2.12 Data Table Support</a>
''' </para>
''' <para>
''' <a href="http://www.codeproject.com/Articles/570682/PDF-File-Writer-Csharp-Class-Library-Version#DrawDataTable">For example of drawing image see 3.13. Draw Book Order Form</a>
''' </para>
''' </remarks>
Public Class PdfTable

    ''' <summary>
    ''' Borders control
    ''' </summary>
    Private _DefaultCellStyle As PdfTableStyle, _DefaultHeaderStyle As PdfTableStyle, _Document As PdfDocument, _HeaderHeight As Double, _Page As PdfPage, _TablePageNumber As Integer, _RowBottomPosition As Double, _RowHeight As Double, _RowNumber As Integer, _Borders As PdfTableBorder

    ''' <summary>
    ''' Gets the table is active flag.
    ''' </summary>
    ''' <remarks>
    ''' The active flag is set by the PdfTableInitialization method.
    ''' </remarks>
    Public Property Active As Boolean

    ''' <summary>
    ''' Gets array of cell items.
    ''' </summary>
    ''' <remarks>
    ''' SetColumnWidth method creates the Cell array. 
    ''' Cell array length is Columns. Each cell controls the drawing of one column.
    ''' </remarks>
    Public ReadOnly Property Cell As PdfTableCell()

    ''' <summary>
    ''' Creates a cell style object as a copy of the default cell style.
    ''' </summary>
    Public ReadOnly Property CellStyle As PdfTableStyle
        Get
            Return New PdfTableStyle(DefaultCellStyle)
        End Get
    End Property

    ''' <summary>
    ''' Gets array of column positions.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Column position is the centre of a border line or the centre of a grid line. 
    ''' </para>
    ''' <para>
    ''' Position[0] is the left side of the table, and Position[Columns] is the right side of the table.
    ''' </para>
    ''' <para>
    ''' The returned array is a copy of the internal array.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property ColumnPosition As Double()
        Get
            Return CType(_ColumnPosition.Clone(), Double())
        End Get
    End Property

    Friend _ColumnPosition As Double()

    ''' <summary>
    ''' Gets the number of columns in the table.
    ''' </summary>
    ''' <value>The number of columns in the table.</value>
    ''' <remarks>SetColumnWidth method sets this value.</remarks>
    Public Property Columns As Integer

    ''' <summary>
    ''' Gets array of column widths.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Column width is from the center of border line or a grid line
    ''' to the center of the next border line or grid line. 
    ''' </para>
    ''' <para>
    ''' The returned array is a copy of the internal array.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property ColumnWidth As Double()
        Get
            Return CType(_ColumnWidth.Clone(), Double())
        End Get
    End Property

    Friend _ColumnWidth As Double()

    ''' <summary>
    ''' Commit page contents to PDF file
    ''' </summary>
    Public Property CommitToPdfFile As Boolean

    ''' <summary>
    ''' Call GC collect frequency
    ''' </summary>
    ''' <remarks>If zero or negative GC will not be called.</remarks>
    Public Property CommitGCCollectFreq As Integer

    ''' <summary>
    ''' Gets the current PDF contents object.
    ''' </summary>
    ''' <value>The current PDF contents object.</value>
    ''' <remarks>
    ''' The initial value is set by the constructor. 
    ''' The value is updated as each new page is added.
    ''' </remarks>
    Public Property Contents As PdfContents

    ''' <summary>
    ''' Gets the default cell style.
    ''' </summary>
    ''' <value>Default cell style.</value>
    ''' <remarks>
    ''' <para>
    ''' The default cell style is used by cells without private style.
    ''' </para>
    ''' <para>
    ''' The returned value is the default cell style object.
    ''' A change to any of the syle properties will affect all cells
    ''' without private style.
    ''' </para>
    ''' </remarks>
    Public Property DefaultCellStyle As PdfTableStyle
        Get
            Return _DefaultCellStyle
        End Get
        Friend Set(value As PdfTableStyle)
            _DefaultCellStyle = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the default header style.
    ''' </summary>
    ''' <value>Default header style.</value>
    ''' <remarks>
    ''' <para>
    ''' The default header style is used by headers without private style.
    ''' </para>
    ''' <para>
    ''' The returned value is the default header style object.
    ''' A change to any of the syle properties will affect all headers
    ''' without private style.
    ''' </para>
    ''' </remarks>
    Public Property DefaultHeaderStyle As PdfTableStyle
        Get
            Return _DefaultHeaderStyle
        End Get
        Friend Set(value As PdfTableStyle)
            _DefaultHeaderStyle = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the PDF document that owns this table.
    ''' </summary>
    ''' <valuw>PDF document the parent of this table.</valuw>
    Public Property Document As PdfDocument
        Get
            Return _Document
        End Get
        Friend Set(value As PdfDocument)
            _Document = value
        End Set
    End Property

    ''' <summary>
    ''' Gets array of header items.
    ''' </summary>
    ''' <remarks>
    ''' SetColumnWidth method creates Header array. Array length is Columns.
    ''' Each header controls the drawing of one column's header.
    ''' </remarks>
    Public ReadOnly Property Header As PdfTableCell()
        Get
            Return _Header
        End Get
    End Property

    Friend _Header As PdfTableCell()

    ''' <summary>
    ''' Gets the current header height.
    ''' </summary>
    ''' <value>Current header height.</value>
    ''' <remarks>
    ''' Header height is calculated once for each page.
    ''' It is the height of the tallest header.
    ''' </remarks>
    Public Property HeaderHeight As Double
        Get
            Return _HeaderHeight
        End Get
        Friend Set(value As Double)
            _HeaderHeight = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the flag controlling the drawing of the header row.
    ''' </summary>
    ''' <remarks>
    ''' If the flag is true (the default), the header is drawn on each page. 
    ''' If the flag is false, the header is drawn one time only at the start of the table.
    ''' </remarks>
    Public Property HeaderOnEachPage As Boolean

    ''' <summary>
    ''' Creates a header style object as a copy of the default header style.
    ''' </summary>
    Public ReadOnly Property HeaderStyle As PdfTableStyle
        Get
            Return New PdfTableStyle(DefaultHeaderStyle)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets minimum row height.
    ''' </summary>
    ''' <value>Minimum row height.</value>
    Public Property MinRowHeight As Double

    ''' <summary>
    ''' Gets or sets minimum row height.
    ''' </summary>
    ''' <value>Minimum header height.</value>
    Public Property MinHeaderHeight As Double

    ''' <summary>
    ''' Gets the current PDF page object.
    ''' </summary>
    ''' <value>The current PDF page object.</value>
    ''' <remarks>
    ''' The initial value is set by the constructor. 
    ''' The value is updated as each new page is added.
    ''' </remarks>
    Public Property Page As PdfPage
        Get
            Return _Page
        End Get
        Friend Set(value As PdfPage)
            _Page = value
        End Set
    End Property

    ''' <summary>
    ''' Table's page number starting with zero
    ''' </summary>
    Public Property TablePageNumber As Integer
        Get
            Return _TablePageNumber
        End Get
        Friend Set(value As Integer)
            _TablePageNumber = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the current row bottom position.
    ''' </summary>
    ''' <value>Row bottom position.</value>
    ''' <remarks>
    ''' Row bottom position is calculated for each row. It is RowTopPosition plus RowHeight. 
    ''' The calculation is done within DrawRow method before any of the cells is drawn.
    ''' </remarks>
    Public Property RowBottomPosition As Double
        Get
            Return _RowBottomPosition
        End Get
        Friend Set(value As Double)
            _RowBottomPosition = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the current row height.
    ''' </summary>
    ''' <value>Current row height.</value>
    ''' <remarks>
    ''' Row height is calculated for each row. It is the height of the tallest cell. 
    ''' The calculation is done within DrawRow method before any of the cells is drawn.
    ''' </remarks>
    Public Property RowHeight As Double
        Get
            Return _RowHeight
        End Get
        Friend Set(value As Double)
            _RowHeight = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the current row number.
    ''' </summary>
    ''' <value>Row number starting with zero.</value>
    Public Property RowNumber As Integer
        Get
            Return _RowNumber
        End Get
        Friend Set(value As Integer)
            _RowNumber = value
        End Set
    End Property

    ''' <summary>
    ''' Gets array of row positions.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Row position is the centre of a border line or the centre of a grid line. 
    ''' </para>
    ''' <para>
    ''' Position[0] is the top side of the table, and Position[Rows] is the bottom side of the table.
    ''' </para>
    ''' <para>
    ''' The returned array is valid after the last row on the page is processed.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property RowPosition As Double()
        Get
            Return CType(BorderYPos.ToArray(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets current row top position.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Row top position can only be set during initialization.
    ''' It should be set by the caller if on the first page the 
    ''' table does not start at the top of the page.
    ''' </para>
    ''' <para>
    ''' Row top position is calculated for each row. It is the last RowBottomPosition plus GridLineWidth. 
    ''' The calculation is done within DrawRow method before any of the cells is drawn.
    ''' </para>
    ''' </remarks>
    Public Property RowTopPosition As Double
        Get
            Return _RowTopPosition
        End Get
        Set(value As Double)
            If Active Then Throw New ApplicationException("PdfTable: Row position must be defined at initialization.")
            _RowTopPosition = value
        End Set
    End Property

    Friend _RowTopPosition As Double

    ''' <summary>
    ''' Gets or sets table area rectangle.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Table area rectangle can only be set at initialization time. 
    ''' </para>
    ''' <para>
    ''' The default table area is the default paper size less 1 inch margin. 
    ''' </para>
    ''' <para>
    ''' Returned value is a copy of the internal rectangle.
    ''' </para>
    ''' </remarks>
    Public Property TableArea As PdfRectangle
        Get
            Return New PdfRectangle(_TableArea)
        End Get
        Set(value As PdfRectangle)
            If Active Then Throw New ApplicationException("PdfTable: Table area must be defined at initialization.")
            _TableArea = value
        End Set
    End Property

    Friend _TableArea As PdfRectangle

    Public Property Borders As PdfTableBorder
        Get
            Return _Borders
        End Get
        Friend Set(value As PdfTableBorder)
            _Borders = value
        End Set
    End Property

    ''' <summary>
    ''' Sets the custom draw cell and header event handler.
    ''' </summary>
    ''' <remarks>
    ''' The custom draw cell and header event handler is called each time a 
    ''' cell or header is about to be drawn and RaiseCustomDrawCellEvent 
    ''' property of cell style is set to true;
    ''' </remarks>
    Public CustomDrawCellEvent As PdfTableCustomDrawCell

    ''' <summary>
    ''' Sets the table end event handler.
    ''' </summary>
    ''' <remarks>
    ''' The table end event handler is called at the end of each page and at the end of the table drawing.
    ''' </remarks>
    Public Event TableEndEvent As PdfTableEnd

    ''' <summary>
    ''' Sets the table start event handler.
    ''' </summary>
    ''' <remarks>
    ''' The table start event handler is called at the start of the table and at the top each subsequent page.
    ''' <code>
    ''' // example of table start event handler to display
    ''' // heading above the table on each page
    ''' void BookListTableStart
    ''' 		(
    ''' 		PdfTable	BookList,
    ''' 		double		TableStartPos
    ''' 		)
    ''' 	{
    ''' 	double PosX = 0.5 * (BookList.TableArea.Left + BookList.TableArea.Right);
    ''' 	double PosY = TableStartPos + TableTitleFont.Descent(16.0) + 0.05;
    ''' 	BookList.Contents.DrawText(TableTitleFont, 16.0, PosX, PosY,
    ''' 		TextJustify.Center, DrawStyle.Normal, Color.Chocolate, "Book List PdfTable Example"); 
    ''' 	return;
    ''' 	}
    ''' 	</code>
    ''' </remarks>
    Public Event TableStartEvent As PdfTableStart

    ' internal members
    Friend BorderLeftPos As Double
    Friend BorderRightPos As Double
    Friend BorderYPos As List(Of Double)
    Friend BorderHeaderActive As Boolean
    Friend BorderRowTopPos As Double
    Friend TableTopLimit As Double
    Friend TableBottomLimit As Double
    Friend TextBoxRowHeight As Double
    Friend TextBoxContinue As Boolean
    Friend DisplayHeader As Boolean
    Friend DrawingActive As Boolean
    Friend Epsilon As Double

    ''' <summary>
    ''' PdfTable constructor.
    ''' </summary>
    ''' <param name="Page">Current PdfPage.</param>
    ''' <param name="Contents">Current PdfContents.</param>
    ''' <param name="Font">Table's default font.</param>
    ''' <param name="FontSize">Table's default font size.</param>
    Public Sub New(Page As PdfPage, Contents As PdfContents, Optional Font As PdfFont = Nothing, Optional FontSize As Double = 9.0)
        ' save arguments
        Document = Page.Document
        Me.Page = Page
        Me.Contents = Contents

        ' See if at least one font is defined. Make it the default font for the table
        If Font Is Nothing Then
            For Each Obj In Document.ObjectArray

                If Obj.GetType() Is GetType(PdfFont) Then
                    Font = CType(Obj, PdfFont)
                    Exit For
                End If
            Next
        End If

        ' initialize default cell style
        DefaultCellStyle = New PdfTableStyle()
        DefaultCellStyle.Font = Font
        DefaultCellStyle.FontSize = FontSize

        DefaultCellStyle.Margin.Right = 3.0 / Document.ScaleFactor
        DefaultCellStyle.Margin.Top = 1.0 / Document.ScaleFactor

        DefaultCellStyle.Margin.Left = DefaultCellStyle.Margin.Right
        DefaultCellStyle.Margin.Bottom = DefaultCellStyle.Margin.Top

        ' initialize default header style
        DefaultHeaderStyle = CellStyle
        DefaultHeaderStyle.BackgroundColor = Color.LightGray

        ' default table area
        TableArea = New PdfRectangle(72.0 / Page.ScaleFactor, 72.0 / Page.ScaleFactor, (Page.Width - 72.0) / Page.ScaleFactor, (Page.Height - 72.0) / Page.ScaleFactor)

        ' set header on each page as the default
        HeaderOnEachPage = True

        ' create table border control
        Borders = New PdfTableBorder(Me)

        ' very small amount 1/300 of an inch
        ' used to guard against rounding errors
        Epsilon = Document.Epsilon
    End Sub

    ''' <summary>
    ''' Sets the number of columns, and column's widths.
    ''' </summary>
    ''' <param name="ColumnWidth">Array of column widths.</param>
    ''' <remarks>
    ''' <para>
    ''' The length of the array sets the number of columns.
    ''' </para>
    ''' <para>
    ''' This method builds two PdfTableCell arrays. One array for data cells, and
    ''' the other array for header cells.
    ''' </para>
    ''' <para>
    ''' The actual column widths will be calculated to fit the width of the table. 
    ''' </para>
    ''' <para>
    ''' The calculation is done as follows. First the library calculates the amount
    ''' of net space available after border lines and grid lines width is taken off
    ''' the width of the table as specified in TableArea. Second, the net space is divided
    ''' in proportion to the requested widths.
    ''' </para>
    ''' </remarks>
    Public Sub SetColumnWidth(ParamArray ColumnWidth As Double())
        ' save column width
        If _ColumnWidth IsNot Nothing OrElse ColumnWidth Is Nothing OrElse ColumnWidth.Length = 0 Then Throw New ApplicationException("PdfTable: SetColumnWidth invalid argument or already defined.")
        _ColumnWidth = ColumnWidth

        ' save number of columns
        Columns = _ColumnWidth.Length

        ' create cell and header arrays
        _Cell = New PdfTableCell(Columns - 1) {}
        _Header = New PdfTableCell(Columns - 1) {}

        For Index = 0 To Columns - 1
            _Cell(Index) = New PdfTableCell(Me, Index, False)
            _Header(Index) = New PdfTableCell(Me, Index, True)
        Next

        ' vertical border control
        Borders.BordersInitialization()
    End Sub

    ''' <summary>
    ''' PdfTable initialization.
    ''' </summary>
    ''' <remarks>
    ''' Normally the system will call this method on first call to DrawRow(). 
    ''' If called by user it must be called after initialization and before 
    ''' the first row is drawn. This method sets the active flag.
    ''' </remarks>
    Public Sub PdfTableInitialization()
        ' initialize table is done
        If Active Then Return

        ' make sure we have columns width array
        If _ColumnWidth Is Nothing Then Throw New ApplicationException("PdfTable: SetColumnWidth array is missing.")

        ' net table width
        Dim NetWidth As Double = _TableArea.Width - Borders.HorizontalBordersTotalWidth()

        ' calculate column width adjustment factor
        Dim Total As Double = 0

        For Each Width In _ColumnWidth
            Total += Width
        Next

        Dim Factor = NetWidth / Total

        ' create column position for border and grid lines
        _ColumnPosition = New Double(Columns + 1 - 1) {}

        ' initial border/grid position
        Dim Position = _TableArea.Left

        ' left border line position
        _ColumnPosition(0) = Position

        ' first column position and width
        Position += Borders.VertBorderHalfWidth(0)
        Dim CellZero = _Cell(0)
        CellZero.FrameLeft = Position
        CellZero.FrameWidth = _ColumnWidth(0) * Factor

        ' first grid line position
        Position += CellZero.FrameWidth + Borders.VertBorderHalfWidth(1)

        ' column width and position
        For Index = 1 To Columns - 1
            ' shortcut
            Dim Cell = _Cell(Index)

            ' column net width
            Cell.FrameWidth = _ColumnWidth(Index) * Factor

            ' grid line position
            _ColumnPosition(Index) = Position

            ' cell left position
            Position += Borders.VertBorderHalfWidth(Index)
            Cell.FrameLeft = Position

            ' next grid line position
            Position += Cell.FrameWidth + Borders.VertBorderHalfWidth(Index + 1)
        Next

        ' last grid line position
        _ColumnPosition(Columns) = Position

        ' columns width after adjustments
        For Index = 0 To Columns - 1
            _ColumnWidth(Index) = _ColumnPosition(Index + 1) - _ColumnPosition(Index)
        Next

        ' copy horizontal info from cell to header
        For Index = 0 To Columns - 1
            ' shortcut
            Dim Cell = _Cell(Index)
            Dim Header = _Header(Index)
            Header.FrameLeft = Cell.FrameLeft
            Header.FrameWidth = Cell.FrameWidth
        Next

        ' user did not define initial row position
        If _RowTopPosition = 0 Then
            _RowTopPosition = _TableArea.Top

            ' make sure initial row position is within table area
        ElseIf _RowTopPosition < _TableArea.Bottom OrElse _RowTopPosition > _TableArea.Top Then
            Throw New ApplicationException("PdfTable: Initial RowPosition outside table area.")
        End If

        ' border positions for border drawing
        BorderHeaderActive = False
        BorderYPos = New List(Of Double)()
        BorderYPos.Add(_RowTopPosition)
        BorderRowTopPos = _RowTopPosition
        BorderLeftPos = TableArea.Left - Borders.VertBorderHalfWidth(0)
        BorderRightPos = TableArea.Right + Borders.VertBorderHalfWidth(Columns)

        ' initial row position
        _RowTopPosition -= Borders.TopBorder.HalfWidth

        ' table top and bottom limit for compare
        TableTopLimit = TableArea.Top - Borders.TopBorder.HalfWidth
        TableBottomLimit = TableArea.Bottom + Borders.BottomBorder.HalfWidth

        ' initialization is done, PdfTable is ready to draw
        Active = True
    End Sub

    ''' <summary>
    ''' Draw one row.
    ''' </summary>
    ''' <param name="NewPage">Force new page.</param>
    ''' <remarks>
    ''' The DrawRow method must be called for each row in the table.
    ''' Before calling this method all PdfTableCell values for the 
    ''' current row must be set. If the NewPage argument is set to true,
    ''' the software will print the row at the top of a new page.
    ''' </remarks>
    Public Sub DrawRow(Optional NewPage As Boolean = False)
        ' we are about to draw the first row
        If Not DrawingActive Then
            ' one time PdfTable initialization
            PdfTableInitialization()

            ' calculate header height
            Dim Index As Integer
            Index = 0

            While Index < Columns AndAlso _Header(Index).Value Is Nothing
                Index += 1
            End While

            If Index < Columns Then
                DisplayHeader = True
                CalculateHeaderHeight()
            End If

            ' call user event handler for start of table on each page
            RaiseEvent TableStartEvent(Me, BorderRowTopPos)
        End If

        ' calculate row height
        CalculateRowHeight()

        ' test for new page
        If NewPage OrElse _RowTopPosition - (TextBoxRowHeight + If(HeaderHeight <> 0.0, HeaderHeight + 2.0 * Borders.HeaderHorBorder.HalfWidth, 0.0)) < TableBottomLimit AndAlso _RowTopPosition <> TableTopLimit Then
            ' create a new page
            CreateNewPage()
        End If

        ' draw header
        If HeaderHeight <> 0.0 Then DrawHeader()

        ' draw row
        DrawOneRow()

        ' set drawing active
        DrawingActive = True

        ' we have textboxes that did not fit on current page
        While TextBoxContinue
            ' create a new page
            CreateNewPage()

            ' draw header
            If HeaderHeight <> 0.0 Then DrawHeader()

            ' calculate row height
            CalculateRowHeight()

            ' draw row
            DrawOneRow()
        End While

        ' update row number
        RowNumber += 1
    End Sub

    Private Sub DrawOneRow()
        ' row bottom position
        RowBottomPosition = _RowTopPosition - RowHeight

        ' row bottom position is below table bottom
        If RowBottomPosition < TableBottomLimit - Epsilon Then
            ' test the smaller row height (TextBox minimum lines)
            If _RowTopPosition - TextBoxRowHeight < TableBottomLimit - Epsilon Then
                Throw New ApplicationException("Table row height is too big")
            End If

            ' adjust bottom position
            RowBottomPosition = TableBottomLimit
        End If

        ' reset texbox continue flag
        TextBoxContinue = False

        ' draw row of cells
        For Each Cell As PdfTableCell In _Cell
            ' calculate top and bottom client space
            Cell.ClientBottom = RowBottomPosition + Cell.Style.Margin.Bottom

            ' call custom draw cell if required and draw header cell
            Cell.ClientTop = _RowTopPosition - Cell.Style.Margin.Top

            ' call custom draw cell if required and draw header cell
            If (CustomDrawCellEvent Is Nothing OrElse Not Cell.Style.RaiseCustomDrawCellEvent OrElse Not CustomDrawCellEvent(Me, Cell)) Then
                Cell.DrawCell()
            End If
        Next

        ' adjust row position to next grid line
        _RowTopPosition = RowBottomPosition - Borders.CellHorBorder.HalfWidth

        ' save for next time
        BorderYPos.Add(_RowTopPosition)
        BorderRowTopPos = _RowTopPosition

        ' adjust row position to next row of cells
        _RowTopPosition -= Borders.CellHorBorder.HalfWidth

        ' reset cell value
        For Each Cell As PdfTableCell In _Cell
            Cell.Reset()
        Next
    End Sub

    Private Sub DrawHeader()
        ' row bottom position
        RowBottomPosition = _RowTopPosition - HeaderHeight
        If RowBottomPosition <= _TableArea.Bottom + 2.0 * Borders.HeaderHorBorder.HalfWidth + Borders.BottomBorder.HalfWidth Then
            Throw New ApplicationException("Table header height is too big")
        End If

        ' draw each column header
        For Each Cell As PdfTableCell In _Header
            ' calculate top and bottom client space
            Cell.ClientBottom = RowBottomPosition + Cell.Style.Margin.Bottom

            ' call custom draw cell if required and draw header cell
            Cell.ClientTop = _RowTopPosition - Cell.Style.Margin.Top


            ' call custom draw cell if required and draw header cell
            If (CustomDrawCellEvent Is Nothing OrElse Not Cell.Style.RaiseCustomDrawCellEvent OrElse Not CustomDrawCellEvent(Me, Cell)) Then
                Cell.DrawCell()
            End If

        Next

        ' adjust row position to next grid line
        _RowTopPosition = RowBottomPosition - Borders.HeaderHorBorder.HalfWidth

        ' save for next time
        BorderHeaderActive = True
        BorderYPos.Add(_RowTopPosition)
        BorderRowTopPos = _RowTopPosition

        ' adjust row position to next row of cells
        _RowTopPosition -= Borders.HeaderHorBorder.HalfWidth

        ' reset header height
        HeaderHeight = 0.0
    End Sub

    ''' <summary>
    ''' Close table.
    ''' </summary>
    ''' <remarks>
    ''' The Close method must be called after the last row was drawn.
    ''' </remarks>
    Public Sub Close()
        ' make sure at least one row was drawn
        If DrawingActive Then
            ' draw border and grid on current page
            DrawBorders()

            ' call user event handler for end of table
            RaiseEvent TableEndEvent(Me, _RowTopPosition)
        End If
    End Sub

    ''' <summary>
    ''' calculate row height
    ''' </summary>
    Private Sub CalculateRowHeight()
        ' initial row height
        RowHeight = MinRowHeight
        TextBoxRowHeight = MinRowHeight

        ' loop through all cells
        For Each Cell As PdfTableCell In _Cell
            ' calculate cell height
            Cell.DrawCellInitialization()

            ' adjust row height if required
            If Cell.CellHeight > RowHeight Then RowHeight = Cell.CellHeight
            If Cell.TextBoxCellHeight > TextBoxRowHeight Then TextBoxRowHeight = Cell.TextBoxCellHeight
        Next
    End Sub

    ''' <summary>
    ''' calculate header height
    ''' </summary>
    Private Sub CalculateHeaderHeight()
        ' initial row height
        HeaderHeight = MinHeaderHeight

        ' loop through all cells
        For Each Cell As PdfTableCell In _Header
            ' calculate cell height
            Cell.DrawCellInitialization()

            ' adjust row height if required
            If Cell.CellHeight > HeaderHeight Then HeaderHeight = Cell.CellHeight
        Next
    End Sub

    ''' <summary>
    ''' start a new page
    ''' </summary>
    Private Sub CreateNewPage()
        ' terminate activity on current page
        If DrawingActive Then
            ' draw border and grid on current page
            DrawBorders()

            ' call user event handler for end of table on each page
            RaiseEvent TableEndEvent(Me, _RowTopPosition)

            ' update page number
            TablePageNumber += 1
        End If

        ' commit to PDF file
        If CommitToPdfFile Then
            Contents.CommitToPdfFile(CommitGCCollectFreq > 0 AndAlso (CommitGCCollectFreq = 1 OrElse TablePageNumber Mod CommitGCCollectFreq = CommitGCCollectFreq - 1))
        End If

        ' create a new page as a clone of the previous page
        Page = New PdfPage(Page)
        Contents = New PdfContents(Page)

        ' reset border lines
        BorderRowTopPos = _TableArea.Top
        BorderYPos.Clear()
        BorderYPos.Add(BorderRowTopPos)

        ' initial row position on the page
        _RowTopPosition = TableTopLimit

        ' calculate header height
        If DrawingActive AndAlso DisplayHeader AndAlso HeaderOnEachPage Then CalculateHeaderHeight()

        ' call user event handler for start of table on each page
        RaiseEvent TableStartEvent(Me, BorderRowTopPos)
    End Sub

    ''' <summary>
    ''' Draw borders and grid lines after the last row on a page is drawn
    ''' or the last row of the table is drawn.
    ''' </summary>
    Private Sub DrawBorders()
        ' draw top border line
        Contents.DrawLine(BorderLeftPos, BorderYPos(0), BorderRightPos, BorderYPos(0), Borders.TopBorder)

        ' row
        Dim RowStart = 1
        Dim RowEnd = BorderYPos.Count - 1

        ' draw horizontal header border line
        If BorderHeaderActive Then
            Contents.DrawLine(BorderLeftPos, BorderYPos(1), BorderRightPos, BorderYPos(1), Borders.HeaderHorBorder)
            RowStart += 1
        End If

        ' draw horizontal cells border lines
        For Row = RowStart To RowEnd - 1
            Contents.DrawLine(BorderLeftPos, BorderYPos(Row), BorderRightPos, BorderYPos(Row), Borders.CellHorBorder)
        Next

        ' draw horizontal bottom border line
        Contents.DrawLine(BorderLeftPos, BorderRowTopPos, BorderRightPos, BorderRowTopPos, Borders.BottomBorder)

        ' draw each vertical border line for header style
        If BorderHeaderActive AndAlso Borders.HeaderVertBorderActive Then
            For Col = 0 To Columns
                Contents.DrawLine(ColumnPosition(Col), BorderYPos(0), ColumnPosition(Col), BorderYPos(1), Borders.HeaderVertBorder(Col))
            Next
        End If

        ' draw each vertical line between cells
        If Borders.CellVertBorderActive Then
            Dim Top = If(BorderHeaderActive, BorderYPos(1), BorderYPos(0))

            For Col = 0 To Columns
                Contents.DrawLine(ColumnPosition(Col), Top, ColumnPosition(Col), BorderRowTopPos, Borders.CellVertBorder(Col))
            Next
        End If
    End Sub
End Class
