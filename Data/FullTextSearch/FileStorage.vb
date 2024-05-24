Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' document text file 
''' </summary>
Public Class FileStorage

    ReadOnly doc_stream As BinaryDataReader
    ReadOnly file As Stream
    ReadOnly offsets As New List(Of Long)

    Sub New(offsets As Long(), doc As Stream)
        Me.offsets = New List(Of Long)(offsets)
        Me.doc_stream = New BinaryDataReader(doc, Encoding.UTF8)
        Me.file = doc
    End Sub

    Public Sub Save(text As String)
        Dim writer As New BinaryDataWriter(file, Encoding.UTF8)

        offsets.Add(file.Length)

        writer.Seek(file.Length, SeekOrigin.Begin)
        writer.Write(text, BinaryStringFormat.DwordLengthPrefix)
        writer.Flush()
    End Sub

    Public Function GetDocument(id As Integer) As String
        Dim offset As Long = offsets(id)
        doc_stream.Seek(offset, SeekOrigin.Begin)
        Return doc_stream.ReadString(BinaryStringFormat.DwordLengthPrefix)
    End Function

    Public Shared Function ReadIndex(file As Stream, ByRef offsets As Long()) As InvertedIndex
        Dim index As InvertedIndex

        If file.Length = 0 Then
            offsets = Nothing
            index = New InvertedIndex
        Else
            Dim reader As New BinaryDataReader(file, Encoding.UTF8)
            Dim nsize As Integer = reader.ReadInt32
            Dim lastId As Integer = reader.ReadInt32
            Dim ids As New Dictionary(Of String, List(Of Integer))
            Dim token As String
            Dim idsize As Integer

            offsets = reader.ReadInt64s(lastId)

            For i As Integer = 0 To nsize - 1
                token = reader.ReadString(BinaryStringFormat.ByteLengthPrefix)
                idsize = reader.ReadInt32
                ids.Add(token, New List(Of Integer)(reader.ReadInt32s(idsize)))
            Next

            index = New InvertedIndex(ids, lastId:=lastId)
        End If

        Call file.Close()
        Call file.Dispose()

        Return index
    End Function

    Public Shared Sub WriteIndex(index As InvertedIndex, offsets As Long(), file As Stream)
        Dim bin As New BinaryDataWriter(file, Encoding.UTF8)

        bin.Write(index.size)
        bin.Write(index.lastId)
        bin.Write(offsets)

        For Each token As NamedCollection(Of Integer) In index.AsEnumerable
            Call bin.Write(token.name, BinaryStringFormat.ByteLengthPrefix)
            Call bin.Write(token.Length)
            Call bin.Write(token.value)
        Next

        Call bin.Flush()
        Call bin.Close()
        Call bin.Dispose()
    End Sub

End Class
