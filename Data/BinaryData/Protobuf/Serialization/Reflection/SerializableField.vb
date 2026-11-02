Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Serialization\Reflection\SerializableField.vb
' /********************************************************************************/

Imports System.Collections
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Schema
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Wire

Namespace Serialization.Reflection

    ''' <summary>
    ''' 一个被 <see cref="ProtoMemberAttribute"/> 标注的成员的读写模型。
    ''' </summary>
    ''' <remarks>
    ''' 该类型在 <see cref="TypeInfo"/> 构建阶段一次性完成全部反射与类型判定，
    ''' 之后的序列化/反序列化只进行 <c>GetValue</c> / <c>SetValue</c> 与纯数值转换。
    ''' </remarks>
    Friend Class SerializableField

        ''' <summary>
        ''' 1970-01-01 在 .NET Ticks 中对应的值。
        ''' </summary>
        Friend Shared ReadOnly UnixEpoch As New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)

        Private ReadOnly _property As PropertyInfo
        Private ReadOnly _field As FieldInfo

        Friend ReadOnly Member As MemberInfo
        Friend ReadOnly Name As String
        Friend ReadOnly FieldNumber As Integer
        Friend ReadOnly DataFormat As ProtoDataFormat
        Friend ReadOnly IsRequired As Boolean

        ''' <summary>
        ''' 成员声明的 CLR 类型（可能为 Nullable(Of T)）。
        ''' </summary>
        Friend ReadOnly DeclaredType As Type

        ''' <summary>
        ''' 去掉 Nullable 包装后的 CLR 类型。
        ''' </summary>
        Friend ReadOnly ValueType As Type

        Friend ReadOnly IsNullableValueType As Boolean
        Friend ReadOnly IsArray As Boolean
        Friend ReadOnly IsList As Boolean
        Friend ReadOnly IsDictionary As Boolean
        Friend ReadOnly IsRepeated As Boolean
        Friend ReadOnly IsPacked As Boolean

        ''' <summary>
        ''' 标量类型（repeated 时为元素类型，map 时为 entry 消息）。
        ''' </summary>
        Friend ReadOnly FieldType As FieldTypeKind

        Friend ReadOnly ElementType As Type
        Friend ReadOnly ElementFieldType As FieldTypeKind
        Friend ReadOnly DictionaryKeyType As Type
        Friend ReadOnly DictionaryValueType As Type
        Friend ReadOnly MapKeyType As FieldTypeKind
        Friend ReadOnly MapValueType As FieldTypeKind

        ''' <summary>
        ''' 该字段（非 map 时）使用的线格式类型。
        ''' </summary>
        Friend ReadOnly Property WireType As WireType
            Get
                Return FieldDescriptor.GetWireType(FieldType)
            End Get
        End Property

        Friend Sub New(attr As ProtoMemberAttribute, member As MemberInfo)
            If attr Is Nothing Then
                Throw New ArgumentNullException(NameOf(attr))
            End If

            If member Is Nothing Then
                Throw New ArgumentNullException(NameOf(member))
            End If

            _property = TryCast(member, PropertyInfo)
            _field = TryCast(member, FieldInfo)

            If _property Is Nothing AndAlso _field Is Nothing Then
                Throw New ArgumentException($"不支持的成员类型：{member.MemberType}", NameOf(member))
            End If

            Member = member
            Name = If(attr.Name, member.Name)
            FieldNumber = attr.FieldNumber
            DataFormat = attr.DataFormat
            IsRequired = attr.IsRequired
            DeclaredType = If(_property IsNot Nothing, _property.PropertyType, _field.FieldType)

            Dim underlying As Type = Nullable.GetUnderlyingType(DeclaredType)

            If underlying IsNot Nothing Then
                ValueType = underlying
                IsNullableValueType = True
            Else
                ValueType = DeclaredType
            End If

            If ValueType Is GetType(Byte()) Then
                ' bytes 是标量类型，而不是 repeated
                FieldType = FieldTypeKind.Bytes
                ElementType = GetType(Byte)
            ElseIf ValueType.IsArray Then
                IsArray = True
                ElementType = ValueType.GetElementType()
            Else
                Dim keyType As Type = Nothing
                Dim valueType As Type = Nothing
                Dim listElement As Type = Nothing

                If TryGetDictionaryTypes(ValueType, keyType, valueType) Then
                    IsDictionary = True
                    DictionaryKeyType = keyType
                    DictionaryValueType = valueType
                    IsRepeated = True
                    MapKeyType = DetermineKind(keyType, ProtoDataFormat.Auto)
                    MapValueType = DetermineKind(valueType, ProtoDataFormat.Auto)
                    FieldType = FieldTypeKind.Message
                ElseIf TryGetListElementType(ValueType, listElement) Then
                    IsList = True
                    IsRepeated = True
                    ElementType = listElement
                End If
            End If

            If IsDictionary Then
                IsPacked = False
                Return
            End If

            If IsRepeated Then
                ElementFieldType = DetermineKind(ElementType, DataFormat)
                FieldType = ElementFieldType
                IsPacked = If(attr.IsPacked, FieldDescriptor.IsPackableKind(ElementFieldType))
            Else
                FieldType = DetermineKind(ValueType, DataFormat)
                ElementFieldType = FieldType
                IsPacked = False
            End If
        End Sub

