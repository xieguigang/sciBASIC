Imports Microsoft.VisualBasic.Data.IO

''' <summary>
''' The wav file model
''' </summary>
Public Class File

    Public Property magic As String
    Public Property fileSize As Integer
    Public Property format As String
    Public Property fmt As FMTSubChunk
    Public Property data As DataSubChunk

    Public Shared Function ParseHeader(wav As BinaryDataReader) As File
        Return New File With {
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