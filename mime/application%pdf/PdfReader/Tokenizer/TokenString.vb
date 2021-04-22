Imports System.Text

Namespace PdfReader
    Public MustInherit Class TokenString
        Inherits TokenObject

        Private _Raw As String

        Public Sub New(ByVal raw As String)
            Me.Raw = raw
        End Sub

        Public Property Raw As String
            Get
                Return _Raw
            End Get
            Private Set(ByVal value As String)
                _Raw = value
            End Set
        End Property

        Public MustOverride ReadOnly Property Resolved As String
        Public MustOverride ReadOnly Property ResolvedAsBytes As Byte()
        Public MustOverride Function BytesToString(ByVal raw As Byte()) As String

        Protected Function EncodedBytesToString(ByVal bytes As Byte()) As String
            ' Check for the UTF16 Byte Order Mark (little endian or big endian versions)
            If bytes.Length > 2 AndAlso bytes(0) = &HFE AndAlso bytes(1) = &HFF Then
                Return GetStringLiteralUTF16(bytes, True)
            ElseIf bytes.Length > 2 AndAlso bytes(0) = &HFF AndAlso bytes(1) = &HFE Then
                Return GetStringLiteralUTF16(bytes, False)
            Else
                ' Not unicode, so treat as ASCII
                Return Encoding.ASCII.GetString(bytes)
            End If
        End Function

        Private Function GetStringLiteralUTF16(ByVal bytes As Byte(), ByVal bigEndian As Boolean) As String
            Dim index = 0
            Dim last = bytes.Length - 1

            If bigEndian Then
                ' Swap byte ordering
                Dim temp As Byte

                While index < last
                    ' Switch byte order of each character pair
                    temp = bytes(index)
                    bytes(index) = bytes(index + 1)
                    bytes(index + 1) = temp
                    index += 2
                End While
            End If

            Return Encoding.Unicode.GetString(bytes)
        End Function
    End Class
End Namespace
