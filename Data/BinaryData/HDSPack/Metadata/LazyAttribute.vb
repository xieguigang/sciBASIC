Imports System.Text
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.ValueTypes

Public Class LazyAttribute

    Public Property attributes As New Dictionary(Of String, AttributeMetadata)

    Public Sub Add(name As String, value As Object)
        Dim attr As AttributeMetadata

        If value Is Nothing Then
            attr = New AttributeMetadata With {
                .name = name,
                .data = Nothing,
                .type = Nothing
            }
        Else
            attr = New AttributeMetadata With {
                .name = name,
                .type = value.GetType.FullName,
                .data = GetBuffer(value)
            }
        End If

        Call attributes.Add(name, attr)
    End Sub

    Public Function GetValue(name As String) As Object
        Dim attr As AttributeMetadata = attributes.TryGetValue(name)

        If attr Is Nothing Then
            Return Nothing
        Else
            Return GetValue(attr)
        End If
    End Function

    Friend Function ToArray() As AttributeMetadata()
        Return attributes.Values.ToArray
    End Function

    Public Shared Function GetValue(attr As AttributeMetadata) As Object
        If attr.data.IsNullOrEmpty Then
            Return Nothing
        End If

        Select Case attr.GetUnderlyingType
            Case GetType(Date) : Return FromUnixTimeStamp(NetworkByteOrderBitConvertor.ToDouble(attr.data, Scan0))
            Case GetType(String) : Return Encoding.UTF8.GetString(attr.data)
            Case GetType(Single) : Return NetworkByteOrderBitConvertor.ToSingle(attr.data, Scan0)
            Case GetType(Double) : Return NetworkByteOrderBitConvertor.ToDouble(attr.data, Scan0)
            Case GetType(Short) : Return NetworkByteOrderBitConvertor.ToInt16(attr.data, Scan0)
            Case GetType(Integer) : Return NetworkByteOrderBitConvertor.ToInt32(attr.data, Scan0)
            Case GetType(Long) : Return NetworkByteOrderBitConvertor.ToInt64(attr.data, Scan0)
            Case GetType(Byte) : Return attr.data(Scan0)
            Case Else
                Throw New NotImplementedException(attr.ToString)
        End Select
    End Function

    Public Shared Function GetBuffer(val As Object) As Byte()
        Select Case val.GetType
            Case GetType(Date) : Return NetworkByteOrderBitConvertor.GetBytes(DirectCast(val, Date).UnixTimeStamp)
            Case GetType(String) : Return Encoding.UTF8.GetBytes(DirectCast(val, String))
            Case GetType(Integer) : Return NetworkByteOrderBitConvertor.GetBytes(DirectCast(val, Integer))
            Case GetType(Long) : Return NetworkByteOrderBitConvertor.GetBytes(DirectCast(val, Long))
            Case GetType(Short) : Return NetworkByteOrderBitConvertor.GetBytes(DirectCast(val, Short))
            Case GetType(Single) : Return NetworkByteOrderBitConvertor.GetBytes(DirectCast(val, Single))
            Case GetType(Double) : Return NetworkByteOrderBitConvertor.GetBytes(DirectCast(val, Double))
            Case GetType(Byte) : Return {DirectCast(val, Byte)}
            Case Else
                Return MsgPackSerializer.SerializeObject(val)
        End Select
    End Function
End Class

