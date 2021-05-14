Imports System.Text.RegularExpressions

Namespace ASN1

    Public Class Index
        ReadOnly reHex As New Regex("^\s*(?:[0-9A-Fa-f][0-9A-Fa-f]\s*)+$")
    End Class
End Namespace