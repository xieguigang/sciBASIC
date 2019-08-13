Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MIME.application.json.Parser

Public Class BSON
    Private mMemoryStream As MemoryStream
    Private mBinaryReader As BinaryReader
    Private mBinaryWriter As BinaryWriter

    Public Shared Function Load(buf As Byte()) As JsonObject
        Dim bson As New BSON(buf)

        Return bson.decodeDocument()
    End Function

    Public Shared Function Dump(obj As JsonObject) As Byte()

        Dim bson As New BSON()
        Dim ms As New MemoryStream()

        bson.encodeDocument(ms, obj)

        Dim buf As Byte() = New Byte(ms.Position - 1) {}
        ms.Seek(0, SeekOrigin.Begin)
        ms.Read(buf, 0, buf.Length)

        Return buf
    End Function

    Private Sub New(Optional buf As Byte() = Nothing)
        If buf IsNot Nothing Then
            mMemoryStream = New MemoryStream(buf)
            mBinaryReader = New BinaryReader(mMemoryStream)
        Else
            mMemoryStream = New MemoryStream()
            mBinaryWriter = New BinaryWriter(mMemoryStream)
        End If
    End Sub

    Private Function decodeElement(ByRef name As String) As JsonElement
        Dim elementType As Byte = mBinaryReader.ReadByte()

        If elementType = &H1 Then
            ' Double
            name = decodeCString()

            Return New JsonValue(New BSONValue(mBinaryReader.ReadDouble()))
        ElseIf elementType = &H2 Then
            ' String
            name = decodeCString()

            Return New JsonValue(New BSONValue(decodeString()))
        ElseIf elementType = &H3 Then
            ' Document
            name = decodeCString()

            Return decodeDocument()
        ElseIf elementType = &H4 Then
            ' Array
            name = decodeCString()

            Return decodeArray()
        ElseIf elementType = &H5 Then
            ' Binary
            name = decodeCString()
            Dim length As Integer = mBinaryReader.ReadInt32()
            Dim binaryType As Byte = mBinaryReader.ReadByte()


            Return New JsonValue(New BSONValue(mBinaryReader.ReadBytes(length)))
        ElseIf elementType = &H8 Then
            ' Boolean
            name = decodeCString()

            Return New JsonValue(New BSONValue(mBinaryReader.ReadBoolean()))
        ElseIf elementType = &H9 Then
            ' DateTime
            name = decodeCString()
            Dim time As Int64 = mBinaryReader.ReadInt64()
            Return New JsonValue(New BSONValue(New DateTime(1970, 1, 1, 0, 0, 0,
                DateTimeKind.Utc) + New TimeSpan(time * 10000)))
        ElseIf elementType = &HA Then
            ' None
            name = decodeCString()
            Return New JsonValue(New BSONValue())
        ElseIf elementType = &H10 Then
            ' Int32
            name = decodeCString()
            Return New JsonValue(New BSONValue(mBinaryReader.ReadInt32()))
        ElseIf elementType = &H12 Then
            ' Int64
            name = decodeCString()
            Return New JsonValue(New BSONValue(mBinaryReader.ReadInt64()))
        End If


        Throw New Exception(String.Format("Don't know elementType={0}", elementType))
    End Function

    Private Function decodeDocument() As JsonObject
        Dim length As Integer = mBinaryReader.ReadInt32() - 4

        Dim obj As New JsonObject()

        Dim i As Integer = CInt(mBinaryReader.BaseStream.Position)
        While mBinaryReader.BaseStream.Position < i + length - 1
            Dim name As String
            Dim value As JsonElement = decodeElement(name)

            obj.Add(name, value)
        End While

        mBinaryReader.ReadByte()
        ' zero
        Return obj
    End Function

    Private Function decodeArray() As JsonArray
        Dim obj As JsonObject = decodeDocument()

        Dim i As Integer = 0
        Dim array As New JsonArray()
        While obj.ContainsKey(Convert.ToString(i))
            array.Add(obj(Convert.ToString(i)))

            i += 1
        End While

        Return array
    End Function

    Private Function decodeString() As String
        Dim length As Integer = mBinaryReader.ReadInt32()
        Dim buf As Byte() = mBinaryReader.ReadBytes(length)

        Return Encoding.UTF8.GetString(buf)
    End Function

    Private Function decodeCString() As String

        Dim ms = New MemoryStream()
        While True
            Dim buf As Byte = CByte(mBinaryReader.ReadByte())
            If buf = 0 Then
                Exit While
            End If
            ms.WriteByte(buf)
        End While

        Return Encoding.UTF8.GetString(ms.GetBuffer(), 0, CInt(ms.Position))
    End Function


    Private Sub encodeElement(ms As MemoryStream, name As String, v As JsonElement)
        Select Case v.GetType
            Case GetType(JsonObject)
                ms.WriteByte(&H3)
                encodeCString(ms, name)
                encodeDocument(ms, TryCast(v, JsonObject))
            Case GetType(JsonArray)
                ms.WriteByte(&H4)
                encodeCString(ms, name)
                encodeArray(ms, TryCast(v, JsonArray))
            Case GetType(JsonValue)
                Dim value As BSONValue = DirectCast(v, JsonValue).value

                Select Case value.valueType
                    Case ValueType.[Double]
                        ms.WriteByte(&H1)
                        encodeCString(ms, name)
                        encodeDouble(ms, value.doubleValue)
                    Case ValueType.[String]
                        ms.WriteByte(&H2)
                        encodeCString(ms, name)
                        encodeString(ms, value.stringValue)
                    Case ValueType.Binary
                        ms.WriteByte(&H5)
                        encodeCString(ms, name)
                        encodeBinary(ms, value.binaryValue)
                    Case ValueType.[Boolean]
                        ms.WriteByte(&H8)
                        encodeCString(ms, name)
                        encodeBool(ms, value.boolValue)
                    Case ValueType.UTCDateTime
                        ms.WriteByte(&H9)
                        encodeCString(ms, name)
                        encodeUTCDateTime(ms, value.dateTimeValue)
                    Case ValueType.None
                        ms.WriteByte(&HA)
                        encodeCString(ms, name)
                    Case ValueType.Int32
                        ms.WriteByte(&H10)
                        encodeCString(ms, name)
                        encodeInt32(ms, value.int32Value)
                    Case ValueType.Int64
                        ms.WriteByte(&H12)
                        encodeCString(ms, name)
                        encodeInt64(ms, value.int64Value)
                    Case Else
                        Throw New InvalidCastException
                End Select
            Case Else
                Throw New NotImplementedException
        End Select
    End Sub

    Private Sub encodeDocument(ms As MemoryStream, obj As JsonObject)

        Dim dms As New MemoryStream()
        For Each str As String In obj.Keys
            encodeElement(dms, str, obj(str))
        Next

        Dim bw As New BinaryWriter(ms)
        bw.Write(CType(dms.Position + 4 + 1, Int32))
        bw.Write(dms.GetBuffer(), 0, CInt(dms.Position))
        bw.Write(CByte(0))
    End Sub

    Private Sub encodeArray(ms As MemoryStream, lst As JsonArray)

        Dim obj = New JsonObject()
        For i As Integer = 0 To lst.Count - 1
            obj.Add(Convert.ToString(i), lst(i))
        Next

        encodeDocument(ms, obj)
    End Sub

    Private Sub encodeBinary(ms As MemoryStream, buf As Byte())
        Dim aBuf As Byte() = BitConverter.GetBytes(buf.Length)
        ms.Write(aBuf, 0, aBuf.Length)
        ms.WriteByte(0)
        ms.Write(buf, 0, buf.Length)
    End Sub

    Private Sub encodeCString(ms As MemoryStream, v As String)
        Dim buf As Byte() = New UTF8Encoding().GetBytes(v)
        ms.Write(buf, 0, buf.Length)
        ms.WriteByte(0)
    End Sub

    Private Sub encodeString(ms As MemoryStream, v As String)
        Dim strBuf As Byte() = New UTF8Encoding().GetBytes(v)
        Dim buf As Byte() = BitConverter.GetBytes(strBuf.Length + 1)

        ms.Write(buf, 0, buf.Length)
        ms.Write(strBuf, 0, strBuf.Length)
        ms.WriteByte(0)
    End Sub

    Private Sub encodeDouble(ms As MemoryStream, v As Double)
        Dim buf As Byte() = BitConverter.GetBytes(v)
        ms.Write(buf, 0, buf.Length)
    End Sub

    Private Sub encodeBool(ms As MemoryStream, v As Boolean)
        Dim buf As Byte() = BitConverter.GetBytes(v)
        ms.Write(buf, 0, buf.Length)
    End Sub

    Private Sub encodeInt32(ms As MemoryStream, v As Int32)
        Dim buf As Byte() = BitConverter.GetBytes(v)
        ms.Write(buf, 0, buf.Length)
    End Sub
    Private Sub encodeInt64(ms As MemoryStream, v As Int64)
        Dim buf As Byte() = BitConverter.GetBytes(v)
        ms.Write(buf, 0, buf.Length)
    End Sub
    Private Sub encodeUTCDateTime(ms As MemoryStream, dt As DateTime)
        Dim span As TimeSpan
        If dt.Kind = DateTimeKind.Local Then
            span = (dt - New DateTime(1970, 1, 1, 0, 0, 0,
                0, DateTimeKind.Utc).ToLocalTime())
        Else
            span = dt - New DateTime(1970, 1, 1, 0, 0, 0,
                0, DateTimeKind.Utc)
        End If
        Dim buf As Byte() = BitConverter.GetBytes(CType(Math.Truncate(span.TotalSeconds * 1000), Int64))
        ms.Write(buf, 0, buf.Length)
    End Sub
End Class

