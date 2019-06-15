Imports Microsoft.VisualBasic.Data.IO

Module Module1

    Sub Main()

        Dim wav = "X:\22222.wav".OpenBinaryReader
        Dim header As wave = wave.ParseHeader(wav)



        Pause()
    End Sub

End Module

Public Class wave

    Public Property magic As String
    Public Property fileSize As Integer
    Public Property format As String

    Public Property fmt As FMTSubChunk
    Public Property data As DataSubChunk

    Public Shared Function ParseHeader(wav As BinaryDataReader) As wave
        Return New wave With {
            .magic = wav.ReadString(4),
            .fileSize = wav.ReadInt32,
            .format = wav.ReadString(4),
            .fmt = New FMTSubChunk With {
             .ChunkID = wav.ReadString(4),
            .ChunkSize = wav.ReadInt32,
            .audioFormat = wav.ReadInt16,
            .Channels = wav.ReadInt16,
            .SampleRate = wav.ReadInt32,
            .ByteRate = wav.ReadInt32,
            .BlockAlign = wav.ReadInt16,
            .BitsPerSample = wav.ReadInt16
        },
        .data = DataSubChunk.ParseData(wav)
        }
    End Function
End Class

Public Class SubChunk

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
            wav.Seek(-3, IO.SeekOrigin.Current)
        Loop

        wav.Seek(-4, IO.SeekOrigin.Current)

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