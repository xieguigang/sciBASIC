#Region "Microsoft.VisualBasic::e095b2781c36f2bb7504e96e8921b727, mime\application%json\BSON\Encoder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class Encoder
    ' 
    '         Sub: encodeArray, encodeBinary, encodeBool, encodeCString, encodeDocument
    '              encodeDouble, encodeElement, encodeInt32, encodeInt64, encodeString
    '              encodeUTCDateTime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports stdNum = System.Math

Namespace BSON

    Public Class Encoder

        Private Sub encodeElement(ms As Stream, name As String, v As JsonElement)
            If v Is Nothing Then
                Return
            End If

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
                    Dim value As BSONValue = DirectCast(v, JsonValue).BSONValue

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

        Public Sub encodeDocument(ms As Stream, obj As JsonObject)
            Dim dms As New MemoryStream()

            For Each str As String In obj.Keys
                encodeElement(dms, str, obj(str))
            Next

            Dim bw As New BinaryWriter(ms)
            bw.Write(CType(dms.Position + 4 + 1, Int32))
            bw.Write(dms.GetBuffer(), 0, CInt(dms.Position))
            bw.Write(CByte(0))
        End Sub

        Public Sub encodeArray(ms As Stream, lst As JsonArray)
            Dim obj As New JsonObject()

            For i As Integer = 0 To lst.Count - 1
                obj.Add(Convert.ToString(i), lst(i))
            Next

            encodeDocument(ms, obj)
        End Sub

        Private Sub encodeBinary(ms As Stream, buf As Byte())
            Dim aBuf As Byte() = BitConverter.GetBytes(buf.Length)
            ms.Write(aBuf, 0, aBuf.Length)
            ms.WriteByte(0)
            ms.Write(buf, 0, buf.Length)
        End Sub

        Private Sub encodeCString(ms As Stream, v As String)
            Dim buf As Byte() = New UTF8Encoding().GetBytes(v)
            ms.Write(buf, 0, buf.Length)
            ms.WriteByte(0)
        End Sub

        Private Sub encodeString(ms As Stream, v As String)
            Dim strBuf As Byte() = New UTF8Encoding().GetBytes(v)
            Dim buf As Byte() = BitConverter.GetBytes(strBuf.Length + 1)

            ms.Write(buf, 0, buf.Length)
            ms.Write(strBuf, 0, strBuf.Length)
            ms.WriteByte(0)
        End Sub

        Private Sub encodeDouble(ms As Stream, v As Double)
            Dim buf As Byte() = BitConverter.GetBytes(v)
            ms.Write(buf, 0, buf.Length)
        End Sub

        Private Sub encodeBool(ms As Stream, v As Boolean)
            Dim buf As Byte() = BitConverter.GetBytes(v)
            ms.Write(buf, 0, buf.Length)
        End Sub

        Private Sub encodeInt32(ms As Stream, v As Int32)
            Dim buf As Byte() = BitConverter.GetBytes(v)
            ms.Write(buf, 0, buf.Length)
        End Sub

        Private Sub encodeInt64(ms As Stream, v As Int64)
            Dim buf As Byte() = BitConverter.GetBytes(v)
            ms.Write(buf, 0, buf.Length)
        End Sub

        Private Sub encodeUTCDateTime(ms As Stream, dt As DateTime)
            Dim span As TimeSpan
            If dt.Kind = DateTimeKind.Local Then
                span = (dt - New DateTime(1970, 1, 1, 0, 0, 0,
                0, DateTimeKind.Utc).ToLocalTime())
            Else
                span = dt - New DateTime(1970, 1, 1, 0, 0, 0,
                0, DateTimeKind.Utc)
            End If
            Dim buf As Byte() = BitConverter.GetBytes(CType(stdNum.Truncate(span.TotalSeconds * 1000), Int64))
            ms.Write(buf, 0, buf.Length)
        End Sub
    End Class
End Namespace
