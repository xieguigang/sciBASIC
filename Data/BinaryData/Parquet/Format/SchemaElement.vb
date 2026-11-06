Option Strict On
Option Explicit On
Option Infer On

Namespace Format

    ''' <summary>
    ''' Parquet 的逻辑类型（<c>LogicalType</c> union 的扁平化表示）。
    ''' </summary>
    Public Class LogicalType

        Public Const KindString As String = "STRING"
        Public Const KindMap As String = "MAP"
        Public Const KindList As String = "LIST"
        Public Const KindEnum As String = "ENUM"
        Public Const KindDecimal As String = "DECIMAL"
        Public Const KindDate As String = "DATE"
        Public Const KindTime As String = "TIME"
        Public Const KindTimestamp As String = "TIMESTAMP"
        Public Const KindInteger As String = "INTEGER"
        Public Const KindUnknown As String = "UNKNOWN"
        Public Const KindJson As String = "JSON"
        Public Const KindBson As String = "BSON"
        Public Const KindUuid As String = "UUID"

        ''' <summary>
        ''' 逻辑类型的名称，取值参见本类型中的 <c>Kind*</c> 常量。
        ''' </summary>
        Public Property Kind As String

        ''' <summary>
        ''' DECIMAL 类型的小数位数。
        ''' </summary>
        Public Property Scale As Integer

        ''' <summary>
        ''' DECIMAL 类型的精度。
        ''' </summary>
        Public Property Precision As Integer

        ''' <summary>
        ''' TIME / TIMESTAMP 类型是否为 UTC 时间。
        ''' </summary>
        Public Property IsAdjustedToUtc As Boolean

        ''' <summary>
        ''' TIME / TIMESTAMP 类型的时间单位：<c>MILLIS</c> / <c>MICROS</c> / <c>NANOS</c>。
        ''' </summary>
        Public Property TimeUnit As String = "MILLIS"

        ''' <summary>
        ''' INTEGER 类型的位宽（8 / 16 / 32 / 64）。
        ''' </summary>
        Public Property BitWidth As Integer

        ''' <summary>
        ''' INTEGER 类型是否为有符号整数。
        ''' </summary>
        Public Property IsSigned As Boolean = True

        Public Shared Function TimestampMillis(Optional adjustedToUtc As Boolean = True) As LogicalType
            Return New LogicalType With {
                .Kind = KindTimestamp,
                .IsAdjustedToUtc = adjustedToUtc,
                .TimeUnit = "MILLIS"
            }
        End Function

        Public Shared Function MakeString() As LogicalType
            Return New LogicalType With {.Kind = KindString}
        End Function

        Public Shared Function MakeList() As LogicalType
            Return New LogicalType With {.Kind = KindList}
        End Function

        Public Shared Function MakeMap() As LogicalType
            Return New LogicalType With {.Kind = KindMap}
        End Function

        Public Shared Function MakeDate() As LogicalType
            Return New LogicalType With {.Kind = KindDate}
        End Function

        Public Overrides Function ToString() As String
            Return Kind
        End Function

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As LogicalType
            Dim result As New LogicalType
            Dim hasValue As Boolean = False

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        result.Kind = KindString
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 2
                        result.Kind = KindMap
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 3
                        result.Kind = KindList
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 4
                        result.Kind = KindEnum
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 5
                        Call ReadDecimal(r, result)
                        hasValue = True
                    Case 6
                        result.Kind = KindDate
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 7
                        Call ReadTimeLike(r, result)
                        result.Kind = KindTime
                        hasValue = True
                    Case 8
                        Call ReadTimeLike(r, result)
                        result.Kind = KindTimestamp
                        hasValue = True
                    Case 10
                        Call ReadInteger(r, result)
                        hasValue = True
                    Case 11
                        result.Kind = KindUnknown
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 12
                        result.Kind = KindJson
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 13
                        result.Kind = KindBson
                        Call r.Skip(field.type)
                        hasValue = True
                    Case 14
                        result.Kind = KindUuid
                        Call r.Skip(field.type)
                        hasValue = True
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            If Not hasValue Then
                Return Nothing
            End If

            Return result
        End Function

        Private Shared Sub ReadDecimal(r As Thrift.ThriftCompactReader, target As LogicalType)
            target.Kind = KindDecimal

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        target.Scale = r.ReadI32()
                    Case 2
                        target.Precision = r.ReadI32()
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()
        End Sub

        Private Shared Sub ReadTimeLike(r As Thrift.ThriftCompactReader, target As LogicalType)
            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        target.IsAdjustedToUtc = r.ReadBoolField(field.type)
                    Case 2
                        target.TimeUnit = ReadTimeUnit(r)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()
        End Sub

        Private Shared Function ReadTimeUnit(r As Thrift.ThriftCompactReader) As String
            Dim unit As String = "MILLIS"

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        unit = "MILLIS"
                        Call r.Skip(field.type)
                    Case 2
                        unit = "MICROS"
                        Call r.Skip(field.type)
                    Case 3
                        unit = "NANOS"
                        Call r.Skip(field.type)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return unit
        End Function

        Private Shared Sub ReadInteger(r As Thrift.ThriftCompactReader, target As LogicalType)
            target.Kind = KindInteger

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        target.BitWidth = CInt(r.ReadByte())
                    Case 2
                        target.IsSigned = r.ReadBoolField(field.type)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()
        End Sub

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            If Kind Is Nothing Then
                Return
            End If

            Call w.WriteStructBegin()

            Select Case Kind
                Case KindString
                    Call WriteEmptyUnion(w, 1)
                Case KindMap
                    Call WriteEmptyUnion(w, 2)
                Case KindList
                    Call WriteEmptyUnion(w, 3)
                Case KindEnum
                    Call WriteEmptyUnion(w, 4)
                Case KindDecimal
                    Call w.WriteFieldHeader(5, Thrift.ThriftType.Struct)
                    Call w.WriteStructBegin()
                    Call w.WriteI32Field(1, Scale)
                    Call w.WriteI32Field(2, Precision)
                    Call w.WriteStructEnd()
                Case KindDate
                    Call WriteEmptyUnion(w, 6)
                Case KindTime
                    Call WriteTimeLike(w, 7)
                Case KindTimestamp
                    Call WriteTimeLike(w, 8)
                Case KindInteger
                    Call w.WriteFieldHeader(10, Thrift.ThriftType.Struct)
                    Call w.WriteStructBegin()
                    Call w.WriteFieldHeader(1, Thrift.ThriftType.Byte)
                    Call w.WriteByte(CByte(BitWidth))
                    Call w.WriteBoolField(2, IsSigned)
                    Call w.WriteStructEnd()
                Case KindUnknown
                    Call WriteEmptyUnion(w, 11)
                Case KindJson
                    Call WriteEmptyUnion(w, 12)
                Case KindBson
                    Call WriteEmptyUnion(w, 13)
                Case KindUuid
                    Call WriteEmptyUnion(w, 14)
                Case Else
                    Throw New ParquetException($"unsupported parquet logical type: {Kind}")
            End Select

            Call w.WriteStructEnd()
        End Sub

        Private Sub WriteTimeLike(w As Thrift.ThriftCompactWriter, fieldId As Integer)
            Call w.WriteFieldHeader(fieldId, Thrift.ThriftType.Struct)
            Call w.WriteStructBegin()
            Call w.WriteBoolField(1, IsAdjustedToUtc)
            Call w.WriteFieldHeader(2, Thrift.ThriftType.Struct)
            Call w.WriteStructBegin()
            Call WriteEmptyUnion(w, TimeUnitId())
            Call w.WriteStructEnd()
            Call w.WriteStructEnd()
        End Sub

        Private Function TimeUnitId() As Integer
            Select Case UCase(If(TimeUnit, "MILLIS"))
                Case "MICROS"
                    Return 2
                Case "NANOS"
                    Return 3
                Case Else
                    Return 1
            End Select
        End Function

        Private Shared Sub WriteEmptyUnion(w As Thrift.ThriftCompactWriter, fieldId As Integer)
            Call w.WriteFieldHeader(fieldId, Thrift.ThriftType.Struct)
            Call w.WriteStructBegin()
            Call w.WriteStructEnd()
        End Sub

    End Class

    ''' <summary>
    ''' Parquet 的 schema 元素（对应 parquet.thrift 的 <c>SchemaElement</c>）。
    ''' <para>
    ''' 文件 footer 中的 schema 字段是一个<b>平坦</b>的列表，通过 <see cref="NumChildren"/> 还原为树。
    ''' </para>
    ''' </summary>
    Public Class SchemaElement

        Public Property Type As ParquetType?

        ''' <summary>
        ''' 只有 FIXED_LEN_BYTE_ARRAY 会使用该字段。
        ''' </summary>
        Public Property TypeLength As Integer?

        Public Property Repetition As FieldRepetitionType?

        Public Property Name As String

        ''' <summary>
        ''' 子节点数量，非零表示当前元素是一个 group。
        ''' </summary>
        Public Property NumChildren As Integer?

        Public Property Converted As ConvertedType?

        Public Property Scale As Integer?

        Public Property Precision As Integer?

        Public Property FieldId As Integer?

        Public Property Logical As LogicalType

        Public ReadOnly Property IsGroup As Boolean
            Get
                Return NumChildren.HasValue AndAlso NumChildren.Value > 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Name}[{If(Type.HasValue, Type.Value.ToString, "group")}]"
        End Function

        Friend Shared Function ReadFrom(r As Thrift.ThriftCompactReader) As SchemaElement
            Dim element As New SchemaElement

            Call r.ReadStructBegin()

            Dim field = r.ReadFieldHeader()

            While field.type <> Thrift.ThriftType.Stop
                Select Case field.id
                    Case 1
                        element.Type = CType(r.ReadI32(), ParquetType)
                    Case 2
                        element.TypeLength = r.ReadI32()
                    Case 3
                        element.Repetition = CType(r.ReadI32(), FieldRepetitionType)
                    Case 4
                        element.Name = r.ReadString()
                    Case 5
                        element.NumChildren = r.ReadI32()
                    Case 6
                        element.Converted = CType(r.ReadI32(), ConvertedType)
                    Case 7
                        element.Scale = r.ReadI32()
                    Case 8
                        element.Precision = r.ReadI32()
                    Case 9
                        element.FieldId = r.ReadI32()
                    Case 10
                        element.Logical = LogicalType.ReadFrom(r)
                    Case Else
                        Call r.Skip(field.type)
                End Select

                field = r.ReadFieldHeader()
            End While

            Call r.ReadStructEnd()

            Return element
        End Function

        Friend Sub WriteTo(w As Thrift.ThriftCompactWriter)
            Call w.WriteStructBegin()

            If Type.HasValue Then
                Call w.WriteI32Field(1, CInt(Type.Value))
            End If
            If TypeLength.HasValue Then
                Call w.WriteI32Field(2, TypeLength.Value)
            End If
            If Repetition.HasValue Then
                Call w.WriteI32Field(3, CInt(Repetition.Value))
            End If

            Call w.WriteStringField(4, If(Name, ""))

            If NumChildren.HasValue Then
                Call w.WriteI32Field(5, NumChildren.Value)
            End If
            If Converted.HasValue Then
                Call w.WriteI32Field(6, CInt(Converted.Value))
            End If

            If Converted.HasValue AndAlso Converted.Value = ConvertedType.DECIMAL Then
                Call w.WriteI32Field(7, If(Scale, 0))
                Call w.WriteI32Field(8, If(Precision, 0))
            End If

            If FieldId.HasValue Then
                Call w.WriteI32Field(9, FieldId.Value)
            End If

            If Logical IsNot Nothing Then
                Call w.WriteFieldHeader(10, Thrift.ThriftType.Struct)
                Call Logical.WriteTo(w)
            End If

            Call w.WriteStructEnd()
        End Sub

    End Class

End Namespace
