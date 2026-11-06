Option Strict On
Option Explicit On
Option Infer On

Imports System.IO
Imports System.Linq

''' <summary>
''' Parquet 文件的写入器。
''' <para>
''' 写入端统一使用规范编码：数据页 <c>PLAIN</c>、级别 <c>RLE</c>（bit-packed hybrid），
''' 可选的 SNAPPY / GZIP / UNCOMPRESSED 压缩，因此生成的文件可以被 PyArrow / pandas 直接打开。
''' </para>
''' </summary>
Public NotInheritable Class ParquetWriter

    Private Sub New()
    End Sub

    ''' <summary>
    ''' 将写入计划序列化为 Parquet 文件。
    ''' </summary>
    Public Shared Sub Write(plan As WritePlan, output As Stream, options As ParquetOptions)
        If plan Is Nothing Then
            Throw New ArgumentNullException(NameOf(plan))
        End If
        If output Is Nothing Then
            Throw New ArgumentNullException(NameOf(output))
        End If

        Dim opts As ParquetOptions = If(options, ParquetOptions.Defaults).Clone()

        If opts.RowGroupSize <= 0 Then
            opts.RowGroupSize = 1000000
        End If

        Dim meta As New Format.FileMetaData With {
            .Version = 2,
            .NumRows = plan.RowCount,
            .Schema = Schema.ParquetSchema.ToMetadata(plan.Root),
            .CreatedBy = $"Microsoft.VisualBasic.DataStorage.Parquet version {GetType(ParquetWriter).Assembly.GetName().Version}"
        }

        Call output.Write(ParquetConstants.Magic, 0, ParquetConstants.Magic.Length)

        Dim rowStart As Integer = 0

        While rowStart < plan.RowCount
            Dim rowCount As Integer = System.Math.Min(opts.RowGroupSize, plan.RowCount - rowStart)
            Dim group As New Format.RowGroup With {
                .NumRows = rowCount,
                .FileOffset = output.Position
            }
            Dim columns As New List(Of Columns.DataColumn)

            For Each column As ColumnPlan In plan.Columns
                Select Case column.Kind
                    Case ColumnKind.Leaf
                        Call columns.Add(BuildFlatColumn(column, rowStart, rowCount))
                    Case Else
                        Call columns.AddRange(Nested.JsonShredder.ShredRange(column, rowStart, rowCount))
                End Select
            Next

            For Each column As Columns.DataColumn In columns
                Dim chunk As Format.ColumnChunk = WriteColumnChunk(output, column, opts)

                Call group.Columns.Add(chunk)
                group.TotalByteSize += chunk.MetaData.TotalUncompressedSize
            Next

            Call meta.RowGroups.Add(group)

            rowStart += rowCount
        End While

        ' ---- footer ----
        Dim footer As New MemoryStream()

        Dim writer As New Thrift.ThriftCompactWriter(footer)

        Call meta.WriteTo(writer)

        Dim footerBytes As Byte() = footer.ToArray()

        Call output.Write(footerBytes, 0, footerBytes.Length)
        Call Encodings.ByteWriter.WriteInt32LE(output, footerBytes.Length)
        Call output.Write(ParquetConstants.Magic, 0, ParquetConstants.Magic.Length)
        Call output.Flush()
    End Sub

    ''' <summary>
    ''' 构建扁平（单层）叶子列在指定行区间内的数据。
    ''' </summary>
    Private Shared Function BuildFlatColumn(plan As ColumnPlan, rowStart As Integer, rowCount As Integer) As Columns.DataColumn
        Dim node As Schema.ParquetSchemaNode = plan.Node
        Dim maxDef As Integer = node.DefinitionLevel
        Dim definitions As Integer()

        If maxDef = 0 Then
            definitions = New Integer() {}
        Else
            definitions = Columns.ArrayUtil.NewArray(Of Integer)(rowCount)

            For i As Integer = 0 To rowCount - 1
                definitions(i) = maxDef
            Next
        End If

        Dim values As Array

        If node.PhysicalType.Value = Format.ParquetType.BYTE_ARRAY Then
            ' 字符串列需要转换为 UTF8 字节序列
            Dim texts As String() = CType(plan.Values, String())

            If plan.NullMask Is Nothing Then
                Dim bytes As Byte()() = Columns.ArrayUtil.NewArray(Of Byte())(rowCount)

                For i As Integer = 0 To rowCount - 1
                    bytes(i) = Utf8(texts(rowStart + i))
                Next

                values = bytes
            Else
                Dim defined As New List(Of Object)

                For i As Integer = 0 To rowCount - 1
                    Dim row As Integer = rowStart + i

                    If plan.NullMask(row) Then
                        If maxDef > 0 Then
                            definitions(i) = maxDef - 1
                        End If
                    Else
                        Call defined.Add(Utf8(texts(row)))
                    End If
                Next

                values = Columns.ValueCast.ToTypedArray(node, defined)
            End If
        ElseIf plan.NullMask Is Nothing Then
            values = Slice(plan.Values, rowStart, rowCount)
        Else
            Dim defined As New List(Of Object)

            For i As Integer = 0 To rowCount - 1
                Dim row As Integer = rowStart + i

                If plan.NullMask(row) Then
                    If maxDef > 0 Then
                        definitions(i) = maxDef - 1
                    End If
                Else
                    Call defined.Add(plan.Values.GetValue(row))
                End If
            Next

            values = Columns.ValueCast.ToTypedArray(node, defined)
        End If

        Return New Columns.DataColumn With {
            .Node = node,
            .NumSlots = rowCount,
            .DefinitionLevels = definitions,
            .RepetitionLevels = New Integer() {},
            .Values = values
        }
    End Function

    ''' <summary>
    ''' 将字符串转换为 UTF8 字节序列（空字符串与空值都写为空字节数组）。
    ''' </summary>
    Private Shared Function Utf8(text As String) As Byte()
        If text Is Nothing Then
            Return New Byte() {}
        End If

        Return System.Text.Encoding.UTF8.GetBytes(text)
    End Function

    ''' <summary>
    ''' 按索引区间截取数组（返回同类型的数组副本）。
    ''' </summary>
    Private Shared Function Slice(values As Array, start As Integer, count As Integer) As Array
        If values Is Nothing Then
            Return Array.CreateInstance(GetType(Object), 0)
        End If

        Dim elementType As Type = values.GetType().GetElementType()
        Dim result As Array = Array.CreateInstance(elementType, count)

        If count > 0 Then
            Array.Copy(values, start, result, 0, System.Math.Min(count, values.Length - start))
        End If

        Return result
    End Function

    ''' <summary>
    ''' 写出一个列块（当前实现只包含一个 DataPageV1）。
    ''' </summary>
    Private Shared Function WriteColumnChunk(output As Stream,
                                             column As Columns.DataColumn,
                                             options As ParquetOptions) As Format.ColumnChunk
        Dim node As Schema.ParquetSchemaNode = column.Node
        Dim maxRep As Integer = node.RepetitionLevel
        Dim maxDef As Integer = node.DefinitionLevel
        Dim body As New MemoryStream()

        If maxRep > 0 Then
            Call WriteLevels(body, column.RepetitionLevels, maxRep)
        End If
        If maxDef > 0 Then
            Call WriteLevels(body, column.DefinitionLevels, maxDef)
        End If

        Call WritePlainValues(body, node, column.Values)

        Dim raw As Byte() = body.ToArray()
        Dim compressed As Byte() = ParquetCodec.Compress(options.Compression, raw)
        Dim header As New Format.PageHeader With {
            .Type = Format.PageType.DATA_PAGE,
            .UncompressedPageSize = raw.Length,
            .CompressedPageSize = compressed.Length,
            .DataPage = New Format.DataPageHeader With {
                .NumValues = column.Count,
                .ValueEncoding = Format.Encoding.PLAIN,
                .DefinitionLevelEncoding = Format.Encoding.RLE,
                .RepetitionLevelEncoding = Format.Encoding.RLE
            }
        }

        If options.WriteStatistics Then
            header.DataPage.Stats = Statistics(column)
        End If

        Dim headerBuffer As New MemoryStream()

        Dim writer As New Thrift.ThriftCompactWriter(headerBuffer)

        Call header.WriteTo(writer)

        Dim headerBytes As Byte() = headerBuffer.ToArray()
        Dim offset As Long = output.Position

        Call output.Write(headerBytes, 0, headerBytes.Length)
        Call output.Write(compressed, 0, compressed.Length)

        Dim chunk As New Format.ColumnChunk With {
            .FileOffset = offset,
            .MetaData = New Format.ColumnMetaData With {
                .Type = node.PhysicalType.Value,
                .Codec = options.Compression,
                .NumValues = column.Count,
                .DataPageOffset = offset,
                .TotalCompressedSize = headerBytes.Length + compressed.Length,
                .TotalUncompressedSize = headerBytes.Length + raw.Length,
                .Stats = If(options.WriteStatistics, header.DataPage.Stats, Nothing),
                .MaxDefinitionLevel = maxDef,
                .MaxRepetitionLevel = maxRep
            }
        }

        Call chunk.MetaData.Encodings.Add(Format.Encoding.RLE)
        Call chunk.MetaData.Encodings.Add(Format.Encoding.PLAIN)
        Call chunk.MetaData.PathInSchema.AddRange(node.Path)

        Return chunk
    End Function

    Private Shared Sub WriteLevels(output As Stream, levels As Integer(), maxLevel As Integer)
        Dim buffer As New MemoryStream()

        Call Encodings.BitPacking.WriteRleBitPackedHybrid(buffer, levels, Encodings.BitPacking.BitWidth(maxLevel))
        Call Encodings.ByteWriter.WriteInt32LE(output, CInt(buffer.Length))
        Call buffer.WriteTo(output)
    End Sub

    Private Shared Sub WritePlainValues(output As Stream, node As Schema.ParquetSchemaNode, values As Array)
        Select Case node.PhysicalType.Value
            Case Format.ParquetType.[BOOLEAN]
                Call Encodings.PlainEncoding.WriteBooleans(output, CType(values, Boolean()))
            Case Format.ParquetType.INT32
                Call Encodings.PlainEncoding.WriteInt32(output, CType(values, Integer()))
            Case Format.ParquetType.INT64
                Call Encodings.PlainEncoding.WriteInt64(output, CType(values, Long()))
            Case Format.ParquetType.FLOAT
                Call Encodings.PlainEncoding.WriteSingle(output, CType(values, Single()))
            Case Format.ParquetType.[DOUBLE]
                Call Encodings.PlainEncoding.WriteDouble(output, CType(values, Double()))
            Case Format.ParquetType.BYTE_ARRAY
                Call Encodings.PlainEncoding.WriteByteArrays(output, CType(values, Byte()()))
            Case Format.ParquetType.FIXED_LEN_BYTE_ARRAY
                Call Encodings.PlainEncoding.WriteFixedLenByteArrays(output, CType(values, Byte()()), node.TypeLength)
            Case Else
                Throw New ParquetException($"unsupported parquet physical type for writing: {node.PhysicalType}")
        End Select
    End Sub

    ''' <summary>
    ''' 计算列统计信息（null 个数以及数值类型的最小值 / 最大值）。
    ''' </summary>
    Private Shared Function Statistics(column As Columns.DataColumn) As Format.Statistics
        Dim stats As New Format.Statistics With {
            .NullCount = CLng(column.Count - column.DefinedCount)
        }

        If column.Values Is Nothing OrElse column.Values.Length = 0 Then
            Return stats
        End If

        Select Case column.Node.PhysicalType.Value
            Case Format.ParquetType.INT32
                Dim data As Integer() = CType(column.Values, Integer())
                Dim min As Integer = Integer.MaxValue
                Dim max As Integer = Integer.MinValue

                For Each value As Integer In data
                    If value < min Then min = value
                    If value > max Then max = value
                Next

                stats.MinValue = BitConverter.GetBytes(min)
                stats.MaxValue = BitConverter.GetBytes(max)
            Case Format.ParquetType.INT64
                Dim data As Long() = CType(column.Values, Long())
                Dim min As Long = Long.MaxValue
                Dim max As Long = Long.MinValue

                For Each value As Long In data
                    If value < min Then min = value
                    If value > max Then max = value
                Next

                stats.MinValue = BitConverter.GetBytes(min)
                stats.MaxValue = BitConverter.GetBytes(max)
            Case Format.ParquetType.FLOAT
                Dim data As Single() = CType(column.Values, Single())

                If Not data.Any(Function(v) Single.IsNaN(v)) Then
                    stats.MinValue = BitConverter.GetBytes(data.Min)
                    stats.MaxValue = BitConverter.GetBytes(data.Max)
                End If
            Case Format.ParquetType.[DOUBLE]
                Dim data As Double() = CType(column.Values, Double())

                If Not data.Any(Function(v) Double.IsNaN(v)) Then
                    stats.MinValue = BitConverter.GetBytes(data.Min)
                    stats.MaxValue = BitConverter.GetBytes(data.Max)
                End If
            Case Else
                ' 字符串与布尔类型只写入 null_count
        End Select

        Return stats
    End Function

End Class
