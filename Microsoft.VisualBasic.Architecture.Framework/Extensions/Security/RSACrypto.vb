Imports System.IO
Imports System.Text
Imports System.Security.Cryptography

Namespace SecurityString

    Public Class RSACrypto : Inherits SecurityString.SecurityStringModel

        Dim rsa As RSACryptoServiceProvider = New RSACryptoServiceProvider()

        Public Overrides Function Decrypt(encrypted() As Byte) As Byte()
            Return rsa.Decrypt(encrypted, False)
        End Function

        Public Overrides Function DecryptString(text As String) As String
            Dim encrypted As Byte() = System.Text.ASCIIEncoding.UTF8.GetBytes(text)
            Dim decText As String = System.Text.ASCIIEncoding.UTF8.GetString(encrypted)
            Return decText
        End Function

        Public Overrides Function Encrypt(input() As Byte) As Byte()
            Return rsa.Encrypt(input, False)
        End Function

        Public Overrides Function EncryptData(text As String) As String
            Dim encrypted() As Byte = rsa.Encrypt(System.Text.ASCIIEncoding.UTF8.GetBytes(text), False)
            Dim encText As String = System.Text.ASCIIEncoding.UTF8.GetString(encrypted)
            Return encText
        End Function
    End Class
End Namespace