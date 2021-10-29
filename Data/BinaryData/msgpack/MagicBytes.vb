Imports Microsoft.VisualBasic.Data.IO.MessagePack.Constants

Public Module MagicBytes

    Public Function IsArray(magic As Byte) As Boolean
        If magic > FixedArray.MIN AndAlso magic <= FixedArray.MIN + 15 Then
            Return True
        Else
            Throw New NotImplementedException
        End If
    End Function

    Public Function TypeOfMagic(magic As Byte) As Type
        Select Case magic
            Case MsgPackFormats.DOUBLE : Return GetType(Double)
            Case MsgPackFormats.FLOAT_32 : Return GetType(Single)
            Case Else
                Throw New NotImplementedException(magic)
        End Select
    End Function
End Module
