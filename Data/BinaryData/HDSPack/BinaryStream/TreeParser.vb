Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.FileIO.Path

Friend Module TreeParser

    Public Function Parse(buffer As Stream) As StreamGroup
        Dim size As Integer
        Dim bin As New BinaryDataReader(buffer)
        Dim root As StreamGroup

        size = bin.ReadInt32
        bin = New BinaryDataReader(New SubStream(buffer, buffer.Position, size))
        root = bin.getCurrentDirectory

        Return root
    End Function

    <Extension>
    Private Function getCurrentDirectory(bin As BinaryDataReader) As StreamGroup
        Dim nfiles As Integer = bin.ReadInt32
        Dim path As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
        Dim tree As New Dictionary(Of String, StreamObject)

        For i As Integer = 1 To nfiles
            Dim flag As Integer = bin.ReadInt32
            Dim obj As StreamObject

            If flag = 0 Then
                ' is file
                obj = bin.getCurrentFile
            Else
                ' is folder
                obj = bin.getCurrentDirectory
            End If

            Call tree.Add(obj.fileName, obj)
        Next

        Return New StreamGroup(New FilePath(path), tree)
    End Function

    <Extension>
    Private Function getCurrentFile(bin As BinaryDataReader) As StreamBlock
        Dim name As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
        Dim offset As Long = bin.ReadInt64
        Dim size As Long = bin.ReadInt64

        Return New StreamBlock(New FilePath(name)) With {
            .offset = offset,
            .size = size
        }
    End Function
End Module
