Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO

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
        Call file.Close()
        Call file.Dispose()
    End Function

    Public Shared Sub WriteIndex(index As InvertedIndex, offsets As Long(), file As Stream)

    End Sub

End Class
