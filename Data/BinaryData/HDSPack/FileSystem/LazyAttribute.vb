Imports System.Reflection
Imports System.Text
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

    Friend Function ToArray() As AttributeMetadata()
        Return attributes.Values.ToArray
    End Function

    Public Shared Function GetValue(attr As AttributeMetadata) As Object
        If attr.data.IsNullOrEmpty Then
            Return Nothing
        End If

        Select Case attr.GetUnderlyingType
            Case GetType(Date) : Return FromUnixTimeStamp(BitConverter.ToDouble(attr.data, Scan0))
            Case GetType(String) : Return Encoding.UTF8.GetString(attr.data)
            Case Else
                Throw New NotImplementedException(attr.ToString)
        End Select
    End Function

    Public Shared Function GetBuffer(val As Object) As Byte()
        Select Case val.GetType
            Case GetType(Date) : Return BitConverter.GetBytes(DirectCast(val, Date).UnixTimeStamp)
            Case GetType(String) : Return Encoding.UTF8.GetBytes(DirectCast(val, String))
            Case GetType(Integer) : Return BitConverter.GetBytes(DirectCast(val, Integer))
            Case GetType(Long) : Return BitConverter.GetBytes(DirectCast(val, Long))
            Case GetType(Short) : Return BitConverter.GetBytes(DirectCast(val, Short))
            Case GetType(Single) : Return BitConverter.GetBytes(DirectCast(val, Single))
            Case GetType(Double) : Return BitConverter.GetBytes(DirectCast(val, Double))
            Case GetType(Byte) : Return {DirectCast(val, Byte)}
            Case Else
                Return MsgPackSerializer.SerializeObject(val)
        End Select
    End Function
End Class

Public Class AttributeMetadata

    Public Property name As String
    Public Property type As String
    Public Property data As Byte()

    Public ReadOnly Property GetUnderlyingType As Type
        Get
            If type.StringEmpty Then
                Return GetType(Void)
            Else
                Return TypeInfo.GetType(type)
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"Dim {name} As {type} = binary({StringFormats.Lanudry(data.Length)})"
    End Function

End Class