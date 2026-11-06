Option Strict On
Option Explicit On
Option Infer On

Imports System.IO
Imports System.Linq

''' <summary>
''' Parquet 文件的读取器。
''' <para>
''' 读取端支持 DataPageV1 / DataPageV2、PLAIN、PLAIN_DICTIONARY / RLE_DICTIONARY、DELTA 系列、
''' BYTE_STREAM_SPLIT、旧式 BIT_PACKED 级别，以及 UNCOMPRESSED / SNAPPY / GZIP 压缩。
''' </para>
''' </summary>
Public NotInheritable Class ParquetReader

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 读取并解析文件尾部的 <c>FileMetaData</c>。
    ''' </summary>
    Public Shared Function ReadMetadata(input As Stream) As Format.FileMetaData
        Dim stream As Stream = EnsureSeekable(input)
        Dim length As Long = stream.Length

        If length < 12 Then
            Throw New ParquetException($"the file is too small ({length} bytes) to be a parquet file!")
        End If

        Call VerifyMagic(stream, 0, "header")
        Call VerifyMagic(stream, length - 4, "footer")

        stream.Seek(length - 8, SeekOrigin.Begin)

        Dim footerLength As Integer = ReadInt32(stream)

        If footerLength <= 0 OrElse footerLength > length - 12 Then
            Throw New ParquetException($"invalid parquet footer length: {footerLength} (file length {length})")
        End If

        stream.Seek(length - 8 - footerLength, SeekOrigin.Begin)

        Dim footer As Byte() = ReadBytes(stream, footerLength)
        Dim reader As New Thrift.ThriftCompactReader(New MemoryStream(footer))

        Return Format.FileMetaData.ReadFrom(reader)
    End Function

    ''' <summary>
    ''' 读取整个 Parquet 文件（所有 row group 合并）。
    ''' </summary>
    Public Shared Function ReadTable(input As Stream) As ParquetTable
        Dim stream As Stream = EnsureSeekable(input)
        Dim meta As Format.FileMetaData = ReadMetadata(stream)
        Dim tableSchema As Schema.ParquetSchema = Schema.ParquetSchema.FromMetadata(meta.Schema)
        Dim table As New ParquetTable With {
            .Schema = tableSchema,
            .NumRows = meta.NumRows,
            .Metadata = meta
        }

        For Each leaf As Schema.ParquetSchemaNode In tableSchema.Leaves
            Dim column As Columns.DataColumn = ReadColumn(stream, meta, leaf)

            If column IsNot Nothing Then
                Call table.Columns.Add(column)
            End If
        Next

        Return table
    End Function

    ''' <summary>
    ''' 读取单个叶子列在所有 row group 中的数据。
    ''' </summary>
    Private Shared Function ReadColumn(input As Stream, meta As Format.FileMetaData, leaf As Schema.ParquetSchemaNode) As Columns.DataColumn
        Dim definitions As New List(Of Integer)
        Dim repetitions As New List(Of Integer)
        Dim collected As New List(Of Object)
        Dim found As Boolean = False

        For Each group As Format.RowGroup In meta.RowGroups
            Dim chunk As Format.ColumnChunk = group.Columns.FirstOrDefault(Function(item) IsSamePath(item, leaf))

            If chunk Is Nothing OrElse chunk.MetaData Is Nothing Then
                Continue For
            End If

            found = True

            Call ReadColumnChunk(input, chunk, leaf, definitions, repetitions, collected)
        Next

        If Not found Then
            Return Nothing
        End If

        Return New Columns.DataColumn With {
            .Node = leaf,
            .NumSlots = definitions.Count,
            .DefinitionLevels = definitions.ToArray,
            .RepetitionLevels = repetitions.ToArray,
            .Values = Columns.ValueCast.ToTypedArray(leaf, collected)
        }
    End Function

    Private Shared Function IsSamePath(chunk As Format.ColumnChunk, leaf As Schema.ParquetSchemaNode) As Boolean
        Dim path As List(Of String) = chunk.MetaData.PathInSchema

        If path Is Nothing OrElse path.Count <> leaf.Path.Count Then
            Return False
        End If

        For i As Integer = 0 To path.Count - 1
            If Not String.Equals(path(i), leaf.Path(i), StringComparison.Ordinal) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Shared Sub ReadColumnChunk(input As Stream,
                                       chunk As Format.ColumnChunk,
                                       leaf As Schema.ParquetSchemaNode,
                                       definitions As List(Of Integer),
                                       repetitions As List(Of Integer),
                                       collected As List(Of Object))
        Dim meta As Format.ColumnMetaData = chunk.MetaData
        Dim offset As Long = meta.DataPageOffset

        If meta.DictionaryPageOffset.HasValue AndAlso meta.DictionaryPageOffset.Value > 0 Then
            offset = System.Math.Min(offset, meta.DictionaryPageOffset.Value)
        End If

        Dim endOffset As Long = offset + meta.TotalCompressedSize
        Dim dictionary As Array = Nothing
        Dim produced As Long = 0

        input.Seek(offset, SeekOrigin.Begin)

        While produced < meta.NumValues AndAlso input.Position < endOffset
            Dim header As Format.PageHeader = Format.PageHeader.ReadFrom(New Thrift.ThriftCompactReader(input))
            Dim page As Byte() = ReadBytes(input, header.CompressedPageSize)

            Select Case header.Type
                Case Format.PageType.DICTIONARY_PAGE
                    dictionary = ReadDictionaryPage(header, page, leaf, meta.Codec)
                Case Format.PageType.DATA_PAGE
                    produced += ReadDataPageV1(header, page, leaf, meta.Codec, dictionary, definitions, repetitions, collected)
                Case Format.PageType.DATA_PAGE_V2
                    produced += ReadDataPageV2(header, page, leaf, meta.Codec, dictionary, definitions, repetitions, collected)
                Case Else
                    ' INDEX_PAGE 等页面直接跳过
            End Select
        End While
    End Sub

    Private Shared Function ReadDictionaryPage(header As Format.PageHeader,
                                               page As Byte(),
                                               leaf As Schema.ParquetSchemaNode,
                                               codec As Format.CompressionCodec) As Array
        Dim body As Byte() = ParquetCodec.Decompress(codec, page, header.UncompressedPageSize)
        Dim reader As New Encodings.ByteReader(body)
        Dim count As Integer = If(header.DictionaryPage Is Nothing, 0, header.DictionaryPage.NumValues)

        Return Encodings.PlainEncoding.ReadValues(reader, count, leaf.PhysicalType.Value, leaf.TypeLength)
    End Function

    Private Shared Function ReadDataPageV1(header As Format.PageHeader,
                                           page As Byte(),
                                           leaf As Schema.ParquetSchemaNode,
                                           codec As Format.CompressionCodec,
                                           dictionary As Array,
                                           definitions As List(Of Integer),
                                           repetitions As List(Of Integer),
                                           collected As List(Of Object)) As Long
        If header.DataPage Is Nothing Then
            Throw New ParquetException($"the data page header of column '{String.Join(".", leaf.Path)}' is missing in the page header!")
        End If

        Dim body As Byte() = ParquetCodec.Decompress(codec, page, header.UncompressedPageSize)
        Dim reader As New Encodings.ByteReader(body)
        Dim pageInfo As Format.DataPageHeader = header.DataPage
        Dim numValues As Integer = pageInfo.NumValues
        Dim definition As Integer() = Nothing
        Dim repetition As Integer() = Nothing

        If leaf.RepetitionLevel > 0 Then
            Dim size As Integer = reader.ReadInt32()
            Dim section As New Encodings.ByteReader(reader.ReadBytes(size))

            repetition = DecodeLevels(section, pageInfo.RepetitionLevelEncoding, numValues, leaf.RepetitionLevel)
        End If

        If leaf.DefinitionLevel > 0 Then
            Dim size As Integer = reader.ReadInt32()
            Dim section As New Encodings.ByteReader(reader.ReadBytes(size))

            definition = DecodeLevels(section, pageInfo.DefinitionLevelEncoding, numValues, leaf.DefinitionLevel)
        End If

        Dim valueCount As Integer = If(definition Is Nothing, numValues, definition.Count(Function(level) level >= leaf.DefinitionLevel))
        Dim values As Array = DecodeValues(reader, pageInfo.ValueEncoding, leaf, valueCount, dictionary)

        Call AppendPage(numValues, definition, repetition, values, definitions, repetitions, collected)

        Return numValues
    End Function

    Private Shared Function ReadDataPageV2(header As Format.PageHeader,
                                           page As Byte(),
                                           leaf As Schema.ParquetSchemaNode,
                                           codec As Format.CompressionCodec,
                                           dictionary As Array,
                                           definitions As List(Of Integer),
                                           repetitions As List(Of Integer),
                                           collected As List(Of Object)) As Long
        If header.DataPageV2 Is Nothing Then
            Throw New ParquetException($"the data page v2 header of column '{String.Join(".", leaf.Path)}' is missing in the page header!")
        End If

        Dim pageInfo As Format.DataPageHeaderV2 = header.DataPageV2
        Dim numValues As Integer = pageInfo.NumValues
        Dim definition As Integer() = Nothing
        Dim repetition As Integer() = Nothing
        Dim position As Integer = 0

        If pageInfo.RepetitionLevelsByteLength > 0 Then
            Dim section As New Encodings.ByteReader(page, position)

            repetition = DecodeLevels(section, Format.Encoding.RLE, numValues, leaf.RepetitionLevel, hasLengthPrefix:=False)
        End If

        position += pageInfo.RepetitionLevelsByteLength

        If pageInfo.DefinitionLevelsByteLength > 0 Then
            Dim section As New Encodings.ByteReader(page, position)

            definition = DecodeLevels(section, Format.Encoding.RLE, numValues, leaf.DefinitionLevel, hasLengthPrefix:=False)
        End If

        position += pageInfo.DefinitionLevelsByteLength

        Dim dataSize As Integer = page.Length - position
        Dim data As Byte() = New Byte(dataSize - 1) {}

        System.Buffer.BlockCopy(page, position, data, 0, dataSize)

        If pageInfo.IsCompressed AndAlso codec <> Format.CompressionCodec.UNCOMPRESSED Then
            data = ParquetCodec.Decompress(codec, data, header.UncompressedPageSize - position)
        End If

        Dim valueCount As Integer = numValues - pageInfo.NumNulls
        Dim reader As New Encodings.ByteReader(data)
        Dim values As Array = DecodeValues(reader, pageInfo.ValueEncoding, leaf, valueCount, dictionary)

        Call AppendPage(numValues, definition, repetition, values, definitions, repetitions, collected)

        Return numValues
    End Function

    Private Shared Sub AppendPage(numValues As Integer,
                                  definition As Integer(),
                                  repetition As Integer(),
                                  values As Array,
                                  definitions As List(Of Integer),
                                  repetitions As List(Of Integer),
                                  collected As List(Of Object))
        For i As Integer = 0 To numValues - 1
            Call definitions.Add(If(definition Is Nothing, 0, definition(i)))
            Call repetitions.Add(If(repetition Is Nothing, 0, repetition(i)))
        Next

        If values Is Nothing Then
            Return
        End If

        For i As Integer = 0 To values.Length - 1
            Call collected.Add(values.GetValue(i))
        Next
    End Sub

    Private Shared Function DecodeLevels(reader As Encodings.ByteReader,
                                         encoding As Format.Encoding,
                                         count As Integer,
                                         maxLevel As Integer,
                                         Optional hasLengthPrefix As Boolean = False) As Integer()
        Select Case encoding
            Case Format.Encoding.RLE
                If hasLengthPrefix Then
                    Dim size As Integer = reader.ReadInt32()
                    Dim section As New Encodings.ByteReader(reader.ReadBytes(size))

                    Return Encodings.BitPacking.ReadRleBitPackedHybrid(section, count, Encodings.BitPacking.BitWidth(maxLevel))
                End If

                Return Encodings.BitPacking.ReadRleBitPackedHybrid(reader, count, Encodings.BitPacking.BitWidth(maxLevel))
            Case Format.Encoding.BIT_PACKED
                Return Encodings.BitPacking.ReadBitPackedLegacy(reader, count, Encodings.BitPacking.BitWidth(maxLevel))
            Case Else
                Throw New ParquetException($"the level encoding {encoding} is not supported!")
        End Select
    End Function

    Private Shared Function DecodeValues(reader As Encodings.ByteReader,
                                         encoding As Format.Encoding,
                                         leaf As Schema.ParquetSchemaNode,
                                         count As Integer,
                                         dictionary As Array) As Array
        Dim physical As Format.ParquetType = leaf.PhysicalType.Value

        Select Case encoding
            Case Format.Encoding.PLAIN
                Return Encodings.PlainEncoding.ReadValues(reader, count, physical, leaf.TypeLength)
            Case Format.Encoding.PLAIN_DICTIONARY, Format.Encoding.RLE_DICTIONARY
                If dictionary Is Nothing Then
                    Throw New ParquetException($"the column '{String.Join(".", leaf.Path)}' uses a dictionary encoded page without a dictionary page!")
                End If

                Return ReadDictionaryIndices(reader, count, dictionary)
            Case Format.Encoding.DELTA_BINARY_PACKED
                Dim deltas As Long() = Encodings.DeltaEncoding.ReadDeltaBinaryPacked(reader)

                If physical = Format.ParquetType.INT32 Then
                    Dim result As Integer() = New Integer(deltas.Length - 1) {}

                    For i As Integer = 0 To deltas.Length - 1
                        result(i) = CInt(deltas(i))
                    Next

                    Return result
                End If

                Return deltas
            Case Format.Encoding.DELTA_LENGTH_BYTE_ARRAY
                Return Encodings.DeltaEncoding.ReadDeltaLengthByteArray(reader)
            Case Format.Encoding.DELTA_BYTE_ARRAY
                Return Encodings.DeltaEncoding.ReadDeltaByteArray(reader)
            Case Format.Encoding.BYTE_STREAM_SPLIT
                Select Case physical
                    Case Format.ParquetType.FLOAT
                        Return Encodings.ByteStreamSplit.ReadSingles(reader, count)
                    Case Format.ParquetType.[DOUBLE]
                        Return Encodings.ByteStreamSplit.ReadDoubles(reader, count)
                    Case Else
                        Return Encodings.ByteStreamSplit.ReadFixedLen(reader, count, leaf.TypeLength)
                End Select
            Case Format.Encoding.RLE
                Dim size As Integer = reader.ReadInt32()
                Dim section As New Encodings.ByteReader(reader.ReadBytes(size))
                Dim raw As Integer() = Encodings.BitPacking.ReadRleBitPackedHybrid(section, count, 1)

                Select Case physical
                    Case Format.ParquetType.[BOOLEAN]
                        Dim result As Boolean() = New Boolean(raw.Length - 1) {}

                        For i As Integer = 0 To raw.Length - 1
                            result(i) = raw(i) <> 0
                        Next

                        Return result
                    Case Format.ParquetType.INT32
                        Return raw
                    Case Else
                        Dim result As Long() = New Long(raw.Length - 1) {}

                        For i As Integer = 0 To raw.Length - 1
                            result(i) = raw(i)
                        Next

                        Return result
                End Select
            Case Else
                Throw New ParquetException($"the value encoding {encoding} of column '{String.Join(".", leaf.Path)}' is not supported!")
        End Select
    End Function

    Private Shared Function ReadDictionaryIndices(reader As Encodings.ByteReader, count As Integer, dictionary As Array) As Array
        If count = 0 Then
            Return Array.CreateInstance(dictionary.GetType().GetElementType(), 0)
        End If

        Dim bitWidth As Integer = CInt(reader.ReadByte())
        Dim indices As Integer() = Encodings.BitPacking.ReadRleBitPackedHybrid(reader, count, bitWidth)
        Dim result As Array = Array.CreateInstance(dictionary.GetType().GetElementType(), count)

        For i As Integer = 0 To count - 1
            Dim index As Integer = indices(i)

            If index < 0 OrElse index >= dictionary.Length Then
                Throw New ParquetException($"invalid dictionary index {index} (dictionary size {dictionary.Length})!")
            End If

            result.SetValue(dictionary.GetValue(index), i)
        Next

        Return result
    End Function

    ' ------------------------------------------------------------------
    '  基础 IO
    ' ------------------------------------------------------------------

    Private Shared Function EnsureSeekable(input As Stream) As Stream
        If input Is Nothing Then
            Throw New ArgumentNullException(NameOf(input))
        End If

        If input.CanSeek Then
            Return input
        End If

        Dim memory As New MemoryStream()

        input.CopyTo(memory)

        memory.Seek(0, SeekOrigin.Begin)

        Return memory
    End Function

    Private Shared Sub VerifyMagic(input As Stream, offset As Long, part As String)
        input.Seek(offset, SeekOrigin.Begin)

        Dim buffer As Byte() = ReadBytes(input, 4)

        For i As Integer = 0 To 3
            If buffer(i) <> ParquetConstants.Magic(i) Then
                Throw New ParquetException($"invalid parquet {part} magic: expected 'PAR1' but got '{System.Text.Encoding.ASCII.GetString(buffer)}'!")
            End If
        Next
    End Sub

    Private Shared Function ReadBytes(input As Stream, count As Integer) As Byte()
        If count < 0 Then
            Throw New ParquetException($"invalid data length: {count}")
        End If

        Dim buffer As Byte() = New Byte(count - 1) {}
        Dim offset As Integer = 0

        While offset < count
            Dim n As Integer = input.Read(buffer, offset, count - offset)

            If n <= 0 Then
                Throw New ParquetException("unexpected end of the parquet file data!")
            End If

            offset += n
        End While

        Return buffer
    End Function

    Private Shared Function ReadInt32(input As Stream) As Integer
        Dim buffer As Byte() = ReadBytes(input, 4)

        If Not BitConverter.IsLittleEndian Then
            Array.Reverse(buffer)
        End If

        Return BitConverter.ToInt32(buffer, 0)
    End Function

End Class
