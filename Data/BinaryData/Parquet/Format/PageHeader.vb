Option Strict On
Option Explicit On
Option Infer On

Namespace Format

    ''' <summary>
    ''' 数据页头（对应 parquet.thrift 的 <c>DataPageHeader</c>，DataPageV1）。
    ''' </summary>
    Public Class DataPageHeader

        Public Property NumValues As Integer

        ''' <summary>
        ''' 数据区使用的编码类型。
        ''' </summary>
        Public Property ValueEncoding As Encoding = Encoding.PLAIN

        Public Property DefinitionLevelEncoding As Encoding = Encoding.RLE

        Public Property RepetitionLevelEncoding As Encoding = Encoding.RLE

        Public Property Stats As Statistics

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As DataPageHeader
            Dim header As New DataPageHeader

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        header.NumValues = r.ReadI32()
                    Case 2
                        header.ValueEncoding = CType(r.ReadI32(), Encoding)
                    Case 3
                        header.DefinitionLevelEncoding = CType(r.ReadI32(), Encoding)
                    Case 4
                        header.RepetitionLevelEncoding = CType(r.ReadI32(), Encoding)
                    Case 5
                        header.Stats = Statistics.ReadFrom(r)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return header
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()
            Call w.WriteI32Field(1, NumValues)
            Call w.WriteI32Field(2, CInt(ValueEncoding))
            Call w.WriteI32Field(3, CInt(DefinitionLevelEncoding))
            Call w.WriteI32Field(4, CInt(RepetitionLevelEncoding))

            If Stats IsNot Nothing Then
                Call w.WriteFieldHeader(5, Thrift.ThriftType.Struct)
                Call Stats.WriteTo(w)
            End If

            Call w.WriteStructEnd()
        End Sub

    End Class

    ''' <summary>
    ''' 字典页头（对应 parquet.thrift 的 <c>DictionaryPageHeader</c>）。
    ''' </summary>
    Public Class DictionaryPageHeader

        Public Property NumValues As Integer

        Public Property DictionaryEncoding As Encoding = Encoding.PLAIN

        Public Property IsSorted As Boolean

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As DictionaryPageHeader
            Dim header As New DictionaryPageHeader

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        header.NumValues = r.ReadI32()
                    Case 2
                        header.DictionaryEncoding = CType(r.ReadI32(), Encoding)
                    Case 3
                        header.IsSorted = r.ReadBoolField(field.type)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return header
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()
            Call w.WriteI32Field(1, NumValues)
            Call w.WriteI32Field(2, CInt(DictionaryEncoding))

            If IsSorted Then
                Call w.WriteBoolField(3, True)
            End If

            Call w.WriteStructEnd()
        End Sub

    End Class

    ''' <summary>
    ''' 数据页头（对应 parquet.thrift 的 <c>DataPageHeaderV2</c>）。
    ''' </summary>
    Public Class DataPageHeaderV2

        Public Property NumValues As Integer

        Public Property NumNulls As Integer

        Public Property NumRows As Integer

        Public Property ValueEncoding As Encoding = Encoding.PLAIN

        Public Property DefinitionLevelsByteLength As Integer

        Public Property RepetitionLevelsByteLength As Integer

        Public Property IsCompressed As Boolean = True

        Public Property Stats As Statistics

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As DataPageHeaderV2
            Dim header As New DataPageHeaderV2

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        header.NumValues = r.ReadI32()
                    Case 2
                        header.NumNulls = r.ReadI32()
                    Case 3
                        header.NumRows = r.ReadI32()
                    Case 4
                        header.ValueEncoding = CType(r.ReadI32(), Encoding)
                    Case 5
                        header.DefinitionLevelsByteLength = r.ReadI32()
                    Case 6
                        header.RepetitionLevelsByteLength = r.ReadI32()
                    Case 7
                        header.IsCompressed = r.ReadBoolField(field.type)
                    Case 8
                        header.Stats = Statistics.ReadFrom(r)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return header
        End Function

    End Class

    ''' <summary>
    ''' 页面头（对应 parquet.thrift 的 <c>PageHeader</c>），紧跟在页面数据之前。
    ''' </summary>
    Public Class PageHeader

        Public Property Type As PageType = PageType.DATA_PAGE

        Public Property UncompressedPageSize As Integer

        Public Property CompressedPageSize As Integer

        Public Property Crc As Integer?

        Public Property DataPage As DataPageHeader

        Public Property DictionaryPage As DictionaryPageHeader

        Public Property DataPageV2 As DataPageHeaderV2

        ''' <summary>
        ''' 从数据流（页面头起始位置）解析一个页面头。
        ''' </summary>
        Public Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As PageHeader
            Dim header As New PageHeader

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        header.Type = CType(r.ReadI32(), PageType)
                    Case 2
                        header.UncompressedPageSize = r.ReadI32()
                    Case 3
                        header.CompressedPageSize = r.ReadI32()
                    Case 4
                        header.Crc = r.ReadI32()
                    Case 5
                        header.DataPage = DataPageHeader.ReadFrom(r)
                    Case 7
                        header.DictionaryPage = DictionaryPageHeader.ReadFrom(r)
                    Case 8
                        header.DataPageV2 = DataPageHeaderV2.ReadFrom(r)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return header
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()

            Call w.WriteI32Field(1, CInt(Type))
            Call w.WriteI32Field(2, UncompressedPageSize)
            Call w.WriteI32Field(3, CompressedPageSize)

            Select Case Type
                Case PageType.DATA_PAGE
                    Call w.WriteFieldHeader(5, Thrift.ThriftType.Struct)
                    Call DataPage.WriteTo(w)
                Case PageType.DICTIONARY_PAGE
                    Call w.WriteFieldHeader(7, Thrift.ThriftType.Struct)
                    Call DictionaryPage.WriteTo(w)
                Case PageType.DATA_PAGE_V2
                    Throw New ParquetException("writing DataPageV2 is not supported by this implementation!")
            End Select

            Call w.WriteStructEnd()
        End Sub

    End Class

End Namespace
