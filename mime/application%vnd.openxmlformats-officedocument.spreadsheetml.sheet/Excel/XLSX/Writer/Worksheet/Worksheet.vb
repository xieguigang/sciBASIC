#Region "Microsoft.VisualBasic::7a12829e9531bbcb26dc86f908163893, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Worksheet\Worksheet.vb"

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

    '   Total Lines: 1982
    '    Code Lines: 1064 (53.68%)
    ' Comment Lines: 744 (37.54%)
    '    - Xml Docs: 98.52%
    ' 
    '   Blank Lines: 174 (8.78%)
    '     File Size: 96.56 KB


    '     Class Worksheet
    ' 
    '         Properties: ActivePane, ActiveStyle, AutoFilterRange, Cells, Columns
    '                     CurrentCellDirection, DefaultColumnWidth, DefaultRowHeight, FreezeSplitPanes, Hidden
    '                     HiddenRows, MergedCells, PaneSplitAddress, PaneSplitLeftWidth, PaneSplitTopHeight
    '                     PaneSplitTopLeftCell, RowHeights, SelectedCellRanges, SelectedCells, SheetID
    '                     SheetName, SheetProtectionPassword, SheetProtectionPasswordHash, SheetProtectionValues, UseSheetProtection
    '                     WorkbookReference
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: CastValue, Copy, GetBoundaryDataNumber, GetBoundaryNumber, (+2 Overloads) GetCell
    '                   GetCurrentColumnNumber, GetCurrentRowNumber, GetFirstCellAddress, GetFirstColumnNumber, GetFirstDataCellAddress
    '                   GetFirstDataColumnNumber, GetFirstDataRowNumber, GetFirstRowNumber, GetLastCellAddress, GetLastColumnNumber
    '                   GetLastDataCellAddress, GetLastDataColumnNumber, GetLastDataRowNumber, GetLastRowNumber, GetMaxRow
    '                   GetMinRow, GetUnusedWorksheetName, (+2 Overloads) HasCell, (+3 Overloads) MergeCells, (+2 Overloads) RemoveCell
    '                   SanitizeWorksheetName, WorksheetExists
    ' 
    '         Sub: AddAllowedActionOnSheetProtection, (+4 Overloads) AddCell, (+4 Overloads) AddCellFormula, (+4 Overloads) AddCellRange, AddCellRangeInternal
    '              (+2 Overloads) AddHiddenColumn, AddHiddenRow, (+3 Overloads) AddNextCell, (+2 Overloads) AddNextCellFormula, (+3 Overloads) AddSelectedCells
    '              ClearActiveStyle, (+2 Overloads) GoToNextColumn, (+2 Overloads) GoToNextRow, RecalculateAutoFilter, RecalculateColumns
    '              RemoveAllowedActionOnSheetProtection, RemoveAutoFilter, (+2 Overloads) RemoveHiddenColumn, RemoveHiddenRow, RemoveMergedCells
    '              RemoveRowHeight, RemoveSelectedCells, ResetColumn, ResetSplit, ResolveMergedCells
    '              SetActiveStyle, (+2 Overloads) SetAutoFilter, SetColumnHiddenState, (+2 Overloads) SetColumnWidth, (+2 Overloads) SetCurrentCellAddress
    '              SetCurrentColumnNumber, SetCurrentRowNumber, (+2 Overloads) SetHorizontalSplit, SetRowHeight, SetRowHiddenState
    '              (+3 Overloads) SetSelectedCells, (+2 Overloads) SetSheetName, SetSheetProtectionPassword, (+2 Overloads) SetSplit, (+4 Overloads) SetStyle
    '              (+2 Overloads) SetVerticalSplit
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  PicoXLSX is a small .NET library to generate XLSX (Microsoft Excel 2007 or newer) files in an easy and native way
'  Copyright Raphael Stoeckli © 2023
'  This library is licensed under the MIT License.
'  You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
' 

Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.FileIO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer.Cell
Imports stdNum = System.Math

