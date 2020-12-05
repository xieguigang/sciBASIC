Imports System.Collections
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Constants
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization.Reflection

Public Module MsgPackIO

    Private Const nullProhibitedExceptionMessage As String = "Null value encountered but is prohibited"

    Friend Function ReadNumArrayElements(reader As BinaryReader) As Integer
        Dim header As Byte = reader.ReadByte()
        Dim numElements = -1

        If header <> Formats.NIL Then
            If header >= FixedArray.MIN AndAlso header <= FixedArray.MAX Then
                numElements = header - FixedArray.MIN
            ElseIf header = Formats.ARRAY_16 Then
                numElements = (reader.ReadByte() << 8) + reader.ReadByte()
            ElseIf header = Formats.ARRAY_32 Then
                numElements = (reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte()
            Else
                Throw New ApplicationException("The serialized data format is invalid due to an invalid array size specification at offset " & reader.BaseStream.Position)
            End If
        End If

        Return numElements
    End Function

    Friend Sub DeserializeArray(array As Array, numElements As Integer, reader As BinaryReader)
        Dim elementType As Type = array.GetType().GetElementType()

        For i = 0 To numElements - 1
            Dim o = DeserializeValue(elementType, reader, NilImplication.Null)
            Dim safeVal As Object = Nothing

            If o IsNot Nothing Then
                If elementType.IsGenericType AndAlso elementType.GetGenericTypeDefinition() Is GetType(Nullable(Of)) Then
                    safeVal = Convert.ChangeType(o, Nullable.GetUnderlyingType(elementType))
                Else
                    safeVal = Convert.ChangeType(o, elementType)
                End If
            End If

            array.SetValue(safeVal, i)
        Next
    End Sub

    Friend Function DeserializeCollection(collection As IList, reader As BinaryReader) As Boolean
        Dim isNull = True
        If Not collection.GetType().IsGenericType Then Throw New NotSupportedException("Only generic List<T> lists are supported")
        Dim elementType As Type = collection.GetType().GetGenericArguments()(0)
        Dim numElements = ReadNumArrayElements(reader)

        If numElements >= 0 Then
            isNull = False

            For i = 0 To numElements - 1
                Dim o = DeserializeValue(elementType, reader, NilImplication.Null)
                Dim safeVal As Object = Nothing

                If o IsNot Nothing Then
                    If elementType.IsGenericType AndAlso elementType.GetGenericTypeDefinition() Is GetType(Nullable(Of)) Then
                        safeVal = Convert.ChangeType(o, Nullable.GetUnderlyingType(elementType))
                    Else
                        safeVal = Convert.ChangeType(o, elementType)
                    End If
                End If

                collection.Add(safeVal)
            Next
        End If

        Return isNull
    End Function

    Friend Function DeserializeCollection(collection As IDictionary, reader As BinaryReader, Optional header As Byte? = Nothing) As Boolean
        Dim isNull = True
        If Not collection.GetType().IsGenericType Then Throw New NotSupportedException("Only generic Dictionary<T,U> dictionaries are supported")
        Dim keyType As Type = collection.GetType().GetGenericArguments()(0)
        Dim valueType As Type = collection.GetType().GetGenericArguments()(1)
        If Not header.HasValue Then header = reader.ReadByte()

        If header <> Formats.NIL Then
            Dim numElements = 0

            If header >= FixedMap.MIN AndAlso header <= FixedMap.MAX Then
                numElements = header.Value - FixedMap.MIN
            ElseIf header = Formats.MAP_16 Then
                numElements = (reader.ReadByte() << 8) + reader.ReadByte()
            ElseIf header = Formats.MAP_32 Then
                numElements = (reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte()
            Else
                Throw New ApplicationException("The serialized data format is invalid due to an invalid map size specification")
            End If

            isNull = False

            For i = 0 To numElements - 1
                Dim key = DeserializeValue(keyType, reader, NilImplication.MemberDefault)
                Dim val = DeserializeValue(valueType, reader, NilImplication.Null)
                Dim safeKey = Convert.ChangeType(key, keyType)
                Dim safeVal As Object = Nothing

                If val IsNot Nothing Then
                    If valueType.IsGenericType AndAlso valueType.GetGenericTypeDefinition() Is GetType(Nullable(Of)) Then
                        safeVal = Convert.ChangeType(val, Nullable.GetUnderlyingType(valueType))
                    ElseIf valueType Is GetType(Object) Then
                        safeVal = val
                    Else
                        safeVal = Convert.ChangeType(val, valueType)
                    End If
                End If

                collection.Add(safeKey, safeVal)
            Next
        End If

        Return isNull
    End Function

    Friend Function ToUnixMillis(dateTime As Date) As Long
        Return CLng(dateTime.ToUniversalTime().Subtract(unixEpocUtc).TotalMilliseconds)
    End Function

    Friend Function ToUnixMillis(span As TimeSpan) As Long
        Return span.TotalMilliseconds
    End Function

    Friend Function ToDateTime(value As Long) As Date
        Return unixEpocUtc.AddMilliseconds(value).ToLocalTime()
    End Function

    Friend Function ToTimeSpan(value As Long) As TimeSpan
        Return New TimeSpan(0, 0, 0, 0, value)
    End Function

    Friend Function DeserializeValue(type As Type, reader As BinaryReader, nilImplication As NilImplication) As Object
        Dim result As Object = Nothing
        Dim isRichType = False

        If type Is GetType(String) Then
            result = ReadMsgPackString(reader, nilImplication)
        ElseIf type Is GetType(Integer) OrElse type Is GetType(UInteger) OrElse type Is GetType(Byte) OrElse type Is GetType(SByte) OrElse type Is GetType(Short) OrElse type Is GetType(UShort) OrElse type Is GetType(Long) OrElse type Is GetType(ULong) OrElse type Is GetType(Integer?) OrElse type Is GetType(UInteger?) OrElse type Is GetType(Byte?) OrElse type Is GetType(SByte?) OrElse type Is GetType(Short?) OrElse type Is GetType(UShort?) OrElse type Is GetType(Long?) OrElse type Is GetType(ULong?) Then
            result = ReadMsgPackInt(reader, nilImplication)
        ElseIf type Is GetType(Char) Then
            result = ReadMsgPackInt(reader, nilImplication)
        ElseIf type Is GetType(Single) Then
            result = ReadMsgPackFloat(reader, nilImplication)
        ElseIf type Is GetType(Double) Then
            result = ReadMsgPackDouble(reader, nilImplication)
        ElseIf type Is GetType(Boolean) OrElse type Is GetType(Boolean) Then
            result = ReadMsgPackBoolean(reader, nilImplication)
        ElseIf type Is GetType(Date) Then
            Dim boxedVal = ReadMsgPackInt(reader, nilImplication)

            If boxedVal Is Nothing Then
                result = Nothing
            Else
                Dim unixEpochTicks As Long = boxedVal
                result = ToDateTime(unixEpochTicks)
            End If
        ElseIf type Is GetType(TimeSpan) Then
            Dim boxedVal = ReadMsgPackInt(reader, nilImplication)

            If boxedVal Is Nothing Then
                result = Nothing
            Else
                Dim unixEpochTicks As Integer = boxedVal
                result = ToTimeSpan(unixEpochTicks)
            End If
        ElseIf type.IsEnum Then
            Dim boxedVal = ReadMsgPackString(reader, nilImplication)

            If boxedVal Is Nothing Then
                result = Nothing
            Else
                Dim enumVal = CStr(boxedVal)

                If Equals(enumVal, "") Then
                    result = Nothing
                Else
                    result = [Enum].Parse(type, enumVal)
                End If
            End If
        ElseIf type.IsArray Then
            Dim numElements = ReadNumArrayElements(reader)

            If numElements = -1 Then
                result = Nothing
            Else
                result = Activator.CreateInstance(type, New Object() {numElements})
                DeserializeArray(CType(result, Array), numElements, reader)
            End If
        ElseIf type Is GetType(Object) Then
            Dim header As Byte = reader.ReadByte()

            If header = Formats.NIL Then
                result = Nothing
            ElseIf header = Bool.TRUE Then
                result = True
            ElseIf header = Bool.FALSE Then
                result = False
            ElseIf header = Formats.FLOAT_64 Then
                result = ReadMsgPackDouble(reader, nilImplication, header)
            ElseIf header = Formats.FLOAT_32 Then
                result = ReadMsgPackFloat(reader, nilImplication, header)
            ElseIf header = Formats.INTEGER_16 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header = Formats.INTEGER_32 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header = Formats.INTEGER_64 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header = Formats.INTEGER_8 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header = Formats.STRING_8 Then
                result = ReadMsgPackString(reader, nilImplication, header)
            ElseIf header = Formats.STRING_16 Then
                result = ReadMsgPackString(reader, nilImplication, header)
            ElseIf header = Formats.STRING_32 Then
                result = ReadMsgPackString(reader, nilImplication, header)
            ElseIf header >= FixedString.MIN AndAlso header <= FixedString.MAX Then
                result = ReadMsgPackString(reader, nilImplication, header)
            ElseIf header = Formats.UNSIGNED_INTEGER_8 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header = Formats.UNSIGNED_INTEGER_16 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header = Formats.UNSIGNED_INTEGER_32 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header = Formats.UNSIGNED_INTEGER_64 Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header >= FixedInteger.POSITIVE_MIN AndAlso header <= FixedInteger.POSITIVE_MAX Then

                If header = 0 Then
                    result = 0
                Else
                    result = ReadMsgPackInt(reader, nilImplication, header)
                End If
            ElseIf header >= FixedInteger.NEGATIVE_MIN AndAlso header <= FixedInteger.NEGATIVE_MAX Then
                result = ReadMsgPackInt(reader, nilImplication, header)
            ElseIf header >= FixedMap.MIN AndAlso header <= FixedMap.MAX OrElse header = Formats.MAP_16 OrElse header = Formats.MAP_32 Then
                result = New Dictionary(Of String, Object)()
                DeserializeCollection(CType(result, Dictionary(Of String, Object)), reader, header)
            Else
                isRichType = True
            End If
        Else
            isRichType = True
        End If

        If isRichType Then
            Dim constructorInfo = type.GetConstructor(Type.EmptyTypes)

            If constructorInfo Is Nothing Then
                Throw New ApplicationException($"Can't deserialize Type [{type}] because it has no default constructor")
            End If

            result = constructorInfo.Invoke(SerializableProperty.EmptyObjArgs)
            result = MsgPackSerializer.DeserializeObject(result, reader, nilImplication)
        End If

        Return result
    End Function

    Friend Function ReadHeader(t As Type, reader As BinaryReader, nilImplication As NilImplication, <Out> ByRef result As Object) As Byte
        result = Nothing
        Dim v As Byte = reader.ReadByte()

        If v = Formats.NIL Then
            If nilImplication = NilImplication.MemberDefault Then
                If t.IsValueType Then
                    result = Activator.CreateInstance(t)
                End If
            ElseIf nilImplication = NilImplication.Prohibit Then
                Throw New ApplicationException(nullProhibitedExceptionMessage)
            End If
        End If

        Return v
    End Function

    Friend Function ReadMsgPackBoolean(reader As BinaryReader, nilImplication As NilImplication) As Object
        Dim result As Object = Nothing
        Dim v As Byte = ReadHeader(GetType(Boolean), reader, nilImplication, result)

        If v <> Formats.NIL Then
            result = v = Bool.TRUE
        End If

        Return result
    End Function

    Friend Function ReadMsgPackFloat(reader As BinaryReader, nilImplication As NilImplication, Optional header As Byte = 0) As Object
        Dim result As Object = Nothing
        Dim v = If(header = 0, ReadHeader(GetType(Single), reader, nilImplication, result), header)

        If v <> Formats.NIL Then
            If v <> Formats.FLOAT_32 Then Throw New ApplicationException("Serialized data doesn't match type being deserialized to")
            Dim data = reader.ReadBytes(4)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            result = BitConverter.ToSingle(data, 0)
        End If

        Return result
    End Function

    Friend Function ReadMsgPackDouble(reader As BinaryReader, nilImplication As NilImplication, Optional header As Byte = 0) As Object
        Dim result As Object = Nothing
        Dim v = If(header = 0, ReadHeader(GetType(Double), reader, nilImplication, result), header)

        If v <> Formats.NIL Then
            If v <> Formats.FLOAT_64 Then Throw New ApplicationException("Serialized data doesn't match type being deserialized to")
            Dim data = reader.ReadBytes(8)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            result = BitConverter.ToDouble(data, 0)
        End If

        Return result
    End Function

    Friend Function ReadMsgPackULong(reader As BinaryReader, nilImplication As NilImplication, Optional header As Byte = 0) As Object
        Dim result As Object = Nothing
        Dim v = If(header = 0, ReadHeader(GetType(ULong), reader, nilImplication, result), header)

        If v <> Formats.NIL Then
            If v <> Formats.UINT_64 Then Throw New ApplicationException("Serialized data doesn't match type being deserialized to")
            result = reader.ReadUInt64()
        End If

        Return result
    End Function

    Friend Function ReadMsgPackInt(reader As BinaryReader, nilImplication As NilImplication, Optional header As Byte = 0) As Object
        Dim result As Object = Nothing
        Dim v = If(header = 0, ReadHeader(GetType(Long), reader, nilImplication, result), header)

        If v <> Formats.NIL Then
            If v <= FixedInteger.POSITIVE_MAX Then
                result = v
            ElseIf v >= FixedInteger.NEGATIVE_MIN Then
                result = -(v - FixedInteger.NEGATIVE_MIN)
            ElseIf v = Formats.UINT_8 Then
                result = reader.ReadByte()
            ElseIf v = Formats.UINT_16 Then
                result = (reader.ReadByte() << 8) + reader.ReadByte()
            ElseIf v = Formats.UINT_32 Then
                result = CUInt((reader.ReadByte() << 24)) + CUInt((reader.ReadByte() << 16)) + CUInt((reader.ReadByte() << 8)) + CUInt(reader.ReadByte())
            ElseIf v = Formats.UINT_64 Then
                result = CULng((reader.ReadByte() << 56)) + CULng((reader.ReadByte() << 48)) + CULng((reader.ReadByte() << 40)) + CULng((reader.ReadByte() << 32)) + CULng((reader.ReadByte() << 24)) + CULng((reader.ReadByte() << 16)) + CULng((reader.ReadByte() << 8)) + CULng(reader.ReadByte())
            ElseIf v = Formats.INT_8 Then
                result = reader.ReadSByte()
            ElseIf v = Formats.INT_16 Then
                Dim data = reader.ReadBytes(2)
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                result = BitConverter.ToInt16(data, 0)
            ElseIf v = Formats.INT_32 Then
                Dim data = reader.ReadBytes(4)
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                result = BitConverter.ToInt32(data, 0)
            ElseIf v = Formats.INT_64 Then
                Dim data = reader.ReadBytes(8)
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                result = BitConverter.ToInt64(data, 0)
            Else
                Throw New ApplicationException("Serialized data doesn't match type being deserialized to")
            End If
        End If

        Return result
    End Function

    Friend Function ReadMsgPackString(reader As BinaryReader, nilImplication As NilImplication, Optional header As Byte = 0) As Object
        Dim result As Object = Nothing
        Dim v = If(header = 0, ReadHeader(GetType(String), reader, nilImplication, result), header)

        If v <> Formats.NIL Then
            Dim length = 0

            If v >= FixedString.MIN AndAlso v <= FixedString.MAX Then
                length = v - FixedString.MIN
            ElseIf v = Formats.STR_8 Then
                length = reader.ReadByte()
            ElseIf v = Formats.STR_16 Then
                length = (reader.ReadByte() << 8) + reader.ReadByte()
            ElseIf v = Formats.STR_32 Then
                length = (reader.ReadByte() << 24) + (reader.ReadByte() << 16) + (reader.ReadByte() << 8) + reader.ReadByte()
            End If

            Dim stringBuffer = reader.ReadBytes(length)
            result = Encoding.UTF8.GetString(stringBuffer)
        End If

        Return result
    End Function

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Boolean)
        If val Then
            writer.Write(Bool.TRUE)
        Else
            writer.Write(Bool.FALSE)
        End If
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Single)
        Dim data = BitConverter.GetBytes(val)
        If BitConverter.IsLittleEndian Then Array.Reverse(data)
        writer.Write(Formats.FLOAT_32)
        writer.Write(data)
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Double)
        Dim data = BitConverter.GetBytes(val)
        If BitConverter.IsLittleEndian Then Array.Reverse(data)
        writer.Write(Formats.FLOAT_64)
        writer.Write(data)
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Date)
        WriteMsgPack(writer, ToUnixMillis(val))
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As TimeSpan)
        WriteMsgPack(writer, ToUnixMillis(val))
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As SByte)
        writer.Write(Formats.INT_8)
        writer.Write(val)
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Byte)
        writer.Write(Formats.UINT_8)
        writer.Write(val)
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Char)
        MsgPackIO.WriteMsgPack(writer, Microsoft.VisualBasic.AscW(val))
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As UShort)
        If val <= FixedInteger.POSITIVE_MAX Then
            writer.Write(CByte(val))
        ElseIf val <= Byte.MaxValue Then
            writer.Write(Formats.UINT_8)
            writer.Write(CByte(val))
        Else
            writer.Write(Formats.UINT_16)
            Dim data = BitConverter.GetBytes(val)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        End If
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Short)
        If val >= 0 AndAlso val <= FixedInteger.POSITIVE_MAX Then
            writer.Write(CByte(val))
        ElseIf val >= 0 AndAlso val <= Byte.MaxValue Then
            writer.Write(Formats.UINT_8)
            writer.Write(CByte(val))
        ElseIf val >= SByte.MinValue AndAlso val <= SByte.MaxValue Then
            writer.Write(Formats.INT_8)
            writer.Write(CSByte(val))
        Else
            writer.Write(Formats.INT_16)
            Dim data = BitConverter.GetBytes(val)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        End If
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As UInteger)
        If val <= FixedInteger.POSITIVE_MAX Then
            writer.Write(CByte(val))
        ElseIf val <= Byte.MaxValue Then
            writer.Write(Formats.UINT_8)
            writer.Write(CByte(val))
        ElseIf val <= UShort.MaxValue Then
            writer.Write(Formats.UINT_16)
            Dim outVal As UShort = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        Else
            writer.Write(Formats.UINT_32)
            Dim data = BitConverter.GetBytes(val)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        End If
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Integer)
        If val >= 0 AndAlso val <= FixedInteger.POSITIVE_MAX Then
            writer.Write(CByte(val))
        ElseIf val >= 0 AndAlso val <= Byte.MaxValue Then
            writer.Write(Formats.UINT_8)
            writer.Write(CByte(val))
        ElseIf val >= SByte.MinValue AndAlso val <= SByte.MaxValue Then
            writer.Write(Formats.INT_8)
            writer.Write(CSByte(val))
        ElseIf val >= Short.MinValue AndAlso val <= Short.MaxValue Then
            writer.Write(Formats.INT_16)
            Dim outVal As Short = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        ElseIf val >= 0 AndAlso val <= UShort.MaxValue Then
            writer.Write(Formats.UINT_16)
            Dim outVal As UShort = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        Else
            writer.Write(Formats.INT_32)
            Dim data = BitConverter.GetBytes(val)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        End If
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As ULong)
        If val <= FixedInteger.POSITIVE_MAX Then
            writer.Write(CByte(val))
        ElseIf val <= Byte.MaxValue Then
            writer.Write(Formats.UINT_8)
            writer.Write(CByte(val))
        ElseIf val <= UShort.MaxValue Then
            writer.Write(Formats.UINT_16)
            Dim outVal As UShort = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        ElseIf val <= UInteger.MaxValue Then
            writer.Write(Formats.UINT_32)
            Dim outVal As UInteger = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        Else
            writer.Write(Formats.UINT_64)
            Dim data = BitConverter.GetBytes(val)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        End If
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, val As Long)
        If val >= 0 AndAlso val <= FixedInteger.POSITIVE_MAX Then
            writer.Write(CByte(val))
        ElseIf val >= 0 AndAlso val <= Byte.MaxValue Then
            writer.Write(Formats.UINT_8)
            writer.Write(CByte(val))
        ElseIf val >= SByte.MinValue AndAlso val <= SByte.MaxValue Then
            writer.Write(Formats.INT_8)
            writer.Write(CSByte(val))
        ElseIf val >= Short.MinValue AndAlso val <= Short.MaxValue Then
            writer.Write(Formats.INT_16)
            Dim outVal As Short = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        ElseIf val >= 0 AndAlso val <= UShort.MaxValue Then
            writer.Write(Formats.UINT_16)
            Dim outVal As UShort = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        ElseIf val >= Integer.MinValue AndAlso val <= Integer.MaxValue Then
            writer.Write(Formats.INT_32)
            Dim outVal As Integer = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        ElseIf val >= 0 AndAlso val <= UInteger.MaxValue Then
            writer.Write(Formats.UINT_32)
            Dim outVal As UInteger = val
            Dim data = BitConverter.GetBytes(outVal)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        Else
            writer.Write(Formats.INT_64)
            Dim data = BitConverter.GetBytes(val)
            If BitConverter.IsLittleEndian Then Array.Reverse(data)
            writer.Write(data)
        End If
    End Sub

    Friend Sub WriteMsgPack(writer As BinaryWriter, s As String)
        If String.IsNullOrEmpty(s) Then
            writer.Write(FixedString.MIN)
        Else
            Dim utf8Bytes = Encoding.UTF8.GetBytes(s)
            Dim length As UInteger = utf8Bytes.Length

            If length <= FixedString.MAX_LENGTH Then
                Dim val As Byte = FixedString.MIN Or length
                writer.Write(val)
            ElseIf length <= Byte.MaxValue Then
                writer.Write(Formats.STR_8)
                writer.Write(CByte(length))
            ElseIf length <= UShort.MaxValue Then
                writer.Write(Formats.STR_16)
                Dim outVal As UShort = length
                Dim data = BitConverter.GetBytes(outVal)
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                writer.Write(data)
            Else
                writer.Write(Formats.STR_32)
                Dim outVal = length
                Dim data = BitConverter.GetBytes(outVal)
                If BitConverter.IsLittleEndian Then Array.Reverse(data)
                writer.Write(data)
            End If

            For i = 0 To utf8Bytes.Length - 1
                writer.Write(utf8Bytes(i))
            Next
        End If
    End Sub

    Friend Sub SerializeEnumerable(collection As IEnumerator, writer As BinaryWriter, serializationMethod As SerializationMethod)
        While collection.MoveNext()
            Dim val = collection.Current
            SerializeValue(val, writer, serializationMethod)
        End While
    End Sub

    Friend Sub SerializeValue(val As Object, writer As BinaryWriter, serializationMethod As SerializationMethod)
        If val Is Nothing Then
            writer.Write(Formats.NIL)
        Else
            Dim t As Type = val.GetType()
            t = If(Nullable.GetUnderlyingType(t), t)

            If t Is GetType(String) Then
                WriteMsgPack(writer, CStr(val))
            ElseIf t Is GetType(Char) OrElse t Is GetType(Char) Then
                WriteMsgPack(writer, CChar(val))
            ElseIf t Is GetType(Single) OrElse t Is GetType(Single) Then
                WriteMsgPack(writer, CSng(val))
            ElseIf t Is GetType(Double) OrElse t Is GetType(Double) Then
                WriteMsgPack(writer, CDbl(val))
            ElseIf t Is GetType(Byte) OrElse t Is GetType(Byte) Then
                WriteMsgPack(writer, CByte(val))
            ElseIf t Is GetType(SByte) OrElse t Is GetType(SByte) Then
                WriteMsgPack(writer, CSByte(val))
            ElseIf t Is GetType(Short) OrElse t Is GetType(Short) Then
                WriteMsgPack(writer, CShort(val))
            ElseIf t Is GetType(UShort) OrElse t Is GetType(UShort) Then
                WriteMsgPack(writer, CUShort(val))
            ElseIf t Is GetType(Integer) OrElse t Is GetType(Integer) Then
                WriteMsgPack(writer, CInt(val))
            ElseIf t Is GetType(UInteger) OrElse t Is GetType(UInteger) Then
                WriteMsgPack(writer, CUInt(val))
            ElseIf t Is GetType(Long) OrElse t Is GetType(Long) Then
                WriteMsgPack(writer, CLng(val))
            ElseIf t Is GetType(ULong) OrElse t Is GetType(ULong) Then
                WriteMsgPack(writer, CULng(val))
            ElseIf t Is GetType(Boolean) OrElse t Is GetType(Boolean) Then
                WriteMsgPack(writer, CBool(val))
            ElseIf t Is GetType(Date) Then
                WriteMsgPack(writer, CDate(val))
            ElseIf t Is GetType(TimeSpan) Then
                WriteMsgPack(writer, CType(val, TimeSpan))
            ElseIf t Is GetType(Decimal) Then
                Throw New ApplicationException("The Decimal Type isn't supported")
            ElseIf t.IsEnum Then
                WriteMsgPack(writer, [Enum].GetName(t, val))
            ElseIf t.IsArray Then
                Dim array As Array = TryCast(val, Array)

                If array Is Nothing Then
                    writer.Write(Formats.NIL)
                Else

                    If array.Length <= 15 Then
                        Dim arrayVal As Byte = FixedArray.MIN + array.Length
                        writer.Write(arrayVal)
                    ElseIf array.Length <= UShort.MaxValue Then
                        writer.Write(Formats.ARRAY_16)
                        Dim data = BitConverter.GetBytes(CUShort(array.Length))
                        If BitConverter.IsLittleEndian Then Array.Reverse(data)
                        writer.Write(data)
                    Else
                        writer.Write(Formats.ARRAY_32)
                        Dim data = BitConverter.GetBytes(CUInt(array.Length))
                        If BitConverter.IsLittleEndian Then Array.Reverse(data)
                        writer.Write(data)
                    End If

                    SerializeEnumerable(array.GetEnumerator(), writer, serializationMethod)
                End If
            ElseIf MsgPackSerializer.IsGenericList(t) Then
                Dim list As IList = TryCast(val, IList)

                If list.Count <= 15 Then
                    Dim arrayVal As Byte = FixedArray.MIN + list.Count
                    writer.Write(arrayVal)
                ElseIf list.Count <= UShort.MaxValue Then
                    writer.Write(Formats.ARRAY_16)
                    Dim data = BitConverter.GetBytes(CUShort(list.Count))
                    If BitConverter.IsLittleEndian Then Array.Reverse(data)
                    writer.Write(data)
                Else
                    writer.Write(Formats.ARRAY_32)
                    Dim data = BitConverter.GetBytes(CUInt(list.Count))
                    If BitConverter.IsLittleEndian Then Array.Reverse(data)
                    writer.Write(data)
                End If

                SerializeEnumerable(list.GetEnumerator(), writer, serializationMethod)
            ElseIf MsgPackSerializer.IsGenericDictionary(t) Then
                Dim dictionary As IDictionary = TryCast(val, IDictionary)

                If dictionary.Count <= 15 Then
                    Dim header As Byte = FixedMap.MIN + dictionary.Count
                    writer.Write(header)
                ElseIf dictionary.Count <= UShort.MaxValue Then
                    writer.Write(Formats.MAP_16)
                    Dim data = BitConverter.GetBytes(CUShort(dictionary.Count))
                    If BitConverter.IsLittleEndian Then Array.Reverse(data)
                    writer.Write(data)
                Else
                    writer.Write(Formats.MAP_32)
                    Dim data = BitConverter.GetBytes(CUInt(dictionary.Count))
                    If BitConverter.IsLittleEndian Then Array.Reverse(data)
                    writer.Write(data)
                End If

                Dim enumerator As IDictionaryEnumerator = dictionary.GetEnumerator()

                While enumerator.MoveNext()
                    SerializeValue(enumerator.Key, writer, serializationMethod)
                    SerializeValue(enumerator.Value, writer, serializationMethod)
                End While
            Else
                MsgPackSerializer.SerializeObject(val, writer)
            End If
        End If
    End Sub
End Module
