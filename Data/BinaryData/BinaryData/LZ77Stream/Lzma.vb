Option Strict On
Option Explicit On

Imports System.IO

Namespace LZ77Stream

    ''' <summary>
    ''' LZMA (alone format) decompression helper.
    ''' </summary>
    Public NotInheritable Class Lzma

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Decompresses a complete .lzma (alone format) stream read from
        ''' <paramref name="input"/> into a single byte array.
        ''' </summary>
        Public Shared Function Decompress(input As Stream) As Byte()
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            Using output As New MemoryStream()
                Using decoder As New LzmaStream(input, leaveOpen:=True)
                    decoder.CopyTo(output)
                End Using
                Return output.ToArray()
            End Using
        End Function

    End Class
End Namespace