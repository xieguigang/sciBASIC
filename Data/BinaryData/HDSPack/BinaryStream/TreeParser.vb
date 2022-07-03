Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.FileIO.Path

Friend Module TreeParser

    Public Function Parse(buffer As Stream, registry As Dictionary(Of String, String)) As StreamGroup
        Dim size As Integer
        Dim bin As New BinaryDataReader(buffer) With {.ByteOrder = ByteOrder.BigEndian}
        Dim root As StreamGroup

        size = bin.ReadInt32
        bin = New BinaryDataReader(New SubStream(buffer, buffer.Position, size)) With {.ByteOrder = ByteOrder.BigEndian}
        root = bin.getCurrentDirectory(registry)

        Return root
    End Function

    <Extension>
    Private Function getCurrentDirectory(bin As BinaryDataReader, registry As Dictionary(Of String, String)) As StreamGroup
        Dim nfiles As Integer = bin.ReadInt32
        Dim path As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
        Dim tree As New Dictionary(Of String, StreamObject)
        Dim attrSize As Integer = bin.ReadInt32
        Dim attrBuf As Byte() = bin.ReadBytes(attrSize)

        For i As Integer = 1 To nfiles
            Dim flag As Integer = bin.ReadInt32
            Dim obj As StreamObject

            If flag = 0 Then
                ' is file
                obj = bin.getCurrentFile(registry)
            Else
                ' is folder
                obj = bin.getCurrentDirectory(registry)
            End If

            Call tree.Add(obj.fileName, obj)
        Next

        Dim dir As New StreamGroup(New FilePath(path), tree)
        dir.attributes = New MemoryStream(attrBuf).UnPack(dir.description, registry)
        Return dir
    End Function

    <Extension>
    Private Function getCurrentFile(bin As BinaryDataReader, registry As Dictionary(Of String, String)) As StreamBlock
        Dim name As String = bin.ReadString(BinaryStringFormat.ZeroTerminated)
        Dim offset As Long = bin.ReadInt64
        Dim size As Long = bin.ReadInt64
        Dim attrSize As Integer = bin.ReadInt32
        Dim attrBuf As Byte() = bin.ReadBytes(attrSize)
        Dim file As New StreamBlock(New FilePath(name)) With {
            .offset = offset,
            .size = size
        }
        file.attributes = New MemoryStream(attrBuf).UnPack(file.description, registry)
        Return file
    End Function
End Module
