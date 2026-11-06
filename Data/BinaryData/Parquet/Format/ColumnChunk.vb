Option Strict On
Option Explicit On
Option Infer On

Namespace Format

    ''' <summary>
    ''' 列统计信息（对应 parquet.thrift 的 <c>Statistics</c>）。
    ''' </summary>
    Public Class Statistics

        ''' <summary>
        ''' 最大值（旧字段，已废弃）。
        ''' </summary>
        Public Property Max As Byte()

        ''' <summary>
        ''' 最小值（旧字段，已废弃）。
        ''' </summary>
        Public Property Min As Byte()

        Public Property NullCount As Long?

        Public Property DistinctCount As Long?

        ''' <summary>
        ''' 最大值（PLAIN 编码的字节序列）。
        ''' </summary>
        Public Property MaxValue As Byte()

        ''' <summary>
        ''' 最小值（PLAIN 编码的字节序列）。
        ''' </summary>
        Public Property MinValue As Byte()

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As Statistics
            Dim stats As New Statistics

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        stats.Max = r.ReadBinary()
                    Case 2
                        stats.Min = r.ReadBinary()
                    Case 3
                        stats.NullCount = r.ReadI64()
                    Case 4
                        stats.DistinctCount = r.ReadI64()
                    Case 5
                        stats.MaxValue = r.ReadBinary()
                    Case 6
                        stats.MinValue = r.ReadBinary()
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return stats
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()

            If NullCount.HasValue Then
                Call w.WriteI64Field(3, NullCount.Value)
            End If
            If MaxValue IsNot Nothing Then
                Call w.WriteBinaryField(5, MaxValue)
            End If
            If MinValue IsNot Nothing Then
                Call w.WriteBinaryField(6, MinValue)
            End If

            Call w.WriteStructEnd()
        End Sub

        Public Overrides Function ToString() As String
            Return $"nulls={If(NullCount, 0)}"
        End Function

    End Class

    ''' <summary>
    ''' 键值对元数据（对应 parquet.thrift 的 <c>KeyValue</c>）。
    ''' </summary>
    Public Class KeyValue

        Public Property Key As String

        Public Property Value As String

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As KeyValue
            Dim kv As New KeyValue

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        kv.Key = r.ReadString()
                    Case 2
                        kv.Value = r.ReadString()
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return kv
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()
            Call w.WriteStringField(1, If(Key, ""))
            Call w.WriteStringField(2, If(Value, ""))
            Call w.WriteStructEnd()
        End Sub

    End Class

    ''' <summary>
    ''' 列块的元数据（对应 parquet.thrift 的 <c>ColumnMetaData</c>）。
    ''' </summary>
    Public Class ColumnMetaData

        Public Property Type As ParquetType = ParquetType.BYTE_ARRAY

        Public Property Encodings As New List(Of Encoding)

        Public Property PathInSchema As New List(Of String)

        Public Property Codec As CompressionCodec = CompressionCodec.UNCOMPRESSED

        Public Property NumValues As Long

        Public Property TotalUncompressedSize As Long

        Public Property TotalCompressedSize As Long

        Public Property DataPageOffset As Long

        Public Property DictionaryPageOffset As Long?

        Public Property Stats As Statistics

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As ColumnMetaData
            Dim meta As New ColumnMetaData

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        meta.Type = CType(r.ReadI32(), ParquetType)
                    Case 2
                        Dim header = r.ReadListHeader()

                        For i As Integer = 0 To header.size - 1
                            Call meta.Encodings.Add(CType(r.ReadI32(), Encoding))
                        Next
                    Case 3
                        Dim header = r.ReadListHeader()

                        For i As Integer = 0 To header.size - 1
                            Call meta.PathInSchema.Add(r.ReadString())
                        Next
                    Case 4
                        meta.Codec = CType(r.ReadI32(), CompressionCodec)
                    Case 5
                        meta.NumValues = r.ReadI64()
                    Case 6
                        meta.TotalUncompressedSize = r.ReadI64()
                    Case 7
                        meta.TotalCompressedSize = r.ReadI64()
                    Case 9
                        meta.DataPageOffset = r.ReadI64()
                    Case 11
                        meta.DictionaryPageOffset = r.ReadI64()
                    Case 12
                        meta.Stats = Statistics.ReadFrom(r)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return meta
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()

            Call w.WriteI32Field(1, CInt(Type))

            Call w.WriteFieldHeader(2, Thrift.ThriftType.List)
            Call w.WriteListHeader(Encodings.Count, Thrift.ThriftType.I32)

            For Each enc As Encoding In Encodings
                Call w.WriteZigZag(CLng(CInt(enc)))
            Next

            Call w.WriteFieldHeader(3, Thrift.ThriftType.List)
            Call w.WriteListHeader(PathInSchema.Count, Thrift.ThriftType.Binary)

            For Each name As String In PathInSchema
                Call w.WriteBinary(System.Text.Encoding.UTF8.GetBytes(name))
            Next

            Call w.WriteI32Field(4, CInt(Codec))
            Call w.WriteI64Field(5, NumValues)
            Call w.WriteI64Field(6, TotalUncompressedSize)
            Call w.WriteI64Field(7, TotalCompressedSize)
            Call w.WriteI64Field(9, DataPageOffset)

            If DictionaryPageOffset.HasValue Then
                Call w.WriteI64Field(11, DictionaryPageOffset.Value)
            End If

            If Stats IsNot Nothing Then
                Call w.WriteFieldHeader(12, Thrift.ThriftType.Struct)
                Call Stats.WriteTo(w)
            End If

            Call w.WriteStructEnd()
        End Sub

    End Class

    ''' <summary>
    ''' 列块（对应 parquet.thrift 的 <c>ColumnChunk</c>）。
    ''' </summary>
    Public Class ColumnChunk

        Public Property FilePath As String

        ''' <summary>
        ''' 列块在文件中的起始偏移量。
        ''' </summary>
        Public Property FileOffset As Long

        Public Property MetaData As ColumnMetaData

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As ColumnChunk
            Dim chunk As New ColumnChunk

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        chunk.FilePath = r.ReadString()
                    Case 2
                        chunk.FileOffset = r.ReadI64()
                    Case 3
                        chunk.MetaData = ColumnMetaData.ReadFrom(r)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return chunk
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()
            Call w.WriteI64Field(2, FileOffset)

            If MetaData IsNot Nothing Then
                Call w.WriteFieldHeader(3, Thrift.ThriftType.Struct)
                Call MetaData.WriteTo(w)
            End If

            Call w.WriteStructEnd()
        End Sub

    End Class

    ''' <summary>
    ''' 行组（对应 parquet.thrift 的 <c>RowGroup</c>）。
    ''' </summary>
    Public Class RowGroup

        Public Property Columns As New List(Of ColumnChunk)

        Public Property TotalByteSize As Long

        Public Property NumRows As Long

        Public Property FileOffset As Long?

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As RowGroup
            Dim group As New RowGroup

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        Dim header = r.ReadListHeader()

                        For i As Integer = 0 To header.size - 1
                            Call group.Columns.Add(ColumnChunk.ReadFrom(r))
                        Next
                    Case 2
                        group.TotalByteSize = r.ReadI64()
                    Case 3
                        group.NumRows = r.ReadI64()
                    Case 5
                        group.FileOffset = r.ReadI64()
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return group
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()

            Call w.WriteStructListField(1, Columns.Count)

            For Each column As ColumnChunk In Columns
                Call column.WriteTo(w)
            Next

            Call w.WriteI64Field(2, TotalByteSize)
            Call w.WriteI64Field(3, NumRows)

            If FileOffset.HasValue Then
                Call w.WriteI64Field(5, FileOffset.Value)
            End If

            Call w.WriteStructEnd()
        End Sub

    End Class

    ''' <summary>
    ''' 文件的整体元数据，位于文件尾部的 footer 中（对应 parquet.thrift 的 <c>FileMetaData</c>）。
    ''' </summary>
    Public Class FileMetaData

        Public Property Version As Integer = 1

        Public Property Schema As New List(Of SchemaElement)

        Public Property NumRows As Long

        Public Property RowGroups As New List(Of RowGroup)

        Public Property KeyValueMetadata As New List(Of KeyValue)

        Public Property CreatedBy As String

        ''' <summary>
        ''' 需要写入的 <c>column_orders</c> 条目数量（通常等于叶子列数量，全部为 TYPE_ORDER）。
        ''' 只有在声明了 column order 之后，外部读取器才会信任统计信息中的 min / max。
        ''' </summary>
        Public Property ColumnOrders As Integer

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As FileMetaData
            Dim meta As New FileMetaData

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        meta.Version = r.ReadI32()
                    Case 2
                        Dim header = r.ReadListHeader()

                        For i As Integer = 0 To header.size - 1
                            Call meta.Schema.Add(SchemaElement.ReadFrom(r))
                        Next
                    Case 3
                        meta.NumRows = r.ReadI64()
                    Case 4
                        Dim header = r.ReadListHeader()

                        For i As Integer = 0 To header.size - 1
                            Call meta.RowGroups.Add(RowGroup.ReadFrom(r))
                        Next
                    Case 5
                        Dim header = r.ReadListHeader()

                        For i As Integer = 0 To header.size - 1
                            Call meta.KeyValueMetadata.Add(KeyValue.ReadFrom(r))
                        Next
                    Case 6
                        meta.CreatedBy = r.ReadString()
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return meta
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()

            Call w.WriteI32Field(1, Version)
            Call w.WriteStructListField(2, Schema.Count)

            For Each element As SchemaElement In Schema
                Call element.WriteTo(w)
            Next

            Call w.WriteI64Field(3, NumRows)
            Call w.WriteStructListField(4, RowGroups.Count)

            For Each group As RowGroup In RowGroups
                Call group.WriteTo(w)
            Next

            If KeyValueMetadata.Count > 0 Then
                Call w.WriteStructListField(5, KeyValueMetadata.Count)

                For Each kv As KeyValue In KeyValueMetadata
                    Call kv.WriteTo(w)
                Next
            End If

            If Not String.IsNullOrEmpty(CreatedBy) Then
                Call w.WriteStringField(6, CreatedBy)
            End If

            If ColumnOrders > 0 Then
                Call w.WriteFieldHeader(7, Thrift.ThriftType.List)
                Call w.WriteListHeader(ColumnOrders, Thrift.ThriftType.Struct)

                For i As Integer = 1 To ColumnOrders
                    ' ColumnOrder union: 1 = TypeDefinedOrder（空结构体）
                    Call w.WriteStructBegin()
                    Call w.WriteFieldHeader(1, Thrift.ThriftType.Struct)
                    Call w.WriteStructBegin()
                    Call w.WriteStructEnd()
                    Call w.WriteStructEnd()
                Next
            End If

            Call w.WriteStructEnd()
        End Sub

    End Class

End Namespace
