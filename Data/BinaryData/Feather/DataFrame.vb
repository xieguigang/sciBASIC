Imports System.IO.MemoryMappedFiles
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl
Imports std = System.Math

''' <summary>
''' Represents a dataframe.
''' 
''' Is untyped, but can be mapped to columns of known types or proxied for easy interop with .NET classes or structs.
''' 
''' Is backed by a MemoryMappedFile, remember to Dispose when done using the dataframe.
''' 
''' Any <see cref="ProxyDataFrame(Of TProxyType)"/>, <see cref="TypedDataFrameBase(Of TRowType)"/>, or <see cref="Value"/> instances
''' (and their enumerables) obtained via a DataFrame become invalid after that DataFrame is disposed.  Be sure to have converted
''' to built-in types prior to Disposing.
''' </summary>
''' <remarks>
''' https://github.com/kevin-montrose/FeatherDotNet
''' </remarks>
Partial Public Class DataFrame
    Implements IDisposable, IDataFrame

    ''' <summary>
    ''' Whether this DataFrame is addressable with base-0 or base-1 indexes.
    ''' </summary>
    Dim _Basis As BasisType

    ''' <summary>
    ''' An enumerable of all the columns in this DataFrame.
    ''' </summary>
    Dim _AllColumns As ColumnEnumerable

    ''' <summary>
    ''' An enumerable of all the rows in this DataFrame.
    ''' </summary>
    Dim _AllRows As RowEnumerable

    ''' <summary>
    ''' A utility accessor for columns in this DataFrame.
    ''' </summary>
    Dim _Columns As ColumnMap

    ''' <summary>
    ''' A utility accessor for rows in this DataFrame.
    ''' </summary>
    Dim _Rows As RowMap

    Private ReadOnly InternalSyncLock As Object = New Object()

    Private File As MemoryMappedFile
    Private View As MemoryMappedViewAccessor
    Friend ReadOnly Metadata As Metadata

    Dim ColumnNameLookup As Dictionary(Of String, Long)

    ''' <summary>
    ''' Number of rows in the DataFrame.
    ''' </summary>
    Public ReadOnly Property RowCount As Long Implements IDataFrame.RowCount
        Get
            Return Metadata.NumRows
        End Get
    End Property
    ''' <summary>
    ''' Number of columns in the DataFrame.
    ''' </summary>
    Public ReadOnly Property ColumnCount As Long Implements IDataFrame.ColumnCount
        Get
            Return Metadata.Columns.Length
        End Get
    End Property

    Public Property Basis As BasisType Implements IDataFrame.Basis
        Get
            Return _Basis
        End Get
        Private Set(value As BasisType)
            _Basis = value
        End Set
    End Property

    Public Property AllColumns As ColumnEnumerable
        Get
            Return _AllColumns
        End Get
        Private Set(value As ColumnEnumerable)
            _AllColumns = value
        End Set
    End Property

    Public Property AllRows As RowEnumerable
        Get
            Return _AllRows
        End Get
        Private Set(value As RowEnumerable)
            _AllRows = value
        End Set
    End Property

    Public Property Columns As ColumnMap
        Get
            Return _Columns
        End Get
        Private Set(value As ColumnMap)
            _Columns = value
        End Set
    End Property

    Public Property Rows As RowMap
        Get
            Return _Rows
        End Get
        Private Set(value As RowMap)
            _Rows = value
        End Set
    End Property

    ''' <summary>
    ''' Return the row at the given index.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetRow(Long,Row)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(rowIndex As Long) As Row
        Get
            Return Rows(rowIndex)
        End Get
    End Property

    ''' <summary>
    ''' Return the column with the given name.
    ''' 
    ''' Will throw if the name is not found.  Use <see cref="TryGetColumn(String,Column)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(columnName As String) As Column
        Get
            Return Columns(columnName)
        End Get
    End Property

    ''' <summary>
    ''' Return the value at the given row and column indexes.
    ''' 
    ''' Will throw if the index is out of bounds.  Use <see cref="TryGetValue(Long,Long,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(rowIndex As Long, columnIndex As Long) As Value Implements IDataFrame.Item
        Get
            Dim value As Value = Nothing
            If Not TryGetValue(rowIndex, columnIndex, value) Then
                Dim minRowIx, minColIx, maxRowIx, maxColIx As Long
                Select Case Basis
                    Case BasisType.One
                        minRowIx = 1
                        maxRowIx = Metadata.NumRows
                        minColIx = 1
                        maxColIx = Metadata.Columns.Length
                    Case BasisType.Zero
                        minRowIx = 0
                        maxRowIx = Metadata.NumRows - 1
                        minColIx = 0
                        maxColIx = Metadata.Columns.Length - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Basis}")
                End Select

                Throw New ArgumentOutOfRangeException($"Address out of range, legal range is [{minRowIx}, {minColIx}] - [{maxRowIx}, {maxColIx}], found [{rowIndex}, {columnIndex}]")
            End If

            Return value
        End Get
    End Property

    ''' <summary>
    ''' Return the value at the given row index in the column with the given name.
    ''' 
    ''' Will throw if the index is out of bounds or the column is not found.  Use <see cref="TryGetValue(Long,String,Value)"/> for non-throwing gets.
    ''' </summary>
    Default Public ReadOnly Property Item(rowIndex As Long, columnName As String) As Value Implements IDataFrame.Item
        Get
            Dim value As Value = Nothing
            If Not TryGetValue(rowIndex, columnName, value) Then
                Dim translatedColumIx As Long
                If Not TryLookupTranslatedColumnIndex(columnName, translatedColumIx) Then
                    Throw New ArgumentOutOfRangeException($"Could not find column for name ""{columnName}""")
                End If

                Dim columnIndex = UntranslateIndex(translatedColumIx)

                Dim minRowIx, minColIx, maxRowIx, maxColIx As Long
                Select Case Basis
                    Case BasisType.One
                        minRowIx = 1
                        maxRowIx = Metadata.NumRows
                        minColIx = 1
                        maxColIx = Metadata.Columns.Length
                    Case BasisType.Zero
                        minRowIx = 0
                        maxRowIx = Metadata.NumRows - 1
                        minColIx = 0
                        maxColIx = Metadata.Columns.Length - 1
                    Case Else
                        Throw New InvalidOperationException($"Unexpected Basis: {Basis}")
                End Select

                Throw New ArgumentOutOfRangeException($"Address out of range, legal range is [{minRowIx}, {minColIx}] - [{maxRowIx}, {maxColIx}], found [{rowIndex}, {columnIndex}]")
            End If

            Return value
        End Get
    End Property

    Friend Sub New(file As MemoryMappedFile, metadata As Metadata, basis As BasisType)
        Me.Basis = basis
        Me.File = file
        Me.Metadata = metadata
        View = Me.File.CreateViewAccessor()

        AllColumns = New ColumnEnumerable(Me)
        AllRows = New RowEnumerable(Me)

        Columns = New ColumnMap(Me)
        Rows = New RowMap(Me)
    End Sub

    ''' <summary>
    ''' Sets column to the column at the given index.
    ''' 
    ''' Returns true if a column exists at that index, and false otherwise.
    ''' </summary>
    Public Function TryGetColumn(index As Long, <Out> ByRef column As Column) As Boolean
        Dim translatedIndex = TranslateIndex(index)

        Return TryGetColumnTranslated(translatedIndex, column)
    End Function

    ''' <summary>
    ''' Sets column to the column with the given name.
    ''' 
    ''' Returns true if a column exists with that name, and false otherwise.
    ''' </summary>
    Public Function TryGetColumn(columnName As String, <Out> ByRef column As Column) As Boolean
        Dim translatedIndex As Long
        If Not TryLookupTranslatedColumnIndex(columnName, translatedIndex) Then
            column = Nothing
            Return False
        End If

        column = New Column With {
.Parent = Me,
.TranslatedColumnIndex = translatedIndex
}
        Return True
    End Function

    ''' <summary>
    ''' Gets row to the row at the given index.
    ''' 
    ''' Returns true if a row exists at that index, and false otherwise.
    ''' </summary>
    Public Function TryGetRow(index As Long, <Out> ByRef row As Row) As Boolean
        Dim translatedIndex = TranslateIndex(index)

        Return TryGetRowTranslated(translatedIndex, row)
    End Function

    ''' <summary>
    ''' Sets value to the value at the row and column indexes passed in.
    ''' 
    ''' If the passed indexes are out of bounds false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(rowIndex As Long, columnIndex As Long, <Out> ByRef value As Value) As Boolean Implements IDataFrame.TryGetValue
        Dim translatedRowIndex = TranslateIndex(rowIndex)
        Dim translatedColumnIndex = TranslateIndex(columnIndex)

        Return TryGetValueTranslated(translatedRowIndex, translatedColumnIndex, value)
    End Function

    ''' <summary>
    ''' Sets value to the value, coerced to the appropriate type, at the row and column indexes passed in.
    ''' 
    ''' If the passed indexes are out of bounds, or the value cannot be coerced, false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(Of T)(rowIndex As Long, columnIndex As Long, <Out> ByRef value As T) As Boolean Implements IDataFrame.TryGetValue
        Dim column As Column = Nothing
        If Not TryGetColumn(columnIndex, column) Then
            value = Nothing
            Return False
        End If

        Return column.TryGetValue(rowIndex, value)
    End Function

    ''' <summary>
    ''' Sets value to the value at the row given row index in the column with the given name.
    ''' 
    ''' If the passed index is out of bounds or no column with the given name exists, false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(rowIndex As Long, columnName As String, <Out> ByRef value As Value) As Boolean Implements IDataFrame.TryGetValue
        Dim translatedRowIndex = TranslateIndex(rowIndex)
        Dim translatedColumnIndex As Long
        If Not TryLookupTranslatedColumnIndex(columnName, translatedColumnIndex) Then
            value = Nothing
            Return False
        End If

        Return TryGetValueTranslated(translatedRowIndex, translatedColumnIndex, value)
    End Function

    ''' <summary>
    ''' Sets value to the value, coerced to the appropriate type, at the given row index in the column with the given name.
    ''' 
    ''' If the passed index is out of bounds, no column with the given name exists, or the value cannot be coerced then false is returned.  Otherwise, true is returned;
    ''' </summary>
    Public Function TryGetValue(Of T)(rowIndex As Long, columnName As String, <Out> ByRef value As T) As Boolean Implements IDataFrame.TryGetValue
        Dim column As Column = Nothing
        If Not TryGetColumn(columnName, column) Then
            value = Nothing
            Return False
        End If

        Return column.TryGetValue(rowIndex, value)
    End Function

    ''' <summary>
    ''' <see cref="IDisposable.Dispose"/>
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        ' burn down View
        If True Then
            Dim copy = View
            If copy IsNot Nothing Then
                copy.Dispose()
                View = Nothing
            End If
        End If

        ' burn down File
        If True Then
            Dim copy = File
            If copy IsNot Nothing Then
                copy.Dispose()
                File = Nothing
            End If
        End If
    End Sub

    Friend Function TypeForTranslated(translatedColumnIndex As Long) As ColumnType
        Dim column = Metadata.Columns(translatedColumnIndex)
        Return column.Type
    End Function

    Friend Function TryGetRowTranslated(translatedRowIndex As Long, <Out> ByRef row As Row) As Boolean
        If translatedRowIndex < 0 OrElse translatedRowIndex >= Metadata.NumRows Then
            row = Nothing
            Return False
        End If

        row = New Row(Me, translatedRowIndex)
        Return True
    End Function

    Friend Function TryGetValueTranslated(translatedRowIndex As Long, translatedColumnIndex As Long, <Out> ByRef value As Value) As Boolean
        If translatedRowIndex < 0 OrElse translatedColumnIndex < 0 Then
            value = Nothing
            Return False
        End If

        If translatedRowIndex >= Metadata.NumRows OrElse translatedColumnIndex >= Metadata.Columns.Length Then
            value = Nothing
            Return False
        End If

        value = New Value(translatedRowIndex, translatedColumnIndex, Me)
        Return True
    End Function

    Friend Function TryGetColumnTranslated(translatedIndex As Long, <Out> ByRef column As Column) As Boolean
        If translatedIndex < 0 OrElse translatedIndex >= Metadata.Columns.Length Then
            column = Nothing
            Return False
        End If

        column = New Column With {
.TranslatedColumnIndex = translatedIndex,
.Parent = Me
}
        Return True
    End Function

    Friend Function TranslateIndex(passedIndex As Long) As Long
        Select Case Basis
            Case BasisType.One
                Return passedIndex - 1
            Case BasisType.Zero
                Return passedIndex
            Case Else
                Throw New InvalidOperationException($"Unexpected Basis: {Basis}")
        End Select
    End Function

    Friend Function UntranslateIndex(translatedIndex As Long) As Long
        Select Case Basis
            Case BasisType.One
                Return translatedIndex + 1
            Case BasisType.Zero
                Return translatedIndex
            Case Else
                Throw New InvalidOperationException($"Unexpected Basis: {Basis}")
        End Select
    End Function

    Friend Function TryLookupTranslatedColumnIndex(columnName As String, <Out> ByRef translatedIndex As Long) As Boolean
        ' only bother to spin this up if we need it
        If Me.ColumnNameLookup Is Nothing Then
            SyncLock InternalSyncLock
                If Me.ColumnNameLookup Is Nothing Then
                    Dim lookup = New Dictionary(Of String, Long)(Metadata.Columns.Length)
                    For i = 0 To Metadata.Columns.Length - 1
                        lookup.Add(Metadata.Columns(i).Name, i)
                    Next

                    Me.ColumnNameLookup = lookup
                End If
            End SyncLock
        End If

        Return Me.ColumnNameLookup.TryGetValue(columnName, translatedIndex)
    End Function

    ' Map from the memory mapped file
    Friend Sub UnsafeFastGetRowRange(Of T)(translatedRowIndex As Long, translatedColumnIndex As Long, array As T(), destinationIndex As Integer, length As Integer)
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim entrySize = columnMetadata.Type.GetAlignment()
        Dim dataStart = columnMetadata.DataOffset
        Dim byteOffset = translatedRowIndex * entrySize

        Dim byteIndex = dataStart + byteOffset
        UnsafeArrayReader(Of T).ReadArray(View, byteIndex, array, 0, length)
    End Sub

    Friend Function IsNullTranslated(translatedRowIndex As Long, translatedColumnIndex As Long) As Boolean
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim nullBitmaskOffset = columnMetadata.NullBitmaskOffset

        Dim nullBitmaskByteOffset = nullBitmaskOffset + translatedRowIndex / 8
        Dim nullBitmaskBitmask = CByte(1 << CByte(translatedRowIndex Mod 8))

        Dim nullBitmaskByte = View.ReadByte(nullBitmaskByteOffset)

        Dim isNull = (nullBitmaskByte And nullBitmaskBitmask) = 0
        Return isNull
    End Function

    Friend Function ReadDouble(translatedRowIndex As Long, translatedColumnIndex As Long) As Double
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.double

        Return View.ReadDouble(valueOffset)
    End Function

    Friend Function ReadFloat(translatedRowIndex As Long, translatedColumnIndex As Long) As Single
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.float

        Return View.ReadSingle(valueOffset)
    End Function

    Friend Function ReadInt8(translatedRowIndex As Long, translatedColumnIndex As Long) As SByte
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.sbyte

        Return View.ReadSByte(valueOffset)
    End Function

    Friend Function ReadUInt8(translatedRowIndex As Long, translatedColumnIndex As Long) As Byte
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.byte

        Return View.ReadByte(valueOffset)
    End Function

    Friend Function ReadInt16(translatedRowIndex As Long, translatedColumnIndex As Long) As Short
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.short

        Return View.ReadInt16(valueOffset)
    End Function

    Friend Function ReadUInt16(translatedRowIndex As Long, translatedColumnIndex As Long) As UShort
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.ushort

        Return View.ReadUInt16(valueOffset)
    End Function

    Friend Function ReadInt32(translatedRowIndex As Long, translatedColumnIndex As Long) As Integer
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.int

        Return View.ReadInt32(valueOffset)
    End Function

    Friend Function ReadUInt32(translatedRowIndex As Long, translatedColumnIndex As Long) As UInteger
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.uint

        Return View.ReadUInt32(valueOffset)
    End Function

    Friend Function ReadInt64(translatedRowIndex As Long, translatedColumnIndex As Long) As Long
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.long

        Return View.ReadInt64(valueOffset)
    End Function

    Friend Function ReadUInt64(translatedRowIndex As Long, translatedColumnIndex As Long) As ULong
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim valueOffset = dataOffset + translatedRowIndex * HeapSizeOf.ulong

        Return View.ReadUInt64(valueOffset)
    End Function

    Const MIN_BYTE_BUFFER_SIZE As Integer = HeapSizeOf.long
    Private ByteBuffer As ThreadLocal(Of Byte()) = New ThreadLocal(Of Byte())()
    Friend Function ReadString(translatedRowIndex As Long, translatedColumnIndex As Long) As String
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Return ReadString(View, RowCount, ByteBuffer, columnMetadata, translatedRowIndex)
    End Function

    Friend Shared Function ReadString(view As MemoryMappedViewAccessor, rowCount As Long, byteBuffer As ThreadLocal(Of Byte()), ByRef columnMetadata As ColumnSpec, translatedRowIndex As Long) As String
        Dim dataOffset = columnMetadata.DataOffset

        Dim stringOffsetOffset = dataOffset + translatedRowIndex * HeapSizeOf.int
        Dim stringOffset = view.ReadInt32(stringOffsetOffset)

        Dim nextStringOffsetOffset = dataOffset + (translatedRowIndex + 1) * HeapSizeOf.int
        Dim nextStringOffset = view.ReadInt32(nextStringOffsetOffset)

        Dim stringDataStart = dataOffset + (rowCount + 1) * HeapSizeOf.int

        Dim stringDataPadding = 0
        If stringDataStart Mod ARROW_ALIGNMENT <> 0 Then
            stringDataPadding = ARROW_ALIGNMENT - CInt(stringDataStart Mod ARROW_ALIGNMENT)
        End If
        stringDataStart += stringDataPadding

        Dim stringDataStartIx = stringDataStart + stringOffset
        Dim stringDataEndIx = stringDataStart + nextStringOffset

        Dim stringLengthLong = stringDataEndIx - stringDataStartIx
        If stringLengthLong = 0 Then Return ""

        If stringLengthLong < 0 OrElse stringLengthLong > Integer.MaxValue Then
            Throw New InvalidOperationException($"Tried to create a string with an absurd length {stringLengthLong:N0}")
        End If

        Dim stringLength = CInt(stringLengthLong)

        Dim buffer = byteBuffer?.Value
        If buffer Is Nothing OrElse buffer.Length < stringLength Then
            Dim newSize As Integer
            If stringLength < 4096 Then
                newSize = stringLength

                ' get it to the nearest power of two
                '   only calculating for the first 32-bits
                '   since the size check on stringLength
                '   should exclude anything larger
                newSize -= 1
                newSize = newSize Or newSize >> 1
                newSize = newSize Or newSize >> 2
                newSize = newSize Or newSize >> 4
                newSize = newSize Or newSize >> 8
                newSize = newSize Or newSize >> 16
                newSize += 1
            Else
                ' round to nearest whole page size
                newSize = stringLength / 4096 * 4096 + stringLength Mod 4096
            End If

            If buffer Is Nothing Then
                newSize = std.Max(newSize, MIN_BYTE_BUFFER_SIZE)
                buffer = New Byte(newSize - 1) {}
            Else
                Array.Resize(buffer, newSize)
            End If

            If byteBuffer IsNot Nothing Then
                byteBuffer.Value = buffer
            End If
        End If

        view.ReadArray(stringDataStartIx, buffer, 0, stringLength)

        Dim ret = Encoding.UTF8.GetString(buffer, 0, stringLength)
        Return ret
    End Function

    Friend Function ReadBool(translatedRowIndex As Long, translatedColumnIndex As Long) As Boolean
        Dim columnMetadata = Metadata.Columns(translatedColumnIndex)
        Dim dataOffset = columnMetadata.DataOffset

        Dim byteOffset = translatedRowIndex / 8
        Dim bitOffset = CByte(translatedRowIndex Mod 8)
        Dim bitMask = CByte(1 << bitOffset)

        Dim boolByte = View.ReadByte(dataOffset + byteOffset)

        Return (boolByte And bitMask) <> 0
    End Function
End Class
