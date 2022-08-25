Imports Microsoft.VisualBasic.Text

Public Interface IReaderDebugAccess

    ReadOnly Property Length As Long
    Property Position As Long

    Function ReadBytes(nsize As Integer) As Byte()

End Interface

Public Class Helpers

    Public Shared Function getDebugView(bin As IReaderDebugAccess, bufSize As Integer) As String
        Dim start As Long
        Dim nsize As Integer

        If bin.Position < bufSize \ 2 Then
            start = 0
        Else
            start = bin.Position - (bufSize \ 2)
        End If

        If start + bufSize > bin.Length Then
            nsize = bin.Length - start
        Else
            nsize = bufSize
        End If

        Dim chars As New List(Of Char)
        Dim c As Char

        For Each b As Byte In bin.ReadBytes(nsize)
            If ASCII.IsNonPrinting(b) Then
                c = "*"c
            Else
                c = Chr(b)
            End If

            If c = vbNullChar Then
                c = "*"
            End If

            chars.Add(c)
        Next

        Return chars.CharString
    End Function
End Class
