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
            .fmt = FMTSubChunk.ParseChunk(wav),
            .data = DataSubChunk.ParseData(wav)
        }
    End Function
End Class