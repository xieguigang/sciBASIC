Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace My

    Public Module RlangInterop

        ''' <summary>
        ''' processing a unicode char like ``&lt;U+767D>`` 
        ''' </summary>
        ''' <param name="output"></param>
        ''' <returns></returns>
        Public Function ProcessingRUniCode(output As String) As String
            Dim unicodes As String() = output.Matches("[<]U[+][A-H0-9]+[>]").Distinct.ToArray
            Dim charCode As Integer
            Dim [char] As Char
            Dim str As New StringBuilder(output)

            For Each code As String In unicodes
                charCode = code.GetStackValue("<", ">").Split("+"c).Last.DoCall(AddressOf i32.GetHexInteger)
                [char] = Strings.ChrW(charCode)
                str.Replace(code, [char])
            Next

            Return str.ToString
        End Function

        Public Function ProcessingRRawUniCode(output As String, Optional encoding As Encodings = Encodings.Unicode) As String
            Dim raw As String() = output.Matches("([<][a-z0-9]{1,2}[>])+").ToArray
            Dim str As New StringBuilder(output)
            Dim bytes As Byte()
            Dim unicodeStr As String
            Dim charset As Encoding = encoding.CodePage

            For Each part As String In raw
                bytes = part _
                    .Matches("[<][a-z0-9]{1,2}[>]") _
                    .Select(Function(strVal)
                                Return strVal.GetStackValue("<", ">")
                            End Function) _
                    .Select(Function(hex)
                                Return CByte(i32.GetHexInteger(hex))
                            End Function) _
                    .ToArray
                unicodeStr = charset.GetString(bytes)

                str.Replace(part, unicodeStr)
            Next

            Return str.ToString
        End Function
    End Module
End Namespace