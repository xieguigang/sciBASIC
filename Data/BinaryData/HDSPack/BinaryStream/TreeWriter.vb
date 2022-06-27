Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Text

Friend Module TreeWriter

    <Extension>
    Public Function GetBuffer(root As StreamGroup) As Byte()
        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms, encoding:=Encodings.UTF8WithoutBOM)
            Dim buf As Byte()

            Call bin.Write(root.files.Length)
            Call bin.Write(root.referencePath.ToString, BinaryStringFormat.ZeroTerminated)

            For Each file As StreamObject In root.files
                If TypeOf file Is StreamGroup Then
                    buf = DirectCast(file, StreamGroup).GetBuffer
                    bin.Write(DirectCast(file, StreamGroup).files.Length + 1)
                Else
                    buf = DirectCast(file, StreamBlock).GetBuffer
                    bin.Write(0)
                End If

                bin.Write(buf)
                bin.Flush()
            Next

            Return ms.ToArray
        End Using
    End Function

    <Extension>
    Private Function GetBuffer(file As StreamBlock) As Byte()
        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms, encoding:=Encodings.UTF8WithoutBOM)
            Call bin.Write(file.referencePath.ToString, BinaryStringFormat.ZeroTerminated)
            Call bin.Write(file.offset)
            Call bin.Write(file.size)
            Call bin.Flush()

            Return ms.ToArray
        End Using
    End Function

End Module
