Imports feather.fbs
Imports FeatherDotNet.Impl
Imports FlatBuffers
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.MemoryMappedFiles
Imports System.Runtime.InteropServices

Namespace FeatherDotNet
    ''' <summary>
    ''' Utility class for reading Feather files.
    ''' </summary>
    Public Module FeatherReader
        ''' <summary>
        ''' Create a dataframe from the given file, with the given basis.
        ''' 
        ''' Throws if the dataframe cannot be created.
        ''' </summary>
        Public Function ReadFromFile(filePath As String, basis As BasisType) As DataFrame
            Dim errorMessage As String
            Dim ret As DataFrame
            If Not TryReadFromFile(filePath, basis, ret, errorMessage) Then
                Throw New InvalidOperationException(errorMessage)
            End If

            Return ret
        End Function

        ''' <summary>
        ''' Create a dataframe from the given file, with the given basis.
        ''' 
        ''' Returns false if the dataframe cannot be created.
        ''' </summary>
        Public Function TryReadFromFile(filePath As String, basis As BasisType, <Out> ByRef frame As DataFrame, <Out> ByRef errorMessage As String) As Boolean
            Dim fileInfo = New FileInfo(filePath)
            Dim fileSize = fileInfo.Length

            Dim memoryMapped As MemoryMappedFile
            Try
                memoryMapped = MemoryMappedFile.CreateFromFile(filePath)
            Catch e As Exception
                errorMessage = $"Encountered {e.GetType().Name} trying to open file ""{filePath}"": {e.Message}"
                frame = Nothing
                Return False
            End Try

            Dim ret = TryRead(memoryMapped, fileSize, basis, frame, errorMessage)
            If Not ret Then
                memoryMapped.Dispose()
            End If

            Return ret
        End Function

        ''' <summary>
        ''' Create a dataframe from a memory mapped file with the given name, with the given basis.
        ''' 
        ''' Throws if the dataframe cannot be created.
        ''' </summary>
        Public Function ReadFromName(name As String, size As Long, basis As BasisType) As DataFrame
            Dim errorMessage As String
            Dim ret As DataFrame
            If Not TryReadFromName(name, size, basis, ret, errorMessage) Then
                Throw New InvalidOperationException(errorMessage)
            End If

            Return ret
        End Function

        ''' <summary>
        ''' Create a dataframe from a memory mapped file with the given name, with the given basis.
        ''' 
        ''' Returns false if the dataframe cannot be created.
        ''' </summary>
        Public Function TryReadFromName(name As String, size As Long, basis As BasisType, <Out> ByRef frame As DataFrame, <Out> ByRef errorMessage As String) As Boolean
            Dim memoryMapped As MemoryMappedFile
            Try
                memoryMapped = MemoryMappedFile.OpenExisting(name)
            Catch e As Exception
                errorMessage = $"Encountered {e.GetType().Name} trying to open name ""{name}"": {e.Message}"
                frame = Nothing
                Return False
            End Try

            Dim ret = TryRead(memoryMapped, size, basis, frame, errorMessage)
            If Not ret Then
                memoryMapped.Dispose()
            End If

            Return ret
        End Function

        ''' <summary>
        ''' Create a dataframe from the bytes passed, with the given basis.
        ''' 
        ''' Throws if the dataframe cannot be created.
        ''' </summary>
        Public Function ReadFromBytes(bytes As Byte(), basis As BasisType) As DataFrame
            Dim errorMessage As String
            Dim ret As DataFrame
            If Not TryReadFromBytes(bytes, basis, ret, errorMessage) Then
                Throw New InvalidOperationException(errorMessage)
            End If

            Return ret
        End Function

        ''' <summary>
        ''' Create a dataframe from the give bytes, with the given basis.
        ''' 
        ''' Returns false if the dataframe cannot be created.
        ''' </summary>
        Public Function TryReadFromBytes(bytes As Byte(), basis As BasisType, <Out> ByRef frame As DataFrame, <Out> ByRef errorMessage As String) As Boolean
            Dim memoryMapped As MemoryMappedFile
            Try
                memoryMapped = MakeMemoryMappedProxy(bytes)
            Catch e As Exception
                errorMessage = $"Encoutered {e.GetType().Name} trying to create a memory mapped proxy for passed bytes: {e.Message}"
                frame = Nothing
                Return False
            End Try

            Dim ret = TryRead(memoryMapped, bytes.Length, basis, frame, errorMessage)
            If Not ret Then
                memoryMapped.Dispose()
            End If

            Return ret
        End Function

        Private Function MakeMemoryMappedProxy(bytes As Byte()) As MemoryMappedFile
            Dim newFile = MemoryMappedFile.CreateNew(NameOf(FeatherDotNet) & "." & NameOf(MakeMemoryMappedProxy) & "." & Guid.NewGuid().ToString(), bytes.Length)
            Try
                Using stream = newFile.CreateViewStream()
                    stream.Write(bytes, 0, bytes.Length)
                End Using

            Catch
                newFile?.Dispose()
                Throw
            End Try
            Return newFile
        End Function

        Private Function TryRead(file As MemoryMappedFile, fileSize As Long, basis As BasisType, <Out> ByRef frame As DataFrame, <Out> ByRef errorMessage As String) As Boolean
            Select Case basis
                Case BasisType.One, BasisType.Zero
                Case Else
                    frame = Nothing
                    errorMessage = $"Unexpected BasisType {basis}"
                    Return False
            End Select

            Dim metadata As Metadata
            If Not TryReadMetaData(file, fileSize, metadata, errorMessage) Then
                frame = Nothing
                Return False
            End If

            frame = New DataFrame(file, metadata, basis)

            errorMessage = Nothing
            Return True
        End Function

        Private Function TryReadMetaData(file As MemoryMappedFile, size As Long, <Out> ByRef metadata As Metadata, <Out> ByRef [error] As String) As Boolean
            If size < MAGIC_HEADER_SIZE * 2 Then
                metadata = Nothing
                [error] = $"File too small ({size:N0} bytes) to be a valid feather file"
                Return False
            End If

            Using accessor = file.CreateViewAccessor()
                Dim leadingHeader = accessor.ReadInt32(0)

                If leadingHeader <> MAGIC_HEADER Then
                    metadata = Nothing
                    [error] = $"Magic header malformed"
                    Return False
                End If

                Dim trailingHeader = accessor.ReadInt32(size - MAGIC_HEADER_SIZE)

                If trailingHeader <> MAGIC_HEADER Then
                    metadata = Nothing
                    [error] = $"Magic footer malformed"
                    Return False
                End If

                Dim metadataSize = accessor.ReadUInt32(size - MAGIC_HEADER_SIZE - HeapSizeOf.uint)

                Dim metadataStart = size - MAGIC_HEADER_SIZE - HeapSizeOf.uint - metadataSize
                If metadataStart < MAGIC_HEADER_SIZE OrElse metadataSize > Integer.MaxValue Then
                    metadata = Nothing
                    [error] = $"Metadata size ({metadataSize:N0}) is invalid"
                    Return False
                End If

                Dim metadataBytes = New Byte(metadataSize - 1) {}
                accessor.ReadArray(metadataStart, metadataBytes, 0, metadataSize)

                ' note: It'd be nice to not actually use flatbuffers for this,
                '   kind of a heavy (re)build dependency for reading, like, 4 
                '   things
                Dim metadataBuffer = New ByteBuffer(metadataBytes)
                Dim metadataCTable = CTable.GetRootAsCTable(metadataBuffer)

                If metadataCTable.Version <> FEATHER_VERSION Then
                    [error] = $"Unexpected version {metadataCTable.Version}, only {FEATHER_VERSION} is supported"
                    metadata = Nothing
                    Return False
                End If

                If metadataCTable.ColumnsLength <= 0 Then
                    [error] = $"Invalid number of columns: {metadataCTable.ColumnsLength:N0}"
                    metadata = Nothing
                    Return False
                End If

                Dim columnSpecs = New ColumnSpec(metadataCTable.ColumnsLength - 1) {}
                For i = 0 To columnSpecs.Length - 1
                    Dim metadataColumn = metadataCTable.Columns(i)
                    Dim name = metadataColumn.Name
                    Dim metadataType = metadataColumn.MetadataType

                    Dim categoryLevels As String() = Nothing
                    Dim precision As DateTimePrecisionType = Nothing

                    Dim arrayDetails = metadataColumn.Values
                    Dim effectiveType = arrayDetails.Type

                    Select Case metadataType
                        Case TypeMetadata.CategoryMetadata
                            If Not TryReadCategoryLevels(accessor, metadataColumn, categoryLevels, [error]) Then
                                metadata = Nothing
                                Return False
                            End If

                        Case TypeMetadata.TimestampMetadata
                            If arrayDetails.Type <> feather.fbs.Type.INT64 Then
                                metadata = Nothing
                                [error] = $"Column {name} has Timestamp metadata, but isn't backed by an Int64 array"
                                Return False
                            End If

                            If Not TryReadTimestampPrecision(metadataColumn, precision, [error]) Then
                                metadata = Nothing
                                Return False
                            End If

                            ' note: this type is spec'd (https://github.com/wesm/feather/blob/master/cpp/src/feather/metadata.fbs#L25), 
                            '  but it looks like R always writes it as an int64?
                            ' Possibly a bug.
                            effectiveType = feather.fbs.Type.TIMESTAMP

                        Case TypeMetadata.TimeMetadata
                            If arrayDetails.Type <> feather.fbs.Type.INT64 Then
                                metadata = Nothing
                                [error] = $"Column {name} has Time metadata, but isn't backed by an Int64 array"
                                Return False
                            End If

                            If Not TryReadTimePrecision(metadataColumn, precision, [error]) Then
                                metadata = Nothing
                                Return False
                            End If

                            ' note: this type is spec'd (https://github.com/wesm/feather/blob/master/cpp/src/feather/metadata.fbs#L27), 
                            '  but it looks like R always writes it as an int64?
                            ' Possibly a bug.
                            effectiveType = feather.fbs.Type.TIME

                        Case TypeMetadata.DateMetadata
                            If arrayDetails.Type <> feather.fbs.Type.INT32 Then
                                metadata = Nothing
                                [error] = $"Column {name} has Time metadata, but isn't backed by an Int32 array"
                                Return False
                            End If

                            ' note: this type is spec'd (https://github.com/wesm/feather/blob/master/cpp/src/feather/metadata.fbs#L26), 
                            '  but it looks like R always writes it as an int32?
                            ' Possibly a bug.
                            effectiveType = feather.fbs.Type.DATE

                        Case TypeMetadata.NONE
                    End Select

                    Dim column As ColumnSpec = Nothing
                    If Not TryMakeColumnSpec(name, effectiveType, arrayDetails, categoryLevels, precision, column, [error]) Then
                        metadata = Nothing
                        Return False
                    End If

                    columnSpecs(i) = column
                Next

                metadata = New Metadata With {
.Columns = columnSpecs,
.NumRows = metadataCTable.NumRows
}
                [error] = Nothing
                Return True
            End Using
        End Function

        Private Function TryGetType(effectiveType As feather.fbs.Type, ByRef array As PrimitiveArray, precision As DateTimePrecisionType, <Out> ByRef type As ColumnType, <Out> ByRef isNullable As Boolean, <Out> ByRef errorMessage As String) As Boolean
            isNullable = array.NullCount <> 0

            If Not isNullable Then
                Select Case effectiveType
                    Case feather.fbs.Type.BINARY
                        type = ColumnType.Binary
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.BOOL
                        type = ColumnType.Bool
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.CATEGORY
                        type = ColumnType.Category
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.DATE
                        type = ColumnType.Date
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.DOUBLE
                        type = ColumnType.Double
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.FLOAT
                        type = ColumnType.Float
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT16
                        type = ColumnType.Int16
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT32
                        type = ColumnType.Int32
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT64
                        type = ColumnType.Int64
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT8
                        type = ColumnType.Int8
                        errorMessage = Nothing
                        Return True


                    Case feather.fbs.Type.TIMESTAMP
                        Select Case precision
                            Case DateTimePrecisionType.Microsecond
                                type = ColumnType.Timestamp_Microsecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Millisecond
                                type = ColumnType.Timestamp_Millisecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Nanosecond
                                type = ColumnType.Timestamp_Nanosecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Second
                                type = ColumnType.Timestamp_Second
                                errorMessage = Nothing
                                Return True
                            Case Else
                                errorMessage = $"Unknown precision {precision}"
                                type = ColumnType.NONE
                                Return False
                        End Select

                    Case feather.fbs.Type.TIME
                        Select Case precision
                            Case DateTimePrecisionType.Microsecond
                                type = ColumnType.Time_Microsecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Millisecond
                                type = ColumnType.Time_Millisecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Nanosecond
                                type = ColumnType.Time_Nanosecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Second
                                type = ColumnType.Time_Second
                                errorMessage = Nothing
                                Return True
                            Case Else
                                errorMessage = $"Unknown precision {precision}"
                                type = ColumnType.NONE
                                Return False
                        End Select

                    Case feather.fbs.Type.UINT16
                        type = ColumnType.Uint16
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.UINT32
                        type = ColumnType.Uint32
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.UINT64
                        type = ColumnType.Uint64
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.UINT8
                        type = ColumnType.Uint8
                        errorMessage = Nothing
                        Return True

                    Case feather.fbs.Type.UTF8
                        type = ColumnType.String
                        errorMessage = Nothing
                        Return True
                    Case Else
                        errorMessage = $"Unknown column type {array.Type}"
                        type = ColumnType.NONE
                        Return False
                End Select
            Else
                Select Case effectiveType
                    Case feather.fbs.Type.BINARY
                        type = ColumnType.NullableBinary
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.BOOL
                        type = ColumnType.NullableBool
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.CATEGORY
                        type = ColumnType.NullableCategory
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.DATE
                        type = ColumnType.NullableDate
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.DOUBLE
                        type = ColumnType.NullableDouble
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.FLOAT
                        type = ColumnType.NullableFloat
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT16
                        type = ColumnType.NullableInt16
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT32
                        type = ColumnType.NullableInt32
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT64
                        type = ColumnType.NullableInt64
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.INT8
                        type = ColumnType.NullableInt8
                        errorMessage = Nothing
                        Return True

                    Case feather.fbs.Type.TIMESTAMP
                        Select Case precision
                            Case DateTimePrecisionType.Microsecond
                                type = ColumnType.NullableTimestamp_Microsecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Millisecond
                                type = ColumnType.NullableTimestamp_Millisecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Nanosecond
                                type = ColumnType.NullableTimestamp_Nanosecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Second
                                type = ColumnType.NullableTimestamp_Second
                                errorMessage = Nothing
                                Return True
                            Case Else
                                errorMessage = $"Unknown precision {precision}"
                                type = ColumnType.NONE
                                Return False
                        End Select

                    Case feather.fbs.Type.TIME
                        Select Case precision
                            Case DateTimePrecisionType.Microsecond
                                type = ColumnType.NullableTime_Microsecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Millisecond
                                type = ColumnType.NullableTime_Millisecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Nanosecond
                                type = ColumnType.NullableTime_Nanosecond
                                errorMessage = Nothing
                                Return True
                            Case DateTimePrecisionType.Second
                                type = ColumnType.NullableTime_Second
                                errorMessage = Nothing
                                Return True
                            Case Else
                                errorMessage = $"Unknown precision {precision}"
                                type = ColumnType.NONE
                                Return False
                        End Select

                    Case feather.fbs.Type.UINT16
                        type = ColumnType.NullableUint16
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.UINT32
                        type = ColumnType.NullableUint32
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.UINT64
                        type = ColumnType.NullableUint64
                        errorMessage = Nothing
                        Return True
                    Case feather.fbs.Type.UINT8
                        type = ColumnType.NullableUint8
                        errorMessage = Nothing
                        Return True

                    Case feather.fbs.Type.UTF8
                        type = ColumnType.NullableString
                        errorMessage = Nothing
                        Return True
                    Case Else
                        errorMessage = $"Unknown column type {array.Type}"
                        type = ColumnType.NONE
                        Return False
                End Select
            End If
        End Function

        Private Function TryReadCategoryLevels(view As MemoryMappedViewAccessor, ByRef column As feather.fbs.Column, <Out> ByRef categoryLevels As String(), <Out> ByRef errorMessage As String) As Boolean
            Dim metadata = column.Metadata(Of CategoryMetadata)()
            If metadata Is Nothing Then
                categoryLevels = Nothing
                errorMessage = $"Couldn't read Category levels for column {column.Name}, no CategoryMetadata"
                Return False
            End If

            Dim metadataValue = metadata
            If metadataValue.Levels Is Nothing Then
                categoryLevels = Nothing
                errorMessage = $"Couldn't read Category levels for column {column.Name}, no Levels"
                Return False
            End If

            Dim levels = metadataValue.Levels
            If levels.Type <> feather.fbs.Type.UTF8 Then
                categoryLevels = Nothing
                errorMessage = $"Found non-string ({levels.Type}) for column {column.Name}"
                Return False
            End If

            Dim categoryColumn As ColumnSpec = Nothing
            If Not TryMakeColumnSpec(NameOf(TryReadCategoryLevels) & "__FAKE__", levels.Type, levels, Nothing, Nothing, categoryColumn, errorMessage) Then
                categoryLevels = Nothing
                Return False
            End If

            categoryLevels = New String(levels.Length - 1) {}

            For i = 0 To levels.Length - 1
                Dim categoryName = DataFrame.ReadString(view, levels.Length, Nothing, categoryColumn, i)
                categoryLevels(i) = categoryName
            Next

            errorMessage = Nothing
            Return True
        End Function

        Private Function TryReadTimestampPrecision(ByRef column As feather.fbs.Column, <Out> ByRef precision As DateTimePrecisionType, <Out> ByRef errorMessage As String) As Boolean
            Dim metadata = column.Metadata(Of TimestampMetadata)()
            If metadata Is Nothing Then
                precision = Nothing
                errorMessage = $"Couldn't read Precision for column {column.Name}, no TimestampMetadada"
                Return False
            End If

            Dim metadataValue = metadata
            Dim timezoneName = metadataValue.Timezone
            Dim unit = metadataValue.Unit

            ' note: supporting other timezones would be nice,
            '   but the timezone included appears to just be 
            '   a passthrough from as.POSIXct (in R anyway)
            '   which lets a bunch of ambiguous junk through
            '   (timezone abbreviations are NOT unique, or
            '   properly spec'd even).
            ' the only thing we can rely on being the same
            '   everywhere is UTC, so constrain to that.
            Dim isntUtc = Not Equals(timezoneName, Nothing) AndAlso Not timezoneName.Equals("UTC", StringComparison.InvariantCultureIgnoreCase) AndAlso Not timezoneName.Equals("GMT", StringComparison.InvariantCultureIgnoreCase)

            If isntUtc Then
                errorMessage = $"Cannot read Timestamp in timezone {timezoneName}, only UTC/GMT is supported"
                precision = Nothing
                Return False
            End If

            Select Case unit
                Case TimeUnit.MICROSECOND
                    precision = DateTimePrecisionType.Microsecond
                Case TimeUnit.MILLISECOND
                    precision = DateTimePrecisionType.Millisecond
                Case TimeUnit.NANOSECOND
                    precision = DateTimePrecisionType.Nanosecond
                Case TimeUnit.SECOND
                    precision = DateTimePrecisionType.Second
                Case Else
                    precision = Nothing
                    errorMessage = $"Couldn't understand TimeUnit {unit}"
                    Return False
            End Select

            errorMessage = Nothing
            Return True
        End Function

        Private Function TryReadTimePrecision(ByRef column As feather.fbs.Column, <Out> ByRef precision As DateTimePrecisionType, <Out> ByRef errorMessage As String) As Boolean
            Dim metadata = column.Metadata(Of TimeMetadata)()
            If metadata Is Nothing Then
                precision = Nothing
                errorMessage = $"Couldn't read Precision for column {column.Name}, no TimeMetadata"
                Return False
            End If

            Dim metadataValue = metadata
            Dim unit = metadataValue.Unit

            Select Case unit
                Case TimeUnit.MICROSECOND
                    precision = DateTimePrecisionType.Microsecond
                Case TimeUnit.MILLISECOND
                    precision = DateTimePrecisionType.Millisecond
                Case TimeUnit.NANOSECOND
                    precision = DateTimePrecisionType.Nanosecond
                Case TimeUnit.SECOND
                    precision = DateTimePrecisionType.Second
                Case Else
                    precision = Nothing
                    errorMessage = $"Couldn't understand TimeUnit {unit}"
                    Return False
            End Select

            errorMessage = Nothing
            Return True
        End Function

        Private Function TryMakeColumnSpec(name As String, effectiveType As feather.fbs.Type, ByRef arrayDetails As PrimitiveArray, categoryLevels As String(), precision As DateTimePrecisionType, <Out> ByRef columnSpec As ColumnSpec, <Out> ByRef errorMessage As String) As Boolean
            Dim arrayOffset = arrayDetails.Offset
            Dim arrayLength = arrayDetails.Length
            Dim arrayNulls = arrayDetails.NullCount
            Dim arrayEncoding = arrayDetails.Encoding

            ' TODO (Dictionary Encoding)
            If arrayEncoding <> Encoding.PLAIN Then
                Throw New NotImplementedException()
            End If
            ' END TODO

            Dim type As ColumnType
            Dim isNullable As Boolean
            If Not TryGetType(effectiveType, arrayDetails, precision, type, isNullable, errorMessage) Then
                columnSpec = Nothing
                Return False
            End If

            Dim numNullBytes As Long = 0
            If isNullable Then
                numNullBytes = arrayLength / 8
                If arrayLength Mod 8 <> 0 Then
                    numNullBytes += 1
                End If
            End If

            ' a naive reading of the spec suggests that the null bitmask should be
            '   aligned based on the _type_ but it appears to always be long
            '   aligned.
            ' this may be a bug in the spec
            Dim nullPadding = 0
            If numNullBytes Mod NULL_BITMASK_ALIGNMENT <> 0 Then
                nullPadding = NULL_BITMASK_ALIGNMENT - CInt(numNullBytes Mod NULL_BITMASK_ALIGNMENT)
            End If
            Dim nullOffset = If(isNullable, arrayOffset, -1)
            Dim dataOffset = If(Not isNullable, arrayOffset, nullOffset + numNullBytes + nullPadding)

            columnSpec = New ColumnSpec With {
.Name = name,
.NullBitmaskOffset = nullOffset,
.DataOffset = dataOffset,
.Length = arrayLength,
.Type = type,
.CategoryLevels = categoryLevels,
.CategoryEnumMap = If(categoryLevels IsNot Nothing, New Dictionary(Of System.Type, CategoryEnumMapType)(), Nothing) ' only spin up this map if we've got categories to potentially map to
}
            errorMessage = Nothing
            Return True
        End Function
    End Module
End Namespace
