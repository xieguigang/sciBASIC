Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.FileIO.Path

Friend Module TreeParser

    Public Function Parse(buffer As Stream) As StreamGroup
        Using bin As New BinaryDataReader(buffer)
            Dim size As Integer = bin.ReadInt32
            Dim path As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
            Dim tree As New Dictionary(Of String, StreamObject)

            Return New StreamGroup(New FilePath(path), tree)
        End Using
    End Function
End Module
