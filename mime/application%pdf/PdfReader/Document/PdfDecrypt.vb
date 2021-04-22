Imports System

Namespace PdfReader
    Public MustInherit Class PdfDecrypt
        Inherits PdfObject

        Public Sub New(ByVal parent As PdfObject)
            MyBase.New(parent)
        End Sub

        Public MustOverride Function DecodeString(ByVal str As PdfString) As String
        Public MustOverride Function DecodeStringAsBytes(ByVal str As PdfString) As Byte()
        Public MustOverride Function DecodeStream(ByVal stream As PdfStream) As String
        Public MustOverride Function DecodeStreamAsBytes(ByVal stream As PdfStream) As Byte()

        Public Shared Function CreateDecrypt(ByVal doc As PdfDocument, ByVal trailer As PdfDictionary) As PdfDecrypt
            Dim ret As PdfDecrypt = New PdfDecryptNone(doc)

            ' Check for optional encryption reference
            Dim encryptRef = trailer.OptionalValue(Of PdfObjectReference)("Encrypt")

            If encryptRef IsNot Nothing Then
                Dim encryptDict = doc.IndirectObjects.OptionalValue(Of PdfDictionary)(encryptRef)
                Dim filter = encryptDict.MandatoryValue(Of PdfName)("Filter")
                Dim v = encryptDict.OptionalValue(Of PdfInteger)("V")

                ' We only implement the simple Standard, Version 1 scheme
                If Equals(filter.Value, "Standard") AndAlso v IsNot Nothing AndAlso v.Value = 1 Then
                    ret = New PdfDecryptStandard(doc, trailer, encryptDict)
                Else
                    Throw New ApplicationException("Can only decrypt the standard handler with version 1.")
                End If
            End If

            Return ret
        End Function
    End Class
End Namespace
