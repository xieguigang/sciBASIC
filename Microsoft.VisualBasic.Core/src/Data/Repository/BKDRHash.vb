Imports System.Text

Namespace Data.Repository

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/fernandezja/ColorHashSharp/blob/master/src/ColorHashSharp/BKDRHash.cs
    ''' </remarks>
    Public NotInheritable Class BKDRHash

        Const PADDING_CHAR As Char = "x"c

        ''' <summary>
        ''' The Number.MAX_SAFE_INTEGER constant represents the maximum safe integer in JavaScript  
        ''' https://developer.mozilla.org/es/docs/Web/JavaScript/Referencia/Objetos_globales/Number/MAX_SAFE_INTEGER
        ''' </summary>
        Const JAVASCRIPT_MAX_SAFE_INTEGER As ULong = 9007199254740991
        Const SEED As ULong = 131
        Const SEED2 As ULong = 137

        Private Sub New()
        End Sub

        Public Shared Function GenerateVersion2(value As String) As ULong
            Dim hashcode As Long = 0
            Dim bytes As Byte()
            ' Make hash more sensitive for short string like 'a', 'b', 'c'
            Dim valueWithPadding = $"{value}{PADDING_CHAR}"
            Dim valueUtf8 = ToUTF8(valueWithPadding)
            Dim max = Long.MaxValue / SEED

            For i As Integer = 0 To valueUtf8.Length - 1
                If hashcode > max Then
                    hashcode = hashcode / SEED2
                End If

                bytes = ToUTF8Bytes(valueUtf8(i).ToString())
                hashcode = hashcode * CLng(SEED) + bytes(0)
            Next

            Return hashcode
        End Function

        ''' <summary>
        ''' BKDR Hash (modified version). Idem  original code.
        ''' https://github.com/zenozeng/color-hash/blob/master/lib/bkdr-hash.js
        ''' Example values nodejs https://repl.it/@Jose_AA/BKDR-Hash
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Shared Function GenerateVersion3(value As String) As ULong
            Dim hash As ULong = 0
            Dim bytes As Byte()
            ' Make hash more sensitive for short string like 'a', 'b', 'c'
            Dim valueWithPadding = $"{value}{PADDING_CHAR}"
            Dim valueUtf8 = ToUTF8(valueWithPadding)
            Dim max = JAVASCRIPT_MAX_SAFE_INTEGER / SEED

            For i As Integer = 0 To valueUtf8.Length - 1
                If hash > max Then
                    hash = hash / SEED2
                End If

                bytes = ToUTF8Bytes(valueUtf8(i).ToString())
                hash = hash * SEED + bytes(0)
            Next

            Return hash
        End Function

        Private Shared Function ToUTF8(value As String) As String
            Dim bytes = Encoding.Default.GetBytes(value)
            Return Encoding.UTF8.GetString(bytes)
        End Function

        Private Shared Function ToUTF8Bytes(value As String) As Byte()
            Dim bytes = Encoding.UTF8.GetBytes(value)
            Return bytes
        End Function
    End Class
End Namespace
