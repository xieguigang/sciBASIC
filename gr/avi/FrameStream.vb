Imports System.IO

Public Class FrameStream

    Public ReadOnly Property length As Integer

    ReadOnly temp$
    ReadOnly begin As Long

    Sub New(ref$, buf As Byte())
        length = buf.Length
        temp = ref

        Using writer As New BinaryWriter(ref.Open(FileMode.OpenOrCreate, doClear:=False))
            begin = writer.BaseStream.Length + 1

            writer.Seek(begin, SeekOrigin.Begin)
            writer.Write(buf)
            writer.Flush()
        End Using
    End Sub

    Public Overrides Function ToString() As String
        Return $"&{begin.ToHexString} [{length} bytes]"
    End Function

    Public Shared Narrowing Operator CType(stream As FrameStream) As Byte()
        Using reader As New BinaryReader(stream.temp.Open(FileMode.Open, doClear:=False))
            Call reader.BaseStream.Seek(stream.begin, SeekOrigin.Begin)
            Return reader.ReadBytes(stream.length)
        End Using
    End Operator
End Class
