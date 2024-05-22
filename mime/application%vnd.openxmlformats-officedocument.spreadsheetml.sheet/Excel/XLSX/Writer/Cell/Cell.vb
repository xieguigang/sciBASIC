#Region "Microsoft.VisualBasic::d17a793e7afd5583d1e3012a8a965201, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\Writer\Cell\Cell.vb"

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

    '   Total Lines: 614
    '    Code Lines: 395 (64.33%)
    ' Comment Lines: 178 (28.99%)
    '    - Xml Docs: 96.07%
    ' 
    '   Blank Lines: 41 (6.68%)
    '     File Size: 28.85 KB


    '     Class Cell
    ' 
    '         Properties: CellAddress, CellAddress2, CellAddressType, CellStyle, ColumnNumber
    '                     DataType, RowNumber, Value
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: CompareTo, ConvertArray, Copy, GetAddressScope, (+4 Overloads) GetCellRange
    '                   ResolveCellAddress, ResolveCellCoordinate, ResolveCellRange, ResolveColumn, ResolveColumnAddress
    '                   SetStyle
    ' 
    '         Sub: RemoveStyle, (+2 Overloads) ResolveCellCoordinate, ResolveCellType, SetCellLockedState, ValidateColumnNumber
    '              ValidateRowNumber
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
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions

