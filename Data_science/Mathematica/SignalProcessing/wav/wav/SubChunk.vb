Imports System.IO
Imports Microsoft.VisualBasic.Data.IO

Public MustInherit Class SubChunk

    Public Property chunkID As String
    Public Property chunkSize As Integer

End Class

Public Class FMTSubChunk : Inherits SubChunk
    Public Property audioFormat As Integer
    Public Property Channels As Integer
    Public Property SampleRate As Integer
    Public Property ByteRate As Integer
    Public Property BlockAlign As Integer
    Public Property BitsPerSample As Integer

    Public Shared Function ParseChunk(wav As BinaryDataReader) As FMTSubChunk
        Dim subchunk1ID As String = wav.ReadString(4)

        ' number data is in little endian
        wav.ByteOrder = ByteOrder.LittleEndian

        Return New FMTSubChunk With {
            .chunkID = subchunk1ID,
            .chunkSize = wav.ReadInt32,
            .audioFormat = wav.ReadInt16,
            .Channels = wav.ReadInt16,
            .SampleRate = wav.ReadInt32,
            .ByteRate = wav.ReadInt32,
            .BlockAlign = wav.ReadInt16,
            .BitsPerSample = wav.ReadInt16
        }
    End Function
End Class

Public Class DataSubChunk : Inherits SubChunk

    Public Property data As Integer()

    Public Shared Function ParseData(wav As BinaryDataReader) As DataSubChunk
        Do While wav.ReadString(4) <> "data"
            wav.Seek(-3, SeekOrigin.Current)
        Loop

        Call wav.Seek(-4, SeekOrigin.Current)

        Return New DataSubChunk With {
            .chunkID = wav.ReadString(4),
            .chunkSize = wav.ReadInt32,
            .data = DataSubChunk.loadData(wav).ToArray
        }
    End Function

    Private Shared Iterator Function loadData(wav As BinaryDataReader) As IEnumerable(Of Integer)
        ' little endian
        wav.ByteOrder = ByteOrder.LittleEndian

        Do While Not wav.EndOfStream
            Yield wav.ReadInt32
        Loop
    End Function
End Class