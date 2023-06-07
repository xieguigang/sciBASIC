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
    Public Class Cell
        Implements IComparable(Of Cell)
        ''' <summary>
        ''' Enum defines the basic data types of a cell
        ''' </summary>
        Public Enum CellType
            ''' <summary>Type for single characters and strings</summary>
            [STRING]
            ''' <summary>Type for all numeric types (long, integer, float, double, short, byte and decimal; signed and unsigned, if available)</summary>
            NUMBER
            ''' <summary>Type for dates (Note: Dates before 1900-01-01 and after 9999-12-31 are not allowed)</summary>
            [DATE]
            ''' <summary>Type for times (Note: Internally handled as OAdate, represented by <see cref="TimeSpan"/>)</summary>
            TIME
            ''' <summary>Type for boolean</summary>
            BOOL
            ''' <summary>Type for Formulas (The cell will be handled differently)</summary>
            FORMULA
            ''' <summary>Type for empty cells. This type is only used for merged cells (all cells except the first of the cell range)</summary>
            EMPTY
            ''' <summary>Default Type, not specified</summary>
            [DEFAULT]
        End Enum

        ''' <summary>
        ''' Enum for the referencing style of the address
        ''' </summary>
        Public Enum AddressType
            ''' <summary>Default behavior (e.g. 'C3')</summary>
            [Default]
            ''' <summary>Row of the address is fixed (e.g. 'C$3')</summary>
            FixedRow
            ''' <summary>Column of the address is fixed (e.g. '$C3')</summary>
            FixedColumn
            ''' <summary>Row and column of the address is fixed (e.g. '$C$3')</summary>
            FixedRowAndColumn
        End Enum

        ''' <summary>
        ''' Enum to define the scope of a passed address string (used in static context)
        ''' </summary>
        Public Enum AddressScope
            ''' <summary>The address represents a single cell</summary>
            SingleAddress
            ''' <summary>The address represents a range of cells</summary>
            Range
            ''' <summary>The address expression is invalid</summary>
            Invalid
        End Enum

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

        ''' <summary>
        ''' Struct representing the cell address as column and row (zero based)
        ''' </summary>
        Public Structure Address
            Implements IEquatable(Of Address), IComparable(Of Address)
            ''' <summary>
            ''' Column number (zero based)
            ''' </summary>
            Public Column As Integer

            ''' <summary>
            ''' Row number (zero based)
            ''' </summary>
            Public Row As Integer

            ''' <summary>
            ''' Referencing type of the address
            ''' </summary>
            Public Type As AddressType

            ''' <summary>
            ''' Initializes a new instance of the <see cref=""/> class
            ''' </summary>
            ''' <param name="column">Column number (zero based).</param>
            ''' <param name="row">Row number (zero based).</param>
            ''' <param name="type">Optional referencing type of the address.</param>
            Public Sub New(column As Integer, row As Integer, Optional type As AddressType = AddressType.Default)
                Me.Column = column
                Me.Row = row
                Me.Type = type
            End Sub

            ''' <summary>
            ''' Initializes a new instance of the <see cref=""/> class
            ''' </summary>
            ''' <param name="address">Address string (e.g. 'A1:B12').</param>
            ''' <param name="type">Optional referencing type of the address.</param>
            Public Sub New(address As String, Optional type As AddressType = AddressType.Default)
                Me.Type = type
                ResolveCellCoordinate(address, Column, Row, type)
            End Sub

            ''' <summary>
            ''' Returns the combined Address
            ''' </summary>
            ''' <returns>Address as string in the format A1 - XFD1048576.</returns>
            Public Function GetAddress() As String
                Return ResolveCellAddress(Column, Row, Type)
            End Function

            ''' <summary>
            ''' Gets the column address (A - XFD)
            ''' </summary>
            ''' <returns>Column address as letter(s).</returns>
            Public Function GetColumn() As String
                Return ResolveColumnAddress(Column)
            End Function

            ''' <summary>
            ''' Overwritten ToString method
            ''' </summary>
            ''' <returns>Returns the cell address (e.g. 'A15').</returns>
            Public Overrides Function ToString() As String
                Return GetAddress()
            End Function

            ''' <summary>
            ''' Compares two addresses whether they are equal
            ''' </summary>
            ''' <param name="o"> Other address.</param>
            ''' <returns>True if equal.</returns>
            Public Overloads Function Equals(o As Address) As Boolean Implements IEquatable(Of Address).Equals
                If Row = o.Row AndAlso Column = o.Column Then
                    Return True
                Else
                    Return False
                End If
            End Function

            ''' <summary>
            ''' Compares two objects whether they are addresses and equal
            ''' </summary>
            ''' <param name="obj"> Other address.</param>
            ''' <returns>True if not null, of the same type and equal.</returns>
            Public Overrides Function Equals(obj As Object) As Boolean
                If Not (TypeOf obj Is Address) Then
                    Return False
                End If
                Return Equals(CType(obj, Address))
            End Function

            ''' <summary>
            ''' Gets the hash code based on the string representation of the address
            ''' </summary>
            ''' <returns>Hash code of the address.</returns>
            Public Overrides Function GetHashCode() As Integer
                Return ToString().GetHashCode()
            End Function


            ' Operator overloads
            Public Shared Operator =(address1 As Address, address2 As Address) As Boolean
                Return address1.Equals(address2)
            End Operator

            Public Shared Operator <>(address1 As Address, address2 As Address) As Boolean
                Return Not address1.Equals(address2)
            End Operator
            ''' <summary>
            ''' Compares two addresses using the column and row numbers
            ''' </summary>
            ''' <param name="other"> Other address.</param>
            ''' <returns>-1 if the other address is greater, 0 if equal and 1 if smaller.</returns>
            Public Function CompareTo(other As Address) As Integer Implements IComparable(Of Address).CompareTo
                Dim thisCoordinate = Column * CLng(Worksheet.MAX_ROW_NUMBER) + Row
                Dim otherCoordinate = other.Column * CLng(Worksheet.MAX_ROW_NUMBER) + other.Row
                Return thisCoordinate.CompareTo(otherCoordinate)
            End Function

            ''' <summary>
            ''' Creates a (dereferenced, if applicable) deep copy of this address
            ''' </summary>
            ''' <returns>Copy of this range.</returns>
            Friend Function Copy() As Address
                Return New Address(Column, Row, Type)
            End Function
        End Structure

        ''' <summary>
        ''' Struct representing a cell range with a start and end address
        ''' </summary>
        Public Structure Range
            ''' <summary>
            ''' End address of the range
            ''' </summary>
            Public EndAddress As Address

            ''' <summary>
            ''' Start address of the range
            ''' </summary>
            Public StartAddress As Address

            ''' <summary>
            ''' Initializes a new instance of the <see cref=""/> class
            ''' </summary>
            ''' <param name="start">Start address of the range.</param>
            ''' <param name="end">End address of the range.</param>
            Public Sub New(start As Address, [end] As Address)
                If start.CompareTo([end]) < 0 Then
                    StartAddress = start
                    EndAddress = [end]
                Else
                    StartAddress = [end]
                    EndAddress = start
                End If
            End Sub

            ''' <summary>
            ''' Initializes a new instance of the <see cref=""/> class
            ''' </summary>
            ''' <param name="range">Address range (e.g. 'A1:B12').</param>
            Public Sub New(range As String)
                Dim r = ResolveCellRange(range)
                If r.StartAddress.CompareTo(r.EndAddress) < 0 Then
                    StartAddress = r.StartAddress
                    EndAddress = r.EndAddress
                Else
                    StartAddress = r.EndAddress
                    EndAddress = r.StartAddress
                End If
            End Sub

            ''' <summary>
            ''' Gets a list of all addresses between the start and end address
            ''' </summary>
            ''' <returns>List of Addresses.</returns>
            Public Function ResolveEnclosedAddresses() As IReadOnlyList(Of Address)
                Dim startColumn, endColumn, startRow, endRow As Integer
                If StartAddress.Column <= EndAddress.Column Then
                    startColumn = StartAddress.Column
                    endColumn = EndAddress.Column
                Else
                    endColumn = StartAddress.Column
                    startColumn = EndAddress.Column
                End If
                If StartAddress.Row <= EndAddress.Row Then
                    startRow = StartAddress.Row
                    endRow = EndAddress.Row
                Else
                    endRow = StartAddress.Row
                    startRow = EndAddress.Row
                End If
                Dim addresses As List(Of Address) = New List(Of Address)()
                For c = startColumn To endColumn
                    For r = startRow To endRow
                        addresses.Add(New Address(c, r))
                    Next
                Next
                Return addresses
            End Function

            ''' <summary>
            ''' Overwritten ToString method
            ''' </summary>
            ''' <returns>Returns the range (e.g. 'A1:B12').</returns>
            Public Overrides Function ToString() As String
                Return StartAddress.ToString() & ":" & EndAddress.ToString()
            End Function

            ''' <summary>
            ''' Creates a (dereferenced, if applicable) deep copy of this range
            ''' </summary>
            ''' <returns>Copy of this range.</returns>
            Friend Function Copy() As Range
                Return New Range(StartAddress.Copy(), EndAddress.Copy())
            End Function

            ''' <summary>
            ''' Compares two objects whether they are ranges and equal. The cell types (possible $ prefix) are considered
            ''' </summary>
            ''' <param name="obj">Other object to compare.</param>
            ''' <returns>True if the two objects are the same range.</returns>
            Public Overrides Function Equals(obj As Object) As Boolean
                If Not (TypeOf obj Is Range) Then
                    Return False
                End If
                Dim other As Range = obj
                Return StartAddress.Equals(other.StartAddress) AndAlso EndAddress.Equals(other.EndAddress)
            End Function

            ''' <summary>
            ''' Gets the hash code of the range object according to its string representation
            ''' </summary>
            ''' <returns>Hash code of the range.</returns>
            Public Overrides Function GetHashCode() As Integer
                Return ToString().GetHashCode()
            End Function


            ' Operator overloads
            Public Shared Operator =(range1 As Range, range2 As Range) As Boolean
                Return range1.Equals(range2)
            End Operator

            Public Shared Operator <>(range1 As Range, range2 As Range) As Boolean
                Return Not range1.Equals(range2)
            End Operator
        End Structure

        ''' <summary>
        ''' Class for handling of basic Excel formulas
        ''' </summary>
        Public NotInheritable Class BasicFormulas
            ''' <summary>
            ''' Returns a cell with a average formula
            ''' </summary>
            ''' <param name="range">Cell range to apply the average operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Average(range As Range) As Cell
                Return Average(Nothing, range)
            End Function

            ''' <summary>
            ''' Returns a cell with a average formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the average operation. Can be null if on the same worksheet.</param>
            ''' <param name="range">Cell range to apply the average operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Average(target As Worksheet, range As Range) As Cell
                Return GetBasicFormula(target, range, "AVERAGE", Nothing)
            End Function

            ''' <summary>
            ''' Returns a cell with a ceil formula
            ''' </summary>
            ''' <param name="address">Address to apply the ceil operation to.</param>
            ''' <param name="decimals">Number of decimals (digits).</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Ceil(address As Address, decimals As Integer) As Cell
                Return Ceil(Nothing, address, decimals)
            End Function

            ''' <summary>
            ''' Returns a cell with a ceil formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the ceil operation. Can be null if on the same worksheet.</param>
            ''' <param name="address">Address to apply the ceil operation to.</param>
            ''' <param name="decimals">Number of decimals (digits).</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Ceil(target As Worksheet, address As Address, decimals As Integer) As Cell
                Return GetBasicFormula(target, New Range(address, address), "ROUNDUP", decimals.ToString(CultureInfo.InvariantCulture))
            End Function

            ''' <summary>
            ''' Returns a cell with a floor formula
            ''' </summary>
            ''' <param name="address">Address to apply the floor operation to.</param>
            ''' <param name="decimals">Number of decimals (digits).</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Floor(address As Address, decimals As Integer) As Cell
                Return Floor(Nothing, address, decimals)
            End Function

            ''' <summary>
            ''' Returns a cell with a floor formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the floor operation. Can be null if on the same worksheet.</param>
            ''' <param name="address">Address to apply the floor operation to.</param>
            ''' <param name="decimals">Number of decimals (digits).</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Floor(target As Worksheet, address As Address, decimals As Integer) As Cell
                Return GetBasicFormula(target, New Range(address, address), "ROUNDDOWN", decimals.ToString(CultureInfo.InvariantCulture))
            End Function

            ''' <summary>
            ''' Returns a cell with a max formula
            ''' </summary>
            ''' <param name="range">Cell range to apply the max operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Max(range As Range) As Cell
                Return Max(Nothing, range)
            End Function

            ''' <summary>
            ''' Returns a cell with a max formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the max operation. Can be null if on the same worksheet.</param>
            ''' <param name="range">Cell range to apply the max operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Max(target As Worksheet, range As Range) As Cell
                Return GetBasicFormula(target, range, "MAX", Nothing)
            End Function

            ''' <summary>
            ''' Returns a cell with a median formula
            ''' </summary>
            ''' <param name="range">Cell range to apply the median operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Median(range As Range) As Cell
                Return Median(Nothing, range)
            End Function

            ''' <summary>
            ''' Returns a cell with a median formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the median operation. Can be null if on the same worksheet.</param>
            ''' <param name="range">Cell range to apply the median operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Median(target As Worksheet, range As Range) As Cell
                Return GetBasicFormula(target, range, "MEDIAN", Nothing)
            End Function

            ''' <summary>
            ''' Returns a cell with a min formula
            ''' </summary>
            ''' <param name="range">Cell range to apply the min operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Min(range As Range) As Cell
                Return Min(Nothing, range)
            End Function

            ''' <summary>
            ''' Returns a cell with a min formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the min operation. Can be null if on the same worksheet.</param>
            ''' <param name="range">Cell range to apply the median operation to.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Min(target As Worksheet, range As Range) As Cell
                Return GetBasicFormula(target, range, "MIN", Nothing)
            End Function

            ''' <summary>
            ''' Returns a cell with a round formula
            ''' </summary>
            ''' <param name="address">Address to apply the round operation to.</param>
            ''' <param name="decimals">Number of decimals (digits).</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Round(address As Address, decimals As Integer) As Cell
                Return Round(Nothing, address, decimals)
            End Function

            ''' <summary>
            ''' Returns a cell with a round formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the round operation. Can be null if on the same worksheet.</param>
            ''' <param name="address">Address to apply the round operation to.</param>
            ''' <param name="decimals">Number of decimals (digits).</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Round(target As Worksheet, address As Address, decimals As Integer) As Cell
                Return GetBasicFormula(target, New Range(address, address), "ROUND", decimals.ToString(CultureInfo.InvariantCulture))
            End Function

            ''' <summary>
            ''' Returns a cell with a sum formula
            ''' </summary>
            ''' <param name="range">Cell range to get a sum of.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Sum(range As Range) As Cell
                Return Sum(Nothing, range)
            End Function

            ''' <summary>
            ''' Returns a cell with a sum formula
            ''' </summary>
            ''' <param name="target">Target worksheet of the sum operation. Can be null if on the same worksheet.</param>
            ''' <param name="range">Cell range to get a sum of.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function Sum(target As Worksheet, range As Range) As Cell
                Return GetBasicFormula(target, range, "SUM", Nothing)
            End Function

            ''' <summary>
            ''' Function to generate a Vlookup as Excel function
            ''' </summary>
            ''' <param name="number">Numeric value for the lookup. Valid types are int, uint, long, ulong, float, double, byte, sbyte, decimal, short and ushort.</param>
            ''' <param name="range">Matrix of the lookup.</param>
            ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
            ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function VLookup(number As Object, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
                Return VLookup(number, Nothing, range, columnIndex, exactMatch)
            End Function

            ''' <summary>
            ''' Function to generate a Vlookup as Excel function
            ''' </summary>
            ''' <param name="number">Numeric value for the lookup.Valid types are int, uint, long, ulong, float, double, byte, sbyte, decimal, short and ushort.</param>
            ''' <param name="rangeTarget">Target worksheet of the matrix. Can be null if on the same worksheet.</param>
            ''' <param name="range">Matrix of the lookup.</param>
            ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
            ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function VLookup(number As Object, rangeTarget As Worksheet, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
                Return GetVLookup(Nothing, New Address(), number, rangeTarget, range, columnIndex, exactMatch, True)
            End Function

            ''' <summary>
            ''' Function to generate a Vlookup as Excel function
            ''' </summary>
            ''' <param name="address">Query address of a cell as string as source of the lookup.</param>
            ''' <param name="range">Matrix of the lookup.</param>
            ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
            ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function VLookup(address As Address, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
                Return VLookup(Nothing, address, Nothing, range, columnIndex, exactMatch)
            End Function

            ''' <summary>
            ''' Function to generate a Vlookup as Excel function
            ''' </summary>
            ''' <param name="queryTarget">Target worksheet of the query argument. Can be null if on the same worksheet.</param>
            ''' <param name="address">Query address of a cell as string as source of the lookup.</param>
            ''' <param name="rangeTarget">Target worksheet of the matrix. Can be null if on the same worksheet.</param>
            ''' <param name="range">Matrix of the lookup.</param>
            ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
            ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Public Shared Function VLookup(queryTarget As Worksheet, address As Address, rangeTarget As Worksheet, range As Range, columnIndex As Integer, exactMatch As Boolean) As Cell
                Return GetVLookup(queryTarget, address, 0, rangeTarget, range, columnIndex, exactMatch, False)
            End Function

            ''' <summary>
            ''' Function to generate a Vlookup as Excel function
            ''' </summary>
            ''' <param name="queryTarget">Target worksheet of the query argument. Can be null if on the same worksheet.</param>
            ''' <param name="address">In case of a reference lookup, query address of a cell as string.</param>
            ''' <param name="number">In case of a numeric lookup, number for the lookup.</param>
            ''' <param name="rangeTarget">Target worksheet of the matrix. Can be null if on the same worksheet.</param>
            ''' <param name="range">Matrix of the lookup.</param>
            ''' <param name="columnIndex">Column index of the target column within the range (1 based).</param>
            ''' <param name="exactMatch">If true, an exact match is applied to the lookup.</param>
            ''' <param name="numericLookup">If true, the lookup is a numeric lookup, otherwise a reference lookup.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Private Shared Function GetVLookup(queryTarget As Worksheet, address As Address, number As Object, rangeTarget As Worksheet, range As Range, columnIndex As Integer, exactMatch As Boolean, numericLookup As Boolean) As Cell
                Dim rangeWidth = range.EndAddress.Column - range.StartAddress.Column + 1
                If columnIndex < 1 OrElse columnIndex > rangeWidth Then
                    Throw New FormatException("The column index on range " & range.ToString() & " can only be between 1 and " & rangeWidth.ToString())
                End If
                Dim culture = CultureInfo.InvariantCulture
                Dim arg1, arg2, arg3, arg4 As String
                If numericLookup Then
                    If number Is Nothing Then
                        Throw New FormatException("The lookup variable can only be a cell address or a numeric value. The passed value was null.")
                    End If
                    Dim t As Type = number.GetType()
                    If t Is GetType(Byte) Then
                        arg1 = CByte(number).ToString("G", culture)
                    ElseIf t Is GetType(SByte) Then
                        arg1 = CSByte(number).ToString("G", culture)
                    ElseIf t Is GetType(Decimal) Then
                        arg1 = CDec(number).ToString("G", culture)
                    ElseIf t Is GetType(Double) Then
                        arg1 = CDbl(number).ToString("G", culture)
                    ElseIf t Is GetType(Single) Then
                        arg1 = CSng(number).ToString("G", culture)
                    ElseIf t Is GetType(Integer) Then
                        arg1 = CInt(number).ToString("G", culture)
                    ElseIf t Is GetType(UInteger) Then
                        arg1 = CUInt(number).ToString("G", culture)
                    ElseIf t Is GetType(Long) Then
                        arg1 = CLng(number).ToString("G", culture)
                    ElseIf t Is GetType(ULong) Then
                        arg1 = CULng(number).ToString("G", culture)
                    ElseIf t Is GetType(Short) Then
                        arg1 = CShort(number).ToString("G", culture)
                    ElseIf t Is GetType(UShort) Then
                        arg1 = CUShort(number).ToString("G", culture)
                    Else
                        Throw New FormatException("The lookup variable can only be a cell address or a numeric value. The value '" & number.ToString() & "' is invalid.")
                    End If
                Else
                    If queryTarget IsNot Nothing Then
                        arg1 = queryTarget.SheetName & "!" & address.ToString()
                    Else
                        arg1 = address.ToString()
                    End If
                End If
                If rangeTarget IsNot Nothing Then
                    arg2 = rangeTarget.SheetName & "!" & range.ToString()
                Else
                    arg2 = range.ToString()
                End If
                arg3 = columnIndex.ToString("G", culture)
                If exactMatch Then
                    arg4 = "TRUE"
                Else
                    arg4 = "FALSE"
                End If
                Return New Cell("VLOOKUP(" & arg1 & "," & arg2 & "," & arg3 & "," & arg4 & ")", CellType.FORMULA)
            End Function

            ''' <summary>
            ''' Function to generate a basic Excel function with one cell range as parameter and an optional post argument
            ''' </summary>
            ''' <param name="target">Target worksheet of the cell reference. Can be null if on the same worksheet.</param>
            ''' <param name="range">Main argument as cell range. If applied on one cell, the start and end address are identical.</param>
            ''' <param name="functionName">Internal Excel function name.</param>
            ''' <param name="postArg">Optional argument.</param>
            ''' <returns>Prepared Cell object, ready to be added to a worksheet.</returns>
            Private Shared Function GetBasicFormula(target As Worksheet, range As Range, functionName As String, postArg As String) As Cell
                Dim arg1, arg2, prefix As String
                If Equals(postArg, Nothing) Then
                    arg2 = ""
                Else
                    arg2 = "," & postArg
                End If
                If target IsNot Nothing Then
                    prefix = target.SheetName & "!"
                Else
                    prefix = ""
                End If
                If range.StartAddress.Equals(range.EndAddress) Then
                    arg1 = prefix & range.StartAddress.ToString()
                Else
                    arg1 = prefix & range.ToString()
                End If
                Return New Cell(functionName & "(" & arg1 & arg2 & ")", CellType.FORMULA)
            End Function
        End Class
    End Class
End Namespace
