Imports System.Text

Public Class FixLengthString

    ReadOnly encoding As Encoding

    Sub New(encoding As Encoding)
        Me.encoding = encoding
    End Sub

    Public Function GetBytes(text$, bytLen%) As Byte()
        Dim bytes As Byte() = encoding.GetBytes(text)

        If bytes.Length > bytLen Then
            Return bytes.Take(bytLen).ToArray
        ElseIf bytes.Length < bytLen Then
            ReDim Preserve bytes(bytLen - 1)
            Return bytes
        Else
            Return bytes
        End If
    End Function

    Public Overrides Function ToString() As String
        Return encoding.ToString
    End Function
End Class
