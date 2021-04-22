Imports System

Namespace PdfReader
    Public Class TokenStringHex
        Inherits TokenString

        Public Sub New(ByVal raw As String)
            MyBase.New(raw)
        End Sub

        Public Overrides ReadOnly Property Resolved As String
            Get
                Return BytesToString(ResolvedAsBytes)
            End Get
        End Property

        Public Overrides ReadOnly Property ResolvedAsBytes As Byte()
            Get
                ' Remove all whitespace from the hex string
                Dim sections = MyBase.Raw.Split(New Char() {Microsoft.VisualBasic.Strings.ChrW(0), Microsoft.VisualBasic.Strings.ChrW(9), Microsoft.VisualBasic.Strings.ChrW(10), Microsoft.VisualBasic.Strings.ChrW(12), Microsoft.VisualBasic.Strings.ChrW(13), " "c})
                Dim hex = String.Join(String.Empty, sections)

                ' If a missing character from last hex pair, then default to 0, as per the spec
                If hex.Length Mod 2 = 1 Then hex += "0"

                ' Convert from hex to bytes
                Dim raw = New Byte(hex.Length / 2 - 1) {}

                For i = 0 To raw.Length - 1
                    raw(i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
                Next

                Return raw
            End Get
        End Property

        Public Overrides Function BytesToString(ByVal bytes As Byte()) As String
            Return EncodedBytesToString(bytes)
        End Function
    End Class
End Namespace
