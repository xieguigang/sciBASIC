Imports System.IO
Imports Microsoft.VisualBasic.Data.IO

Public MustInherit Class SubChunk

    Public Property ChunkID As String
    Public Property ChunkSize As Integer

End Class

Public Class FMTSubChunk : Inherits SubChunk
    Public Property audioFormat As Integer
    Public Property Channels As Integer
    Public Property SampleRate As Integer
    Public Property ByteRate As Integer
    Public Property BlockAlign As Integer
    Public Property BitsPerSample As Integer
End Class

Public Class DataSubChunk : Inherits SubChunk

    Public Property Data As Integer()

    Public Shared Function ParseData(wav As BinaryDataReader) As DataSubChunk
        Do While wav.ReadString(4) <> "data"
            wav.Seek(-3, SeekOrigin.Current)
        Loop

        Call wav.Seek(-4, SeekOrigin.Current)

        Return New DataSubChunk With {
            .ChunkID = wav.ReadString(4),
            .ChunkSize = wav.ReadInt32,
            .Data = DataSubChunk.loadData(wav).ToArray
        }
    End Function

    Private Shared Iterator Function loadData(wav As BinaryDataReader) As IEnumerable(Of Integer)
        Do While Not wav.EndOfStream
            Yield wav.ReadInt32
        Loop
    End Function
End Class