Namespace XLSX.Writer

    ''' <summary>
    ''' Class representing a worksheet of a workbook
    ''' </summary>
    Public Class Worksheet
        ''' <summary>
        ''' Threshold, using when floats are compared
        ''' </summary>
        Private Const FLOAT_THRESHOLD As Single = 0.0001F

        ''' <summary>
        ''' Maximum number of characters a worksheet name can have
        ''' </summary>
        Public Shared ReadOnly MAX_WORKSHEET_NAME_LENGTH As Integer = 31

        ''' <summary>
        ''' Default column width as constant
        ''' </summary>
        Public Const DEFAULT_COLUMN_WIDTH As Single = 10.0F

        ''' <summary>
        ''' Default row height as constant
        ''' </summary>
        Public Const DEFAULT_ROW_HEIGHT As Single = 15.0F

        ''' <summary>
        ''' Maximum column number (zero-based) as constant
        ''' </summary>
        Public Const MAX_COLUMN_NUMBER As Integer = 16383

        ''' <summary>
        ''' Minimum column number (zero-based) as constant
        ''' </summary>
        Public Const MIN_COLUMN_NUMBER As Integer = 0

        ''' <summary>
        ''' Minimum column width as constant
        ''' </summary>
        Public Const MIN_COLUMN_WIDTH As Single = 0F

        ''' <summary>
        ''' Minimum row height as constant
        ''' </summary>
        Public Const MIN_ROW_HEIGHT As Single = 0F

        ''' <summary>
        ''' Maximum column width as constant
        ''' </summary>
        Public Const MAX_COLUMN_WIDTH As Single = 255.0F

        ''' <summary>
        ''' Maximum row number (zero-based) as constant
        ''' </summary>
        Public Const MAX_ROW_NUMBER As Integer = 1048575

        ''' <summary>
        ''' Minimum row number (zero-based) as constant
        ''' </summary>
        Public Const MIN_ROW_NUMBER As Integer = 0

        ''' <summary>
        ''' Maximum row height as constant
        ''' </summary>
        Public Const MAX_ROW_HEIGHT As Single = 409.5F

        ''' <summary>
        ''' Defines the activeStyle
        ''' </summary>
        Private activeStyleField As Style

        ''' <summary>
        ''' Defines the autoFilterRange
        ''' </summary>
        Private autoFilterRangeField As Range?

        ''' <summary>
        ''' Defines the cells
        ''' </summary>
        Private ReadOnly cellsField As Dictionary(Of String, Cell)

        ''' <summary>
        ''' Defines the columns
        ''' </summary>
        Private ReadOnly columnsField As Dictionary(Of Integer, Column)

        ''' <summary>
        ''' Defines the sheetName
        ''' </summary>
        Private sheetNameField As String

        ''' <summary>
        ''' Defines the currentRowNumber
        ''' </summary>
        Private currentRowNumber As Integer

        ''' <summary>
        ''' Defines the currentColumnNumber
        ''' </summary>
        Private currentColumnNumber As Integer

        ''' <summary>
        ''' Defines the defaultRowHeight
        ''' </summary>
        Private defaultRowHeightField As Single

        ''' <summary>
        ''' Defines the defaultColumnWidth
        ''' </summary>
        Private defaultColumnWidthField As Single

        ''' <summary>
        ''' Defines the rowHeights
        ''' </summary>
        Private ReadOnly rowHeightsField As Dictionary(Of Integer, Single)

        ''' <summary>
        ''' Defines the hiddenRows
        ''' </summary>
        Private ReadOnly hiddenRowsField As Dictionary(Of Integer, Boolean)

        ''' <summary>
        ''' Defines the mergedCells
        ''' </summary>
        Private ReadOnly mergedCellsField As Dictionary(Of String, Range)

        ''' <summary>
        ''' Defines the sheetProtectionValues
        ''' </summary>
        Private ReadOnly sheetProtectionValuesField As List(Of SheetProtectionValue)

        ''' <summary>
        ''' Defines the useActiveStyle
        ''' </summary>
        Private useActiveStyle As Boolean

        ''' <summary>
        ''' Defines the hidden
        ''' </summary>
        Private hiddenField As Boolean

        ''' <summary>
        ''' Defines the workbookReference
        ''' </summary>
        Private workbookReferenceField As Workbook

        ''' <summary>
        ''' Defines the sheetProtectionPassword
        ''' </summary>
        Private sheetProtectionPasswordField As String = Nothing

        ''' <summary>
        ''' Defines the sheetProtectionPasswordHash
        ''' </summary>
        Private sheetProtectionPasswordHashField As String = Nothing

        ''' <summary>
        ''' Defines the selectedCells
        ''' </summary>
        Private selectedCellsField As List(Of Range)

        ''' <summary>
        ''' Defines the freezeSplitPanes
        ''' </summary>
        Private freezeSplitPanesField As Boolean?

        ''' <summary>
        ''' Defines the paneSplitLeftWidth
        ''' </summary>
        Private paneSplitLeftWidthField As Single?

        ''' <summary>
        ''' Defines the paneSplitTopHeight
        ''' </summary>
        Private paneSplitTopHeightField As Single?

        ''' <summary>
        ''' Defines the paneSplitTopLeftCell
        ''' </summary>
        Private paneSplitTopLeftCellField As Address?

        ''' <summary>
        ''' Defines the paneSplitAddress
        ''' </summary>
        Private paneSplitAddressField As Address?

        ''' <summary>
        ''' Defines the activePane
        ''' </summary>
        Private activePaneField As WorksheetPane?

        ''' <summary>
        ''' Defines the sheetID
        ''' </summary>
        Private sheetIDField As Integer

        ''' <summary>
        ''' Gets the range of the auto-filter. Wrapped to Nullable to provide null as value. If null, no auto-filter is applied
        ''' </summary>
        Public ReadOnly Property AutoFilterRange As Range?
            Get
                Return autoFilterRangeField
            End Get
        End Property

        ''' <summary>
        ''' Gets the cells of the worksheet as dictionary with the cell address as key and the cell object as value
        ''' </summary>
        Public ReadOnly Property Cells As Dictionary(Of String, Cell)
            Get
                Return cellsField
            End Get
        End Property

        ''' <summary>
        ''' Gets all columns with non-standard properties, like auto filter applied or a special width as dictionary with the zero-based column index as key and the column object as value
        ''' </summary>
        Public ReadOnly Property Columns As Dictionary(Of Integer, Column)
            Get
                Return columnsField
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the direction when using AddNextCell method
        ''' </summary>
        Public Property CurrentCellDirection As CellDirection

        ''' <summary>
        ''' Gets or sets the default column width
        ''' </summary>
        Public Property DefaultColumnWidth As Single
            Get
                Return defaultColumnWidthField
            End Get
            Set(value As Single)
                If value < MIN_COLUMN_WIDTH OrElse value > MAX_COLUMN_WIDTH Then
                    Throw New RangeException("OutOfRangeException", "The passed default column width is out of range (" & MIN_COLUMN_WIDTH.ToString() & " to " & MAX_COLUMN_WIDTH.ToString() & ")")
                End If
                defaultColumnWidthField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the default Row height
        ''' </summary>
        Public Property DefaultRowHeight As Single
            Get
                Return defaultRowHeightField
            End Get
            Set(value As Single)
                If value < MIN_ROW_HEIGHT OrElse value > MAX_ROW_HEIGHT Then
                    Throw New RangeException("OutOfRangeException", "The passed default row height is out of range (" & MIN_ROW_HEIGHT.ToString() & " to " & MAX_ROW_HEIGHT.ToString() & ")")
                End If
                defaultRowHeightField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the hidden rows as dictionary with the zero-based row number as key and a boolean as value. True indicates hidden, false visible.
        ''' </summary>
        Public ReadOnly Property HiddenRows As Dictionary(Of Integer, Boolean)
            Get
                Return hiddenRowsField
            End Get
        End Property

        ''' <summary>
        ''' Gets the merged cells (only references) as dictionary with the cell address as key and the range object as value
        ''' </summary>
        Public ReadOnly Property MergedCells As Dictionary(Of String, Range)
            Get
                Return mergedCellsField
            End Get
        End Property

        ''' <summary>
        ''' Gets defined row heights as dictionary with the zero-based row number as key and the height (float from 0 to 409.5) as value
        ''' </summary>
        Public ReadOnly Property RowHeights As Dictionary(Of Integer, Single)
            Get
                Return rowHeightsField
            End Get
        End Property

        ''' <summary>
        ''' Returns either null (if no cells are selected), or the first defined range of selected cells
        ''' </summary>
        ''' <remarks>Use <see cref="SelectedCellRanges"/> to get all defined ranges</remarks>
        <Obsolete("This method is a deprecated subset of the function SelectedCellRanges. SelectedCellRanges will get this function name in a future version. Therefore, the type will change")>
        Public ReadOnly Property SelectedCells As Range?
            Get
                If selectedCellsField.Count = 0 Then
                    Return Nothing
                Else
                    Return selectedCellsField(0)
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets all ranges of selected cells of this worksheet. An empty list is returned if no cells are selected
        ''' </summary>
        Public ReadOnly Property SelectedCellRanges As List(Of Range)
            Get
                Return selectedCellsField
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the internal ID of the worksheet
        ''' </summary>
        Public Property SheetID As Integer
            Get
                Return sheetIDField
            End Get
            Set(value As Integer)
                If value < 1 Then
                    Throw New FormatException("The ID " & value.ToString() & " is invalid. Worksheet IDs must be >0")
                End If
                sheetIDField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the name of the worksheet
        ''' </summary>
        Public Property SheetName As String
            Get
                Return sheetNameField
            End Get
            Set(value As String)
                SetSheetName(value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the password used for sheet protection. See <see cref="SetSheetProtectionPassword"/> to set the password
        ''' </summary>
        Public ReadOnly Property SheetProtectionPassword As String
            Get
                Return sheetProtectionPasswordField
            End Get
        End Property

        ''' <summary>
        ''' gets the encrypted hash of the password, defined with <see cref="SheetProtectionPassword"/>. The value will be null, if no password is defined
        ''' </summary>
        Public ReadOnly Property SheetProtectionPasswordHash As String
            Get
                Return sheetProtectionPasswordHashField
            End Get
        End Property

        ''' <summary>
        ''' Gets the list of SheetProtectionValues. These values define the allowed actions if the worksheet is protected
        ''' </summary>
        Public ReadOnly Property SheetProtectionValues As List(Of SheetProtectionValue)
            Get
                Return sheetProtectionValuesField
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets whether the worksheet is protected. If true, protection is enabled
        ''' </summary>
        Public Property UseSheetProtection As Boolean

        ''' <summary>
        ''' Gets or sets the Reference to the parent Workbook
        ''' </summary>
        Public Property WorkbookReference As Workbook
            Get
                Return workbookReferenceField
            End Get
            Set(value As Workbook)
                workbookReferenceField = value
                If value IsNot Nothing Then
                    workbookReferenceField.ValidateWorksheets()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets whether the worksheet is hidden. If true, the worksheet is not listed as tab in the workbook's worksheet selection<br/>
        ''' If the worksheet is not part of a workbook, or the only one in the workbook, an exception will be thrown.<br/>
        ''' If the worksheet is the selected one, and attempted to set hidden, an exception will be thrown. Define another selected worksheet prior to this call, in this case.
        ''' </summary>
        Public Property Hidden As Boolean
            Get
                Return hiddenField
            End Get
            Set(value As Boolean)
                hiddenField = value
                If value AndAlso workbookReferenceField IsNot Nothing Then
                    workbookReferenceField.ValidateWorksheets()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets the height of the upper, horizontal split pane, measured from the top of the window.<br/>
        ''' The value is nullable. If null, no horizontal split of the worksheet is applied.<br/>
        ''' The value is only applicable to split the worksheet into panes, but not to freeze them.<br/>
        ''' See also: <see cref="PaneSplitAddress"/>
        ''' </summary>
        Public ReadOnly Property PaneSplitTopHeight As Single?
            Get
                Return paneSplitTopHeightField
            End Get
        End Property

        ''' <summary>
        ''' Gets the width of the left, vertical split pane, measured from the left of the window.<br/>
        ''' The value is nullable. If null, no vertical split of the worksheet is applied<br/>
        ''' The value is only applicable to split the worksheet into panes, but not to freeze them.<br/>
        ''' See also: <see cref="PaneSplitAddress"/>
        ''' </summary>
        Public ReadOnly Property PaneSplitLeftWidth As Single?
            Get
                Return paneSplitLeftWidthField
            End Get
        End Property

        ''' <summary>
        ''' Gets the FreezeSplitPanes
        ''' Gets whether split panes are frozen.<br/>
        ''' The value is nullable. If null, no freezing is applied. This property also does not apply if <see cref="PaneSplitAddress"/> is null
        ''' </summary>
        Public ReadOnly Property FreezeSplitPanes As Boolean?
            Get
                Return freezeSplitPanesField
            End Get
        End Property

        ''' <summary>
        ''' Gets the Top Left cell address of the bottom right pane if applicable and splitting is applied.<br/>
        ''' The column is only relevant for vertical split, whereas the row component is only relevant for a horizontal split.<br/>
        ''' The value is nullable. If null, no splitting was defined.
        ''' </summary>
        Public ReadOnly Property PaneSplitTopLeftCell As Address?
            Get
                Return paneSplitTopLeftCellField
            End Get
        End Property

        ''' <summary>
        ''' Gets the split address for frozen panes or if pane split was defined in number of columns and / or rows.<br/> 
        ''' For vertical splits, only the column component is considered. For horizontal splits, only the row component is considered.<br/>
        ''' The value is nullable. If null, no frozen panes or split by columns / rows are applied to the worksheet. 
        ''' However, splitting can still be applied, if the value is defined in characters.<br/>
        ''' See also: <see cref="PaneSplitLeftWidth"/> and <see cref="PaneSplitTopHeight"/> for splitting in characters (without freezing)
        ''' </summary>
        Public ReadOnly Property PaneSplitAddress As Address?
            Get
                Return paneSplitAddressField
            End Get
        End Property

        ''' <summary>
        ''' Gets the active Pane is splitting is applied.<br/>
        ''' The value is nullable. If null, no splitting was defined
        ''' </summary>
        Public ReadOnly Property ActivePane As WorksheetPane?
            Get
                Return activePaneField
            End Get
        End Property

        ''' <summary>
        ''' Gets the active Style of the worksheet. If null, no style is defined as active
        ''' </summary>
        Public ReadOnly Property ActiveStyle As Style
            Get
                Return activeStyleField
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Worksheet"/> class
        ''' </summary>
        Public Sub New()
            CurrentCellDirection = CellDirection.ColumnToColumn
            cellsField = New Dictionary(Of String, Cell)()
            currentRowNumber = 0
            currentColumnNumber = 0
            defaultColumnWidthField = DEFAULT_COLUMN_WIDTH
            defaultRowHeightField = DEFAULT_ROW_HEIGHT
            rowHeightsField = New Dictionary(Of Integer, Single)()
            mergedCellsField = New Dictionary(Of String, Range)()
            sheetProtectionValuesField = New List(Of SheetProtectionValue)()
            hiddenRowsField = New Dictionary(Of Integer, Boolean)()
            columnsField = New Dictionary(Of Integer, Column)()
            selectedCellsField = New List(Of Range)()
            activeStyleField = Nothing
            workbookReferenceField = Nothing
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Worksheet"/> class
        ''' </summary>
        ''' <param name="name">The name<see cref="String"/>.</param>
        Public Sub New(name As String)
            Me.New()
            SetSheetName(name)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Worksheet"/> class
        ''' </summary>
        ''' <param name="name">Name of the worksheet.</param>
        ''' <param name="id">ID of the worksheet (for internal use).</param>
        ''' <param name="reference">Reference to the parent Workbook.</param>
        Public Sub New(name As String, id As Integer, reference As Workbook)
            Me.New()
            SetSheetName(name)
            SheetID = id
            workbookReferenceField = reference
        End Sub

        ''' <summary>
        ''' Adds an object to the next cell position. If the type of the value does not match with one of the supported data types, it will be casted to a String. A prepared object of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="value">Unspecified value to insert.</param>
        Public Sub AddNextCell(value As Object)
            AddNextCell(CastValue(value, currentColumnNumber, currentRowNumber), True, Nothing)
        End Sub

        ''' <summary>
        ''' Adds an object to the next cell position. If the type of the value does not match with one of the supported data types, it will be casted to a String. A prepared object of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="value">Unspecified value to insert.</param>
        ''' <param name="style">Style object to apply on this cell.</param>
        Public Sub AddNextCell(value As Object, style As Style)
            AddNextCell(CastValue(value, currentColumnNumber, currentRowNumber), True, style)
        End Sub

        ''' <summary>
        ''' Method to insert a generic cell to the next cell position
        ''' </summary>
        ''' <param name="cell">Cell object to insert.</param>
        ''' <param name="incremental">If true, the address value (row or column) will be incremented, otherwise not.</param>
        ''' <param name="style">If not null, the defined style will be applied to the cell, otherwise no style or the default style will be applied.</param>
        Private Sub AddNextCell(cell As Cell, incremental As Boolean, style As Style)
            ' date and time styles are already defined by the passed cell object
            If style IsNot Nothing OrElse activeStyleField IsNot Nothing AndAlso useActiveStyle Then

                If cell.CellStyle Is Nothing AndAlso useActiveStyle Then
                    cell.SetStyle(activeStyleField)
                ElseIf cell.CellStyle Is Nothing AndAlso style IsNot Nothing Then
                    cell.SetStyle(style)
                ElseIf cell.CellStyle IsNot Nothing AndAlso useActiveStyle Then
                    Dim mixedStyle As Style = CType(cell.CellStyle.Copy(), Style)
                    mixedStyle.Append(activeStyleField)
                    cell.SetStyle(mixedStyle)
                ElseIf cell.CellStyle IsNot Nothing AndAlso style IsNot Nothing Then
                    Dim mixedStyle As Style = CType(cell.CellStyle.Copy(), Style)
                    mixedStyle.Append(style)
                    cell.SetStyle(mixedStyle)
                End If
            End If
            Dim address = cell.CellAddress
            If cellsField.ContainsKey(address) Then
                cellsField(address) = cell
            Else
                cellsField.Add(address, cell)
            End If
            If incremental Then
                If CurrentCellDirection = CellDirection.ColumnToColumn Then
                    currentColumnNumber += 1
                ElseIf CurrentCellDirection = CellDirection.RowToRow Then
                    currentRowNumber += 1
                    ' else = disabled
                End If
            Else
                If CurrentCellDirection = CellDirection.ColumnToColumn Then
                    currentColumnNumber = cell.ColumnNumber + 1
                    currentRowNumber = cell.RowNumber
                ElseIf CurrentCellDirection = CellDirection.RowToRow Then
                    currentColumnNumber = cell.ColumnNumber
                    currentRowNumber = cell.RowNumber + 1
                End If
                ' else = Disabled
            End If
        End Sub

        ''' <summary>
        ''' Method to cast a value or align an object of the type Cell to the context of the worksheet
        ''' </summary>
        ''' <param name="value">Unspecified value or object of the type Cell.</param>
        ''' <param name="column">Column index.</param>
        ''' <param name="row">Row index.</param>
        ''' <returns>Cell object.</returns>
        Private Function CastValue(value As Object, column As Integer, row As Integer) As Cell
            Dim c As Cell
            If value IsNot Nothing AndAlso value.GetType() Is GetType(Cell) Then
                c = CType(value, Cell)
                c.CellAddress2 = New Address(column, row)
            Else
                c = New Cell(value, CellType.DEFAULT, column, row)
            End If
            Return c
        End Function

        ''' <summary>
        ''' Adds an object to the defined cell address. If the type of the value does not match with one of the supported data types, it will be casted to a String. A prepared object of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="value">Unspecified value to insert.</param>
        ''' <param name="columnNumber">Column number (zero based).</param>
        ''' <param name="rowNumber">Row number (zero based).</param>
        Public Sub AddCell(value As Object, columnNumber As Integer, rowNumber As Integer)
            AddNextCell(CastValue(value, columnNumber, rowNumber), False, Nothing)
        End Sub

        ''' <summary>
        ''' Adds an object to the defined cell address. If the type of the value does not match with one of the supported data types, it will be casted to a String. A prepared object of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="value">Unspecified value to insert.</param>
        ''' <param name="columnNumber">Column number (zero based).</param>
        ''' <param name="rowNumber">Row number (zero based).</param>
        ''' <param name="style">Style to apply on the cell.</param>
        Public Sub AddCell(value As Object, columnNumber As Integer, rowNumber As Integer, style As Style)
            AddNextCell(CastValue(value, columnNumber, rowNumber), False, style)
        End Sub

        ''' <summary>
        ''' Adds an object to the defined cell address. If the type of the value does not match with one of the supported data types, it will be casted to a String. A prepared object of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="value">Unspecified value to insert.</param>
        ''' <param name="address">Cell address in the format A1 - XFD1048576.</param>
        Public Sub AddCell(value As Object, address As String)
            Dim column, row As Integer
            ResolveCellCoordinate(address, column, row)
            AddCell(value, column, row)
        End Sub

        ''' <summary>
        ''' Adds an object to the defined cell address. If the type of the value does not match with one of the supported data types, it will be casted to a String. A prepared object of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="value">Unspecified value to insert.</param>
        ''' <param name="address">Cell address in the format A1 - XFD1048576.</param>
        ''' <param name="style">Style to apply on the cell.</param>
        Public Sub AddCell(value As Object, address As String, style As Style)
            Dim column, row As Integer
            ResolveCellCoordinate(address, column, row)
            AddCell(value, column, row, style)
        End Sub

        ''' <summary>
        ''' Adds a cell formula as string to the defined cell address
        ''' </summary>
        ''' <param name="formula">Formula to insert.</param>
        ''' <param name="address">Cell address in the format A1 - XFD1048576.</param>
        Public Sub AddCellFormula(formula As String, address As String)
            Dim column, row As Integer
            ResolveCellCoordinate(address, column, row)
            Dim c As Cell = New Cell(formula, CellType.FORMULA, column, row)
            AddNextCell(c, False, Nothing)
        End Sub

        ''' <summary>
        ''' Adds a cell formula as string to the defined cell address
        ''' </summary>
        ''' <param name="formula">Formula to insert.</param>
        ''' <param name="address">Cell address in the format A1 - XFD1048576.</param>
        ''' <param name="style">Style to apply on the cell.</param>
        Public Sub AddCellFormula(formula As String, address As String, style As Style)
            Dim column, row As Integer
            ResolveCellCoordinate(address, column, row)
            Dim c As Cell = New Cell(formula, CellType.FORMULA, column, row)
            AddNextCell(c, False, style)
        End Sub

        ''' <summary>
        ''' Adds a cell formula as string to the defined cell address
        ''' </summary>
        ''' <param name="formula">Formula to insert.</param>
        ''' <param name="columnNumber">Column number (zero based).</param>
        ''' <param name="rowNumber">Row number (zero based).</param>
        Public Sub AddCellFormula(formula As String, columnNumber As Integer, rowNumber As Integer)
            Dim c As Cell = New Cell(formula, CellType.FORMULA, columnNumber, rowNumber)
            AddNextCell(c, False, Nothing)
        End Sub

        ''' <summary>
        ''' Adds a cell formula as string to the defined cell address
        ''' </summary>
        ''' <param name="formula">Formula to insert.</param>
        ''' <param name="columnNumber">Column number (zero based).</param>
        ''' <param name="rowNumber">Row number (zero based).</param>
        ''' <param name="style">Style to apply on the cell.</param>
        Public Sub AddCellFormula(formula As String, columnNumber As Integer, rowNumber As Integer, style As Style)
            Dim c As Cell = New Cell(formula, CellType.FORMULA, columnNumber, rowNumber)
            AddNextCell(c, False, style)
        End Sub

        ''' <summary>
        ''' Adds a formula as string to the next cell position
        ''' </summary>
        ''' <param name="formula">Formula to insert.</param>
        Public Sub AddNextCellFormula(formula As String)
            Dim c As Cell = New Cell(formula, CellType.FORMULA, currentColumnNumber, currentRowNumber)
            AddNextCell(c, True, Nothing)
        End Sub

        ''' <summary>
        ''' Adds a formula as string to the next cell position
        ''' </summary>
        ''' <param name="formula">Formula to insert.</param>
        ''' <param name="style">Style to apply on the cell.</param>
        Public Sub AddNextCellFormula(formula As String, style As Style)
            Dim c As Cell = New Cell(formula, CellType.FORMULA, currentColumnNumber, currentRowNumber)
            AddNextCell(c, True, style)
        End Sub

        ''' <summary>
        ''' Adds a list of object values to a defined cell range. If the type of the a particular value does not match with one of the supported data types, it will be casted to a String. Prepared objects of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="values">List of unspecified objects to insert.</param>
        ''' <param name="startAddress">Start address.</param>
        ''' <param name="endAddress">End address.</param>
        Public Sub AddCellRange(values As IReadOnlyList(Of Object), startAddress As Address, endAddress As Address)
            AddCellRangeInternal(values, startAddress, endAddress, Nothing)
        End Sub

        ''' <summary>
        ''' Adds a list of object values to a defined cell range. If the type of the a particular value does not match with one of the supported data types, it will be casted to a String. Prepared objects of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="values">List of unspecified objects to insert.</param>
        ''' <param name="startAddress">Start address.</param>
        ''' <param name="endAddress">End address.</param>
        ''' <param name="style">Style to apply on the all cells of the range.</param>
        Public Sub AddCellRange(values As IReadOnlyList(Of Object), startAddress As Address, endAddress As Address, style As Style)
            AddCellRangeInternal(values, startAddress, endAddress, style)
        End Sub

        ''' <summary>
        ''' Adds a list of object values to a defined cell range. If the type of the a particular value does not match with one of the supported data types, it will be casted to a String. Prepared objects of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="values">List of unspecified objects to insert.</param>
        ''' <param name="cellRange">Cell range as string in the format like A1:D1 or X10:X22.</param>
        Public Sub AddCellRange(values As IReadOnlyList(Of Object), cellRange As String)
            Dim range = ResolveCellRange(cellRange)
            AddCellRangeInternal(values, range.StartAddress, range.EndAddress, Nothing)
        End Sub

        ''' <summary>
        ''' Adds a list of object values to a defined cell range. If the type of the a particular value does not match with one of the supported data types, it will be casted to a String. Prepared objects of the type Cell will not be casted but adjusted
        ''' </summary>
        ''' <param name="values">List of unspecified objects to insert.</param>
        ''' <param name="cellRange">Cell range as string in the format like A1:D1 or X10:X22.</param>
        ''' <param name="style">Style to apply on the all cells of the range.</param>
        Public Sub AddCellRange(values As IReadOnlyList(Of Object), cellRange As String, style As Style)
            Dim range = ResolveCellRange(cellRange)
            AddCellRangeInternal(values, range.StartAddress, range.EndAddress, style)
        End Sub

        ''' <summary>
        ''' Internal function to add a generic list of value to the defined cell range
        ''' </summary>
        ''' <typeparam name="T">Data type of the generic value list.</typeparam>
        ''' <param name="values">List of values.</param>
        ''' <param name="startAddress">Start address.</param>
        ''' <param name="endAddress">End address.</param>
        ''' <param name="style">Style to apply on the all cells of the range.</param>
        Private Sub AddCellRangeInternal(Of T)(values As IReadOnlyList(Of T), startAddress As Address, endAddress As Address, style As Style)
            Dim addresses As List(Of Address) = TryCast(GetCellRange(startAddress, endAddress), List(Of Address))
            If values.Count <> addresses.Count Then
                Throw New RangeException("OutOfRangeException", "The number of passed values (" & values.Count.ToString() & ") differs from the number of cells within the range (" & addresses.Count.ToString() & ")")
            End If
            Dim list As List(Of Cell) = TryCast(ConvertArray(values), List(Of Cell))
            Dim len = values.Count
            For i = 0 To len - 1
                list(i).RowNumber = addresses(i).Row
                list(i).ColumnNumber = addresses(i).Column
                AddNextCell(list(i), False, style)
            Next
        End Sub

        ''' <summary>
        ''' Removes a previous inserted cell at the defined address
        ''' </summary>
        ''' <param name="columnNumber">Column number (zero based).</param>
        ''' <param name="rowNumber">Row number (zero based).</param>
        ''' <returns>Returns true if the cell could be removed (existed), otherwise false (did not exist).</returns>
        Public Function RemoveCell(columnNumber As Integer, rowNumber As Integer) As Boolean
            Dim address = ResolveCellAddress(columnNumber, rowNumber)
            Return cellsField.Remove(address)
        End Function

        ''' <summary>
        ''' Removes a previous inserted cell at the defined address
        ''' </summary>
        ''' <param name="address">Cell address in the format A1 - XFD1048576.</param>
        ''' <returns>Returns true if the cell could be removed (existed), otherwise false (did not exist).</returns>
        Public Function RemoveCell(address As String) As Boolean
            Dim row, column As Integer
            ResolveCellCoordinate(address, column, row)
            Return RemoveCell(column, row)
        End Function

        ''' <summary>
        ''' Sets the passed style on the passed cell range. If cells are already existing, the style will be added or replaced
        ''' </summary>
        ''' <param name="cellRange">Cell range to apply the style.</param>
        ''' <param name="style">Style to apply.</param>
        Public Sub SetStyle(cellRange As Range, style As Style)
            Dim addresses As IReadOnlyList(Of Address) = cellRange.ResolveEnclosedAddresses()
            For Each address In addresses
                Dim key As String = address.GetAddress()
                If cellsField.ContainsKey(key) Then
                    If style Is Nothing Then
                        cellsField(key).RemoveStyle()
                    Else
                        cellsField(key).SetStyle(style)
                    End If
                Else
                    If style IsNot Nothing Then
                        AddCell(Nothing, address.Column, address.Row, style)
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Sets the passed style on the passed cell range, derived from a start and end address. If cells are already existing, the style will be added or replaced
        ''' Sets the passed style on the passed cell range, derived from a start and end address. If cells are already existing, the style will be added or replaced
        ''' </summary>
        ''' <param name="startAddress">Start address of the cell range.</param>
        ''' <param name="endAddress">End address of the cell range.</param>
        ''' <param name="style">Style to apply or null to clear the range.</param>
        Public Sub SetStyle(startAddress As Address, endAddress As Address, style As Style)
            SetStyle(New Range(startAddress, endAddress), style)
        End Sub

        ''' <summary>
        ''' Sets the passed style on the passed (singular) cell address. If the cell is already existing, the style will be added or replaced
        ''' Sets the passed style on the passed (singular) cell address. If the cell is already existing, the style will be added or replaced
        ''' </summary>
        ''' <param name="address">Cell address to apply the style.</param>
        ''' <param name="style">Style to apply or null to clear the range.</param>
        Public Sub SetStyle(address As Address, style As Style)
            SetStyle(address, address, style)
        End Sub

        ''' <summary>
        ''' Sets the passed style on the passed address expression. Such an expression may be a single cell or a cell range
        ''' Sets the passed style on the passed address expression. Such an expression may be a single cell or a cell range
        ''' Sets the passed style on the passed address expression. Such an expression may be a single cell or a cell range
        ''' </summary>
        ''' <param name="addressExpression">Expression of a cell address or range of addresses.</param>
        ''' <param name="style">Style to apply or null to clear the range.</param>
        Public Sub SetStyle(addressExpression As String, style As Style)
            Dim scope = GetAddressScope(addressExpression)
            If scope = AddressScope.SingleAddress Then
                Dim address As Address = New Address(addressExpression)
                SetStyle(address, style)
            ElseIf scope = AddressScope.Range Then
                Dim range As Range = New Range(addressExpression)
                SetStyle(range, style)
            Else
                Throw New FormatException("The passed address'" & addressExpression & "' is neither a cell address, nor a range")
            End If
        End Sub

        ''' <summary>
        ''' Method to add allowed actions if the worksheet is protected. If one or more values are added, UseSheetProtection will be set to true
        ''' </summary>
        ''' <param name="typeOfProtection">Allowed action on the worksheet or cells.</param>
        Public Sub AddAllowedActionOnSheetProtection(typeOfProtection As SheetProtectionValue)
            If Not sheetProtectionValuesField.Contains(typeOfProtection) Then
                If typeOfProtection = SheetProtectionValue.selectLockedCells AndAlso Not sheetProtectionValuesField.Contains(SheetProtectionValue.selectUnlockedCells) Then
                    sheetProtectionValuesField.Add(SheetProtectionValue.selectUnlockedCells)
                End If
                sheetProtectionValuesField.Add(typeOfProtection)
                UseSheetProtection = True
            End If
        End Sub

        ''' <summary>
        ''' Sets the defined column as hidden
        ''' </summary>
        ''' <param name="columnNumber">Column number to hide on the worksheet.</param>
        Public Sub AddHiddenColumn(columnNumber As Integer)
            SetColumnHiddenState(columnNumber, True)
        End Sub

        ''' <summary>
        ''' Sets the defined column as hidden
        ''' </summary>
        ''' <param name="columnAddress">Column address to hide on the worksheet.</param>
        Public Sub AddHiddenColumn(columnAddress As String)
            Dim columnNumber = ResolveColumn(columnAddress)
            SetColumnHiddenState(columnNumber, True)
        End Sub

        ''' <summary>
        ''' Sets the defined row as hidden
        ''' </summary>
        ''' <param name="rowNumber">Row number to hide on the worksheet.</param>
        Public Sub AddHiddenRow(rowNumber As Integer)
            SetRowHiddenState(rowNumber, True)
        End Sub

        ''' <summary>
        ''' Clears the active style of the worksheet. All later added calls will contain no style unless another active style is set
        ''' </summary>
        Public Sub ClearActiveStyle()
            useActiveStyle = False
            activeStyleField = Nothing
        End Sub

        ''' <summary>
        ''' Gets the cell of the specified address
        ''' </summary>
        ''' <param name="address">Address of the cell.</param>
        ''' <returns>Cell object.</returns>
        Public Function GetCell(address As Address) As Cell
            If Not cellsField.ContainsKey(address.GetAddress()) Then
                Throw New WorksheetException("The cell with the address " & address.GetAddress() & " does not exist in this worksheet")
            End If
            Return cellsField(address.GetAddress())
        End Function

        ''' <summary>
        ''' Gets the cell of the specified column and row number (zero-based)
        ''' </summary>
        ''' <param name="columnNumber">Column number of the cell.</param>
        ''' <param name="rowNumber">Row number of the cell.</param>
        ''' <returns>Cell object.</returns>
        Public Function GetCell(columnNumber As Integer, rowNumber As Integer) As Cell
            Return GetCell(New Address(columnNumber, rowNumber))
        End Function

        ''' <summary>
        ''' Gets whether the specified address exists in the worksheet. Existing means that a value was stored at the address
        ''' </summary>
        ''' <param name="address">Address to check.</param>
        ''' <returns>The <see cref="Boolean"/>.</returns>
        Public Function HasCell(address As Address) As Boolean
            Return cellsField.ContainsKey(address.GetAddress())
        End Function

        ''' <summary>
        ''' Gets whether the specified address exists in the worksheet. Existing means that a value was stored at the address
        ''' </summary>
        ''' <param name="columnNumber">Column number of the cell to check (zero-based).</param>
        ''' <param name="rowNumber">Row number of the cell to check (zero-based).</param>
        ''' <returns>The <see cref="Boolean"/>.</returns>
        Public Function HasCell(columnNumber As Integer, rowNumber As Integer) As Boolean
            Return HasCell(New Address(columnNumber, rowNumber))
        End Function

        ''' <summary>
        ''' Resets the defined column, if existing. The corresponding instance will be removed from <see cref="Columns"/>
        ''' </summary>
        ''' <param name="columnNumber">Column number to reset (zero-based).</param>
        Public Sub ResetColumn(columnNumber As Integer)
            If columnsField.ContainsKey(columnNumber) AndAlso Not columnsField(columnNumber).HasAutoFilter Then ' AutoFilters cannot have gaps 
                columnsField.Remove(columnNumber)
            ElseIf columnsField.ContainsKey(columnNumber) Then
                columnsField(columnNumber).IsHidden = False
                columnsField(columnNumber).Width = DEFAULT_COLUMN_WIDTH
            End If
        End Sub

        ''' <summary>
        ''' Gets the first existing column number in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based column number. In case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetFirstColumnNumber() As Integer
            Return GetBoundaryNumber(False, True)
        End Function

        ''' <summary>
        ''' Gets the first existing column number with data in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based column number. In case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetFirstDataColumnNumber() As Integer
            Return GetBoundaryDataNumber(False, True, True)
        End Function

        ''' <summary>
        ''' Gets the first existing row number in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based row number. In case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetFirstRowNumber() As Integer
            Return GetBoundaryNumber(True, True)
        End Function

        ''' <summary>
        ''' Gets the first existing row number with data in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based row number. In case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetFirstDataRowNumber() As Integer
            Return GetBoundaryDataNumber(True, True, True)
        End Function

        ''' <summary>
        ''' Gets the last existing column number in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based column number. In case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetLastColumnNumber() As Integer
            Return GetBoundaryNumber(False, False)
        End Function

        ''' <summary>
        ''' Gets the last existing column number with data in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based column number. in case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetLastDataColumnNumber() As Integer
            Return GetBoundaryDataNumber(False, False, True)
        End Function

        ''' <summary>
        ''' Gets the last existing row number in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based row number. In case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetLastRowNumber() As Integer
            Return GetBoundaryNumber(True, False)
        End Function

        ''' <summary>
        ''' Gets the last existing row number with data in the current worksheet (zero-based)
        ''' </summary>
        ''' <returns>Zero-based row number. in case of an empty worksheet, -1 will be returned.</returns>
        Public Function GetLastDataRowNumber() As Integer
            Return GetBoundaryDataNumber(True, False, True)
        End Function

        ''' <summary>
        ''' Gets the last existing cell in the current worksheet (bottom right)
        ''' </summary>
        ''' <returns>Nullable Cell Address. If no cell address could be determined, null will be returned.</returns>
        Public Function GetLastCellAddress() As Address?
            Dim lastRow As Integer = GetLastRowNumber()
            Dim lastColumn As Integer = GetLastColumnNumber()
            If lastRow < 0 OrElse lastColumn < 0 Then
                Return Nothing
            End If
            Return New Address(lastColumn, lastRow)
        End Function

        ''' <summary>
        ''' Gets the last existing cell with data in the current worksheet (bottom right)
        ''' </summary>
        ''' <returns>Nullable Cell Address. If no cell address could be determined, null will be returned.</returns>
        Public Function GetLastDataCellAddress() As Address?
            Dim lastRow As Integer = GetLastDataRowNumber()
            Dim lastColumn As Integer = GetLastDataColumnNumber()
            If lastRow < 0 OrElse lastColumn < 0 Then
                Return Nothing
            End If
            Return New Address(lastColumn, lastRow)
        End Function

        ''' <summary>
        ''' Gets the first existing cell in the current worksheet (bottom right)
        ''' </summary>
        ''' <returns>Nullable Cell Address. If no cell address could be determined, null will be returned.</returns>
        Public Function GetFirstCellAddress() As Address?
            Dim firstRow As Integer = GetFirstRowNumber()
            Dim firstColumn As Integer = GetFirstColumnNumber()
            If firstRow < 0 OrElse firstColumn < 0 Then
                Return Nothing
            End If
            Return New Address(firstColumn, firstRow)
        End Function

        ''' <summary>
        ''' Gets the first existing cell with data in the current worksheet (bottom right)
        ''' </summary>
        ''' <returns>Nullable Cell Address. If no cell address could be determined, null will be returned.</returns>
        Public Function GetFirstDataCellAddress() As Address?
            Dim firstRow As Integer = GetFirstDataRowNumber()
            Dim firstColumn As Integer = GetLastDataColumnNumber()
            If firstRow < 0 OrElse firstColumn < 0 Then
                Return Nothing
            End If
            Return New Address(firstColumn, firstRow)
        End Function

        ''' <summary>
        ''' Gets either the minimum or maximum row or column number, considering only calls with data
        ''' </summary>
        ''' <param name="row">If true, the min or max row is returned, otherwise the column.</param>
        ''' <param name="min">If true, the min value of the row or column is defined, otherwise the max value.</param>
        ''' <param name="ignoreEmpty">If true, empty cell values are ignored, otherwise considered without checking the content.</param>
        ''' <returns>Min or max number, or -1 if not defined.</returns>
        Private Function GetBoundaryDataNumber(row As Boolean, min As Boolean, ignoreEmpty As Boolean) As Integer
            If cellsField.Count = 0 Then
                Return -1
            End If
            If Not ignoreEmpty Then
                If row AndAlso min Then
                    Return cellsField.Min(Function(x) x.Value.RowNumber)
                ElseIf row Then
                    Return cellsField.Max(Function(x) x.Value.RowNumber)
                ElseIf min Then
                    Return cellsField.Min(Function(x) x.Value.ColumnNumber)
                Else
                    Return cellsField.Max(Function(x) x.Value.ColumnNumber)
                End If
            End If
            Dim nonEmptyCells As List(Of Cell) = cellsField.Values.Where(Function(x) x.Value IsNot Nothing).ToList()
            If nonEmptyCells.Count = 0 Then
                Return -1
            End If
            If row AndAlso min Then
                Return nonEmptyCells.Where(Function(x) Not Equals(x.Value.ToString(), String.Empty)).Min(Function(x) x.RowNumber)
            ElseIf row Then
                Return nonEmptyCells.Where(Function(x) Not Equals(x.Value.ToString(), String.Empty)).Max(Function(x) x.RowNumber)
            ElseIf min Then
                Return nonEmptyCells.Where(Function(x) Not Equals(x.Value.ToString(), String.Empty)).Max(Function(x) x.ColumnNumber)
            Else
                Return nonEmptyCells.Where(Function(x) Not Equals(x.Value.ToString(), String.Empty)).Min(Function(x) x.ColumnNumber)
            End If
        End Function

        ''' <summary>
        ''' Gets either the minimum or maximum row or column number, considering all available data
        ''' </summary>
        ''' <param name="row">If true, the min or max row is returned, otherwise the column.</param>
        ''' <param name="min">If true, the min value of the row or column is defined, otherwise the max value.</param>
        ''' <returns>Min or max number, or -1 if not defined.</returns>
        Private Function GetBoundaryNumber(row As Boolean, min As Boolean) As Integer
            Dim cellBoundary = GetBoundaryDataNumber(row, min, False)
            If row Then
                Dim heightBoundary = -1
                If rowHeightsField.Count > 0 Then
                    heightBoundary = If(min, RowHeights.Min(Function(x) x.Key), RowHeights.Max(Function(x) x.Key))
                End If
                Dim hiddenBoundary = -1
                If hiddenRowsField.Count > 0 Then
                    hiddenBoundary = If(min, HiddenRows.Min(Function(x) x.Key), HiddenRows.Max(Function(x) x.Key))
                End If
                Return If(min, GetMinRow(cellBoundary, heightBoundary, hiddenBoundary), GetMaxRow(cellBoundary, heightBoundary, hiddenBoundary))
            Else
                Dim columnDefBoundary = -1
                If columnsField.Count > 0 Then
                    columnDefBoundary = If(min, Columns.Min(Function(x) x.Key), Columns.Max(Function(x) x.Key))
                End If
                If min Then
                    Return If(cellBoundary > 0 AndAlso cellBoundary < columnDefBoundary, cellBoundary, columnDefBoundary)
                Else
                    Return If(cellBoundary > 0 AndAlso cellBoundary > columnDefBoundary, cellBoundary, columnDefBoundary)
                End If
            End If
        End Function

        ''' <summary>
        ''' Gets the maximum row coordinate either from cell data, height definitions or hidden rows
        ''' </summary>
        ''' <param name="cellBoundary">Row number of max cell data.</param>
        ''' <param name="heightBoundary">Row number of max defined row height.</param>
        ''' <param name="hiddenBoundary">Row number of max defined hidden row.</param>
        ''' <returns>Max row number or -1 if nothing valid defined.</returns>
        Private Function GetMaxRow(cellBoundary As Integer, heightBoundary As Integer, hiddenBoundary As Integer) As Integer
            Dim highest = -1
            If cellBoundary >= 0 Then
                highest = cellBoundary
            End If
            If heightBoundary >= 0 AndAlso heightBoundary > highest Then
                highest = heightBoundary
            End If
            If hiddenBoundary >= 0 AndAlso hiddenBoundary > highest Then
                highest = hiddenBoundary
            End If
            Return highest
        End Function

        ''' <summary>
        ''' Gets the minimum row coordinate either from cell data, height definitions or hidden rows
        ''' </summary>
        ''' <param name="cellBoundary">Row number of min cell data.</param>
        ''' <param name="heightBoundary">Row number of min defined row height.</param>
        ''' <param name="hiddenBoundary">Row number of min defined hidden row.</param>
        ''' <returns>Min row number or -1 if nothing valid defined.</returns>
        Private Function GetMinRow(cellBoundary As Integer, heightBoundary As Integer, hiddenBoundary As Integer) As Integer
            Dim lowest = Integer.MaxValue
            If cellBoundary >= 0 Then
                lowest = cellBoundary
            End If
            If heightBoundary >= 0 AndAlso heightBoundary < lowest Then
                lowest = heightBoundary
            End If
            If hiddenBoundary >= 0 AndAlso hiddenBoundary < lowest Then
                lowest = hiddenBoundary
            End If
            Return If(lowest = Integer.MaxValue, -1, lowest)
        End Function

        ''' <summary>
        ''' Gets the current column number (zero based)
        ''' </summary>
        ''' <returns>Column number (zero-based).</returns>
        Public Function GetCurrentColumnNumber() As Integer
            Return currentColumnNumber
        End Function

        ''' <summary>
        ''' Gets the current row number (zero based)
        ''' </summary>
        ''' <returns>Row number (zero-based).</returns>
        Public Function GetCurrentRowNumber() As Integer
            Return currentRowNumber
        End Function

        ''' <summary>
        ''' Moves the current position to the next column
        ''' </summary>
        Public Sub GoToNextColumn()
            currentColumnNumber += 1
            currentRowNumber = 0
            ValidateColumnNumber(currentColumnNumber)
        End Sub

        ''' <summary>
        ''' Moves the current position to the next column with the number of cells to move
        ''' </summary>
        ''' <param name="numberOfColumns">Number of columns to move.</param>
        ''' <param name="keepRowPosition">If true, the row position is preserved, otherwise set to 0.</param>
        Public Sub GoToNextColumn(numberOfColumns As Integer, Optional keepRowPosition As Boolean = False)
            currentColumnNumber += numberOfColumns
            If Not keepRowPosition Then
                currentRowNumber = 0
            End If
            ValidateColumnNumber(currentColumnNumber)
        End Sub

        ''' <summary>
        ''' Moves the current position to the next row (use for a new line)
        ''' </summary>
        Public Sub GoToNextRow()
            currentRowNumber += 1
            currentColumnNumber = 0
            ValidateRowNumber(currentRowNumber)
        End Sub

        ''' <summary>
        ''' Moves the current position to the next row with the number of cells to move (use for a new line)
        ''' </summary>
        ''' <param name="numberOfRows">Number of rows to move.</param>
        ''' <param name="keepColumnPosition">If true, the column position is preserved, otherwise set to 0.</param>
        Public Sub GoToNextRow(numberOfRows As Integer, Optional keepColumnPosition As Boolean = False)
            currentRowNumber += numberOfRows
            If Not keepColumnPosition Then
                currentColumnNumber = 0
            End If
            ValidateRowNumber(currentRowNumber)
        End Sub

        ''' <summary>
        ''' Merges the defined cell range
        ''' </summary>
        ''' <param name="cellRange">Range to merge.</param>
        ''' <returns>Returns the validated range of the merged cells (e.g. 'A1:B12').</returns>
        Public Function MergeCells(cellRange As Range) As String
            Return MergeCells(cellRange.StartAddress, cellRange.EndAddress)
        End Function

        ''' <summary>
        ''' Merges the defined cell range
        ''' </summary>
        ''' <param name="cellRange">Range to merge (e.g. 'A1:B12').</param>
        ''' <returns>Returns the validated range of the merged cells (e.g. 'A1:B12').</returns>
        Public Function MergeCells(cellRange As String) As String
            Dim range = ResolveCellRange(cellRange)
            Return MergeCells(range.StartAddress, range.EndAddress)
        End Function

        ''' <summary>
        ''' Merges the defined cell range
        ''' </summary>
        ''' <param name="startAddress">Start address of the merged cell range.</param>
        ''' <param name="endAddress">End address of the merged cell range.</param>
        ''' <returns>Returns the validated range of the merged cells (e.g. 'A1:B12').</returns>
        Public Function MergeCells(startAddress As Address, endAddress As Address) As String
            Dim key As String = startAddress.ToString() & ":" & endAddress.ToString()
            Dim value As Range = New Range(startAddress, endAddress)
            Dim enclosedAddress As IReadOnlyList(Of Address) = value.ResolveEnclosedAddresses()
            For Each item In mergedCellsField
                If Enumerable.ToList(Enumerable.Intersect(item.Value.ResolveEnclosedAddresses(), enclosedAddress)).Count > 0 Then
                    Throw New RangeException("ConflictingRangeException", "The passed range: " & value.ToString() & " contains cells that are already in the defined merge range: " & item.Key)
                End If
            Next
            mergedCellsField.Add(key, value)
            Return key
        End Function

        ''' <summary>
        ''' Method to recalculate the auto filter (columns) of this worksheet. This is an internal method. There is no need to use it
        ''' </summary>
        Friend Sub RecalculateAutoFilter()
            If autoFilterRangeField Is Nothing Then
                Return
            End If
            Dim start = autoFilterRangeField.Value.StartAddress.Column
            Dim [end] = autoFilterRangeField.Value.EndAddress.Column
            Dim endRow = 0
            For Each item In Cells
                If item.Value.ColumnNumber < start OrElse item.Value.ColumnNumber > [end] Then
                    Continue For
                End If
                If item.Value.RowNumber > endRow Then
                    endRow = item.Value.RowNumber
                End If
            Next
            Dim c As Column
            For i As Integer = start To [end]
                If Not columnsField.ContainsKey(i) Then
                    c = New Column(i)
                    c.HasAutoFilter = True
                    columnsField.Add(i, c)
                Else
                    columnsField(i).HasAutoFilter = True
                End If
            Next
            Dim temp As Range = New Range()
            temp.StartAddress = New Address(start, 0)
            temp.EndAddress = New Address([end], endRow)
            autoFilterRangeField = temp
        End Sub

        ''' <summary>
        ''' Method to recalculate the collection of columns of this worksheet. This is an internal method. There is no need to use it
        ''' </summary>
        Friend Sub RecalculateColumns()
            Dim columnsToDelete As List(Of Integer) = New List(Of Integer)()
            For Each col In columnsField
                If Not col.Value.HasAutoFilter AndAlso Not col.Value.IsHidden AndAlso stdNum.Abs(col.Value.Width - DEFAULT_COLUMN_WIDTH) <= FLOAT_THRESHOLD Then
                    columnsToDelete.Add(col.Key)
                End If
                If Not col.Value.HasAutoFilter AndAlso Not col.Value.IsHidden AndAlso stdNum.Abs(col.Value.Width - DEFAULT_COLUMN_WIDTH) <= FLOAT_THRESHOLD Then
                    columnsToDelete.Add(col.Key)
                End If
            Next
            For Each index In columnsToDelete
                columnsField.Remove(index)
            Next
        End Sub

        ''' <summary>
        ''' Method to resolve all merged cells of the worksheet. Only the value of the very first cell of the locked cells range will be visible. The other values are still present (set to EMPTY) but will not be stored in the worksheet.<br/>
        ''' This is an internal method. There is no need to use it
        ''' </summary>
        Friend Sub ResolveMergedCells()
            Dim mergeStyle = Style.BasicStyles.MergeCellStyle
            Dim cell As Cell
            For Each range As KeyValuePair(Of String, Range) In MergedCells
                Dim pos = 0
                Dim addresses As List(Of Address) = TryCast(GetCellRange(range.Value.StartAddress, range.Value.EndAddress), List(Of Address))
                For Each address In addresses
                    If Not Cells.ContainsKey(address.GetAddress()) Then
                        cell = New Cell()
                        cell.DataType = CellType.EMPTY
                        cell.RowNumber = address.Row
                        cell.ColumnNumber = address.Column
                        AddCell(cell, cell.ColumnNumber, cell.RowNumber)
                    Else
                        cell = Cells(address.GetAddress())
                    End If
                    If pos <> 0 Then
                        If cell.CellStyle Is Nothing Then
                            cell.SetStyle(mergeStyle)
                        Else
                            Dim mixedMergeStyle = cell.CellStyle
                            ' TODO: There should be a better possibility to identify particular style elements that deviates
                            mixedMergeStyle.CurrentCellXf.ForceApplyAlignment = mergeStyle.CurrentCellXf.ForceApplyAlignment
                            cell.SetStyle(mixedMergeStyle)
                        End If
                    End If
                    pos += 1
                Next
            Next
        End Sub

        ''' <summary>
        ''' Removes auto filters from the worksheet
        ''' </summary>
        Public Sub RemoveAutoFilter()
            autoFilterRangeField = Nothing
        End Sub

        ''' <summary>
        ''' Sets a previously defined, hidden column as visible again
        ''' </summary>
        ''' <param name="columnNumber">Column number to make visible again.</param>
        Public Sub RemoveHiddenColumn(columnNumber As Integer)
            SetColumnHiddenState(columnNumber, False)
        End Sub

        ''' <summary>
        ''' Sets a previously defined, hidden column as visible again
        ''' </summary>
        ''' <param name="columnAddress">Column address to make visible again.</param>
        Public Sub RemoveHiddenColumn(columnAddress As String)
            Dim columnNumber = ResolveColumn(columnAddress)
            SetColumnHiddenState(columnNumber, False)
        End Sub

        ''' <summary>
        ''' Sets a previously defined, hidden row as visible again
        ''' </summary>
        ''' <param name="rowNumber">Row number to hide on the worksheet.</param>
        Public Sub RemoveHiddenRow(rowNumber As Integer)
            SetRowHiddenState(rowNumber, False)
        End Sub

        ''' <summary>
        ''' Removes the defined merged cell range
        ''' </summary>
        ''' <param name="range">Cell range to remove the merging.</param>
        Public Sub RemoveMergedCells(range As String)
            If Not Equals(range, Nothing) Then
                range = range.ToUpper()
            End If
            If Equals(range, Nothing) OrElse Not mergedCellsField.ContainsKey(range) Then
                Throw New RangeException("UnknownRangeException", "The cell range " & range & " was not found in the list of merged cell ranges")
            End If

            Dim addresses As List(Of Address) = TryCast(GetCellRange(range), List(Of Address))
            For Each address In addresses
                If cellsField.ContainsKey(address.GetAddress()) Then
                    Dim cell As Cell = cellsField(address.ToString())
                    If Style.BasicStyles.MergeCellStyle.Equals(cell.CellStyle) Then
                        cell.RemoveStyle()
                    End If
                    cell.ResolveCellType() ' resets the type
                End If
            Next
            mergedCellsField.Remove(range)
        End Sub

        ''' <summary>
        ''' Removes the cell selection of this worksheet
        ''' </summary>
        Public Sub RemoveSelectedCells()
            selectedCellsField.Clear()
        End Sub

        ''' <summary>
        ''' Removes the defined, non-standard row height
        ''' </summary>
        ''' <param name="rowNumber">Row number (zero-based).</param>
        Public Sub RemoveRowHeight(rowNumber As Integer)
            If rowHeightsField.ContainsKey(rowNumber) Then
                rowHeightsField.Remove(rowNumber)
            End If
        End Sub

        ''' <summary>
        ''' Removes an allowed action on the current worksheet or its cells
        ''' </summary>
        ''' <param name="value">Allowed action on the worksheet or cells.</param>
        Public Sub RemoveAllowedActionOnSheetProtection(value As SheetProtectionValue)
            If sheetProtectionValuesField.Contains(value) Then
                sheetProtectionValuesField.Remove(value)
            End If
        End Sub

        ''' <summary>
        ''' Sets the active style of the worksheet. This style will be assigned to all later added cells
        ''' </summary>
        ''' <param name="style">Style to set as active style.</param>
        Public Sub SetActiveStyle(style As Style)
            If style Is Nothing Then
                useActiveStyle = False
            Else
                useActiveStyle = True
            End If
            activeStyleField = style
        End Sub

        ''' <summary>
        ''' Sets the column auto filter within the defined column range
        ''' </summary>
        ''' <param name="startColumn">Column number with the first appearance of an auto filter drop down.</param>
        ''' <param name="endColumn">Column number with the last appearance of an auto filter drop down.</param>
        Public Sub SetAutoFilter(startColumn As Integer, endColumn As Integer)
            Dim start = ResolveCellAddress(startColumn, 0)
            Dim [end] = ResolveCellAddress(endColumn, 0)
            If endColumn < startColumn Then
                SetAutoFilter([end] & ":" & start)
            Else
                SetAutoFilter(start & ":" & [end])
            End If
        End Sub

        ''' <summary>
        ''' Sets the column auto filter within the defined column range
        ''' </summary>
        ''' <param name="range">Range to apply auto filter on. The range could be 'A1:C10' for instance. The end row will be recalculated automatically when saving the file.</param>
        Public Sub SetAutoFilter(range As String)
            autoFilterRangeField = ResolveCellRange(range)
            RecalculateAutoFilter()
            RecalculateColumns()
        End Sub

        ''' <summary>
        ''' Sets the defined column as hidden or visible
        ''' </summary>
        ''' <param name="columnNumber">Column number to hide on the worksheet.</param>
        ''' <param name="state">If true, the column will be hidden, otherwise be visible.</param>
        Private Sub SetColumnHiddenState(columnNumber As Integer, state As Boolean)
            ValidateColumnNumber(columnNumber)
            If columnsField.ContainsKey(columnNumber) Then
                columnsField(columnNumber).IsHidden = state
            ElseIf state Then
                Dim c As Column = New Column(columnNumber)
                c.IsHidden = True
                columnsField.Add(columnNumber, c)
            End If
            If Not columnsField(columnNumber).IsHidden AndAlso stdNum.Abs(columnsField(columnNumber).Width - DEFAULT_COLUMN_WIDTH) <= FLOAT_THRESHOLD AndAlso Not columnsField(columnNumber).HasAutoFilter Then
                columnsField.Remove(columnNumber)
            End If
        End Sub

        ''' <summary>
        ''' Sets the width of the passed column address
        ''' </summary>
        ''' <param name="columnAddress">Column address (A - XFD).</param>
        ''' <param name="width">Width from 0 to 255.0.</param>
        Public Sub SetColumnWidth(columnAddress As String, width As Single)
            Dim columnNumber = ResolveColumn(columnAddress)
            SetColumnWidth(columnNumber, width)
        End Sub

        ''' <summary>
        ''' Sets the width of the passed column number (zero-based)
        ''' </summary>
        ''' <param name="columnNumber">Column number (zero-based, from 0 to 16383).</param>
        ''' <param name="width">Width from 0 to 255.0.</param>
        Public Sub SetColumnWidth(columnNumber As Integer, width As Single)
            ValidateColumnNumber(columnNumber)
            If width < MIN_COLUMN_WIDTH OrElse width > MAX_COLUMN_WIDTH Then
                Throw New RangeException("OutOfRangeException", "The column width (" & width.ToString() & ") is out of range. Range is from " & MIN_COLUMN_WIDTH.ToString() & " to " & MAX_COLUMN_WIDTH.ToString() & " (chars).")
            End If
            If columnsField.ContainsKey(columnNumber) Then
                columnsField(columnNumber).Width = width
            Else
                Dim c As Column = New Column(columnNumber)
                c.Width = width
                columnsField.Add(columnNumber, c)
            End If
        End Sub

        ''' <summary>
        ''' Set the current cell address
        ''' </summary>
        ''' <param name="columnNumber">Column number (zero based).</param>
        ''' <param name="rowNumber">Row number (zero based).</param>
        Public Sub SetCurrentCellAddress(columnNumber As Integer, rowNumber As Integer)
            SetCurrentColumnNumber(columnNumber)
            SetCurrentRowNumber(rowNumber)
        End Sub

        ''' <summary>
        ''' Set the current cell address
        ''' </summary>
        ''' <param name="address">Cell address in the format A1 - XFD1048576.</param>
        Public Sub SetCurrentCellAddress(address As String)
            Dim row, column As Integer
            ResolveCellCoordinate(address, column, row)
            SetCurrentCellAddress(column, row)
        End Sub

        ''' <summary>
        ''' Sets the current column number (zero based)
        ''' </summary>
        ''' <param name="columnNumber">Column number (zero based).</param>
        Public Sub SetCurrentColumnNumber(columnNumber As Integer)
            ValidateColumnNumber(columnNumber)
            currentColumnNumber = columnNumber
        End Sub

        ''' <summary>
        ''' Sets the current row number (zero based)
        ''' </summary>
        ''' <param name="rowNumber">Row number (zero based).</param>
        Public Sub SetCurrentRowNumber(rowNumber As Integer)
            ValidateRowNumber(rowNumber)
            currentRowNumber = rowNumber
        End Sub

        ''' <summary>
        ''' Sets a single range of selected cells on this worksheet. All existing ranges will be removed
        ''' </summary>
        ''' <param name="range">Range to set as single cell range for selected cells</param>
        <Obsolete("This method is a deprecated subset of the function AddSelectedCells. It will be removed in a future version")>
        Public Sub SetSelectedCells(range As Range)
            RemoveSelectedCells()
            AddSelectedCells(range)
        End Sub

        ''' <summary>
        ''' Sets the selected cells on this worksheet
        ''' </summary>
        ''' <param name="startAddress">Start address of the range to set as single cell range for selected cells</param>
        ''' <param name="endAddress">End address of the range to set as single cell range for selected cells</param>
        <Obsolete("This method is a deprecated subset of the function AddSelectedCells. It will be removed in a future version")>
        Public Sub SetSelectedCells(startAddress As Address, endAddress As Address)
            SetSelectedCells(New Range(startAddress, endAddress))
        End Sub

        ''' <summary>
        ''' Sets a single range of selected cells on this worksheet. All existing ranges will be removed. Null will remove all selected cells
        ''' </summary>
        ''' <param name="range">Range as string to set as single cell range for selected cells, or null to remove the selected cells</param>
        <Obsolete("This method is a deprecated subset of the function AddSelectedCells. It will be removed in a future version")>
        Public Sub SetSelectedCells(range As String)
            If Equals(range, Nothing) Then
                selectedCellsField.Clear()
                Return
            Else
                SetSelectedCells(New Range(range))
            End If
        End Sub

        ''' <summary>
        ''' Adds a range to the selected cells on this worksheet
        ''' </summary>
        ''' <param name="range">Cell range to be added as selected cells</param>
        Public Sub AddSelectedCells(range As Range)
            selectedCellsField.Add(range)
        End Sub

        ''' <summary>
        ''' Adds a range to the selected cells on this worksheet
        ''' </summary>
        ''' <param name="startAddress">Start address of the range to add</param>
        ''' <param name="endAddress">End address of the range to add</param>
        Public Sub AddSelectedCells(startAddress As Address, endAddress As Address)
            selectedCellsField.Add(New Range(startAddress, endAddress))
        End Sub

        ''' <summary>
        ''' Adds a range to the selected cells on this worksheet. Null or empty as value will be ignored
        ''' </summary>
        ''' <param name="range">Cell range to add as selected cells</param>
        Public Sub AddSelectedCells(range As String)
            If Not Equals(range, Nothing) Then
                selectedCellsField.Add(ResolveCellRange(range))
            End If
        End Sub

        ''' <summary>
        ''' Sets or removes the password for worksheet protection. If set, UseSheetProtection will be also set to true
        ''' </summary>
        ''' <param name="password">Password (UTF-8) to protect the worksheet. If the password is null or empty, no password will be used.</param>
        Public Sub SetSheetProtectionPassword(password As String)
            If String.IsNullOrEmpty(password) Then
                sheetProtectionPasswordField = Nothing
                sheetProtectionPasswordHashField = Nothing
                UseSheetProtection = False
            Else
                sheetProtectionPasswordField = password
                sheetProtectionPasswordHashField = LowLevel.GeneratePasswordHash(password)
                UseSheetProtection = True
            End If
        End Sub

        ''' <summary>
        ''' Sets the height of the passed row number (zero-based)
        ''' </summary>
        ''' <param name="rowNumber">Row number (zero-based, 0 to 1048575).</param>
        ''' <param name="height">Height from 0 to 409.5.</param>
        Public Sub SetRowHeight(rowNumber As Integer, height As Single)
            ValidateRowNumber(rowNumber)
            If height < MIN_ROW_HEIGHT OrElse height > MAX_ROW_HEIGHT Then
                Throw New RangeException("OutOfRangeException", "The row height (" & height.ToString() & ") is out of range. Range is from " & MIN_ROW_HEIGHT.ToString() & " to " & MAX_ROW_HEIGHT.ToString() & " (equals 546px).")
            End If
            If rowHeightsField.ContainsKey(rowNumber) Then
                rowHeightsField(rowNumber) = height
            Else
                rowHeightsField.Add(rowNumber, height)
            End If
        End Sub

        ''' <summary>
        ''' Sets the defined row as hidden or visible
        ''' </summary>
        ''' <param name="rowNumber">Row number to make visible again.</param>
        ''' <param name="state">If true, the row will be hidden, otherwise visible.</param>
        Private Sub SetRowHiddenState(rowNumber As Integer, state As Boolean)
            ValidateRowNumber(rowNumber)
            If hiddenRowsField.ContainsKey(rowNumber) Then
                If state Then
                    hiddenRowsField(rowNumber) = True
                Else
                    hiddenRowsField.Remove(rowNumber)
                End If
            ElseIf state Then
                hiddenRowsField.Add(rowNumber, True)
            End If
        End Sub

        ''' <summary>
        ''' Validates and sets the worksheet name
        ''' </summary>
        ''' <param name="name">Name to set.</param>
        Public Sub SetSheetName(name As String)
            If String.IsNullOrEmpty(name) Then
                Throw New FormatException("the worksheet name must be between 1 and " & MAX_WORKSHEET_NAME_LENGTH.ToString() & " characters")
            End If
            If name.Length > MAX_WORKSHEET_NAME_LENGTH Then
                Throw New FormatException("the worksheet name must be between 1 and " & MAX_WORKSHEET_NAME_LENGTH.ToString() & " characters")
            End If
            Dim regex As Regex = New Regex("[\[\]\*\?/\\]")
            Dim match = regex.Match(name)
            If match.Captures.Count > 0 Then
                Throw New FormatException("the worksheet name must not contain the characters [  ]  * ? / \ ")
            End If
            sheetNameField = name
        End Sub

        ''' <summary>
        ''' Sets the name of the worksheet
        ''' </summary>
        ''' <param name="name">Name of the worksheet.</param>
        ''' <param name="sanitize">If true, the filename will be sanitized automatically according to the specifications of Excel.</param>
        Public Sub SetSheetName(name As String, sanitize As Boolean)
            If sanitize Then
                sheetNameField = "" ' Empty name (temporary) to prevent conflicts during sanitizing
                sheetNameField = SanitizeWorksheetName(name, workbookReferenceField)
            Else
                SetSheetName(name)
            End If
        End Sub

        ''' <summary>
        ''' Sets the horizontal split of the worksheet into two panes. The measurement in characters cannot be used to freeze panes
        ''' </summary>
        ''' <param name="topPaneHeight">Height (similar to row height) from top of the worksheet to the split line in characters.</param>
        ''' <param name="topLeftCell">Top Left cell address of the bottom right pane (if applicable). Only the row component is important in a horizontal split.</param>
        ''' <param name="activePane">Active pane in the split window.</param>
        Public Sub SetHorizontalSplit(topPaneHeight As Single, topLeftCell As Address, activePane As WorksheetPane)
            SetSplit(Nothing, topPaneHeight, topLeftCell, activePane)
        End Sub

        ''' <summary>
        ''' Sets the horizontal split of the worksheet into two panes. The measurement in rows can be used to split and freeze panes
        ''' </summary>
        ''' <param name="numberOfRowsFromTop">Number of rows from top of the worksheet to the split line. The particular row heights are considered.</param>
        ''' <param name="freeze">If true, all panes are frozen, otherwise remains movable.</param>
        ''' <param name="topLeftCell">Top Left cell address of the bottom right pane (if applicable). Only the row component is important in a horizontal split.</param>
        ''' <param name="activePane">Active pane in the split window.</param>
        Public Sub SetHorizontalSplit(numberOfRowsFromTop As Integer, freeze As Boolean, topLeftCell As Address, activePane As WorksheetPane)
            SetSplit(Nothing, numberOfRowsFromTop, freeze, topLeftCell, activePane)
        End Sub

        ''' <summary>
        ''' Sets the vertical split of the worksheet into two panes. The measurement in characters cannot be used to freeze panes
        ''' </summary>
        ''' <param name="leftPaneWidth">Width (similar to column width) from left of the worksheet to the split line in characters.</param>
        ''' <param name="topLeftCell">Top Left cell address of the bottom right pane (if applicable). Only the column component is important in a vertical split.</param>
        ''' <param name="activePane">Active pane in the split window.</param>
        Public Sub SetVerticalSplit(leftPaneWidth As Single, topLeftCell As Address, activePane As WorksheetPane)
            SetSplit(leftPaneWidth, Nothing, topLeftCell, activePane)
        End Sub

        ''' <summary>
        ''' Sets the vertical split of the worksheet into two panes. The measurement in columns can be used to split and freeze panes
        ''' </summary>
        ''' <param name="numberOfColumnsFromLeft">Number of columns from left of the worksheet to the split line. The particular column widths are considered.</param>
        ''' <param name="freeze">If true, all panes are frozen, otherwise remains movable.</param>
        ''' <param name="topLeftCell">Top Left cell address of the bottom right pane (if applicable). Only the column component is important in a vertical split.</param>
        ''' <param name="activePane">Active pane in the split window.</param>
        Public Sub SetVerticalSplit(numberOfColumnsFromLeft As Integer, freeze As Boolean, topLeftCell As Address, activePane As WorksheetPane)
            SetSplit(numberOfColumnsFromLeft, Nothing, freeze, topLeftCell, activePane)
        End Sub

        ''' <summary>
        ''' Sets the horizontal and vertical split of the worksheet into four panes. The measurement in rows and columns can be used to split and freeze panes
        ''' </summary>
        ''' <param name="numberOfColumnsFromLeft">The numberOfColumnsFromLeft<see cref="Integer."/>.</param>
        ''' <param name="numberOfRowsFromTop">The numberOfRowsFromTop<see cref="Integer."/>.</param>
        ''' <param name="freeze">If true, all panes are frozen, otherwise remains movable.</param>
        ''' <param name="topLeftCell">Top Left cell address of the bottom right pane (if applicable).</param>
        ''' <param name="activePane">Active pane in the split window.</param>
        Public Sub SetSplit(numberOfColumnsFromLeft As Integer?, numberOfRowsFromTop As Integer?, freeze As Boolean, topLeftCell As Address, activePane As WorksheetPane)
            If freeze Then
                If numberOfColumnsFromLeft IsNot Nothing AndAlso topLeftCell.Column < numberOfColumnsFromLeft.Value Then
                    Throw New WorksheetException("The column number " & topLeftCell.Column.ToString() & " is not valid for a frozen, vertical split with the split pane column number " & numberOfColumnsFromLeft.Value.ToString())
                End If
                If numberOfRowsFromTop IsNot Nothing AndAlso topLeftCell.Row < numberOfRowsFromTop.Value Then
                    Throw New WorksheetException("The row number " & topLeftCell.Row.ToString() & " is not valid for a frozen, horizontal split height the split pane row number " & numberOfRowsFromTop.Value.ToString())
                End If
            End If
            paneSplitLeftWidthField = Nothing
            paneSplitTopHeightField = Nothing
            freezeSplitPanesField = freeze
            Dim row = If(numberOfRowsFromTop IsNot Nothing, numberOfRowsFromTop.Value, 0)
            Dim column = If(numberOfColumnsFromLeft IsNot Nothing, numberOfColumnsFromLeft.Value, 0)
            paneSplitAddressField = New Address(column, row)
            paneSplitTopLeftCellField = topLeftCell
            activePaneField = activePane
        End Sub

        ''' <summary>
        ''' Sets the horizontal and vertical split of the worksheet into four panes. The measurement in characters cannot be used to freeze panes
        ''' </summary>
        ''' <param name="leftPaneWidth">The leftPaneWidth<see cref="Single."/>.</param>
        ''' <param name="topPaneHeight">The topPaneHeight<see cref="Single."/>.</param>
        ''' <param name="topLeftCell">Top Left cell address of the bottom right pane (if applicable).</param>
        ''' <param name="activePane">Active pane in the split window.</param>
        Public Sub SetSplit(leftPaneWidth As Single?, topPaneHeight As Single?, topLeftCell As Address, activePane As WorksheetPane)
            paneSplitLeftWidthField = leftPaneWidth
            paneSplitTopHeightField = topPaneHeight
            freezeSplitPanesField = Nothing
            paneSplitAddressField = Nothing
            paneSplitTopLeftCellField = topLeftCell
            activePaneField = activePane
        End Sub

        ''' <summary>
        ''' Resets splitting of the worksheet into panes, as well as their freezing
        ''' </summary>
        Public Sub ResetSplit()
            paneSplitLeftWidthField = Nothing
            paneSplitTopHeightField = Nothing
            freezeSplitPanesField = Nothing
            paneSplitAddressField = Nothing
            paneSplitTopLeftCellField = Nothing
            activePaneField = Nothing
        End Sub

        ''' <summary>
        ''' Creates a (dereferenced) deep copy of this worksheet
        ''' </summary>
        ''' <returns>The <see cref="Worksheet"/>.</returns>
        Public Function Copy() As Worksheet
            Dim lCopy As Worksheet = New Worksheet()
            For Each cell In cellsField
                lCopy.AddCell(cell.Value.Copy(), cell.Key)
            Next
            lCopy.activePaneField = activePaneField
            lCopy.activeStyleField = activeStyleField
            If autoFilterRangeField.HasValue Then
                lCopy.autoFilterRangeField = autoFilterRangeField.Value.Copy()
            End If
            For Each column In columnsField
                lCopy.columnsField.Add(column.Key, column.Value.Copy())
            Next
            lCopy.CurrentCellDirection = CurrentCellDirection
            lCopy.currentColumnNumber = currentColumnNumber
            lCopy.currentRowNumber = currentRowNumber
            lCopy.defaultColumnWidthField = defaultColumnWidthField
            lCopy.defaultRowHeightField = defaultRowHeightField
            lCopy.freezeSplitPanesField = freezeSplitPanesField
            lCopy.hiddenField = hiddenField
            For Each row In hiddenRowsField
                lCopy.hiddenRowsField.Add(row.Key, row.Value)
            Next
            For Each cell In mergedCellsField
                lCopy.mergedCellsField.Add(cell.Key, cell.Value.Copy())
            Next
            If paneSplitAddressField.HasValue Then
                lCopy.paneSplitAddressField = paneSplitAddressField.Value.Copy()
            End If
            lCopy.paneSplitLeftWidthField = paneSplitLeftWidthField
            lCopy.paneSplitTopHeightField = paneSplitTopHeightField
            If paneSplitTopLeftCellField.HasValue Then
                lCopy.paneSplitTopLeftCellField = paneSplitTopLeftCellField.Value.Copy()
            End If
            For Each row In rowHeightsField
                lCopy.rowHeightsField.Add(row.Key, row.Value)
            Next
            If selectedCellsField.Count > 0 Then
                For Each selectedCellRange In selectedCellsField
                    lCopy.AddSelectedCells(selectedCellRange.Copy())
                Next
            End If
            lCopy.sheetProtectionPasswordField = sheetProtectionPasswordField
            lCopy.sheetProtectionPasswordHashField = sheetProtectionPasswordHashField
            For Each value As SheetProtectionValue In sheetProtectionValuesField
                lCopy.sheetProtectionValuesField.Add(value)
            Next
            lCopy.useActiveStyle = useActiveStyle
            lCopy.UseSheetProtection = UseSheetProtection
            Return lCopy
        End Function

        ''' <summary>
        ''' Sanitizes a worksheet name
        ''' </summary>
        ''' <param name="input">Name to sanitize.</param>
        ''' <param name="workbook">Workbook reference.</param>
        ''' <returns>Name of the sanitized worksheet.</returns>
        Public Shared Function SanitizeWorksheetName(input As String, workbook As Workbook) As String
            If Equals(input, Nothing) Then
                input = "Sheet1"
            End If
            Dim len = input.Length
            If len > 31 Then
                len = 31
            ElseIf len = 0 Then
                input = "Sheet1"
            End If
            Dim sb As StringBuilder = New StringBuilder(31)
            Dim c As Char
            For i = 0 To len - 1
                c = input(i)
                If c = "["c OrElse c = "]"c OrElse c = "*"c OrElse c = "?"c OrElse c = "\"c OrElse c = "/"c Then
                    sb.Append("_"c)
                Else
                    sb.Append(c)
                End If
            Next
            Return GetUnusedWorksheetName(sb.ToString(), workbook)
        End Function

        ''' <summary>
        ''' Determines the next unused worksheet name in the passed workbook
        ''' </summary>
        ''' <param name="name">Original name to start the check.</param>
        ''' <param name="workbook">Workbook to look for existing worksheets.</param>
        ''' <returns>Not yet used worksheet name.</returns>
        Private Shared Function GetUnusedWorksheetName(name As String, workbook As Workbook) As String
            If workbook Is Nothing Then
                Throw New WorksheetException("The workbook reference is null")
            End If
            If Not WorksheetExists(name, workbook) Then
                Return name
            End If
            Dim regex As Regex = New Regex("^(.*?)(\d{1,31})$")
            Dim match = regex.Match(name)
            Dim prefix = name
            Dim number = 1
            If match.Groups.Count > 1 Then
                prefix = match.Groups(1).Value
                Integer.TryParse(match.Groups(2).Value, number)
                ' if this failed, the start number is 0 (parsed number was >max. int32)
            End If
            While True
                Dim numberString = number.ToString("G", CultureInfo.InvariantCulture)
                If numberString.Length + prefix.Length > MAX_WORKSHEET_NAME_LENGTH Then
                    Dim endIndex = prefix.Length - (numberString.Length + prefix.Length - MAX_WORKSHEET_NAME_LENGTH)
                    prefix = prefix.Substring(0, endIndex)
                End If
                Dim newName = prefix & numberString
                If Not WorksheetExists(newName, workbook) Then
                    Return newName
                End If
                number += 1
            End While
        End Function

        ''' <summary>
        ''' Checks whether a worksheet with the given name exists
        ''' </summary>
        ''' <param name="name">Name to check.</param>
        ''' <param name="workbook">Workbook reference.</param>
        ''' <returns>True if the name exits, otherwise false.</returns>
        Private Shared Function WorksheetExists(name As String, workbook As Workbook) As Boolean
            If workbook Is Nothing Then
                Throw New WorksheetException("The workbook reference is null")
            End If
            Dim len = workbook.Worksheets.Count
            For i = 0 To len - 1
                If Equals(workbook.Worksheets(i).SheetName, name) Then
                    Return True
                End If
            Next
            Return False
        End Function
    End Class
End Namespace
