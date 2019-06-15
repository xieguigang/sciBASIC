Imports System.IO
Imports Microsoft.VisualBasic.Data.IO

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