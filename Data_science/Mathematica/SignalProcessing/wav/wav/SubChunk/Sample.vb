
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure Sample

    ''' <summary>
    ''' 一个sample之中是由好几个声道的数据构成的
    ''' </summary>
    ''' <remarks>
    ''' 在这里使用Short来兼容8bit和16bit的数据
    ''' </remarks>
    Dim channels As Short()

    Public Overrides Function ToString() As String
        Return channels.GetJson
    End Function

    Friend Shared Iterator Function Parse16Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Dim sampleSize = channels * 2

        Do While Not wav.EndOfStream AndAlso (wav.Position + sampleSize <= wav.Length)
            Yield New Sample With {
                .channels = wav.ReadInt16s(channels)
            }
        Loop
    End Function

    Friend Shared Iterator Function Parse8Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Throw New NotImplementedException
    End Function

    Friend Shared Iterator Function Parse32Bit(wav As BinaryDataReader, channels As Integer) As IEnumerable(Of Sample)
        Throw New NotImplementedException
    End Function

End Structure