Namespace XLSX.Writer

    ''' <summary>
    ''' Class representing a cell of a worksheet
    ''' </summary>
    Public Class Cell : Implements IComparable(Of Cell)

        ''' <summary>
        ''' Defines the cellStyle
        ''' </summary>
        Private cellStyleField As Style

        ''' <summary>
        ''' Defines the columnNumber
        ''' </summary>
        Private columnNumberField As Integer

        ''' <summary>
        ''' Defines the rowNumber
        ''' </summary>
        Private rowNumberField As Integer

        ''' <summary>
        ''' Defines the value
        ''' </summary>
        Private valueField As Object

        ''' <summary>
        ''' Gets or sets the combined cell Address as string in the format A1 - XFD1048576
        ''' </summary>
        Public Property CellAddress As String
            Get
                Return ResolveCellAddress(ColumnNumber, RowNumber)
            End Get
            Set(value As String)
                Dim addressType As AddressType
                ResolveCellCoordinate(value, columnNumberField, rowNumberField, addressType)
                CellAddressType = addressType
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the CellAddress2
        ''' </summary>
        Public Property CellAddress2 As Address
            Get
                Return New Address(ColumnNumber, RowNumber, CellAddressType)
            End Get
            Set(value As Address)
                ColumnNumber = value.Column
                RowNumber = value.Row
                CellAddressType = value.Type
            End Set
        End Property

        ''' <summary>
        ''' Gets the assigned style of the cell
        ''' </summary>
        Public ReadOnly Property CellStyle As Style
            Get
                Return cellStyleField
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the ColumnNumber
        ''' </summary>
        Public Property ColumnNumber As Integer
            Get
                Return columnNumberField
            End Get
            Set(value As Integer)
                If value < Worksheet.MIN_COLUMN_NUMBER OrElse value > Worksheet.MAX_COLUMN_NUMBER Then
                    Throw New RangeException("OutOfRangeException", "The passed column number (" & value.ToString() & ") is out of range. Range is from " & Worksheet.MIN_COLUMN_NUMBER.ToString() & " to " & Worksheet.MAX_COLUMN_NUMBER.ToString() & " (" & (Worksheet.MAX_COLUMN_NUMBER + 1).ToString() & " rows).")
                End If
                columnNumberField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the DataType
        ''' </summary>
        Public Property DataType As CellType

        ''' <summary>
        ''' Gets or sets the RowNumber
        ''' </summary>
        Public Property RowNumber As Integer
            Get
                Return rowNumberField
            End Get
            Set(value As Integer)
                If value < Worksheet.MIN_ROW_NUMBER OrElse value > Worksheet.MAX_ROW_NUMBER Then
                    Throw New RangeException("OutOfRangeException", "The passed row number (" & value.ToString() & ") is out of range. Range is from " & Worksheet.MIN_ROW_NUMBER.ToString() & " to " & Worksheet.MAX_ROW_NUMBER.ToString() & " (" & (Worksheet.MAX_ROW_NUMBER + 1).ToString() & " rows).")
                End If
                rowNumberField = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the optional address type that can be part of the cell address.
        ''' </summary>
        Public Property CellAddressType As AddressType

        ''' <summary>
        ''' Gets or sets the Value
        ''' </summary>
        Public Property Value As Object
            Get
                Return valueField
            End Get
            Set(value As Object)
                valueField = value
                ResolveCellType()
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Cell"/> class
        ''' </summary>
        Public Sub New()
            DataType = CellType.DEFAULT
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Cell"/> class
        ''' </summary>
        ''' <param name="value">Value of the cell.</param>
        ''' <param name="type">Type of the cell.</param>
        Public Sub New(value As Object, type As CellType)
            If type = CellType.EMPTY Then
                valueField = Nothing
            Else
                valueField = value
            End If
            DataType = type
            If type = CellType.DEFAULT Then
                ResolveCellType()
            End If
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Cell"/> class
        ''' </summary>
        ''' <param name="value">Value of the cell.</param>
        ''' <param name="type">Type of the cell.</param>
        ''' <param name="address">Address of the cell.</param>
        Public Sub New(value As Object, type As CellType, address As String)
            If type = CellType.EMPTY Then
                valueField = Nothing
            Else
                valueField = value
            End If
            DataType = type
            CellAddress = address
            If type = CellType.DEFAULT Then
                ResolveCellType()
            End If
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Cell"/> class
        ''' </summary>
        ''' <param name="value">Value of the cell.</param>
        ''' <param name="type">Type of the cell.</param>
        ''' <param name="column">Column number of the cell (zero-based).</param>
        ''' <param name="row">Row number of the cell (zero-based).</param>
        Public Sub New(value As Object, type As CellType, column As Integer, row As Integer)
            Me.New(value, type)
            ColumnNumber = column
            RowNumber = row
            If type = CellType.DEFAULT Then
                ResolveCellType()
            End If
        End Sub

        ''' <summary>
        ''' Implemented CompareTo method
        ''' </summary>
        ''' <param name="other">Object to compare.</param>
        ''' <returns>0 if values are the same, -1 if this object is smaller, 1 if it is bigger.</returns>
        Public Function CompareTo(other As Cell) As Integer Implements IComparable(Of Cell).CompareTo
            If RowNumber = other.RowNumber Then
                Return ColumnNumber.CompareTo(other.ColumnNumber)
            End If
            Return RowNumber.CompareTo(other.RowNumber)
        End Function

        ''' <summary>
        ''' Removes the assigned style from the cell
        ''' </summary>
        Public Sub RemoveStyle()
            cellStyleField = Nothing
        End Sub

        ''' <summary>
        ''' Method resets the Cell type and tries to find the actual type. This is used if a Cell was created with the CellType DEFAULT or automatically if a value was set by <see cref="Value"/>. 
        ''' CellType FORMULA will skip this method and EMPTY will discard the value of the cell
        ''' </summary>
        Public Sub ResolveCellType()
            If valueField Is Nothing Then
                DataType = CellType.EMPTY
                valueField = Nothing
                Return
            End If
            If DataType = CellType.FORMULA Then
                Return
            End If
            Dim t As Type = valueField.GetType()
            If t Is GetType(Boolean) Then
                DataType = CellType.BOOL
            ElseIf t Is GetType(Byte) OrElse t Is GetType(SByte) Then
                DataType = CellType.NUMBER
            ElseIf t Is GetType(Decimal) Then
                DataType = CellType.NUMBER
            ElseIf t Is GetType(Double) Then
                DataType = CellType.NUMBER
            ElseIf t Is GetType(Single) Then
                DataType = CellType.NUMBER
            ElseIf t Is GetType(Integer) OrElse t Is GetType(UInteger) Then
                DataType = CellType.NUMBER
            ElseIf t Is GetType(Long) OrElse t Is GetType(ULong) Then
                DataType = CellType.NUMBER
            ElseIf t Is GetType(Short) OrElse t Is GetType(UShort) Then
                DataType = CellType.NUMBER
            ElseIf t Is GetType(Date) Then
                DataType = CellType.DATE ' Not native but standard
            ElseIf t Is GetType(TimeSpan) Then
                DataType = CellType.TIME ' Not native but standard
            Else
                DataType = CellType.STRING
            End If ' Default (char, string, object)
        End Sub

        ''' <summary>
        ''' Sets the lock state of the cell
        ''' </summary>
        ''' <param name="isLocked">If true, the cell will be locked if the worksheet is protected.</param>
        ''' <param name="isHidden">If true, the value of the cell will be invisible if the worksheet is protected.</param>
        Public Sub SetCellLockedState(isLocked As Boolean, isHidden As Boolean)
            Dim lockStyle As Style
            If cellStyleField Is Nothing Then
                lockStyle = New Style()
            Else
                lockStyle = cellStyleField.CopyStyle()
            End If
            lockStyle.CurrentCellXf.Locked = isLocked
            lockStyle.CurrentCellXf.Hidden = isHidden
            SetStyle(lockStyle)
        End Sub

        ''' <summary>
        ''' Sets the style of the cell
        ''' </summary>
        ''' <param name="style">Style to assign.</param>
        ''' <param name="unmanaged">Internally used: If true, the style repository is not invoked and only the style object of the cell is updated. Do not use!.</param>
        ''' <returns>If the passed style already exists in the repository, the existing one will be returned, otherwise the passed one.</returns>
        Public Function SetStyle(style As Style, Optional unmanaged As Boolean = False) As Style
            If style Is Nothing Then
                Throw New StyleException("A reference is missing in the style definition", "No style to assign was defined")
            End If
            cellStyleField = If(unmanaged, style, StyleRepository.Instance.AddStyle(style))
            Return cellStyleField
        End Function

        ''' <summary>
        ''' Copies this cell into a new one. The style is considered if not null
        ''' </summary>
        ''' <returns>Copy of this cell.</returns>
        Friend Function Copy() As Cell
            Dim lCopy As Cell = New Cell()
            lCopy.valueField = valueField
            lCopy.DataType = DataType
            lCopy.CellAddress = CellAddress
            lCopy.CellAddressType = CellAddressType
            If cellStyleField IsNot Nothing Then
                lCopy.SetStyle(cellStyleField, True)
            End If
            Return lCopy
        End Function

        ''' <summary>
        ''' Converts a List of supported objects into a list of cells
        ''' </summary>
        ''' <typeparam name="tT">Generic data type.</typeparam>
        ''' <param name="list">List of generic objects.</param>
        ''' <returns>List of cells.</returns>
        Public Shared Function ConvertArray(Of tT)(list As IEnumerable(Of tT)) As IEnumerable(Of Cell)
            Dim output As List(Of Cell) = New List(Of Cell)()
            Dim c As Cell
            Dim o As Object
            Dim t As Type
            For Each item In list
                If item Is Nothing Then
                    c = New Cell(Nothing, CellType.EMPTY)
                    output.Add(c)
                    Continue For
                End If
                o = item ' intermediate object is necessary to cast the types below
                t = item.GetType()
                If t Is GetType(Cell) Then
                    c = TryCast(item, Cell)
                ElseIf t Is GetType(Boolean) Then
                    c = New Cell(CBool(o), CellType.BOOL)
                ElseIf t Is GetType(Byte) Then
                    c = New Cell(CByte(o), CellType.NUMBER)
                ElseIf t Is GetType(SByte) Then
                    c = New Cell(CSByte(o), CellType.NUMBER)
                ElseIf t Is GetType(Decimal) Then
                    c = New Cell(CDec(o), CellType.NUMBER)
                ElseIf t Is GetType(Double) Then
                    c = New Cell(CDbl(o), CellType.NUMBER)
                ElseIf t Is GetType(Single) Then
                    c = New Cell(CSng(o), CellType.NUMBER)
                ElseIf t Is GetType(Integer) Then
                    c = New Cell(CInt(o), CellType.NUMBER)
                ElseIf t Is GetType(UInteger) Then
                    c = New Cell(CUInt(o), CellType.NUMBER)
                ElseIf t Is GetType(Long) Then
                    c = New Cell(CLng(o), CellType.NUMBER)
                ElseIf t Is GetType(ULong) Then
                    c = New Cell(CULng(o), CellType.NUMBER)
                ElseIf t Is GetType(Short) Then
                    c = New Cell(CShort(o), CellType.NUMBER)
                ElseIf t Is GetType(UShort) Then
                    c = New Cell(CUShort(o), CellType.NUMBER)
                ElseIf t Is GetType(Date) Then
                    c = New Cell(CDate(o), CellType.DATE)
                    c.SetStyle(Style.BasicStyles.DateFormat)
                ElseIf t Is GetType(TimeSpan) Then
                    c = New Cell(CType(o, TimeSpan), CellType.TIME)
                    c.SetStyle(Style.BasicStyles.TimeFormat)
                ElseIf t Is GetType(String) Then
                    c = New Cell(CStr(o), CellType.STRING) ' Default = unspecified object
                Else
                    c = New Cell(o.ToString(), CellType.DEFAULT)
                End If
                output.Add(c)
            Next
            Return output
        End Function

        ''' <summary>
        ''' Gets a list of cell addresses from a cell range (format A1:B3 or AAD556:AAD1000)
        ''' </summary>
        ''' <param name="range">Range to process.</param>
        ''' <returns>List of cell addresses.</returns>
        Public Shared Function GetCellRange(range As String) As IEnumerable(Of Address)
            Dim range2 = ResolveCellRange(range)
            Return GetCellRange(range2.StartAddress, range2.EndAddress)
        End Function

        ''' <summary>
        ''' Get a list of cell addresses from a cell range
        ''' </summary>
        ''' <param name="startAddress">Start address as string in the format A1 - XFD1048576.</param>
        ''' <param name="endAddress">End address as string in the format A1 - XFD1048576.</param>
        ''' <returns>List of cell addresses.</returns>
        Public Shared Function GetCellRange(startAddress As String, endAddress As String) As IEnumerable(Of Address)
            Dim start = ResolveCellCoordinate(startAddress)
            Dim [end] = ResolveCellCoordinate(endAddress)
            Return GetCellRange(start, [end])
        End Function

        ''' <summary>
        ''' Get a list of cell addresses from a cell range
        ''' </summary>
        ''' <param name="startColumn">Start column (zero based).</param>
        ''' <param name="startRow">Start row (zero based).</param>
        ''' <param name="endColumn">End column (zero based).</param>
        ''' <param name="endRow">End row (zero based).</param>
        ''' <returns>List of cell addresses.</returns>
        Public Shared Function GetCellRange(startColumn As Integer, startRow As Integer, endColumn As Integer, endRow As Integer) As IEnumerable(Of Address)
            Dim start As Address = New Address(startColumn, startRow)
            Dim [end] As Address = New Address(endColumn, endRow)
            Return GetCellRange(start, [end])
        End Function

        ''' <summary>
        ''' Get a list of cell addresses from a cell range
        ''' </summary>
        ''' <param name="startAddress">Start address.</param>
        ''' <param name="endAddress">End address.</param>
        ''' <returns>List of cell addresses.</returns>
        Public Shared Function GetCellRange(startAddress As Address, endAddress As Address) As IEnumerable(Of Address)
            Dim startColumn, endColumn, startRow, endRow As Integer
            If startAddress.Column < endAddress.Column Then
                startColumn = startAddress.Column
                endColumn = endAddress.Column
            Else
                startColumn = endAddress.Column
                endColumn = startAddress.Column
            End If
            If startAddress.Row < endAddress.Row Then
                startRow = startAddress.Row
                endRow = endAddress.Row
            Else
                startRow = endAddress.Row
                endRow = startAddress.Row
            End If
            Dim output As List(Of Address) = New List(Of Address)()
            For column = startColumn To endColumn
                For row = startRow To endRow
                    output.Add(New Address(column, row))
                Next
            Next
            Return output
        End Function

        ''' <summary>
        ''' Gets the address of a cell by the column and row number (zero based)
        ''' </summary>
        ''' <param name="column">Column number of the cell (zero-based).</param>
        ''' <param name="row">Row number of the cell (zero-based).</param>
        ''' <param name="type">Optional referencing type of the address.</param>
        ''' <returns>Cell Address as string in the format A1 - XFD1048576. Depending on the type, Addresses like '$A55', 'B$2' or '$A$5' are possible outputs.</returns>
        Public Shared Function ResolveCellAddress(column As Integer, row As Integer, Optional type As AddressType = AddressType.Default) As String
            If column > Worksheet.MAX_COLUMN_NUMBER OrElse column < Worksheet.MIN_COLUMN_NUMBER Then
                Throw New RangeException("OutOfRangeException", "The column number (" & column.ToString() & ") is out of range. Range is from " & Worksheet.MIN_COLUMN_NUMBER.ToString() & " to " & Worksheet.MAX_COLUMN_NUMBER.ToString() & " (" & (Worksheet.MAX_COLUMN_NUMBER + 1).ToString() & " columns).")
            End If
            Select Case type
                Case AddressType.FixedRowAndColumn
                    Return "$" & ResolveColumnAddress(column) & "$" & (row + 1).ToString()
                Case AddressType.FixedColumn
                    Return "$" & ResolveColumnAddress(column) & (row + 1).ToString()
                Case AddressType.FixedRow
                    Return ResolveColumnAddress(column) & "$" & (row + 1).ToString()
                Case Else
                    Return ResolveColumnAddress(column) & (row + 1).ToString()
            End Select
        End Function

        ''' <summary>
        ''' Gets the column and row number (zero based) of a cell by the address
        ''' </summary>
        ''' <param name="address">Address as string in the format A1 - XFD1048576.</param>
        ''' <returns>Struct with row and column.</returns>
        Public Shared Function ResolveCellCoordinate(address As String) As Address
            Dim row, column As Integer
            Dim type As AddressType
            ResolveCellCoordinate(address, column, row, type)
            Return New Address(column, row, type)
        End Function

        ''' <summary>
        ''' Gets the column and row number (zero based) of a cell by the address
        ''' </summary>
        ''' <param name="address">Address as string in the format A1 - XFD1048576.</param>
        ''' <param name="column">Column number of the cell (zero-based) as out parameter.</param>
        ''' <param name="row">Row number of the cell (zero-based) as out parameter.</param>
        Public Shared Sub ResolveCellCoordinate(address As String, <Out> ByRef column As Integer, <Out> ByRef row As Integer)
            Dim dummy As AddressType
            ResolveCellCoordinate(address, column, row, dummy)
        End Sub

        ''' <summary>
        ''' Gets the column and row number (zero based) of a cell by the address
        ''' </summary>
        ''' <param name="address">Address as string in the format A1 - XFD1048576.</param>
        ''' <param name="column">Column number of the cell (zero-based) as out parameter.</param>
        ''' <param name="row">Row number of the cell (zero-based) as out parameter.</param>
        ''' <param name="addressType">Address type of the cell (if defined as modifiers in the address string).</param>
        Public Shared Sub ResolveCellCoordinate(address As String, <Out> ByRef column As Integer, <Out> ByRef row As Integer, <Out> ByRef addressType As AddressType)
            If String.IsNullOrEmpty(address) Then
                Throw New FormatException("The cell address is null or empty and could not be resolved")
            End If
            address = address.ToUpper()
            Dim pattern As Regex = New Regex("(^(\$?)([A-Z]{1,3})(\$?)([0-9]{1,7})$)")
            Dim matcher = pattern.Match(address)
            If matcher.Groups.Count <> 6 Then
                Throw New FormatException("The format of the cell address (" & address & ") is malformed")
            End If
            Dim digits = Integer.Parse(matcher.Groups(5).Value, CultureInfo.InvariantCulture)
            column = ResolveColumn(matcher.Groups(3).Value)
            row = digits - 1
            ValidateRowNumber(row)
            If Not String.IsNullOrEmpty(matcher.Groups(2).Value) AndAlso Not String.IsNullOrEmpty(matcher.Groups(4).Value) Then
                addressType = AddressType.FixedRowAndColumn
            ElseIf Not String.IsNullOrEmpty(matcher.Groups(2).Value) AndAlso String.IsNullOrEmpty(matcher.Groups(4).Value) Then
                addressType = AddressType.FixedColumn
            ElseIf String.IsNullOrEmpty(matcher.Groups(2).Value) AndAlso Not String.IsNullOrEmpty(matcher.Groups(4).Value) Then
                addressType = AddressType.FixedRow
            Else
                addressType = AddressType.Default
            End If
        End Sub

        ''' <summary>
        ''' Resolves a cell range from the format like A1:B3 or AAD556:AAD1000
        ''' </summary>
        ''' <param name="range">Range to process.</param>
        ''' <returns>Range object.</returns>
        Public Shared Function ResolveCellRange(range As String) As Range
            If String.IsNullOrEmpty(range) Then
                Throw New FormatException("The cell range is null or empty and could not be resolved")
            End If
            Dim split = range.Split(":"c)
            If split.Length <> 2 Then
                Throw New FormatException("The cell range (" & range & ") is malformed and could not be resolved")
            End If
            Return New Range(ResolveCellCoordinate(split(0)), ResolveCellCoordinate(split(1)))
        End Function

        ''' <summary>
        ''' Gets the column number from the column address (A - XFD)
        ''' </summary>
        ''' <param name="columnAddress">Column address (A - XFD).</param>
        ''' <returns>Column number (zero-based).</returns>
        Public Shared Function ResolveColumn(columnAddress As String) As Integer
            If String.IsNullOrEmpty(columnAddress) Then
                Throw New RangeException("A general range exception occurred", "The passed address was null or empty")
            End If
            columnAddress = columnAddress.ToUpper()
            Dim chr As Integer
            Dim result = 0
            Dim multiplier = 1
            For i = columnAddress.Length - 1 To 0 Step -1
                chr = AscW(columnAddress(i))
                chr -= 64
                result += chr * multiplier
                multiplier *= 26
            Next
            ValidateColumnNumber(result - 1)
            Return result - 1
        End Function

        ''' <summary>
        ''' Gets the column address (A - XFD)
        ''' </summary>
        ''' <param name="columnNumber">Column number (zero-based).</param>
        ''' <returns>Column address (A - XFD).</returns>
        Public Shared Function ResolveColumnAddress(columnNumber As Integer) As String
            If columnNumber > Worksheet.MAX_COLUMN_NUMBER OrElse columnNumber < Worksheet.MIN_COLUMN_NUMBER Then
                Throw New RangeException("OutOfRangeException", "The column number (" & columnNumber.ToString() & ") is out of range. Range is from " & Worksheet.MIN_COLUMN_NUMBER.ToString() & " to " & Worksheet.MAX_COLUMN_NUMBER.ToString() & " (" & (Worksheet.MAX_COLUMN_NUMBER + 1).ToString() & " columns).")
            End If
            ' A - XFD
            Dim j = 0
            Dim k = 0
            Dim l = 0
            Dim sb As StringBuilder = New StringBuilder()
            For i = 0 To columnNumber
                If j > 25 Then
                    k += 1
                    j = 0
                End If
                If k > 25 Then
                    l += 1
                    k = 0
                End If
                j += 1
            Next
            If l > 0 Then
                sb.Append(Microsoft.VisualBasic.ChrW(l + 64))
            End If
            If k > 0 Then
                sb.Append(Microsoft.VisualBasic.ChrW(k + 64))
            End If
            sb.Append(Microsoft.VisualBasic.ChrW(j + 64))
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Gets the scope of the passed address (string expression). Scope means either single cell address or range
        ''' </summary>
        ''' <param name="addressExpression">Address expression.</param>
        ''' <returns>Scope of the address expression.</returns>
        Public Shared Function GetAddressScope(addressExpression As String) As AddressScope
            Try
                ResolveCellCoordinate(addressExpression)
                Return AddressScope.SingleAddress
            Catch
                Try
                    ResolveCellRange(addressExpression)
                    Return AddressScope.Range
                Catch
                    Return AddressScope.Invalid
                End Try
            End Try
        End Function

        ''' <summary>
        ''' Validates the passed (zero-based) column number. an exception will be thrown if the column is invalid
        ''' </summary>
        ''' <param name="column">Number to check.</param>
        Public Shared Sub ValidateColumnNumber(column As Integer)
            If column > Worksheet.MAX_COLUMN_NUMBER OrElse column < Worksheet.MIN_COLUMN_NUMBER Then
                Throw New RangeException("A general range exception occurred", "The column number (" & column.ToString() & ") is out of range. Range is from " & Worksheet.MIN_COLUMN_NUMBER.ToString() & " to " & Worksheet.MAX_COLUMN_NUMBER.ToString() & " (" & (Worksheet.MAX_COLUMN_NUMBER + 1).ToString() & " columns).")
            End If
        End Sub

        ''' <summary>
        ''' Validates the passed (zero-based) row number. an exception will be thrown if the row is invalid
        ''' </summary>
        ''' <param name="row">Number to check.</param>
        Public Shared Sub ValidateRowNumber(row As Integer)
            If row > Worksheet.MAX_ROW_NUMBER OrElse row < Worksheet.MIN_ROW_NUMBER Then
                Throw New RangeException("A general range exception occurred", "The row number (" & row.ToString() & ") is out of range. Range is from " & Worksheet.MIN_ROW_NUMBER.ToString() & " to " & Worksheet.MAX_ROW_NUMBER.ToString() & " (" & (Worksheet.MAX_ROW_NUMBER + 1).ToString() & " rows).")
            End If
        End Sub

    End Class
End Namespace