#Region "WRITE"

        ''' <summary>
        ''' 写出该字段（含 tag）。
        ''' </summary>
        Friend Sub WriteField(owner As Object, writer As ProtoWriter, context As SerializationContext)
            Dim value As Object = GetValue(owner)

            If IsDictionary Then
                Call WriteMap(value, writer, context)
            ElseIf IsRepeated Then
                Call WriteRepeated(value, writer, context)
            Else
                Call WriteSingle(value, writer, context)
            End If
        End Sub

        Private Sub WriteSingle(value As Object, writer As ProtoWriter, context As SerializationContext)
            If value Is Nothing Then
                Return
            End If

            If context.SkipDefaults AndAlso Not IsRequired AndAlso IsDefaultValue(value) Then
                Return
            End If

            If FieldType = FieldTypeKind.Message Then
                If writer.Depth + 1 > context.MaxDepth Then
                    Throw New InvalidOperationException($"消息嵌套深度超过上限 {context.MaxDepth}，疑似循环引用")
                End If

                Dim nested As TypeInfo = context.GetTypeInfo(ValueType)
                Dim item As Object = value

                Call writer.WriteMessage(FieldNumber, Sub(w) nested.WriteTo(item, w, context))
            Else
                Call writer.WriteTag(FieldNumber, WireType)
                Call WriteKindValue(value, FieldType, writer, context)
            End If
        End Sub

        Private Sub WriteRepeated(value As Object, writer As ProtoWriter, context As SerializationContext)
            If value Is Nothing Then
                Return
            End If

            Dim items As IEnumerable = DirectCast(value, IEnumerable)

            If IsPacked Then
                ' 先写入子缓冲，再回填 varint 长度；空集合不产生任何字节
                Using buffer As New MemoryStream()
                    Using child As New ProtoWriter(buffer, leaveOpen:=True)
                        For Each item As Object In items
                            Call WriteKindValue(item, FieldType, child, context)
                        Next

                        Call child.Flush()
                    End Using

                    If buffer.Length > 0 Then
                        Call writer.WriteTag(FieldNumber, WireType.LengthDelimited)
                        Call writer.WriteLengthPrefix(CInt(buffer.Length))
                        Call buffer.WriteTo(writer.BaseWriter.BaseStream)
                    End If
                End Using

                Return
            End If

            For Each item As Object In items
                If item Is Nothing Then
                    Continue For
                End If

                Call WriteRepeatedItem(item, writer, context)
            Next
        End Sub

        Private Sub WriteRepeatedItem(item As Object, writer As ProtoWriter, context As SerializationContext)
            If ElementFieldType = FieldTypeKind.Message Then
                If writer.Depth + 1 > context.MaxDepth Then
                    Throw New InvalidOperationException($"消息嵌套深度超过上限 {context.MaxDepth}，疑似循环引用")
                End If

                Dim nested As TypeInfo = context.GetTypeInfo(ElementType)
                Dim value As Object = item

                Call writer.WriteMessage(FieldNumber, Sub(w) nested.WriteTo(value, w, context))
            Else
                Call writer.WriteTag(FieldNumber, WireType)
                Call WriteKindValue(item, ElementFieldType, writer, context)
            End If
        End Sub

        Private Sub WriteMap(value As Object, writer As ProtoWriter, context As SerializationContext)
            If value Is Nothing Then
                Return
            End If

            Dim entries As IDictionary = DirectCast(value, IDictionary)

            If entries.Count = 0 Then
                Return
            End If

            Dim keyKind As FieldTypeKind = MapKeyType
            Dim valueKind As FieldTypeKind = MapValueType
            Dim valueClrType As Type = DictionaryValueType

            For Each key As Object In entries.Keys
                Dim localKey As Object = key
                Dim localValue As Object = entries(key)

                Call writer.WriteMessage(FieldNumber,
                    Sub(w)
                        Call w.WriteTag(1, FieldDescriptor.GetWireType(keyKind))
                        Call WriteKindValue(localKey, keyKind, w, context)

                        If localValue Is Nothing Then
                            Return
                        End If

                        If valueKind = FieldTypeKind.Message Then
                            Dim nested As TypeInfo = context.GetTypeInfo(valueClrType)
                            Dim item As Object = localValue

                            Call w.WriteMessage(2, Sub(inner) nested.WriteTo(item, inner, context))
                        Else
                            Call w.WriteTag(2, FieldDescriptor.GetWireType(valueKind))
                            Call WriteKindValue(localValue, valueKind, w, context)
                        End If
                    End Sub)
            Next
        End Sub

        ''' <summary>
        ''' 按标量类型写出单个值（不含 tag）。
        ''' </summary>
        Private Shared Sub WriteKindValue(value As Object, kind As FieldTypeKind,
                                          writer As ProtoWriter, context As SerializationContext)

            Select Case kind
                Case FieldTypeKind.Int32
                    Call writer.WriteInt32(Convert.ToInt32(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.Int64
                    Call writer.WriteInt64(Convert.ToInt64(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.UInt32
                    Call writer.WriteUInt32(Convert.ToUInt32(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.UInt64
                    Call writer.WriteUInt64(Convert.ToUInt64(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.SInt32
                    Call writer.WriteSInt32(Convert.ToInt32(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.SInt64
                    Call writer.WriteSInt64(Convert.ToInt64(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.Bool
                    Call writer.WriteBoolean(Convert.ToBoolean(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.[Enum]
                    Call writer.WriteEnumValue(Convert.ToInt32(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.Fixed32
                    Call writer.WriteFixed32(Convert.ToUInt32(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.SFixed32
                    Call writer.WriteSFixed32(Convert.ToInt32(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.Float
                    Call writer.WriteSingle(Convert.ToSingle(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.Fixed64
                    Call writer.WriteFixed64(Convert.ToUInt64(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.SFixed64
                    Call writer.WriteSFixed64(Convert.ToInt64(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.[Double]
                    Call writer.WriteDouble(Convert.ToDouble(value, CultureInfo.InvariantCulture))
                Case FieldTypeKind.[String]
                    Call writer.WriteString(DirectCast(value, String))
                Case FieldTypeKind.Bytes
                    Call writer.WriteBytes(DirectCast(value, Byte()))
                Case FieldTypeKind.Decimal128
                    Call writer.WriteBytes(GetDecimalBytes(Convert.ToDecimal(value, CultureInfo.InvariantCulture)))
                Case FieldTypeKind.DateTimeUnixMilliseconds
                    Call writer.WriteInt64(ToUnixMilliseconds(DirectCast(value, DateTime)))
                Case FieldTypeKind.DateTimeTicks
                    Call writer.WriteInt64(DirectCast(value, DateTime).Ticks)
                Case FieldTypeKind.Guid128
                    Call writer.WriteBytes(DirectCast(value, Guid).ToByteArray())
                Case Else
                    Throw New NotSupportedException($"不支持序列化的字段类型：{kind}")
            End Select
        End Sub

#End Region

#Region "READ"

        ''' <summary>
        ''' 读取一个字段的值并写入 <paramref name="owner"/>。
        ''' </summary>
        ''' <param name="pending">repeated 数组的累积缓冲（键为字段号），由 <see cref="TypeInfo"/> 统一回填。</param>
        Friend Sub ReadField(owner As Object, reader As ProtoReader, actualWireType As WireType,
                             context As SerializationContext,
                             ByRef pending As Dictionary(Of Integer, IList))

            If IsDictionary Then
                Call ReadMap(owner, reader, context)
            ElseIf IsRepeated Then
                Call ReadRepeated(owner, reader, actualWireType, context, pending)
            Else
                Call ReadSingle(owner, reader, actualWireType, context)
            End If
        End Sub

        Private Sub ReadSingle(owner As Object, reader As ProtoReader, actualWireType As WireType,
                               context As SerializationContext)

            If FieldType = FieldTypeKind.Message Then
                If actualWireType <> WireType.LengthDelimited Then
                    Call reader.SkipField(actualWireType)
                    Return
                End If

                If reader.Depth + 1 > context.MaxDepth Then
                    Throw New InvalidOperationException($"消息嵌套深度超过上限 {context.MaxDepth}，疑似循环引用或恶意数据")
                End If

                Dim length As Integer = reader.ReadLengthPrefix()

                Call reader.PushLimit(length)

                Try
                    Dim nested As TypeInfo = context.GetTypeInfo(ValueType)
                    Dim message As Object = nested.ReadFrom(reader, GetValue(owner), context)

                    Call SetValue(owner, Coerce(message))
                Finally
                    Call reader.PopLimit()
                End Try

                Return
            End If

            If actualWireType = WireType Then
                Call SetValue(owner, Coerce(ReadKindValue(reader, FieldType, context)))
            ElseIf actualWireType = WireType.LengthDelimited AndAlso FieldDescriptor.IsPackableKind(FieldType) Then
                ' 兼容以 packed 形式出现的单值
                Dim length As Integer = reader.ReadLengthPrefix()

                Call reader.PushLimit(length)

                Try
                    If Not reader.EndOfStream Then
                        Call SetValue(owner, Coerce(ReadKindValue(reader, FieldType, context)))
                    End If
                Finally
                    Call reader.PopLimit()
                End Try
            Else
                Call reader.SkipField(actualWireType)
            End If
        End Sub

        Private Sub ReadRepeated(owner As Object, reader As ProtoReader, actualWireType As WireType,
                                 context As SerializationContext,
                                 ByRef pending As Dictionary(Of Integer, IList))

            Dim elementKind As FieldTypeKind = ElementFieldType

            If elementKind = FieldTypeKind.Message Then
                If actualWireType <> WireType.LengthDelimited Then
                    Call reader.SkipField(actualWireType)
                    Return
                End If

                If reader.Depth + 1 > context.MaxDepth Then
                    Throw New InvalidOperationException($"消息嵌套深度超过上限 {context.MaxDepth}，疑似循环引用或恶意数据")
                End If

                Dim length As Integer = reader.ReadLengthPrefix()
                Dim item As Object

                Call reader.PushLimit(length)

                Try
                    item = context.GetTypeInfo(ElementType).ReadFrom(reader, Nothing, context)
                Finally
                    Call reader.PopLimit()
                End Try

                Call AppendValue(owner, item, pending)
            ElseIf actualWireType = WireType.LengthDelimited AndAlso FieldDescriptor.IsPackableKind(elementKind) Then
                Dim length As Integer = reader.ReadLengthPrefix()

                Call reader.PushLimit(length)

                Try
                    Do While Not reader.EndOfStream
                        Call AppendValue(owner, ReadKindValue(reader, elementKind, context), pending)
                    Loop
                Finally
                    Call reader.PopLimit()
                End Try
            ElseIf actualWireType = FieldDescriptor.GetWireType(elementKind) Then
                Call AppendValue(owner, ReadKindValue(reader, elementKind, context), pending)
            Else
                Call reader.SkipField(actualWireType)
            End If
        End Sub

        Private Sub ReadMap(owner As Object, reader As ProtoReader, context As SerializationContext)
            Dim length As Integer = reader.ReadLengthPrefix()

            Call reader.PushLimit(length)

            Dim key As Object = Nothing
            Dim value As Object = Nothing

            Try
                Dim fieldNumber As Integer
                Dim wireType As WireType

                Do While reader.ReadTag(fieldNumber, wireType)
                    Select Case fieldNumber
                        Case 1
                            If wireType = FieldDescriptor.GetWireType(MapKeyType) Then
                                key = CoerceTo(ReadKindValue(reader, MapKeyType, context), DictionaryKeyType)
                            Else
                                Call reader.SkipField(wireType)
                            End If

                        Case 2
                            If MapValueType = FieldTypeKind.Message Then
                                If wireType <> WireType.LengthDelimited Then
                                    Call reader.SkipField(wireType)
                                Else
                                    Dim childLength As Integer = reader.ReadLengthPrefix()

                                    Call reader.PushLimit(childLength)

                                    Try
                                        value = context.GetTypeInfo(DictionaryValueType).ReadFrom(reader, Nothing, context)
                                    Finally
                                        Call reader.PopLimit()
                                    End Try
                                End If
                            ElseIf wireType = FieldDescriptor.GetWireType(MapValueType) Then
                                value = CoerceTo(ReadKindValue(reader, MapValueType, context), DictionaryValueType)
                            Else
                                Call reader.SkipField(wireType)
                            End If

                        Case Else
                            Call reader.SkipField(wireType)
                    End Select
                Loop
            Finally
                Call reader.PopLimit()
            End Try

            If key Is Nothing Then
                Return
            End If

            Dim entries As Object = GetValue(owner)

            If entries Is Nothing Then
                entries = NewDictionaryInstance()
                Call SetValue(owner, entries)
            End If

            Dim dictionary As IDictionary = DirectCast(entries, IDictionary)

            dictionary(key) = value
        End Sub

        ''' <summary>
        ''' 读取单个标量值（不含 tag）。
        ''' </summary>
        Private Shared Function ReadKindValue(reader As ProtoReader, kind As FieldTypeKind,
                                              context As SerializationContext) As Object

            Select Case kind
                Case FieldTypeKind.Int32
                    Return reader.ReadInt32()
                Case FieldTypeKind.Int64
                    Return reader.ReadInt64()
                Case FieldTypeKind.UInt32
                    Return reader.ReadUInt32()
                Case FieldTypeKind.UInt64
                    Return reader.ReadUInt64()
                Case FieldTypeKind.SInt32
                    Return reader.ReadSInt32()
                Case FieldTypeKind.SInt64
                    Return reader.ReadSInt64()
                Case FieldTypeKind.Bool
                    Return reader.ReadBoolean()
                Case FieldTypeKind.[Enum]
                    Return reader.ReadInt32()
                Case FieldTypeKind.Fixed32
                    Return reader.ReadFixed32()
                Case FieldTypeKind.SFixed32
                    Return reader.ReadSFixed32()
                Case FieldTypeKind.Float
                    Return reader.ReadSingle()
                Case FieldTypeKind.Fixed64
                    Return reader.ReadFixed64()
                Case FieldTypeKind.SFixed64
                    Return reader.ReadSFixed64()
                Case FieldTypeKind.[Double]
                    Return reader.ReadDouble()
                Case FieldTypeKind.[String]
                    Return reader.ReadString()
                Case FieldTypeKind.Bytes
                    Return reader.ReadBytes()
                Case FieldTypeKind.Decimal128
                    Return ReadDecimal(reader.ReadBytes())
                Case FieldTypeKind.DateTimeUnixMilliseconds
                    Return FromUnixMilliseconds(reader.ReadInt64())
                Case FieldTypeKind.DateTimeTicks
                    Return New DateTime(reader.ReadInt64())
                Case FieldTypeKind.Guid128
                    Return New Guid(reader.ReadBytes())
                Case Else
                    Throw New NotSupportedException($"不支持反序列化的字段类型：{kind}")
            End Select
        End Function

#End Region

#Region "COLLECTION HELPERS"

        Private Sub AppendValue(owner As Object, value As Object, ByRef pending As Dictionary(Of Integer, IList))
            If IsArray Then
                If pending Is Nothing Then
                    pending = New Dictionary(Of Integer, IList)()
                End If

                Dim list As IList = Nothing

                If Not pending.TryGetValue(FieldNumber, list) Then
                    list = DirectCast(Activator.CreateInstance(GetType(List(Of )).MakeGenericType(ElementType)), IList)
                    pending(FieldNumber) = list
                End If

                Call list.Add(CoerceTo(value, ElementType))
                Return
            End If

            Dim target As Object = GetValue(owner)

            If target Is Nothing Then
                target = NewListInstance()
                Call SetValue(owner, target)
            End If

            Call DirectCast(target, IList).Add(CoerceTo(value, ElementType))
        End Sub

        ''' <summary>
        ''' 把累积的数组元素回填到目标属性。
        ''' </summary>
        Friend Sub SetArrayFromList(owner As Object, list As IList)
            If Not IsArray OrElse list Is Nothing Then
                Return
            End If

            Dim array As Array = Array.CreateInstance(ElementType, list.Count)

            For i As Integer = 0 To list.Count - 1
                Call array.SetValue(CoerceTo(list(i), ElementType), i)
            Next

            Call SetValue(owner, array)
        End Sub

        Private Function NewListInstance() As Object
            Return Activator.CreateInstance(GetType(List(Of )).MakeGenericType(ElementType))
        End Function

        Private Function NewDictionaryInstance() As Object
            Return Activator.CreateInstance(
                GetType(Dictionary(Of ,)).MakeGenericType(DictionaryKeyType, DictionaryValueType))
        End Function

#End Region

#Region "MEMBER ACCESS"

        Friend Function GetValue(owner As Object) As Object
            If _property IsNot Nothing Then
                Return _property.GetValue(owner, Nothing)
            Else
                Return _field.GetValue(owner)
            End If
        End Function

        Friend Sub SetValue(owner As Object, value As Object)
            If _property IsNot Nothing Then
                Call _property.SetValue(owner, value, Nothing)
            Else
                Call _field.SetValue(owner, value)
            End If
        End Sub

        Private Function Coerce(value As Object) As Object
            Return CoerceTo(value, ValueType)
        End Function

        Private Shared Function CoerceTo(value As Object, target As Type) As Object
            If value Is Nothing OrElse target Is Nothing Then
                Return value
            End If

            Dim underlying As Type = Nullable.GetUnderlyingType(target)

            If underlying IsNot Nothing Then
                target = underlying
            End If

            If target.IsInstanceOfType(value) Then
                Return value
            End If

            If target.IsEnum Then
                Return System.Enum.ToObject(target, Convert.ToInt64(value, CultureInfo.InvariantCulture))
            End If

            Return Convert.ChangeType(value, target, CultureInfo.InvariantCulture)
        End Function

#End Region

#Region "TYPE MAPPING"

        ''' <summary>
        ''' 把 CLR 类型映射为 protobuf 标量类型。
        ''' </summary>
        Friend Shared Function DetermineKind(type As Type, format As ProtoDataFormat) As FieldTypeKind
            Select Case format
                Case ProtoDataFormat.Decimal128
                    Return FieldTypeKind.Decimal128
                Case ProtoDataFormat.DateTimeUnixMilliseconds
                    Return FieldTypeKind.DateTimeUnixMilliseconds
                Case ProtoDataFormat.DateTimeTicks
                    Return FieldTypeKind.DateTimeTicks
                Case ProtoDataFormat.Guid128
                    Return FieldTypeKind.Guid128
            End Select

            Dim kind As FieldTypeKind

            If type Is Nothing Then
                Return FieldTypeKind.Message
            End If

            If type.IsEnum Then
                kind = FieldTypeKind.[Enum]
            ElseIf type Is GetType(Byte()) Then
                kind = FieldTypeKind.Bytes
            ElseIf type Is GetType(Guid) Then
                kind = FieldTypeKind.Guid128
            Else
                Select Case Type.GetTypeCode(type)
                    Case TypeCode.Boolean
                        kind = FieldTypeKind.Bool
                    Case TypeCode.Byte, TypeCode.UInt16, TypeCode.UInt32
                        kind = FieldTypeKind.UInt32
                    Case TypeCode.SByte, TypeCode.Int16, TypeCode.Int32
                        kind = FieldTypeKind.Int32
                    Case TypeCode.UInt64
                        kind = FieldTypeKind.UInt64
                    Case TypeCode.Int64
                        kind = FieldTypeKind.Int64
                    Case TypeCode.Single
                        kind = FieldTypeKind.Float
                    Case TypeCode.[Double]
                        kind = FieldTypeKind.[Double]
                    Case TypeCode.[String]
                        kind = FieldTypeKind.[String]
                    Case TypeCode.Decimal
                        kind = FieldTypeKind.Decimal128
                    Case TypeCode.DateTime
                        kind = FieldTypeKind.DateTimeUnixMilliseconds
                    Case Else
                        kind = FieldTypeKind.Message
                End Select
            End If

            Select Case format
                Case ProtoDataFormat.ZigZag
                    If kind = FieldTypeKind.Int32 Then
                        Return FieldTypeKind.SInt32
                    ElseIf kind = FieldTypeKind.Int64 Then
                        Return FieldTypeKind.SInt64
                    End If
                Case ProtoDataFormat.Fixed32
                    If kind = FieldTypeKind.Int32 Then
                        Return FieldTypeKind.SFixed32
                    ElseIf kind = FieldTypeKind.UInt32 Then
                        Return FieldTypeKind.Fixed32
                    End If
                Case ProtoDataFormat.Fixed64
                    If kind = FieldTypeKind.Int64 Then
                        Return FieldTypeKind.SFixed64
                    ElseIf kind = FieldTypeKind.UInt64 Then
                        Return FieldTypeKind.Fixed64
                    End If
            End Select

            Return kind
        End Function

        Private Shared Function TryGetListElementType(type As Type, ByRef element As Type) As Boolean
            If type.IsGenericType Then
                Dim definition As Type = type.GetGenericTypeDefinition()

                If definition Is GetType(List(Of )) OrElse
                   definition Is GetType(IList(Of )) OrElse
                   definition Is GetType(ICollection(Of )) OrElse
                   definition Is GetType(IEnumerable(Of )) Then

                    element = type.GetGenericArguments()(0)
                    Return True
                End If
            End If

            For Each item As Type In type.GetInterfaces()
                If item.IsGenericType Then
                    Dim definition As Type = item.GetGenericTypeDefinition()

                    If definition Is GetType(IList(Of )) OrElse definition Is GetType(ICollection(Of )) Then
                        element = item.GetGenericArguments()(0)
                        Return True
                    End If
                End If
            Next

            Return False
        End Function

        Private Shared Function TryGetDictionaryTypes(type As Type, ByRef key As Type, ByRef value As Type) As Boolean
            Dim candidates As New List(Of Type)

            If type.IsGenericType Then
                Call candidates.Add(type)
            End If

            For Each item As Type In type.GetInterfaces()
                If item.IsGenericType Then
                    Call candidates.Add(item)
                End If
            Next

            For Each candidate As Type In candidates
                Dim definition As Type = candidate.GetGenericTypeDefinition()

                If definition Is GetType(IDictionary(Of ,)) OrElse definition Is GetType(Dictionary(Of ,)) Then
                    Dim arguments As Type() = candidate.GetGenericArguments()

                    key = arguments(0)
                    value = arguments(1)

                    Return True
                End If
            Next

            Return False
        End Function

#End Region

#Region "VALUE HELPERS"

        Private Shared Function IsDefaultValue(value As Object) As Boolean
            If value Is Nothing Then
                Return True
            End If

            Dim type As Type = value.GetType()

            If type Is GetType(String) Then
                Return DirectCast(value, String).Length = 0
            End If

            If type Is GetType(Byte()) Then
                Return DirectCast(value, Byte()).Length = 0
            End If

            If type Is GetType(Guid) Then
                Return DirectCast(value, Guid) = Guid.Empty
            End If

            If type.IsEnum Then
                Return Convert.ToInt64(value, CultureInfo.InvariantCulture) = 0L
            End If

            Select Case Type.GetTypeCode(type)
                Case TypeCode.Boolean
                    Return Not DirectCast(value, Boolean)
                Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                     TypeCode.Single, TypeCode.[Double], TypeCode.Decimal
                    Return Convert.ToDecimal(value, CultureInfo.InvariantCulture) = 0D
                Case TypeCode.DateTime
                    Return DirectCast(value, DateTime) = DateTime.MinValue
                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function GetDecimalBytes(value As Decimal) As Byte()
            Dim bits As Integer() = Decimal.GetBits(value)
            Dim buffer(0 To 15) As Byte

            For i As Integer = 0 To 3
                Dim segment As Integer = bits(i)
                Dim offset As Integer = i * 4

                buffer(offset) = CByte(segment And &HFF)
                buffer(offset + 1) = CByte((segment >> 8) And &HFF)
                buffer(offset + 2) = CByte((segment >> 16) And &HFF)
                buffer(offset + 3) = CByte((segment >> 24) And &HFF)
            Next

            Return buffer
        End Function

        Private Shared Function ReadDecimal(buffer As Byte()) As Decimal
            If buffer Is Nothing OrElse buffer.Length <> 16 Then
                Throw New WireFormatException($"decimal 的编码长度应为 16 字节，实际为 {If(buffer Is Nothing, 0, buffer.Length)} 字节")
            End If

            Dim bits(0 To 3) As Integer

            For i As Integer = 0 To 3
                Dim offset As Integer = i * 4

                bits(i) = CInt(buffer(offset)) Or
                          (CInt(buffer(offset + 1)) << 8) Or
                          (CInt(buffer(offset + 2)) << 16) Or
                          (CInt(buffer(offset + 3)) << 24)
            Next

            Return New Decimal(bits)
        End Function

        Private Shared Function ToUnixMilliseconds(value As DateTime) As Long
            Dim utc As DateTime = If(value.Kind = DateTimeKind.Utc, value, value.ToUniversalTime())
            Return CLng((utc - UnixEpoch).TotalMilliseconds)
        End Function

        Private Shared Function FromUnixMilliseconds(value As Long) As DateTime
            Return UnixEpoch.AddMilliseconds(value)
        End Function

#End Region

    End Class

End Namespace
