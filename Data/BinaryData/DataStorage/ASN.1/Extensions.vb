Imports System.Text.RegularExpressions

Namespace ASN1

    <HideModuleName> Module Extensions

        Public Function stringCut(str$, len%) As String
            If (str.Length > len) Then
                str = str.Substring(0, len) & "..."
            End If

            Return str
        End Function

        Public Const hexDigits = "0123456789ABCDEF"
        Public Const b64Safe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"

        Public ReadOnly reTimeS As New Regex("^(\d\d)(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])([01]\d|2[0-3])(?:([0-5]\d)(?:([0-5]\d)(?:[.,](\d{1,3}))?)?)?(Z|[-+](?:[0]\d|1[0-2])([0-5]\d)?)?$", RegexICMul)
        Public ReadOnly reTimeL As New Regex("^(\d\d\d\d)(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])([01]\d|2[0-3])(?:([0-5]\d)(?:([0-5]\d)(?:[.,](\d{1,3}))?)?)?(Z|[-+](?:[0]\d|1[0-2])([0-5]\d)?)?$", RegexICMul)

        Public Function hexByte(b As Byte) As String
            Return hexDigits((b >> 4) And &HF) & hexDigits(b And &HF)
        End Function
    End Module

End Namespace